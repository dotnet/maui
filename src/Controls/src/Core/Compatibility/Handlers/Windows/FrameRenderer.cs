#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using WBorder = Microsoft.UI.Xaml.Controls.Border;
using WRect = Windows.Foundation.Rect;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class FrameRenderer : ViewRenderer<Frame, WBorder>
	{
		const int FrameBorderThickness = 1;

		public static IPropertyMapper<Frame, FrameRenderer> Mapper
			= new PropertyMapper<Frame, FrameRenderer>(VisualElementRendererMapper)
			{
				// https://github.com/dotnet/maui/issues/11880
				// Dimension constraints need to be propagated to the container 
				// in order for the `PlatformMeasure` calls to return the correct
				// get desired size
				[nameof(IView.Width)] = MapWidth,
				[nameof(IView.Height)] = MapHeight,
				[nameof(IView.MinimumHeight)] = MapMinimumHeight,
				[nameof(IView.MaximumHeight)] = MapMaximumHeight,
				[nameof(IView.MinimumWidth)] = MapMinimumWidth,
				[nameof(IView.MaximumWidth)] = MapMaximumWidth,
			};

		public static CommandMapper<Frame, FrameRenderer> CommandMapper
			= new CommandMapper<Frame, FrameRenderer>(VisualElementRendererCommandMapper);

		public FrameRenderer() : this(Mapper, CommandMapper)
		{
		}

		public FrameRenderer(IPropertyMapper mapper)
			: this(mapper, CommandMapper)
		{
		}

		public FrameRenderer(IPropertyMapper mapper, CommandMapper commandMapper) : base(mapper, commandMapper)
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
					SetNativeControl(new WBorder());

				PackChild();
				UpdateBorder();
				UpdateCornerRadius();
				UpdatePadding();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "Content")
			{
				PackChild();
			}
			else if (e.PropertyName == Frame.BorderColorProperty.PropertyName || e.PropertyName == Frame.HasShadowProperty.PropertyName)
			{
				UpdateBorder();
			}
			else if (e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
			{
				UpdateCornerRadius();
			}
			else if (e.PropertyName == Frame.PaddingProperty.PropertyName)
			{
				UpdatePadding();
			}
		}

		void UpdatePadding()
		{
			Control.Padding = Element.Padding.ToPlatform();
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			// We need this so the `Border` control will arrange and have a size
			Control?.Arrange(new WRect(0, 0, finalSize.Width, finalSize.Height));
			return new global::Windows.Foundation.Size(Math.Max(0, finalSize.Width), Math.Max(0, finalSize.Height));
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			Control?.Measure(availableSize);

			if (Control?.DesiredSize is not null)
				return Control.DesiredSize;

			return MinimumSize().ToPlatform();
		}

		protected override void UpdateBackgroundColor()
		{
			UpdateBackground();
		}

		protected override void UpdateBackground()
		{
			Color backgroundColor = Element.BackgroundColor;
			Brush background = Element.Background;

			if (Control != null)
			{
				if (Brush.IsNullOrEmpty(background))
					Control.Background = backgroundColor.IsDefault() ?
						new Microsoft.UI.Xaml.Media.SolidColorBrush((global::Windows.UI.Color)Resources["SystemAltHighColor"]) : backgroundColor.ToPlatform();
				else
					Control.Background = background.ToBrush();
			}
		}

		void PackChild()
		{
			if (Element.Content == null)
			{
				Control.Child = null;
				return;
			}

			var view = new ContentPanel
			{
				CrossPlatformMeasure = ((IContentView)Element).CrossPlatformMeasure,
				CrossPlatformArrange = ((IContentView)Element).CrossPlatformArrange
			};

			view.Content = Element.Content.ToPlatform(MauiContext);
			Control.Child = view;
		}

		void UpdateBorder()
		{
			if (Element.BorderColor.IsNotDefault())
			{
				Control.BorderBrush = Element.BorderColor.ToPlatform();
				Control.BorderThickness = WinUIHelpers.CreateThickness(FrameBorderThickness);
			}
			else
			{
				Control.BorderBrush = new Color(0, 0, 0, 0).ToPlatform();
			}
		}

		void UpdateCornerRadius()
		{
			float cornerRadius = Element.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // default corner radius

			Control.CornerRadius = WinUIHelpers.CreateCornerRadius(cornerRadius);
		}

		// https://github.com/dotnet/maui/issues/11880
		// Dimension constraints need to be propagated to the container 
		// in order for the `PlatformMeasure` calls to return the correct
		// get desired size
		static void MapWidth(IViewHandler handler, IView view)
		{
			VisualElementRendererMapper.UpdateProperty(handler, view, nameof(IView.Width));
			handler.ToPlatform().UpdateWidth(view);
		}

		static void MapHeight(IViewHandler handler, IView view)
		{
			VisualElementRendererMapper.UpdateProperty(handler, view, nameof(IView.Height));
			handler.ToPlatform().UpdateHeight(view);
		}

		static void MapMinimumHeight(IViewHandler handler, IView view)
		{
			VisualElementRendererMapper.UpdateProperty(handler, view, nameof(IView.MinimumHeight));
			handler.ToPlatform().UpdateMinimumHeight(view);
		}

		static void MapMaximumHeight(IViewHandler handler, IView view)
		{
			VisualElementRendererMapper.UpdateProperty(handler, view, nameof(IView.MaximumHeight));
			handler.ToPlatform().UpdateMaximumHeight(view);
		}

		static void MapMinimumWidth(IViewHandler handler, IView view)
		{
			VisualElementRendererMapper.UpdateProperty(handler, view, nameof(IView.MinimumWidth));
			handler.ToPlatform().UpdateMinimumWidth(view);
		}

		static void MapMaximumWidth(IViewHandler handler, IView view)
		{
			VisualElementRendererMapper.UpdateProperty(handler, view, nameof(IView.MaximumWidth));
			handler.ToPlatform().UpdateMaximumWidth(view);
		}
	}
}