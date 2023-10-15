namespace Xigit
{
    public class Color
    {
        public static uint Black = RGB(0, 0, 0);
        public static uint White = RGB(255, 255, 255);
        public static uint Red = RGB(255, 0, 0);
        public static uint Green = RGB(0, 255, 0);
        public static uint Blue = RGB(0, 0, 255);
        public static uint Yellow = RGB(255, 255, 0);
        public static uint Cyan = RGB(0, 255, 255);
        public static uint Magenta = RGB(255, 0, 255);

        public static uint RGB(int r, int g, int b)
        {
            return (uint)((r & 0xFF) | ((g & 0xFF) << 8) | ((b & 0xFF) << 16));
        }
    }
}