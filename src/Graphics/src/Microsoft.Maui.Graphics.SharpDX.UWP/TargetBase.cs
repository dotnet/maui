// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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

using Windows.Foundation;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;

namespace Microsoft.Maui.Graphics.SharpDX
{
    /// <summary>
    ///     This class is an abstract class responsible to maintain a list of
    ///     render task, render target view / depth stencil view and Direct2D
    ///     render target.
    /// </summary>
    /// <remarks>
    ///     SharpDX CommonDX is inspired from the DirectXBase C++ class from Win8
    ///     Metro samples, but the design is slightly improved in order to reuse
    ///     components more easily.
    ///     <see cref="DeviceManager" /> is responsible for device creation.
    ///     <see cref="TargetBase" /> is responsible for rendering, render target
    ///     creation.
    ///     Initialization and Rendering is event driven based, allowing a better
    ///     reuse of different components.
    /// </remarks>
    public abstract class TargetBase : DisposeCollector
    {
        protected Texture2D backBuffer;
        protected Bitmap1 bitmapTarget;
        protected DepthStencilView depthStencilView;
        protected RenderTargetView renderTargetView;

        /// <summary>
        ///     Gets the <see cref="DeviceManager" /> attached to this instance.
        /// </summary>
        public DeviceManager DeviceManager { get; private set; }

        /// <summary>
        ///     Gets the Direct3D RenderTargetView used by this target.
        /// </summary>
        public RenderTargetView RenderTargetView => renderTargetView;

        public Texture2D BackBuffer => backBuffer;

        /// <summary>
        ///     Gets the Direct3D DepthStencilView used by this target.
        /// </summary>
        public DepthStencilView DepthStencilView => depthStencilView;

        /// <summary>
        ///     Gets the Direct2D RenderTarget used by this target.
        /// </summary>
        public Bitmap1 BitmapTarget2D => bitmapTarget;

        /// <summary>
        ///     Gets the bounds of the control linked to this render target
        /// </summary>
        public Rect RenderTargetBounds { get; protected set; }

        /// <summary>
        ///     Gets the size in pixels of the Direct3D RenderTarget
        /// </summary>
        public Size RenderTargetSize => new Size(RenderTargetBounds.Width, RenderTargetBounds.Height);

        /// <summary>
        ///     Gets the bounds of the control linked to this render target
        /// </summary>
        public Rect ControlBounds { get; protected set; }


        /// <summary>
        ///     Gets the current bounds of the control linked to this render target
        /// </summary>
        protected abstract Rect CurrentControlBounds { get; }

        /// <summary>
        ///     Event fired when size of the underlying control is changed
        /// </summary>
        public event Action<TargetBase> OnSizeChanged;

        /// <summary>
        ///     Event fired when rendering is performed by this target
        /// </summary>
        public event Action<TargetBase> OnRender;

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        /// <param name="deviceManager">The device manager</param>
        public virtual void Initialize(DeviceManager deviceManager)
        {
            DeviceManager = deviceManager;

            // If the DPI is changed, we need to perform a OnSizeChanged event
            deviceManager.OnDpiChanged -= devices_OnDpiChanged;
            deviceManager.OnDpiChanged += devices_OnDpiChanged;
        }

        /// <summary>
        ///     Notifies for size changed
        /// </summary>
        public virtual void UpdateForSizeChange()
        {
            Rect newBounds = CurrentControlBounds;

            if (newBounds.Width != ControlBounds.Width ||
                newBounds.Height != ControlBounds.Height)
            {
                // Store the window bounds so the next time we get a SizeChanged event we can
                // avoid rebuilding everything if the size is identical.
                ControlBounds = newBounds;

                if (OnSizeChanged != null)
                    OnSizeChanged(this);
            }
        }

        /// <summary>
        ///     Render all events registered on event <see cref="OnRender" />
        /// </summary>
        public virtual void RenderAll()
        {
            if (OnRender != null)
                OnRender(this);
        }

        private void devices_OnDpiChanged(DeviceManager obj)
        {
            if (OnSizeChanged != null)
                OnSizeChanged(this);
        }
    }
}