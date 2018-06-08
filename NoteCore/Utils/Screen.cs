using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Display = System.Windows.Forms.Screen;

// ReSharper disable PossibleNullReferenceException

namespace NoteCore.Utils
{
    public static class Screen
    {
        private static Matrix GetSizeFactors(Visual element)
        {
            Matrix transformToDevice;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
            {
                transformToDevice = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var source2 = new HwndSource(new HwndSourceParameters()))
                {
                    transformToDevice = source2.CompositionTarget.TransformToDevice;
                }
            }
            return transformToDevice;
        }

        public static double HorizontalDpiToPixel(UIElement element, double x) => x * GetSizeFactors(element).M11;

        public static double VerticalDpiToPixel(UIElement element, double y) => y * GetSizeFactors(element).M22;

        public static double HorizontalPixelToDpi(UIElement element, double x) => x / GetSizeFactors(element).M11;

        public static double VerticalPixelToDpi(UIElement element, double y) => y / GetSizeFactors(element).M22;

        public static Rect ScreenRectFromWindow(Window window)
        {
            var size = WpfScreen.GetScreenFrom(window).DisplaySize;
            var x = HorizontalPixelToDpi(window, size.X);
            var y = VerticalPixelToDpi(window, size.Y);
            var screenWidth = HorizontalPixelToDpi(window, size.Width);
            var screenHeight = VerticalPixelToDpi(window, size.Height);
            return new Rect(x, y, screenWidth, screenHeight);
        }
    }
    public class WpfScreen
    {
        private readonly Display _display;

        internal WpfScreen(Display display)
        {
            _display = display;
        }

        public static WpfScreen GetScreenFrom(Window window)
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            var display = Display.FromHandle(windowInteropHelper.Handle);
            return new WpfScreen(display);
        }

        public Rect DisplaySize => new Rect(_display.Bounds.X, _display.Bounds.Y, _display.Bounds.Width, _display.Bounds.Height);
    }
}