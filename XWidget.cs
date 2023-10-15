using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace Xigit
{
    public class XWidget
    {
        public int width, height;
        public string name;
        public XContainer container;

        private static IntPtr hWnd;

        private delegate IntPtr WindowProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private static WindowProcDelegate? wndProcDelegate;

        public XWidget(int width, int height, string name)
        {
            this.width = width;
            this.height = height;
            this.name = name;
            container = new XContainer();

            Open(50, 50);
        }

        public void Render()
        {
            if (container == null) return;
            container.Render(this);
        }

        public void SetContainer(XContainer container)
        {
            this.container = container;
        }

        public void AddElement(XElement element)
        {
            if (container == null) return;
            container.AddElement(element);
        }

        private void Open(int x, int y)
        {
            WNDCLASSEX wcex = new WNDCLASSEX();
            wcex.cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX));
            wndProcDelegate = new WindowProcDelegate(WndProc);
            wcex.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
            wcex.hInstance = GetModuleHandle(null);
            wcex.lpszClassName = "XWidget";

            ushort atom = RegisterClassEx(ref wcex);
            if (atom == 0)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to register window class. Error code: {lastError}");
            }

            hWnd = CreateWindowEx(
                0,
                "XWidget",
                name,
                WS_CAPTION | WS_SYSMENU | WS_VISIBLE,
                x,
                y,
                width,
                height,
                IntPtr.Zero,
                IntPtr.Zero,
                GetModuleHandle(null),
                IntPtr.Zero
            );

            if (hWnd == IntPtr.Zero)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to create window. Error code: {lastError}");
            }

            Show();
        }

        private void Show()
        {
            while (true)
            {
                if (!GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
                    break;

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, uint color)
        {
            IntPtr hdc = GetDC(hWnd);
            IntPtr hBrush = CreateSolidBrush(color);
            IntPtr hPrevBrush = SelectObject(hdc, hBrush);

            Rectangle(hdc, x, y, x + width, y + height);

            SelectObject(hdc, hPrevBrush);
            DeleteObject(hBrush);

            ReleaseDC(hWnd, hdc);
        }

        public void DrawPixel(int x, int y, uint color)
        {
            DrawRectangle(x, y, 1, 1, color);
        }

        public void DrawText(int x, int y, string text, uint color)
        {
            IntPtr hdc = GetDC(hWnd);

            SetTextColor(hdc, (int)color);
            TextOut(hdc, x, y, text, text.Length);

            ReleaseDC(hWnd, hdc);
        }

        public void DrawLine(int x1, int y1, int x2, int y2, uint color)
        {
            IntPtr hdc = GetDC(hWnd);
            IntPtr hPen = CreatePen(PS_SOLID, 1, color);
            IntPtr hPrevPen = SelectObject(hdc, hPen);

            MoveToEx(hdc, x1, y1, IntPtr.Zero);
            LineTo(hdc, x2, y2);

            SelectObject(hdc, hPrevPen);
            DeleteObject(hPen);

            ReleaseDC(hWnd, hdc);
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_PAINT:
                    {
                        PAINTSTRUCT ps;
                        IntPtr hdc = BeginPaint(hWnd, out ps);

                        // Handle painting

                        EndPaint(hWnd, ref ps);
                        return IntPtr.Zero;
                    }

                case WM_DESTROY:
                    PostQuitMessage(0);
                    return IntPtr.Zero;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        const uint WM_PAINT = 0x000F;
        const uint WM_DESTROY = 0x0002;
        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WS_CAPTION = 0x00C00000;
        const uint WS_SYSMENU = 0x00080000;
        const uint WS_VISIBLE = 0x10000000;
        const uint WS_CHILD = 0x40000000;
        const uint WS_BORDER = 0x00800000;
        const uint ES_MULTILINE = 0x0004;
        const uint ES_AUTOVSCROLL = 0x0040;
        const int PS_SOLID = 0;
        const uint WM_GETTEXT = 0x000D;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateSolidBrush(uint color);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool Rectangle(IntPtr hdc, int left, int top, int right, int bottom);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

        [DllImport("gdi32.dll")]
        private static extern int SetTextColor(IntPtr hdc, int crColor);

        [DllImport("user32.dll")]
        private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

        [DllImport("gdi32.dll")]
        private static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);

        [DllImport("gdi32.dll")]
        private static extern bool LineTo(IntPtr hdc, int x, int y);

        [DllImport("user32.dll")]
        private static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hWnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}