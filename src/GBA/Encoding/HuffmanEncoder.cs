using System;
using System.Collections.Generic;
using System.IO;

namespace BinarySerializer.Nintendo.GBA
{
    // Implemented from dsdecmp, todo: refactor code to follow project coding style
    public class HuffmanEncoder : BaseEncoder
    {
        protected override byte HeaderValue => EncodedBlockSize;

        /// <summary>
        /// The block size to use when encoding
        /// </summary>
        public byte EncodedBlockSize { get; set; } = 8;

        public override int ID => 2;
        public override string Name => "GBA_Huffman";

        protected override void DecodeStream(Reader reader, Stream output, uint decompressedSize, byte headerValue)
        {
            /*
                Data Header (32bit)
                    Bit0-3   Data size in bit units (normally 4 or 8)
                    Bit4-7   Compressed type (must be 2 for Huffman)
                    Bit8-31  24bit size of decompressed data in bytes
                Tree Size (8bit)
                    Bit0-7   Size of Tree Table/2-1 (ie. Offset to Compressed Bitstream)
                Tree Table (list of 8bit nodes, starting with the root node)
                    Root Node and Non-Data-Child Nodes are:
                    Bit0-5   Offset to next child node,
                            Next child node0 is at (CurrentAddr AND NOT 1)+Offset*2+2
                            Next child node1 is at (CurrentAddr AND NOT 1)+Offset*2+2+1
                    Bit6     Node1 End Flag (1=Next child node is data)
                    Bit7     Node0 End Flag (1=Next child node is data)
                    Data nodes are (when End Flag was set in parent node):
                    Bit0-7   Data (upper bits should be zero if Data Size is less than 8)
                Compressed Bitstream (stored in units of 32bits)
                    Bit0-31  Node Bits (Bit31=First Bit)  (0=Node0, 1=Node1)
            */

            int blockSize = headerValue;

            #region Read the Huff-tree

            int treeSize = reader.ReadByte();
            treeSize = (treeSize + 1) * 2;

            long treeEnd = (reader.BaseStream.Position - 1) + treeSize;

            int recursionCount = 0;

            // the relative offset may be 4 more (when the initial decompressed size is 0), but
            // since it's relative that doesn't matter, especially when it only matters if
            // the given value is odd or even.
            HuffTreeNode rootNode = new HuffTreeNode(reader, false, 5, treeEnd, ref recursionCount);

            // re-position the stream after the tree (the stream is currently positioned after the root
            // node, which is located at the start of the tree definition)
            reader.BaseStream.Position = treeEnd;

            #endregion

            // the current u32 we are reading bits from.
            uint data = 0;
            // the amount of bits left to read from <data>
            byte bitsLeft = 0;

            // a cache used for writing when the block size is four bits
            int cachedByte = -1;

            // the current output size
            int currentSize = 0;
            HuffTreeNode currentNode = rootNode;

            while (currentSize < decompressedSize)
            {
                #region find the next reference to a data node

                while (!currentNode.IsData)
                {
                    // if there are no bits left to read in the data, get a new byte from the input
                    if (bitsLeft == 0)
                    {
                        data = reader.ReadUInt32();
                        bitsLeft = 32;
                    }

                    // get the next bit
                    bitsLeft--;
                    bool nextIsOne = (data & (1 << bitsLeft)) != 0;
                    // go to the next node, the direction of the child depending on the value of the current/next bit
                    currentNode = nextIsOne ? currentNode.Child1 : currentNode.Child0;
                }

                #endregion

                #region write the data in the current node (when possible)

                switch (blockSize)
                {
                    case 8:
                        // just copy the data if the block size is a full byte
                        output.WriteByte(currentNode.Data);
                        currentSize++;
                        break;

                    case 4:
                        // cache the first half of the data if the block size is a half byte
                        if (cachedByte < 0)
                        {
                            cachedByte = currentNode.Data;
                        }
                        else
                        {
                            // if we already cached a half-byte, combine the two halves and write the full byte.
                            cachedByte |= (currentNode.Data << 4);
                            output.WriteByte((byte)cachedByte);
                            currentSize++;
                            // be sure to forget the two written half-bytes
                            cachedByte = -1;
                        }
                        break;

                    default:
                        throw new Exception($"Unsupported block size {blockSize}");
                }

                #endregion

                // make sure to start over next round
                currentNode = rootNode;
            }
        }

        protected override void EncodeStream(Reader reader, Writer writer)
        {
            throw new NotImplementedException();

            // TODO: Below code appears to not work
            /*
            switch (EncodedBlockSize)
            {
                case 4:
                    Encode4(reader, writer);
                    break;

                case 8:
                    Encode8(reader, writer);
                    break;

                default:
                    throw new Exception($"Unsupported block size {EncodedBlockSize}");
            }*/
        }

        private void Encode4(Reader reader, Writer writer)
        {
            // cache the input, as we need to build a frequency table
            byte[] inputData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

            // build that frequency table.
            int[] frequencies = new int[0x10];
            foreach (byte b in inputData)
            {
                frequencies[b & 0xF]++;
                frequencies[(b >> 4) & 0xF]++;
            }

            #region Build the Huffman tree

            SimpleReversedPrioQueue<int, HuffTreeNode> leafQueue = new SimpleReversedPrioQueue<int, HuffTreeNode>();
            SimpleReversedPrioQueue<int, HuffTreeNode> nodeQueue = new SimpleReversedPrioQueue<int, HuffTreeNode>();
            int nodeCount = 0;
            // make all leaf nodes, and put them in the leaf queue. Also save them for later use.
            HuffTreeNode[] leaves = new HuffTreeNode[0x10];
            for (int i = 0; i < 0x10; i++)
            {
                // there is no need to store leaves that are not used
                if (frequencies[i] == 0)
                    continue;
                HuffTreeNode node = new HuffTreeNode((byte)i, true, null, null);
                leaves[i] = node;
                leafQueue.Enqueue(frequencies[i], node);
                nodeCount++;
            }

            while (leafQueue.Count + nodeQueue.Count > 1)
            {
                // get the two nodes with the lowest priority.
                HuffTreeNode one = GetLowest(leafQueue, nodeQueue, out int onePrio);
                HuffTreeNode two = GetLowest(leafQueue, nodeQueue, out int twoPrio);

                // give those two a common parent, and put that node in the node queue
                HuffTreeNode newNode = new HuffTreeNode(0, false, one, two);
                nodeQueue.Enqueue(onePrio + twoPrio, newNode);
                nodeCount++;
            }

            HuffTreeNode root = nodeQueue.Dequeue(out int _);
            // set the depth of all nodes in the tree, such that we know for each leaf how long
            // its codeword is.
            root.Depth = 0;

            #endregion

            // now that we have a tree, we can write that tree and follow with the data.

            #region write the tree

            writer.Write((byte)((nodeCount - 1) / 2));

            // use a breadth-first traversal to store the tree, such that we do not need to store/calculate the side of each sub-tree.
            // because the data is only 4 bits long, no tree will ever let the offset field overflow.
            LinkedList<HuffTreeNode> printQueue = new LinkedList<HuffTreeNode>();
            printQueue.AddLast(root);
            while (printQueue.Count > 0)
            {
                HuffTreeNode node = printQueue.First.Value;
                printQueue.RemoveFirst();
                if (node.IsData)
                {
                    writer.Write(node.Data);
                }
                else
                {
                    // bits 0-5: 'offset' = # nodes in queue left
                    // bit 6: node1 end flag
                    // bit 7: node0 end flag
                    byte data = (byte)(printQueue.Count / 2);
                    if (data > 0x3F)
                        throw new InvalidDataException("BUG: offset overflow in 4-bit huffman.");
                    data = (byte)(data & 0x3F);
                    if (node.Child0.IsData)
                        data |= 0x80;
                    if (node.Child1.IsData)
                        data |= 0x40;
                    writer.Write(data);

                    printQueue.AddLast(node.Child0);
                    printQueue.AddLast(node.Child1);
                }
            }

            #endregion

            #region write the data

            // the codewords are stored in blocks of 32 bits
            uint datablock = 0;
            byte bitsLeftToWrite = 32;

            for (int i = 0; i < inputData.Length; i++)
            {
                byte data = inputData[i];

                for (int j = 0; j < 2; j++)
                {
                    HuffTreeNode node = leaves[(data >> (4 - j * 4)) & 0xF];
                    // the depth of the node is the length of the codeword required to encode the byte
                    int depth = node.Depth;
                    bool[] path = new bool[depth];
                    for (int d = 0; d < depth; d++)
                    {
                        path[depth - d - 1] = node.IsChild1;
                        node = node.Parent;
                    }
                    for (int d = 0; d < depth; d++)
                    {
                        if (bitsLeftToWrite == 0)
                        {
                            writer.Write(datablock);
                            datablock = 0;
                            bitsLeftToWrite = 32;
                        }
                        bitsLeftToWrite--;
                        if (path[d])
                            datablock |= (uint)(1 << bitsLeftToWrite);
                        // no need to OR the buffer with 0 if it is child0
                    }
                }
            }

            // write the partly filled data block as well
            if (bitsLeftToWrite != 32)
                writer.Write(datablock);

            #endregion
        }

        private void Encode8(Reader reader, Writer writer)
        {
            // cache the input, as we need to build a frequency table
            byte[] inputData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

            // build that frequency table.
            int[] frequencies = new int[0x100];
            foreach (byte b in inputData)
                frequencies[b]++;

            #region Build the Huffman tree

            SimpleReversedPrioQueue<int, HuffTreeNode> leafQueue = new SimpleReversedPrioQueue<int, HuffTreeNode>();
            SimpleReversedPrioQueue<int, HuffTreeNode> nodeQueue = new SimpleReversedPrioQueue<int, HuffTreeNode>();
            int nodeCount = 0;

            // make all leaf nodes, and put them in the leaf queue. Also save them for later use.
            HuffTreeNode[] leaves = new HuffTreeNode[0x100];
            for (int i = 0; i < 0x100; i++)
            {
                // there is no need to store leaves that are not used
                if (frequencies[i] == 0)
                    continue;
                HuffTreeNode node = new HuffTreeNode((byte)i, true, null, null);
                leaves[i] = node;
                leafQueue.Enqueue(frequencies[i], node);
                nodeCount++;
            }

            while (leafQueue.Count + nodeQueue.Count > 1)
            {
                // get the two nodes with the lowest priority.
                HuffTreeNode one = GetLowest(leafQueue, nodeQueue, out int onePrio);
                HuffTreeNode two = GetLowest(leafQueue, nodeQueue, out int twoPrio);

                // give those two a common parent, and put that node in the node queue
                HuffTreeNode newNode = new HuffTreeNode(0, false, one, two);
                nodeQueue.Enqueue(onePrio + twoPrio, newNode);
                nodeCount++;
            }

            HuffTreeNode root = nodeQueue.Dequeue(out int _);

            // set the depth of all nodes in the tree, such that we know for each leaf how long
            // its codeword is.
            root.Depth = 0;

            #endregion

            // now that we have a tree, we can write that tree and follow with the data.

            #region write the tree

            writer.Write((byte)((nodeCount - 1) / 2));

            // use a breadth-first traversal to store the tree, such that we do not need to store/calculate the size of each sub-tree.
            // NO! BF results in an ordering that may overflow the offset field.

            // find the BF order of all nodes that have two leaves as children. We're going to insert them in an array in reverse BF order,
            // inserting the parent whenever both children have been inserted.

            LinkedList<HuffTreeNode> leafStemQueue = new LinkedList<HuffTreeNode>();

            #region fill the leaf queue; first->last will be reverse BF
            LinkedList<HuffTreeNode> nodeCodeStack = new LinkedList<HuffTreeNode>();
            nodeCodeStack.AddLast(root);
            while (nodeCodeStack.Count > 0)
            {
                HuffTreeNode node = nodeCodeStack.First.Value;
                nodeCodeStack.RemoveFirst();
                if (node.IsData)
                    continue;
                if (node.Child0.IsData && node.Child1.IsData)
                {
                    leafStemQueue.AddFirst(node);
                }
                else
                {
                    nodeCodeStack.AddLast(node.Child0);
                    nodeCodeStack.AddLast(node.Child1);
                }

            }
            #endregion

            HuffTreeNode[] nodeArray = new HuffTreeNode[0x1FF]; // this array does not contain the leaves themselves!
            while (leafStemQueue.Count > 0)
            {
                Insert(leafStemQueue.First.Value, nodeArray, 0x3F + 1);
                leafStemQueue.RemoveFirst();
            }

            // update the indices to ignore all gaps
            int nodeIndex = 0;
            foreach (HuffTreeNode t in nodeArray)
            {
                if (t != null)
                    t.index = nodeIndex++;
            }

            // write the nodes in their given order. However when 'writing' a node, write the data of its children instead.
            // the root node is always the first node.
            byte rootData = 0;
            if (root.Child0.IsData)
                rootData |= 0x80;
            if (root.Child1.IsData)
                rootData |= 0x40;
            writer.Write(rootData); 

            foreach (HuffTreeNode t in nodeArray)
            {
                if (t == null) 
                    continue;
                
                // nodes in this array are never data!
                HuffTreeNode node0 = t.Child0;
                if (node0.IsData)
                    writer.Write(node0.Data);
                else
                {
                    int offset = node0.index - t.index - 1;
                    if (offset > 0x3F)
                        throw new Exception("Offset overflow!");
                    byte data = (byte)offset;
                    if (node0.Child0.IsData)
                        data |= 0x80;
                    if (node0.Child1.IsData)
                        data |= 0x40;
                    writer.Write(data);
                }

                HuffTreeNode node1 = t.Child1;
                if (node1.IsData)
                    writer.Write(node1.Data);
                else
                {
                    int offset = node1.index - t.index - 1;
                    if (offset > 0x3F)
                        throw new Exception("Offset overflow!");
                    byte data = (byte)offset;
                    if (node0.Child0.IsData)
                        data |= 0x80;
                    if (node0.Child1.IsData)
                        data |= 0x40;
                    writer.Write(data);
                }
            }
            #endregion

            #region write the data

            // the codewords are stored in blocks of 32 bits
            uint datablock = 0;
            byte bitsLeftToWrite = 32;

            foreach (byte data in inputData)
            {
                HuffTreeNode node = leaves[data];
                // the depth of the node is the length of the codeword required to encode the byte
                int depth = node.Depth;
                bool[] path = new bool[depth];
                for (int d = 0; d < depth; d++)
                {
                    path[depth - d - 1] = node.IsChild1;
                    node = node.Parent;
                }
                for (int d = 0; d < depth; d++)
                {
                    if (bitsLeftToWrite == 0)
                    {
                        writer.Write(datablock);
                        datablock = 0;
                        bitsLeftToWrite = 32;
                    }
                    bitsLeftToWrite--;
                    if (path[d])
                        datablock |= (uint)(1 << bitsLeftToWrite);
                    // no need to OR the buffer with 0 if it is child0
                }
            }

            // write the partly filled data block as well
            if (bitsLeftToWrite != 32)
                writer.Write(datablock);

            #endregion
        }

        /// <summary>
        /// Inserts the given node into the given array, in such a location that
        /// the offset to both of its children is at most the given maximum, and as large as possible.
        /// In order to do this, the contents of the array may be shifted to the right.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        /// <param name="array">The array to insert the node in.</param>
        /// <param name="maxOffset">The maximum offset between parent and children.</param>
        private void Insert(HuffTreeNode node, HuffTreeNode[] array, int maxOffset)
        {
            // if the node has two data-children, insert it as far to the end as possible.
            if (node.Child0.IsData && node.Child1.IsData)
            {
                for (int i = array.Length - 1; i >= 0; i--)
                {
                    if (array[i] == null)
                    {
                        array[i] = node;
                        node.index = i;
                        break;
                    }
                }
            }
            else
            {
                // if the node is not data, insert it as far left as possible.
                // we know that both children are already present.
                int offset = Math.Max(node.Child0.index - maxOffset, node.Child1.index - maxOffset);
                offset = Math.Max(0, offset);
                if (offset >= node.Child0.index || offset >= node.Child1.index)
                {
                    // it may be that the childen are too far apart, with lots of empty entries in-between.
                    // shift the bottom child right until the node fits in its left-most place for the top child.
                    // (there should be more than enough room in the array)
                    while (offset >= Math.Min(node.Child0.index, node.Child1.index))
                        ShiftRight(array, Math.Min(node.Child0.index, node.Child1.index), maxOffset);
                    while (array[offset] != null)
                        ShiftRight(array, offset, maxOffset);
                    array[offset] = node;
                    node.index = offset;
                }
                else
                {
                    for (int i = offset; i < node.Child0.index && i < node.Child1.index; i++)
                    {
                        if (array[i] == null)
                        {
                            array[i] = node;
                            node.index = i;
                            break;
                        }
                    }
                }
            }

            if (node.index < 0)
                throw new Exception("Node could not be inserted!");

            // if the insertion of this node means that the parent has both children inserted, insert the parent.
            if (node.Parent != null)
            {
                if ((node.Parent.Child0.index >= 0 || node.Parent.Child0.IsData)
                    && (node.Parent.Child1.index >= 0 || node.Parent.Child1.IsData))
                    Insert(node.Parent, array, maxOffset);
            }
        }

        /// <summary>
        /// Shifts the node at the given index one to the right.
        /// If the distance between parent and child becomes too large due to this shift, the parent is shifted as well.
        /// </summary>
        /// <param name="array">The array to shift the node in.</param>
        /// <param name="idx">The index of the node to shift.</param>
        /// <param name="maxOffset">The maximum distance between parent and children.</param>
        private void ShiftRight(HuffTreeNode[] array, int idx, int maxOffset)
        {
            HuffTreeNode node = array[idx];
            if (array[idx + 1] != null)
                ShiftRight(array, idx + 1, maxOffset);
            if (node.Parent.index > 0 && node.index - maxOffset + 1 > node.Parent.index)
                ShiftRight(array, node.Parent.index, maxOffset);
            if (node != array[idx])
                return; // already done indirectly.
            array[idx + 1] = array[idx];
            array[idx] = null;
            node.index++;
        }

        /// <summary>
        /// Gets the tree node with the lowest priority (frequency) from the leaf and node queues.
        /// If the priority is the same for both head items in the queues, the node from the leaf queue is picked.
        /// </summary>
        private HuffTreeNode GetLowest(SimpleReversedPrioQueue<int, HuffTreeNode> leafQueue, SimpleReversedPrioQueue<int, HuffTreeNode> nodeQueue, out int prio)
        {
            if (leafQueue.Count == 0)
                return nodeQueue.Dequeue(out prio);
            else if (nodeQueue.Count == 0)
                return leafQueue.Dequeue(out prio);
            else
            {
                leafQueue.Peek(out int leafPrio);
                nodeQueue.Peek(out int nodePrio);
                
                // pick a node from the leaf queue when the priorities are equal.
                if (leafPrio <= nodePrio)
                    return leafQueue.Dequeue(out prio);
                else
                    return nodeQueue.Dequeue(out prio);
            }
        }

        /// <summary>
        /// A single node in a Huffman tree.
        /// </summary>
        private class HuffTreeNode
        {
            #region Fields & Properties: Data & IsData
            
            /// <summary>
            /// The data contained in this node. May not mean anything when <code>isData == false</code>
            /// </summary>
            private readonly byte data;
            
            /// <summary>
            /// A flag indicating if this node has been filled.
            /// </summary>
            private readonly bool isFilled;

            /// <summary>
            /// The data contained in this node. May not mean anything when <code>isData == false</code>.
            /// Throws a NullReferenceException when this node has not been defined (ie: reference was outside the
            /// bounds of the tree definition)
            /// </summary>
            public byte Data
            {
                get
                {
                    if (!isFilled) 
                        throw new NullReferenceException("Reference to an undefined node in the huffman tree.");
                    
                    return data;
                }
            }

            /// <summary>
            /// Returns true if this node represents data.
            /// </summary>
            public bool IsData { get; }

            #endregion

            #region Field & Properties: Children & Parent

            /// <summary>
            /// The child of this node at side 0
            /// </summary>
            public HuffTreeNode Child0 { get; }

            /// <summary>
            /// The child of this node at side 1
            /// </summary>
            public HuffTreeNode Child1 { get; }

            /// <summary>
            /// The parent node of this node.
            /// </summary>
            public HuffTreeNode Parent { get; private set; }
            
            /// <summary>
            /// Determines if this is the Child0 of the parent node. Assumes there is a parent.
            /// </summary>
            public bool IsChild0 => Parent.Child0 == this;

            /// <summary>
            /// Determines if this is the Child1 of the parent node. Assumes there is a parent.
            /// </summary>
            public bool IsChild1 => Parent.Child1 == this;

            #endregion

            #region Field & Property: Depth
            
            private int depth;
            
            /// <summary>
            /// Get or set the depth of this node. Will not be set automatically, but
            /// will be set recursively (the depth of all child nodes will be updated when this is set).
            /// </summary>
            public int Depth
            {
                get => depth;
                set
                {
                    depth = value;
                    
                    // recursively set the depth of the child nodes.
                    if (!IsData)
                    {
                        Child0.Depth = depth + 1;
                        Child1.Depth = depth + 1;
                    }
                }
            }

            #endregion

            #region Property: Size
            
            /// <summary>
            /// Calculates the size of the sub-tree with this node as root.
            /// </summary>
            public int Size
            {
                get
                {
                    if (IsData)
                        return 1;

                    return 1 + Child0.Size + Child1.Size;
                }
            }
            
            #endregion

            /// <summary>
            /// The index of this node in the array for building the proper ordering.
            /// If -1, this node has not yet been placed in the array.
            /// </summary>
            internal int index = -1;

            #region Constructor(data, isData, child0, child1)
            
            /// <summary>
            /// Manually creates a new node for a huffman tree.
            /// </summary>
            /// <param name="data">The data for this node.</param>
            /// <param name="isData">If this node represents data.</param>
            /// <param name="child0">The child of this node on the 0 side.</param>
            /// <param name="child1">The child of this node on the 1 side.</param>
            public HuffTreeNode(byte data, bool isData, HuffTreeNode child0, HuffTreeNode child1)
            {
                this.data = data;
                IsData = isData;
                Child0 = child0;
                Child1 = child1;
                isFilled = true;
                
                if (!isData)
                {
                    Child0.Parent = this;
                    Child1.Parent = this;
                }
            }
            #endregion

            #region Constructor(Stream, isData, relOffset, maxStreamPos)

            /// <summary>
            /// Creates a new node in the Huffman tree.
            /// </summary>
            /// <param name="reader">The stream to read from. It is assumed that there is (at least)
            /// one more byte available to read.</param>
            /// <param name="isData">If this node is a data-node.</param>
            /// <param name="relOffset">The offset of this node in the source data, relative to the start
            /// of the compressed file.</param>
            /// <param name="maxStreamPos">The indicated end of the huffman tree. If the stream is past
            /// this position, the tree is invalid.</param>
            public HuffTreeNode(Reader reader, bool isData, long relOffset, long maxStreamPos, ref int recursionCount)
            {
                // Sanity check to avoid out of memory exceptions
                if (recursionCount++ > 0xFFFF)
                    throw new Exception("Huffman data contains too many nodes");

                /*
                 Tree Table (list of 8bit nodes, starting with the root node)
                    Root Node and Non-Data-Child Nodes are:
                    Bit0-5   Offset to next child node,
                            Next child node0 is at (CurrentAddr AND NOT 1)+Offset*2+2
                            Next child node1 is at (CurrentAddr AND NOT 1)+Offset*2+2+1
                    Bit6     Node1 End Flag (1=Next child node is data)
                    Bit7     Node0 End Flag (1=Next child node is data)
                    Data nodes are (when End Flag was set in parent node):
                    Bit0-7   Data (upper bits should be zero if Data Size is less than 8)
                 */

                if (reader.BaseStream.Position >= maxStreamPos)
                {
                    // this happens when part of the tree is unused.
                    isFilled = false;
                    return;
                }

                isFilled = true;
                
                data = reader.ReadByte();
                IsData = isData;

                if (!IsData)
                {
                    int offset = data & 0x3F;
                    bool zeroIsData = (data & 0x80) > 0;
                    bool oneIsData = (data & 0x40) > 0;

                    // off AND NOT 1 == off XOR (off AND 1)
                    long zeroRelOffset = (relOffset ^ (relOffset & 1)) + offset * 2 + 2;

                    long currStreamPos = reader.BaseStream.Position;

                    // position the stream right before the 0-node
                    reader.BaseStream.Position += (zeroRelOffset - relOffset) - 1;

                    // read the 0-node
                    Child0 = new HuffTreeNode(reader, zeroIsData, zeroRelOffset, maxStreamPos, ref recursionCount)
                    {
                        Parent = this
                    };

                    // the 1-node is directly behind the 0-node
                    Child1 = new HuffTreeNode(reader, oneIsData, zeroRelOffset + 1, maxStreamPos, ref recursionCount)
                    {
                        Parent = this
                    };

                    // reset the stream position to right behind this node's data
                    reader.BaseStream.Position = currStreamPos;
                }
            }

            #endregion

            /// <summary>
            /// Generates and returns a string-representation of the huffman tree starting at this node.
            /// </summary>
            public override string ToString()
            {
                if (IsData)
                    return $"<{data:X2}>";
                else
                    return $"[{Child0},{Child1}]";
            }
        }

        /// <summary>
        /// Very simplistic implementation of a priority queue that returns items with lowest priority first.
        /// This is not the most efficient implementation, but required the least work while using the classes
        /// from the .NET collections, and without requiring importing another dll or several more class files
        /// in order to make it work.
        /// </summary>
        /// <typeparam name="TPrio">The type of the priority values.</typeparam>
        /// <typeparam name="TValue">The type of item to put into the queue.</typeparam>
        private class SimpleReversedPrioQueue<TPrio, TValue>
        {
            /// <summary>
            /// Creates a new, empty reverse priority queue.
            /// </summary>
            public SimpleReversedPrioQueue()
            {
                items = new SortedDictionary<TPrio, LinkedList<TValue>>();
                Count = 0;
            }

            private readonly SortedDictionary<TPrio, LinkedList<TValue>> items;

            /// <summary>
            /// Gets the number of items in this queue.
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// Enqueues the given value, using the given priority.
            /// </summary>
            /// <param name="priority">The priority of the value.</param>
            /// <param name="value">The value to enqueue.</param>
            public void Enqueue(TPrio priority, TValue value)
            {
                if (!items.ContainsKey(priority))
                    items.Add(priority, new LinkedList<TValue>());
                items[priority].AddLast(value);
                Count++;
            }

            /// <summary>
            /// Gets the current value with the lowest priority from this queue, without dequeueing the value.
            /// </summary>
            /// <param name="priority">The priority of the returned value.</param>
            /// <returns>The current value with the lowest priority.</returns>
            /// <exception cref="IndexOutOfRangeException">If there are no items left in this queue.</exception>
            public TValue Peek(out TPrio priority)
            {
                if (Count == 0)
                    throw new IndexOutOfRangeException();
                
                foreach (KeyValuePair<TPrio, LinkedList<TValue>> kvp in this.items)
                {
                    priority = kvp.Key;
                    return kvp.Value.First.Value;
                }
                
                throw new IndexOutOfRangeException();
            }

            /// <summary>
            /// Dequeues the current value at the head of thisreverse priority queue.
            /// </summary>
            /// <param name="priority">The priority of the dequeued value.</param>
            /// <returns>The dequeued value, that used to be at the head of this queue.</returns>
            /// <exception cref="IndexOutOfRangeException">If this queue does not contain any items.</exception>
            public TValue Dequeue(out TPrio priority)
            {
                if (Count == 0)
                    throw new IndexOutOfRangeException();
                
                LinkedList<TValue> lowestLL = null;
                priority = default;
                foreach (KeyValuePair<TPrio, LinkedList<TValue>> kvp in items)
                {
                    lowestLL = kvp.Value;
                    priority = kvp.Key;
                    break;
                }

                TValue returnValue = lowestLL!.First.Value;
                lowestLL.RemoveFirst();
                // remove unused linked lists. priorities will only grow.
                if (lowestLL.Count == 0)
                {
                    items.Remove(priority);
                }
                Count--;
                return returnValue;
            }
        }
    }
}