#nullable disable
using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
/*
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
*/

namespace Microsoft.Maui
{
	partial class WrapperView : Grid, IDisposable
	{
		/*
		readonly Canvas _shadowCanvas;
		SpriteVisual _shadowVisual;
		DropShadow _dropShadow;
		*/

		FrameworkElement _child;

		public WrapperView()
		{
			/*
			_shadowCanvas = new Canvas();

			Children.Add(_shadowCanvas);
			*/
		}

		public FrameworkElement Child
		{
			get { return _child; }
			set
			{
				if (_child != null)
				{
					//_child.SizeChanged -= OnChildSizeChanged;
					Children.Remove(_child);
				}

				if (value == null)
					return;

				_child = value;
				//_child.SizeChanged += OnChildSizeChanged;
				Children.Add(_child);
			}
		}

		public void Dispose()
		{
			//DisposeShadow();
		}

		partial void ClipChanged()
		{
			UpdateClip();
		}

		void UpdateClip()
		{
			if (Child == null)
				return;

			var clipGeometry = Clip;

			if (clipGeometry == null)
				return;

			double width = Child.ActualWidth;
			double height = Child.ActualHeight;

			if (height <= 0 && width <= 0)
				return;

			var visual = ElementCompositionPreview.GetElementVisual(Child);
			var compositor = visual.Compositor;

			var pathSize = new Graphics.Rectangle(0, 0, width, height);
			var clipPath = clipGeometry.PathForBounds(pathSize);
			var device = CanvasDevice.GetSharedDevice();
			var geometry = clipPath.AsPath(device);

			var path = new CompositionPath(geometry);
			var pathGeometry = compositor.CreatePathGeometry(path);
			var geometricClip = compositor.CreateGeometricClip(pathGeometry);

			visual.Clip = geometricClip;
		}

		/*	  
		internal bool HasShadow => _dropShadow != null;
		 
		async partial void ShadowChanged()
		{
			if (HasShadow)
				UpdateShadow();
			else
				await CreateShadowAsync();
		}

		async void OnChildSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateClip();

			if (HasShadow)
				UpdateShadowSize();
			else
				await CreateShadowAsync();
		}

		void DisposeShadow()
		{
			if (_shadowCanvas == null)
				return;

			var shadowHost = _shadowCanvas.Children[0];

			if (shadowHost != null)
				ElementCompositionPreview.SetElementChildVisual(shadowHost, null);

			if (_shadowCanvas.Children.Count > 0)
				_shadowCanvas.Children.RemoveAt(0);

			if (_shadowVisual != null)
				_shadowVisual.Dispose();

			if (_dropShadow != null)
				_dropShadow.Dispose();
		}

		async Task CreateShadowAsync()
		{
			if (Child == null || Shadow == null)
				return;

			double width = Child.ActualWidth;
			double height = Child.ActualHeight;

			if (height <= 0 && width <= 0)
				return;

			// TODO: Fix ArgumentException
			if (Clip != null)
				await Task.Delay(500);

			var ttv = Child.TransformToVisual(_shadowCanvas);
			Windows.Foundation.Point offset = ttv.TransformPoint(new Windows.Foundation.Point(0, 0));

			var shadowHost = new Rectangle()
			{
				Fill = new SolidColorBrush(Colors.Transparent),
				Width = width,
				Height = height
			};

			Canvas.SetLeft(shadowHost, offset.X);
			Canvas.SetTop(shadowHost, offset.Y);

			_shadowCanvas.Children.Insert(0, shadowHost);

			var hostVisual = ElementCompositionPreview.GetElementVisual(_shadowCanvas);
			var compositor = hostVisual.Compositor;

			_dropShadow = compositor.CreateDropShadow();
			_dropShadow.BlurRadius = (float)Shadow.Value.Radius * 2;
			_dropShadow.Opacity = (float)Shadow.Value.Opacity;
			_dropShadow.Color = Shadow.Value.Color.ToWindowsColor();
			_dropShadow.Offset = new Vector3((float)Shadow.Value.Offset.Width, (float)Shadow.Value.Offset.Height, 0);

			_dropShadow.Mask = await Child.GetAlphaMaskAsync();

			_shadowVisual = compositor.CreateSpriteVisual();
			_shadowVisual.Size = new Vector2((float)width, (float)height);
			_shadowVisual.Shadow = _dropShadow;

			ElementCompositionPreview.SetElementChildVisual(shadowHost, _shadowVisual);
		}

		void UpdateShadow()
		{
			if (_dropShadow != null)
			{
				_dropShadow.BlurRadius = (float)Shadow.Value.Radius * 2;
				_dropShadow.Opacity = (float)Shadow.Value.Opacity;
				_dropShadow.Color = Shadow.Value.Color.ToWindowsColor();
				_dropShadow.Offset = new Vector3((float)Shadow.Value.Offset.Width, (float)Shadow.Value.Offset.Height, 0);
			}

			UpdateShadowSize();
		}

		void UpdateShadowSize()
		{
			if (_shadowVisual != null)
			{
				if (Child is FrameworkElement child)
				{
					float width = (float)child.ActualWidth;
					float height = (float)child.ActualHeight;

					_shadowVisual.Size = new Vector2(width, height);
				}
			}
		}
		*/
	}
}