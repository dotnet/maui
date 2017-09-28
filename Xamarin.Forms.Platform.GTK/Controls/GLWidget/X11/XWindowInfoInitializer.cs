using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Platform.X11;

namespace OpenTK.X11
{
    /// <summary>
    /// Handler class for initializing <see cref="IWindowInfo"/> objects under the X11 platform for both GTK2 and GTK3.
    /// </summary>
    public static class XWindowInfoInitializer
    {
        const string UnixLibGdkName = "libgdk-x11-2.0.so.0";

        private const string UnixLibX11Name = "libX11.so.6";
        private const string UnixLibGLName = "libGL.so.1";

        /// <summary>
        /// Initializes an <see cref="IWindowInfo"/> under the X11 platform.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="displayHandle"></param>
        /// <param name="screenNumber"></param>
        /// <param name="gdkWindowHandle"></param>
        /// <param name="gdkRootWindowHandle"></param>
        /// <returns></returns>
        public static IWindowInfo Initialize(GraphicsMode mode, IntPtr displayHandle, int screenNumber, IntPtr gdkWindowHandle, IntPtr gdkRootWindowHandle)
        {
            IntPtr display = gdk_x11_display_get_xdisplay(displayHandle);

            IntPtr windowXid = gdk_x11_drawable_get_xid(gdkWindowHandle);
            IntPtr rootWindowXid = gdk_x11_drawable_get_xid(gdkRootWindowHandle);

            IntPtr visualInfo;
            if (mode.Index.HasValue)
            {
                XVisualInfo info = new XVisualInfo
                {
                    VisualID = mode.Index.Value
                };

                int dummy;
                visualInfo = XGetVisualInfo(display, XVisualInfoMask.ID, ref info, out dummy);
            }
            else
            {
                visualInfo = GetVisualInfo(mode, display, screenNumber);
            }

            IWindowInfo retval = Utilities.CreateX11WindowInfo(display, screenNumber, windowXid, rootWindowXid, visualInfo);
            XFree(visualInfo);

            return retval;
        }

        private static IntPtr XGetVisualInfo(IntPtr display, XVisualInfoMask infoMask, ref XVisualInfo template, out int nitems)
        {
            return XGetVisualInfoInternal(display, (IntPtr)(int)infoMask, ref template, out nitems);
        }

        private static IntPtr GetVisualInfo(GraphicsMode mode, IntPtr display, int screenNumber)
        {
            try
            {
                int[] attributes = CreateAttributeList(mode).ToArray();
                return glXChooseVisual(display, screenNumber, attributes);
            }
            catch (DllNotFoundException e)
            {
                throw new DllNotFoundException("OpenGL dll not found!", e);
            }
            catch (EntryPointNotFoundException enf)
            {
                throw new EntryPointNotFoundException("Glx entry point not found!", enf);
            }
        }

        private static List<int> CreateAttributeList(GraphicsMode mode)
        {
            List<int> attributeList = new List<int>(24);

            attributeList.Add((int)GLXAttribute.RGBA);

            if (mode.Buffers > 1)
            {
                attributeList.Add((int)GLXAttribute.DOUBLEBUFFER);
            }

            if (mode.Stereo)
            {
                attributeList.Add((int)GLXAttribute.STEREO);
            }

            attributeList.Add((int)GLXAttribute.RED_SIZE);
            attributeList.Add(mode.ColorFormat.Red / 4); // TODO support 16-bit

            attributeList.Add((int)GLXAttribute.GREEN_SIZE);
            attributeList.Add(mode.ColorFormat.Green / 4); // TODO support 16-bit

            attributeList.Add((int)GLXAttribute.BLUE_SIZE);
            attributeList.Add(mode.ColorFormat.Blue / 4); // TODO support 16-bit

            attributeList.Add((int)GLXAttribute.ALPHA_SIZE);
            attributeList.Add(mode.ColorFormat.Alpha / 4); // TODO support 16-bit

            attributeList.Add((int)GLXAttribute.DEPTH_SIZE);
            attributeList.Add(mode.Depth);

            attributeList.Add((int)GLXAttribute.STENCIL_SIZE);
            attributeList.Add(mode.Stencil);

            //attributeList.Add(GLX_AUX_BUFFERS);
            //attributeList.Add(Buffers);

            attributeList.Add((int)GLXAttribute.ACCUM_RED_SIZE);
            attributeList.Add(mode.AccumulatorFormat.Red / 4);// TODO support 16-bit

            attributeList.Add((int)GLXAttribute.ACCUM_GREEN_SIZE);
            attributeList.Add(mode.AccumulatorFormat.Green / 4);// TODO support 16-bit

            attributeList.Add((int)GLXAttribute.ACCUM_BLUE_SIZE);
            attributeList.Add(mode.AccumulatorFormat.Blue / 4);// TODO support 16-bit

            attributeList.Add((int)GLXAttribute.ACCUM_ALPHA_SIZE);
            attributeList.Add(mode.AccumulatorFormat.Alpha / 4);// TODO support 16-bit

            attributeList.Add((int)GLXAttribute.NONE);

            return attributeList;
        }

        [DllImport(UnixLibX11Name, EntryPoint = "XGetVisualInfo")]
        private static extern IntPtr XGetVisualInfoInternal(IntPtr display, IntPtr infoMask, ref XVisualInfo template, out int nitems);

        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibX11Name)]
        private static extern void XFree(IntPtr handle);

        /// <summary> Returns the X resource (window or pixmap) belonging to a GdkDrawable. </summary>
        /// <remarks> XID gdk_x11_drawable_get_xid(GdkDrawable *drawable); </remarks>
        /// <param name="gdkDisplay"> The GdkDrawable. </param>
        /// <returns> The ID of drawable's X resource. </returns>
        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibGdkName)]
        static extern IntPtr gdk_x11_drawable_get_xid(IntPtr gdkDisplay);

        /// <summary> Returns the X display of a GdkDisplay. </summary>
        /// <remarks> Display* gdk_x11_display_get_xdisplay(GdkDisplay *display); </remarks>
        /// <param name="gdkDisplay"> The GdkDrawable. </param>
        /// <returns> The X Display of the GdkDisplay. </returns>
        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibGdkName)]
        private static extern IntPtr gdk_x11_display_get_xdisplay(IntPtr gdkDisplay);

        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibGLName)]
        private static extern IntPtr glXChooseVisual(IntPtr display, int screen, int[] attr);
    }
}