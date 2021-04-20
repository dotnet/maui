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
using Windows.UI.Xaml.Media.Imaging;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.DXGI.Device;
using Point = SharpDX.Point;
using System;

namespace Microsoft.Maui.Graphics.SharpDX
{
	/// <summary>
	///     Target to render to a <see cref="SurfaceImageSource" />, used to integrate
	///     DirectX content into a XAML brush.
	/// </summary>
	public class SurfaceImageSourceTarget : TargetBase
	{
		private readonly int _pixelHeight;
		private readonly int _pixelWidth;
		private readonly SurfaceImageSource _surfaceImageSource;
		private readonly ISurfaceImageSourceNative _surfaceImageSourceNative;
		private readonly SurfaceViewData[] _viewDatas = new SurfaceViewData[2];
		private int _nextViewDataIndex;
		private RawPoint _position;
		private bool _dirty = true;

		/// <summary>
		///     Initializes a new <see cref="SurfaceImageSourceTarget" /> instance.
		/// </summary>
		/// <param name="pixelWidth">Width of the target in pixels</param>
		/// <param name="pixelHeight">Height of the target in pixels</param>
		public SurfaceImageSourceTarget(int pixelWidth, int pixelHeight, bool supportOpacity = false)
		{
			this._pixelWidth = pixelWidth;
			this._pixelHeight = pixelHeight;
			_surfaceImageSource = new SurfaceImageSource(pixelWidth, pixelHeight, supportOpacity);
			_surfaceImageSourceNative = Collect(ComObject.As<ISurfaceImageSourceNative>(_surfaceImageSource));
			_viewDatas[0] = Collect(new SurfaceViewData());
			_viewDatas[1] = Collect(new SurfaceViewData());
		}

		public bool Dirty
		{
			get => _dirty;
			set => _dirty = value;
		}

		/// <summary>
		///     Gets the <see cref="SurfaceImageSource" /> to be used by brushes.
		/// </summary>
		public SurfaceImageSource ImageSource => _surfaceImageSource;

		/// <inveritdoc />
		protected override Rect CurrentControlBounds => new Rect(0, 0, _pixelWidth, _pixelHeight);

		/// <summary>
		///     Gets the relative position to use to draw on the surface.
		/// </summary>
		public RawPoint DrawingPosition => _position;

		/// <inveritdoc />
		public override void Initialize(DeviceManager deviceManager)
		{
			base.Initialize(deviceManager);
			var device = Collect(DeviceManager.DeviceDirect3D.QueryInterface<Device>());
			_surfaceImageSourceNative.Device = device;
		}

		/// <inveritdoc />
		public override void RenderAll()
		{
			if (!_dirty) return;

			SurfaceViewData viewData = null;

			var regionToDraw = new global::SharpDX.Mathematics.Interop.RawRectangle(0, 0, _pixelWidth, _pixelHeight);

			// Unlike other targets, we can only get the DXGI surface to render to
			// just before rendering.
			var surface = _surfaceImageSourceNative.BeginDraw(regionToDraw, out _position);
			using (surface)
			{
				// Cache DXGI surface in order to avoid recreate all render target view, depth stencil...etc.
				// Is it the right way to do it?
				// It seems that ISurfaceImageSourceNative.BeginDraw is returning 2 different DXGI surfaces (when the application is in foreground)
				// or different DXGI surfaces (when the application is in background).
				foreach (SurfaceViewData surfaceViewData in _viewDatas)
				{
					if (surfaceViewData.SurfacePointer == surface.NativePointer)
					{
						viewData = surfaceViewData;
						break;
					}
				}


				if (viewData == null)
				{
					viewData = _viewDatas[_nextViewDataIndex];
					_nextViewDataIndex = (_nextViewDataIndex + 1)%_viewDatas.Length;

					// Make sure that previous was disposed.
					viewData.Dispose();
					viewData.SurfacePointer = surface.NativePointer;

					// Allocate a new renderTargetView if size is different
					// Cache the render target dimensions in our helper class for convenient use.
					viewData.BackBuffer = surface.QueryInterface<Texture2D>();
					{
						Texture2DDescription desc = viewData.BackBuffer.Description;
						viewData.RenderTargetSize = new Size(desc.Width, desc.Height);
						viewData.RenderTargetView = new RenderTargetView(DeviceManager.DeviceDirect3D,
							viewData.BackBuffer);
					}

					// Create a descriptor for the depth/stencil buffer.
					// Allocate a 2-D surface as the depth/stencil buffer.
					// Create a DepthStencil view on this surface to use on bind.
					// TODO: Recreate a DepthStencilBuffer is inefficient. We should only have one depth buffer. Shared depth buffer?
					using (var depthBuffer = new Texture2D(DeviceManager.DeviceDirect3D, new Texture2DDescription
					{
						Format = Format.D24_UNorm_S8_UInt,
						ArraySize = 1,
						MipLevels = 1,
						Width = (int) viewData.RenderTargetSize.Width,
						Height = (int) viewData.RenderTargetSize.Height,
						SampleDescription = new SampleDescription(1, 0),
						BindFlags = BindFlags.DepthStencil,
					}))
						viewData.DepthStencilView = new DepthStencilView(DeviceManager.DeviceDirect3D, depthBuffer,
							new DepthStencilViewDescription {Dimension = DepthStencilViewDimension.Texture2D});

					// Now we set up the Direct2D render target bitmap linked to the swapchain.
					// Whenever we render to this bitmap, it will be directly rendered to the
					// swapchain associated with the window.
					var bitmapProperties = new BitmapProperties1(
						new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
						96,
						96,
						BitmapOptions.Target | BitmapOptions.CannotDraw);

					// Direct2D needs the dxgi version of the backbuffer surface pointer.
					// Get a D2D surface from the DXGI back buffer to use as the D2D render target.
					viewData.BitmapTarget = new Bitmap1(DeviceManager.ContextDirect2D, surface, bitmapProperties);

					// Create a viewport descriptor of the full window size.
					viewData.Viewport = new ViewportF(_position.X, _position.Y,
						(float) viewData.RenderTargetSize.Width - _position.X,
						(float) viewData.RenderTargetSize.Height - _position.Y, 0.0f, 1.0f);
				}

				backBuffer = viewData.BackBuffer;
				renderTargetView = viewData.RenderTargetView;
				depthStencilView = viewData.DepthStencilView;
				RenderTargetBounds = new Rect(viewData.Viewport.X, viewData.Viewport.Y, viewData.Viewport.Width,
					viewData.Viewport.Height);
				bitmapTarget = viewData.BitmapTarget;

				DeviceManager.ContextDirect2D.Target = viewData.BitmapTarget;

				// Set the current viewport using the descriptor.
				DeviceManager.ContextDirect3D.Rasterizer.SetViewport(viewData.Viewport);

				// Perform the actual rendering of this target
				base.RenderAll();
			}

			_surfaceImageSourceNative.EndDraw();
			_dirty = false;
		}

		/// <summary>
		///     This class is used to store attached render target view to DXGI surfaces.
		/// </summary>
		private class SurfaceViewData : IDisposable
		{
			public Texture2D BackBuffer;
			public Bitmap1 BitmapTarget;
			public DepthStencilView DepthStencilView;
			public Size RenderTargetSize;
			public RenderTargetView RenderTargetView;
			public IntPtr SurfacePointer;
			public ViewportF Viewport;

			public void Dispose()
			{
				Utilities.Dispose(ref BitmapTarget);
				Utilities.Dispose(ref RenderTargetView);
				Utilities.Dispose(ref DepthStencilView);
				Utilities.Dispose(ref BackBuffer);
				SurfacePointer = IntPtr.Zero;
			}
		}
	}
}
