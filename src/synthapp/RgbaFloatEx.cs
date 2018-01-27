using Veldrid;

namespace SynthApp
{
    public static class RgbaFloatEx
    {
        public static uint ToUIntArgb(this RgbaFloat color)
        {
            return Util.RgbaToArgb(color.ToVector4());
        }
    }
}
