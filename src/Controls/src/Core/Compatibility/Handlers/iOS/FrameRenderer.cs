#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Controls.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class FrameRenderer : VisualElementRenderer<Frame>
	{
		public static IPropertyMapper<Frame, FrameRenderer> Mapper
			= new PropertyMapper<Frame, FrameRenderer>(VisualElementRendererMapper);

		public static CommandMapper<Frame, FrameRenderer> CommandMapper
			= new CommandMapper<Frame, FrameRenderer>(VisualElementRendererCommandMapper);

		FrameView _actualView;
		CGSize _previousSize;
		bool _isDisposed;

		public FrameRenderer() : base(Mapper, CommandMapper)
		{
			_actualView = new FrameView();
			AddSubview(_actualView);
		}

		public FrameRenderer(IPropertyMapper mapper)
			: this(mapper, CommandMapper)
		{
		}

		public FrameRenderer(IPropertyMapper mapper, CommandMapper commandMapper) : base(mapper, commandMapper)
		{
			AutoPackage = false;
		}

		public override void AddSubview(UIView view)
		{
			if (view != _actualView)
				_actualView.AddSubview(view);
			else
				base.AddSubview(view);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				_actualView.CrossPlatformLayout = e.NewElement;

				SetupLayer();
				UpdateShadow();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
				e.PropertyName == VisualElement.BackgroundProperty.PropertyName ||
				e.PropertyName == Microsoft.Maui.Controls.Frame.BorderColorProperty.PropertyName ||
				e.PropertyName == Microsoft.Maui.Controls.Frame.CornerRadiusProperty.PropertyName ||
				e.PropertyName == Microsoft.Maui.Controls.Frame.IsClippedToBoundsProperty.PropertyName ||
				e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				SetupLayer();
			else if (e.PropertyName == Controls.Frame.HasShadowProperty.PropertyName)
				UpdateShadow();
		}

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
#pragma warning disable CA1422 // Validate platform compatibility
			base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
			// Make sure the control adheres to changes in UI theme
			if (OperatingSystem.IsIOSVersionAtLeast(13) && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				SetupLayer();
		}

		public virtual void SetupLayer()
		{
			if (_actualView == null)
				return;
			if (Element is not Frame element)
				return;

			float cornerRadius = element.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // default corner radius

			_actualView.Layer.CornerRadius = cornerRadius;
			_actualView.Layer.MasksToBounds = cornerRadius > 0;

			if (element.BackgroundColor == null)
				_actualView.Layer.BackgroundColor = Microsoft.Maui.Platform.ColorExtensions.BackgroundColor.CGColor;
			else
			{
				// BackgroundColor gets set on the base class too which messes with
				// the corner radius, shadow, etc. so override that behaviour here
				BackgroundColor = UIColor.Clear;
				_actualView.Layer.BackgroundColor = element.BackgroundColor.ToCGColor();
			}

			_actualView.Layer.RemoveBackgroundLayer();

			if (!Brush.IsNullOrEmpty(element.Background))
			{
				var backgroundLayer = this.GetBackgroundLayer(element.Background);

				if (backgroundLayer != null)
				{
					_actualView.Layer.BackgroundColor = UIColor.Clear.CGColor;

					backgroundLayer.BackgroundColor = ColorExtensions.BackgroundColor.CGColor;
					backgroundLayer.CornerRadius = cornerRadius;

					Layer.InsertBackgroundLayer(backgroundLayer, 0);
				}
			}

			if (element.BorderColor == null)
			{
				_actualView.Layer.BorderColor = UIColor.Clear.CGColor;
				_actualView.Layer.BorderWidth = 0;
			}
			else
			{
				var borderWidth = (int)(element is IBorderElement be ? be.BorderWidth : 1);
				borderWidth = Math.Max(1, borderWidth);

				_actualView.Layer.BorderColor = element.BorderColor.ToCGColor();
				_actualView.Layer.BorderWidth = borderWidth;
			}

			Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			Layer.ShouldRasterize = true;
			Layer.MasksToBounds = false;

			_actualView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			_actualView.Layer.ShouldRasterize = true;
			_actualView.Layer.MasksToBounds = element.IsClippedToBoundsSet(true);
		}

		void UpdateShadow()
		{
			if (Element is IElement element)
				element.Handler?.UpdateValue(nameof(IView.Shadow));
		}

		public override void LayoutSubviews()
		{
			if (_previousSize != Bounds.Size)
			{
				SetNeedsDisplay();
				this.UpdateBackgroundLayer();
			}

			base.LayoutSubviews();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			return _actualView.SizeThatFits(size);
		}

		public override void Draw(CGRect rect)
		{
			if (_actualView != null)
				_actualView.Frame = Bounds;

			base.Draw(rect);

			_previousSize = Bounds.Size;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			base.Dispose(disposing);

			if (disposing)
			{
				if (_actualView != null)
				{
					for (var i = 0; i < _actualView.GestureRecognizers?.Length; i++)
						_actualView.GestureRecognizers.Remove(_actualView.GestureRecognizers[i]);

					for (var j = 0; j < _actualView.Subviews.Length; j++)
						_actualView.Subviews.Remove(_actualView.Subviews[j]);

					_actualView.RemoveFromSuperview();
					_actualView.Dispose();
					_actualView = null;
				}
			}
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		class FrameView : Microsoft.Maui.Platform.ContentView
		{
			public override void RemoveFromSuperview()
			{
				for (var i = Subviews.Length - 1; i >= 0; i--)
				{
					var item = Subviews[i];
					item.RemoveFromSuperview();
				}
			}

			public override bool PointInside(CGPoint point, UIEvent uievent)
			{
				foreach (var view in Subviews)
				{
					if (view.HitTest(ConvertPointToView(point, view), uievent) != null)
						return true;
				}

				return false;
			}
		}
	}
}
