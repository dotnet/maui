using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Microsoft.Maui.Cli;

/// <summary>
/// Native P/Invoke declarations and helpers for Windows-specific functionality.
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class NativeMethods
{
    // System metrics constants
    const int SM_XVIRTUALSCREEN = 76;
    const int SM_YVIRTUALSCREEN = 77;
    const int SM_CXVIRTUALSCREEN = 78;
    const int SM_CYVIRTUALSCREEN = 79;

    // Raster operation codes
    const int SRCCOPY = 0x00CC0020;

    [LibraryImport("user32.dll")]
    private static partial nint GetDC(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ReleaseDC(nint hWnd, nint hDC);

    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(int nIndex);

    [LibraryImport("gdi32.dll")]
    private static partial nint CreateCompatibleDC(nint hdc);

    [LibraryImport("gdi32.dll")]
    private static partial nint CreateCompatibleBitmap(nint hdc, int nWidth, int nHeight);

    [LibraryImport("gdi32.dll")]
    private static partial nint SelectObject(nint hdc, nint hgdiobj);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(nint hdcDest, int xDest, int yDest, int wDest, int hDest,
                               nint hdcSrc, int xSrc, int ySrc, int rop);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(nint hObject);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteDC(nint hdc);

    /// <summary>
    /// Captures a screenshot of the entire virtual screen and saves it to a file.
    /// </summary>
    /// <param name="outputPath">The path to save the screenshot.</param>
    public static void CaptureScreen(string outputPath)
    {
        nint hBitmap = nint.Zero;
        nint hdcMem = nint.Zero;
        nint hdcScreen = nint.Zero;
        try
        {
            var virtualScreen = new Rectangle(
                GetSystemMetrics(SM_XVIRTUALSCREEN),
                GetSystemMetrics(SM_YVIRTUALSCREEN),
                GetSystemMetrics(SM_CXVIRTUALSCREEN),
                GetSystemMetrics(SM_CYVIRTUALSCREEN));

            hdcScreen = GetDC(nint.Zero);
            hdcMem = CreateCompatibleDC(hdcScreen);
            hBitmap = CreateCompatibleBitmap(hdcScreen, virtualScreen.Width, virtualScreen.Height);
            var hOld = SelectObject(hdcMem, hBitmap);

            BitBlt(hdcMem, 0, 0, virtualScreen.Width, virtualScreen.Height,
                hdcScreen, virtualScreen.X, virtualScreen.Y, SRCCOPY);

            SelectObject(hdcMem, hOld);

            // Save bitmap to file using GDI+
            using var bitmap = Image.FromHbitmap(hBitmap);
            bitmap.Save(outputPath, ImageFormat.Png);
        }
        finally
        {
            if (hBitmap != nint.Zero)
            {
                DeleteObject(hBitmap);
            }
            if (hdcMem != nint.Zero)
            {
                DeleteDC(hdcMem);
            }
            if (hdcScreen != nint.Zero)
            {
                ReleaseDC(nint.Zero, hdcScreen);
            }
        }
    }
}
