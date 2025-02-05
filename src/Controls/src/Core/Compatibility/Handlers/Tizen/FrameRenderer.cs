using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using Tizen.UIExtensions.NUI;
using IMeasurable = Tizen.UIExtensions.Common.IMeasurable;
using NColor = Tizen.NUI.Color;
using TColor = Tizen.UIExtensions.Common.Color;
using TLayoutParamPolicies = Tizen.NUI.BaseComponents.LayoutParamPolicies;
using TShadow = Tizen.NUI.Shadow;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	[System.Obsolete("Frame is obsolete as of .NET 9. Please use Border instead.")]
	public class FrameRenderer : ContentViewGroup, IPlatformViewHandler, IMeasurable
	{
		public static IPropertyMapper<Frame, FrameRenderer> Mapper
			= new PropertyMapper<Frame, FrameRenderer>(ViewRenderer.VisualElementRendererMapper)
			{
				[Frame.HasShadowProperty.PropertyName] = (h, _) => h.UpdateShadow(),
				[VisualElement.BackgroundColorProperty.PropertyName] = (h, _) => h.UpdateBackgroundColor(),
				[VisualElement.BackgroundProperty.PropertyName] = (h, _) => h.UpdateBackground(),
				[Frame.CornerRadiusProperty.PropertyName] = (h, _) => h.UpdateCornerRadius(),
				[Frame.BorderColorProperty.PropertyName] = (h, _) => h.UpdateBorderColor(),
				[Microsoft.Maui.Controls.Compatibility.Layout.IsClippedToBoundsProperty.PropertyName] = (h, _) => h.UpdateClippedToBounds(),
				[Frame.ContentProperty.PropertyName] = (h, _) => h.UpdateContent()
			};

		public static CommandMapper<Frame, FrameRenderer> CommandMapper
			= new CommandMapper<Frame, FrameRenderer>(ViewRenderer.VisualElementRendererCommandMapper);

		bool _disposed;

		SKClipperView _clipperView;

		IMauiContext? _mauiContext;
		ViewHandlerDelegator<Frame> _viewHandlerWrapper;
		IPlatformViewHandler? _contentHandler;


		public FrameRenderer() : base(null)
		{
			_clipperView = new SKClipperView()
			{
				WidthSpecification = TLayoutParamPolicies.MatchParent,
				HeightSpecification = TLayoutParamPolicies.MatchParent
			};
			_clipperView.DrawClippingArea += OnDrawClippingArea;

			Children.Add(_clipperView);

			BorderlineColor = NColor.Transparent;
			BorderlineWidth = 1.0.ToScaledPixel();
			_viewHandlerWrapper = new ViewHandlerDelegator<Frame>(Mapper, CommandMapper, this);
		}

		protected Frame? Element
		{
			get { return _viewHandlerWrapper.Element; }
			set
			{
				if (value != null)
					(this as IPlatformViewHandler).SetVirtualView(value);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				Element?.Handler?.DisconnectHandler();
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			if (e.NewElement != null)
			{
				CrossPlatformArrange = e.NewElement.CrossPlatformArrange;
				CrossPlatformMeasure = e.NewElement.CrossPlatformMeasure;
			}
		}

		protected virtual void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_disposed)
			{
				return;
			}

			if (Element != null && e.PropertyName != null)
				_viewHandlerWrapper.UpdateProperty(e.PropertyName);
		}

		void OnDrawClippingArea(object? sender, SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs e)
		{
			if (Element == null || _disposed)
				return;

			var canvas = e.Surface.Canvas;
			var directRect = e.Info.Rect;
			canvas.Clear();

			if (!Element.IsClippedToBounds)
			{
				canvas.Clear(SKColors.White);
				return;
			}
			var radius = ((double)Element.CornerRadius).ToScaledPixel();
			using var paint = new SKPaint
			{
				Color = SKColors.White,
			};
			canvas.DrawRoundRect(directRect, new SKSize(radius, radius), paint);
		}

		void UpdateClippedToBounds()
		{
			if (Element == null || _disposed)
				return;

			_clipperView.Invalidate();
		}

		void UpdateBackgroundColor()
		{
			if (Element == null || _disposed)
				return;

			if (Element.BackgroundColor.IsNotDefault())
				BackgroundColor = Element.BackgroundColor.ToNUIColor();
			else
				BackgroundColor = Colors.White.ToNUIColor();
		}

		void UpdateBackground()
		{
			if (Element == null || _disposed)
				return;

			// If BackgroundColor is valid, do not update Background
			if (Element.BackgroundColor.IsNotDefault())
				return;

			var color = ((Paint)Element.Background)?.ToColor();

			if (color != null)
				BackgroundColor = color.ToNUIColor();
			else
				BackgroundColor = Colors.White.ToNUIColor();
		}

		void UpdateBorderColor()
		{
			if (Element == null || _disposed)
				return;

			if (Element.BorderColor.IsNotDefault())
				BorderlineColor = Element.BorderColor.ToNUIColor();
			else
				BorderlineColor = NColor.Transparent;
		}

		void UpdateShadow()
		{
			if (Element == null || _disposed)
				return;

			if (Element.HasShadow)
			{
				BoxShadow = new TShadow(2.0.ToScaledPixel(), TColor.FromHex("#111111").ToNative());
			}
			else
			{
				BoxShadow = null;
			}
		}

		void UpdateCornerRadius()
		{
			if (Element == null || _disposed)
				return;

			CornerRadius = ((double)Element.CornerRadius).ToScaledPixel();
		}

		void UpdateContent()
		{
			var content = Element?.Content;

			if (_contentHandler != null)
			{
				if (_contentHandler.PlatformView != null)
				{
					_clipperView.Remove(_contentHandler.PlatformView);
				}
				_contentHandler?.Dispose();
				_contentHandler = null;
			}


			if (content == null || _mauiContext == null || _disposed)
			{
				return;
			}

			var platformView = content.ToPlatform(_mauiContext);
			platformView.WidthSpecification = TLayoutParamPolicies.MatchParent;
			platformView.HeightSpecification = TLayoutParamPolicies.MatchParent;
			_clipperView.Add(platformView);
			if (content.Handler is IPlatformViewHandler thandler)
			{
				_contentHandler = thandler;
			}
		}


		TSize IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			if (Element?.Handler is IPlatformViewHandler pvh && Element is IContentView cv)
			{
				return pvh.MeasureVirtualView(availableWidth.ToScaledDP(), availableHeight.ToScaledDP(), cv.CrossPlatformMeasure).ToPixel();
			}
			else
			{
				return NaturalSize2D.ToCommon();
			}
		}

		#region IPlatformViewHandler
		Size IViewHandler.GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);
		}

		bool IViewHandler.HasContainer { get => false; set { } }

		object? IViewHandler.ContainerView => null;

		IView? IViewHandler.VirtualView => Element;

		object IElementHandler.PlatformView => this;

		Maui.IElement? IElementHandler.VirtualView => Element;

		IMauiContext? IElementHandler.MauiContext => _mauiContext;

		Tizen.NUI.BaseComponents.View? IPlatformViewHandler.PlatformView => this;

		Tizen.NUI.BaseComponents.View? IPlatformViewHandler.ContainerView => this;

		void IViewHandler.PlatformArrange(Graphics.Rect rect) =>
			this.PlatformArrangeHandler(rect);

		void IElementHandler.SetMauiContext(IMauiContext mauiContext) =>
			_mauiContext = mauiContext;

		void IElementHandler.SetVirtualView(Maui.IElement view) =>
			_viewHandlerWrapper.SetVirtualView(view, OnElementChanged, false);

		void IElementHandler.UpdateValue(string property)
		{
			if (Element != null)
			{
				OnElementPropertyChanged(Element, new PropertyChangedEventArgs(property));
			}
		}

		void IElementHandler.Invoke(string command, object? args)
		{
			_viewHandlerWrapper.Invoke(command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			_viewHandlerWrapper.DisconnectHandler();
		}
		#endregion
	}
}
