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

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;
using Device = SharpDX.Direct2D1.Device;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using Factory = SharpDX.DirectWrite.Factory;
using Factory1 = SharpDX.Direct2D1.Factory1;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;

namespace Microsoft.Maui.Graphics.SharpDX
{
    /// <summary>
    ///     This class handles device creation for Direct2D, Direct3D, DirectWrite
    ///     and WIC.
    /// </summary>
    /// <remarks>
    ///     SharpDX CommonDX is inspired from the DirectXBase C++ class from Win8
    ///     Metro samples, but the design is slightly improved in order to reuse
    ///     components more easily.
    ///     DeviceManager is responsible for device creation.
    ///     TargetBase is responsible for rendering, render target
    ///     creation.
    ///     Initialization and Rendering is event driven based, allowing a better
    ///     reuse of different components.
    /// </remarks>
    public class DeviceManager : DisposeCollector
    {
        // Declare Direct2D Objects
        protected DeviceContext d2dContext;
        protected Device d2dDevice;
        protected Factory1 d2dFactory;

        // Declare DirectWrite & Windows Imaging Component Objects
        protected global::SharpDX.Direct3D11.DeviceContext d3dContext;
        protected global::SharpDX.Direct3D11.Device d3dDevice;
        protected float dpi;
        protected Factory dwriteFactory;
        protected FeatureLevel featureLevel;
        protected ImagingFactory2 wicFactory;

        /// <summary>
        ///     Gets the Direct3D11 device.
        /// </summary>
        public global::SharpDX.Direct3D11.Device DeviceDirect3D => d3dDevice;

        /// <summary>
        ///     Gets the Direct3D11 context.
        /// </summary>
        public global::SharpDX.Direct3D11.DeviceContext ContextDirect3D => d3dContext;

        /// <summary>
        ///     Gets the Direct2D factory.
        /// </summary>
        public Factory1 FactoryDirect2D => d2dFactory;

        /// <summary>
        ///     Gets the Direct2D device.
        /// </summary>
        public Device DeviceDirect2D => d2dDevice;

        /// <summary>
        ///     Gets the Direct2D context.
        /// </summary>
        public DeviceContext ContextDirect2D => d2dContext;

        /// <summary>
        ///     Gets the DirectWrite factory.
        /// </summary>
        public Factory FactoryDirectWrite => dwriteFactory;

        /// <summary>
        ///     Gets the WIC factory.
        /// </summary>
        public ImagingFactory2 WICFactory => wicFactory;

        /// <summary>
        ///     Gets or sets the DPI.
        /// </summary>
        /// <remarks>
        ///     This method will fire the event <see cref="OnDpiChanged" />
        ///     if the dpi is modified.
        /// </remarks>
        public virtual float Dpi
        {
            get => dpi;
            set
            {
                if (dpi != value)
                {
                    dpi = value;
                    d2dContext.DotsPerInch = new Size2F(96, 96);

                    if (OnDpiChanged != null)
                        OnDpiChanged(this);
                }
            }
        }

        /// <summary>
        ///     This event is fired when the <see cref="Dpi" /> is called,
        /// </summary>
        public event Action<DeviceManager> OnDpiChanged;

        /// <summary>
        ///     This event is fired when the DeviceMamanger is initialized by the <see cref="Initialize" /> method.
        /// </summary>
        public event Action<DeviceManager> OnInitialize;

        /// <summary>
        ///     Initialize this instance.
        /// </summary>
        /// <param name="dpi">The DPI</param>
        public virtual void Initialize(float dpi)
        {
            CreateDeviceIndependentResources();
            CreateDeviceResources();

            if (OnInitialize != null)
                OnInitialize(this);

            Dpi = dpi;
        }

        /// <summary>
        ///     Creates device independent resources.
        /// </summary>
        /// <remarks>
        ///     This method is called at the initialization of this instance.
        /// </remarks>
        protected virtual void CreateDeviceIndependentResources()
        {
#if DEBUG
            const DebugLevel debugLevel = DebugLevel.Information;
#else
            const DebugLevel debugLevel = DebugLevel.None;
#endif
            // Dispose previous references and set to null
            RemoveAndDispose(ref d2dFactory);
            RemoveAndDispose(ref dwriteFactory);
            RemoveAndDispose(ref wicFactory);

            // Allocate new references
            d2dFactory = Collect(new Factory1(FactoryType.SingleThreaded, debugLevel));
            dwriteFactory = Collect(new Factory(global::SharpDX.DirectWrite.FactoryType.Shared));
            wicFactory = Collect(new ImagingFactory2());
        }

        /// <summary>
        ///     Creates device resources.
        /// </summary>
        /// <remarks>
        ///     This method is called at the initialization of this instance.
        /// </remarks>
        protected virtual void CreateDeviceResources()
        {
            // Dispose previous references and set to null
            if (d3dDevice != null) RemoveAndDispose(ref d3dDevice);
            if (d3dContext != null) RemoveAndDispose(ref d3dContext);
            if (d2dDevice != null) RemoveAndDispose(ref d2dDevice);
            if (d2dContext != null) RemoveAndDispose(ref d2dContext);

            // Allocate new references
            // Enable compatibility with Direct2D
            // Retrieve the Direct3D 11.1 device amd device context
            DeviceCreationFlags creationFlags = DeviceCreationFlags.VideoSupport | DeviceCreationFlags.BgraSupport;

            // Decomment this line to have Debug. Unfortunately, debug is sometimes crashing applications, so it is disable by default
            try
            {
                // Try to create it with Video Support
                // If it is not working, we just use BGRA
                // Force to FeatureLevel.Level_9_1
                d3dDevice = new global::SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags);
            }
            catch (Exception)
            {
                creationFlags = DeviceCreationFlags.BgraSupport;
                d3dDevice = new global::SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags);
            }
            featureLevel = d3dDevice.FeatureLevel;

            // Get Direct3D 11.1 context
            d3dContext = Collect(new global::SharpDX.Direct3D11.DeviceContext(d3dDevice));

            // Create Direct2D device
            using (var dxgiDevice = d3dDevice.QueryInterface<global::SharpDX.DXGI.Device3>())
            {
                d2dDevice = Collect(new Device(d2dFactory, dxgiDevice));
            }
            // Create Direct2D context
            d2dContext = Collect(new DeviceContext(d2dDevice, DeviceContextOptions.None));
        }
    }
}