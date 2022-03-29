using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Shapes;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : Grid, IDisposable
	{
		readonly Canvas _shadowCanvas;
		SpriteVisual? _shadowVisual;
		DropShadow? _dropShadow;
		Path? _borderPath;

		FrameworkElement? _child;

		public WrapperView()
		{
			_shadowCanvas = new Canvas();
			_borderPath = new Path();

			Children.Add(_shadowCanvas);
			Children.Add(_borderPath);
		}

		public FrameworkElement? Child
		{
			get { return _child; }
			internal set
			{
				if (_child != null)
				{
					_child.SizeChanged -= OnChildSizeChanged;
					Children.Remove(_child);
				}

				if (value == null)
					return;

				_child = value;
				_child.SizeChanged += OnChildSizeChanged;
				Children.Add(_child);
			}
		}

		internal bool HasShadow => _dropShadow != null;

		public void Dispose()
		{
			DisposeClip();
			DisposeShadow();
			DisposeBorder();
		}

		partial void ClipChanged() => UpdateClip();

		partial void BorderChanged() => UpdateBorder();

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

			var pathSize = new Graphics.Rect(0, 0, width, height);
			var clipPath = clipGeometry.PathForBounds(pathSize);
			var device = CanvasDevice.GetSharedDevice();
			System.Diagnostics.Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362));
			var geometry = clipPath.AsPath(device);

			var path = new CompositionPath(geometry);
			var pathGeometry = compositor.CreatePathGeometry(path);
			var geometricClip = compositor.CreateGeometricClip(pathGeometry);

			visual.Clip = geometricClip;
		}

		void DisposeClip()
		{
			var visual = ElementCompositionPreview.GetElementVisual(Child);
			visual.Clip = null;
		}
		
		void DisposeBorder()
		{
			_borderPath = null;
		}

		void UpdateBorder()
		{
			if (Border == null)
				return;

			IShape? borderShape = Border.Shape;
			_borderPath?.UpdateBorderShape(borderShape, ActualWidth, ActualHeight);

			_borderPath?.UpdateStroke(Border.Stroke);
			_borderPath?.UpdateStrokeThickness(Border.StrokeThickness);
			_borderPath?.UpdateStrokeDashPattern(Border.StrokeDashPattern);
			_borderPath?.UpdateBorderDashOffset(Border.StrokeDashOffset);
			_borderPath?.UpdateStrokeMiterLimit(Border.StrokeMiterLimit);
			_borderPath?.UpdateStrokeLineCap(Border.StrokeLineCap);
			_borderPath?.UpdateStrokeLineJoin(Border.StrokeLineJoin);
		}

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
			UpdateBorder();

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
			global::Windows.Foundation.Point offset = ttv.TransformPoint(new global::Windows.Foundation.Point(0, 0));

			var shadowHost = new UI.Xaml.Shapes.Rectangle()
			{
				Fill = new SolidColorBrush(UI.Colors.Transparent),
				Width = width,
				Height = height
			};

			Canvas.SetLeft(shadowHost, offset.X);
			Canvas.SetTop(shadowHost, offset.Y);

			_shadowCanvas.Children.Insert(0, shadowHost);

			var hostVisual = ElementCompositionPreview.GetElementVisual(_shadowCanvas);
			var compositor = hostVisual.Compositor;

			_dropShadow = compositor.CreateDropShadow();
			SetShadowProperties(_dropShadow, Shadow);

			_dropShadow.Mask = await Child.GetAlphaMaskAsync();

			_shadowVisual = compositor.CreateSpriteVisual();
			_shadowVisual.Size = new Vector2((float)width, (float)height);
			_shadowVisual.Shadow = _dropShadow;

			ElementCompositionPreview.SetElementChildVisual(shadowHost, _shadowVisual);
		}

		void UpdateShadow()
		{
			if (_dropShadow != null)
				SetShadowProperties(_dropShadow, Shadow);

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

		static void SetShadowProperties(DropShadow dropShadow, IShadow? mauiShadow)
		{
			float blurRadius = 1;
			float opacity = 0;
			var shadowColor = Graphics.Colors.Transparent;
			var offset = new Graphics.Point(1, 1);

			if (mauiShadow != null)
			{
				blurRadius = mauiShadow.Radius * 2;
				opacity = mauiShadow.Opacity;
				shadowColor = mauiShadow.Paint.ToColor();
				offset = mauiShadow.Offset;
			}

			dropShadow.BlurRadius = blurRadius;
			dropShadow.Opacity = opacity;
			if (shadowColor != null)
				dropShadow.Color = shadowColor.ToWindowsColor();
			dropShadow.Offset = new Vector3((float)offset.X, (float)offset.Y, 0);
		}
	}
}