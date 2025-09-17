using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics;
#if MAUI_GRAPHICS_WIN2D
using Microsoft.Maui.Graphics.Win2D;
#else
using Microsoft.Maui.Graphics.Platform;
#endif
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using WSize = Windows.Foundation.Size;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : Grid, IDisposable
	{
		Canvas? _shadowCanvas;
		UIElementCollection? _shadowCanvasCachedChildren;

		SpriteVisual? _shadowVisual;
		DropShadow? _dropShadow;
		Rectangle? _shadowHost;
		WSize _shadowHostSize;
		Path? _borderPath;

		FrameworkElement? _child;

		UIElementCollection? _cachedChildren;

		[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Panel.Children property is banned to enforce use of this CachedChildren property.")]
		internal UIElementCollection CachedChildren
		{
			get
			{
				_cachedChildren ??= Children;
				return _cachedChildren;
			}
		}

		public WrapperView()
		{
		}

		long _visibilityDependencyPropertyCallbackToken;
		public FrameworkElement? Child
		{
			get { return _child; }
			internal set
			{
				if (_child is not null)
				{
					_child.SizeChanged -= OnChildSizeChanged;
					_child.UnregisterPropertyChangedCallback(VisibilityProperty, _visibilityDependencyPropertyCallbackToken);
					CachedChildren.Remove(_child);
				}

				if (value is null)
				{
					return;
				}

				_child = value;
				_child.SizeChanged += OnChildSizeChanged;
				_visibilityDependencyPropertyCallbackToken = _child.RegisterPropertyChangedCallback(VisibilityProperty, OnChildVisibilityChanged);
				CachedChildren.Add(_child);
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
			if (Child is null)
			{
				return;
			}

			var clipGeometry = Clip;

			if (clipGeometry is null)
			{
				return;
			}

			double width = Child.ActualWidth;
			double height = Child.ActualHeight;

			if (height <= 0 && width <= 0)
			{
				return;
			}

			var visual = ElementCompositionPreview.GetElementVisual(Child);
			var compositor = visual.Compositor;

			var pathSize = new Graphics.Rect(0, 0, width, height);
			var clipPath = clipGeometry.PathForBounds(pathSize);
			var device = CanvasDevice.GetSharedDevice();
			var geometry = clipPath.AsPath(device);

			var path = new CompositionPath(geometry);
			var pathGeometry = compositor.CreatePathGeometry(path);
			var geometricClip = compositor.CreateGeometricClip(pathGeometry);

			// The clip needs to consider the child's offset in case it is in a different position because of a different alignment.
			geometricClip.Offset = new Vector2(-Child.ActualOffset.X, -Child.ActualOffset.Y);
			visual.Clip = geometricClip;
			//When the clip is updated, the shadow must be updated as well
			UpdateShadowAsync().FireAndForget(IPlatformApplication.Current?.Services?.CreateLogger(nameof(WrapperView)));
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
			if (Border is null)
			{
				return;
			}

			if (_borderPath is null)
			{
				_borderPath = new();

				int index = _shadowCanvas is not null ? 1 : 0;
				CachedChildren.Insert(index, _borderPath);
			}

			IShape? borderShape = Border.Shape;
			_borderPath.UpdateBorderShape(borderShape, ActualWidth, ActualHeight);

			_borderPath.UpdateStroke(Border.Stroke);
			_borderPath.UpdateStrokeThickness(Border.StrokeThickness);
			_borderPath.UpdateStrokeDashPattern(Border.StrokeDashPattern);
			_borderPath.UpdateBorderDashOffset(Border.StrokeDashOffset);
			_borderPath.UpdateStrokeMiterLimit(Border.StrokeMiterLimit);
			_borderPath.UpdateStrokeLineCap(Border.StrokeLineCap);
			_borderPath.UpdateStrokeLineJoin(Border.StrokeLineJoin);
		}

		partial void ShadowChanged()
		{
			if (HasShadow)
			{
				UpdateShadowAsync().FireAndForget(IPlatformApplication.Current?.Services?.CreateLogger(nameof(WrapperView)));
			}
			else
			{
				CreateShadowAsync().FireAndForget(IPlatformApplication.Current?.Services?.CreateLogger(nameof(WrapperView)));
			}
		}

		void OnChildSizeChanged(object sender, SizeChangedEventArgs e)
		{
			_shadowHostSize = e.NewSize;

			UpdateClip();
			UpdateBorder();
			UpdateShadowAsync().FireAndForget(IPlatformApplication.Current?.Services?.CreateLogger(nameof(WrapperView)));
		}

		void OnChildVisibilityChanged(DependencyObject sender, DependencyProperty dp)
		{
			// OnChildSizeChanged does not fire for Visibility changes to child
			if (sender is FrameworkElement child && _shadowCanvasCachedChildren?.Count > 0)
			{
				var shadowHost = _shadowCanvasCachedChildren[0];
				shadowHost.Visibility = child.Visibility;
			}
		}

		void DisposeShadow()
		{
			if (_shadowCanvas is null || _shadowCanvasCachedChildren is null)
			{
				return;
			}

			if (_shadowHost is not null)
			{
				ElementCompositionPreview.SetElementChildVisual(_shadowHost, null);
			}

			if (_shadowCanvasCachedChildren.Count > 0)
			{
				_shadowCanvasCachedChildren.RemoveAt(0);
			}

			_shadowVisual?.Dispose();
			_shadowVisual = null;

			_dropShadow?.Dispose();
			_dropShadow = null;

			_shadowCanvasCachedChildren = null;
			_shadowCanvas = null;
		}

		async Task CreateShadowAsync()
		{
			if (Child == null || Shadow == null || Shadow.Paint == null)
			{
				return;
			}

			var visual = ElementCompositionPreview.GetElementVisual(Child);

			if (Clip != null && visual.Clip == null)
			{
				return;
			}

			double width = _shadowHostSize.Width;
			double height = _shadowHostSize.Height;

			if (_shadowCanvas is null)
			{
				_shadowCanvas = new();

#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons and it is better to cache it.
				_shadowCanvasCachedChildren = _shadowCanvas.Children;
#pragma warning restore RS0030 // Do not use banned APIs

				// Shadow canvas must be the first child. The order of children (i.e. shadow canvas and border path) matters.
				CachedChildren.Insert(0, _shadowCanvas);
			}

			var ttv = Child.TransformToVisual(_shadowCanvas);
			global::Windows.Foundation.Point offset = ttv.TransformPoint(new global::Windows.Foundation.Point(0, 0));

			_shadowHost = new UI.Xaml.Shapes.Rectangle()
			{
				Fill = new SolidColorBrush(UI.Colors.Transparent),
				Width = width,
				Height = height
			};

			Canvas.SetLeft(_shadowHost, offset.X);
			Canvas.SetTop(_shadowHost, offset.Y);

			_shadowCanvasCachedChildren?.Insert(0, _shadowHost);

			var hostVisual = ElementCompositionPreview.GetElementVisual(_shadowCanvas);
			var compositor = hostVisual.Compositor;

			_dropShadow = compositor.CreateDropShadow();
			await SetShadowPropertiesAsync(_dropShadow, Shadow);

			_shadowVisual = compositor.CreateSpriteVisual();
			_shadowVisual.Size = new Vector2((float)width, (float)height);

			_shadowVisual.Shadow = _dropShadow;

			ElementCompositionPreview.SetElementChildVisual(_shadowHost, _shadowVisual);
		}

		async Task UpdateShadowAsync()
		{
			if (_dropShadow != null)
			{
				await SetShadowPropertiesAsync(_dropShadow, Shadow);
			}

			UpdateShadowSize();
		}

		void UpdateShadowSize()
		{
			if (Child is FrameworkElement frameworkElement)
			{
				float width = (float)_shadowHostSize.Width;

				if (width <= 0)
				{
					width = (float)frameworkElement.ActualWidth;
				}

				float height = (float)_shadowHostSize.Height;

				if (height <= 0)
				{
					height = (float)frameworkElement.ActualHeight;
				}

				if (_shadowVisual is not null)
				{
					_shadowVisual.Size = new Vector2(width, height);
				}

				if (_shadowHost is not null)
				{
					_shadowHost.Width = width;
					_shadowHost.Height = height;

					Canvas.SetLeft(_shadowHost, Child.ActualOffset.X);
					Canvas.SetTop(_shadowHost, Child.ActualOffset.Y);
				}
			}
		}

		async Task SetShadowPropertiesAsync(DropShadow dropShadow, IShadow? mauiShadow)
		{
			float blurRadius = 10f;
			float opacity = 1f;
			Graphics.Color? shadowColor = Colors.Black;
			Graphics.Point offset = Graphics.Point.Zero;

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
			{
				dropShadow.Color = shadowColor.ToWindowsColor();
			}

			dropShadow.Offset = new Vector3((float)offset.X, (float)offset.Y, 0);
			dropShadow.Mask = await Child.GetAlphaMaskAsync();
		}
	}
}