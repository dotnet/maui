// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SharpDX.Desktop
{
    /// <summary>
    /// RenderLoop provides a rendering loop infrastructure. See remarks for usage. 
    /// </summary>
    /// <remarks>
    /// Use static <see cref="Run(System.Windows.Forms.Control,SharpDX.Windows.RenderLoop.RenderCallback)"/>  
    /// method to directly use a renderloop with a render callback or use your own loop:
    /// <code>
    /// control.Show();
    /// using (var loop = new RenderLoop(control))
    /// {
    ///     while (loop.NextFrame())
    ///     {
    ///        // Perform draw operations here.
    ///     }
    /// }
    /// </code>
    /// Note that the main control can be changed at anytime inside the loop.
    /// </remarks>
    public class RenderLoop : IDisposable
    {
        private IntPtr controlHandle;
        private Control control;
        private bool isControlAlive;
        private bool switchControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLoop"/> class.
        /// </summary>
        public RenderLoop() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLoop"/> class.
        /// </summary>
        public RenderLoop(Control control)
        {
            Control = control;
        }

        /// <summary>
        /// Gets or sets the control to associate with the current render loop.
        /// </summary>
        /// <value>The control.</value>
        /// <exception cref="System.InvalidOperationException">Control is already disposed</exception>
        public Control Control
        {
            get => control;
            set
            {
                if(control == value) return;

                // Remove any previous control
                if(control != null && !switchControl)
                {
                    isControlAlive = false;
                    control.Disposed -= ControlDisposed;
                    controlHandle = IntPtr.Zero;
                }

                if (value != null && value.IsDisposed)
                {
                    throw new InvalidOperationException("Control is already disposed");
                }

                control = value;
                switchControl = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the render loop should use the default <see cref="Application.DoEvents"/> instead of a custom window message loop lightweight for GC. Default is false.
        /// </summary>
        /// <value><c>true</c> if the render loop should use the default <see cref="Application.DoEvents"/> instead of a custom window message loop (default false); otherwise, <c>false</c>.</value>
        /// <remarks>By default, RenderLoop is using a custom window message loop that is more lightweight than <see cref="Application.DoEvents" /> to process windows event message. 
        /// Set this parameter to true to use the default <see cref="Application.DoEvents"/>.</remarks>
        public bool UseApplicationDoEvents { get; set; }

        /// <summary>
        /// Calls this method on each frame.
        /// </summary>
        /// <returns><c>true</c> if if the control is still active, <c>false</c> otherwise.</returns>
        /// <exception cref="System.InvalidOperationException">An error occured </exception>
        public bool NextFrame()
        {
            // Setup new control
            // TODO this is not completely thread-safe. We should use a lock to handle this correctly
            if (switchControl && control != null)
            {
                controlHandle = control.Handle;
                control.Disposed += ControlDisposed;
                isControlAlive = true;
                switchControl = false;
            }

            if(isControlAlive)
            {
                if(UseApplicationDoEvents)
                {
                    // Revert back to Application.DoEvents in order to support Application.AddMessageFilter
                    // Seems that DoEvents is compatible with Mono unlike Application.Run that was not running
                    // correctly.
                    Application.DoEvents();
                }
                else
                {
                    var localHandle = controlHandle;
                    if (localHandle != IntPtr.Zero)
                    {
                        // Previous code not compatible with Application.AddMessageFilter but faster then DoEvents
                        while (Win32Native.PeekMessage(out var msg, IntPtr.Zero, 0, 0, 0) != 0)
                        {
                            if (Win32Native.GetMessage(out msg, IntPtr.Zero, 0, 0) == -1)
                            {
                                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                    "An error happened in rendering loop while processing windows messages. Error: {0}",
                                    Marshal.GetLastWin32Error()));
                            }

                            // NCDESTROY event?
                            if (msg.msg == 130)
                            {
                                isControlAlive = false;
                            }

                            var message = new Message() { HWnd = msg.handle, LParam = msg.lParam, Msg = (int)msg.msg, WParam = msg.wParam };
                            if (!Application.FilterMessage(ref message))
                            {
                                Win32Native.TranslateMessage(ref msg);
                                Win32Native.DispatchMessage(ref msg);
                            }
                        }
                    }
                }
            }

            return isControlAlive || switchControl;
        }

        private void ControlDisposed(object sender, EventArgs e)
        {
            isControlAlive = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Control = null;
        }

        /// <summary>
        /// Delegate for the rendering loop.
        /// </summary>
        public delegate void RenderCallback();

        /// <summary>
        /// Runs the specified main loop in the specified context.
        /// </summary>
        public static void Run(ApplicationContext context, RenderCallback renderCallback)
        {
            Run(context.MainForm, renderCallback);
        }

        /// <summary>
        /// Runs the specified main loop for the specified windows form.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="renderCallback">The rendering callback.</param>
        /// <param name="useApplicationDoEvents">if set to <c>true</c> indicating whether the render loop should use the default <see cref="Application.DoEvents"/> instead of a custom window message loop lightweight for GC. Default is false.</param>
        /// <exception cref="System.ArgumentNullException">form
        /// or
        /// renderCallback</exception>
        public static void Run(Control form, RenderCallback renderCallback, bool useApplicationDoEvents = false)
        {
            if(form == null) throw new ArgumentNullException("form");
            if(renderCallback == null) throw new ArgumentNullException("renderCallback");

            form.Show();
            using (var renderLoop = new RenderLoop(form) { UseApplicationDoEvents = useApplicationDoEvents })
            {
                while(renderLoop.NextFrame())
                {
                    renderCallback();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is application idle.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is application idle; otherwise, <c>false</c>.
        /// </value>
        public static bool IsIdle
        {
            get
            {
                return (bool)(Win32Native.PeekMessage(out var msg, IntPtr.Zero, 0, 0, 0) == 0);
            }
        }
   }
}