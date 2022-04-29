namespace BinarySerializer.Nintendo.GBA
{
    public enum OBJ_ATTR_ObjectMode
    {
        /// <summary>
        /// Normal rendering
        /// </summary>
        REG = 0,

        /// <summary>
        /// Sprite is an affine sprite, using affine matrix
        /// </summary>
        AFF = 1,

        /// <summary>
        /// Disables rendering (hides the sprite)
        /// </summary>
        HIDE = 2,

        /// <summary>
        /// Affine sprite using double rendering area
        /// </summary>
        AFF_DBL = 3,
    }
}