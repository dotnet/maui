#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Devices;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class TemplateHelpers
	{

		public static IPlatformViewHandler GetHandler(View view, IMauiContext context)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}
			var handler = view.Handler;

			if (handler == null)
				handler = view.ToHandler(context);

			(handler.PlatformView as UIView).Frame = view.Bounds.ToCGRect();

			return (IPlatformViewHandler)handler;
		}

		public static (UIView PlatformView, VisualElement FormsElement) RealizeView(object view, DataTemplate viewTemplate, ItemsView itemsView)
		{
			if (view is OnPlatform<View> onPlatformView && onPlatformView.Platforms.Count > 0)
			{
				var platform = DeviceInfo.Current.Platform;
				foreach (var platformView in onPlatformView.Platforms)
				{
					if (platformView.Platform.Contains(platform.ToString()))
					{
						view = platformView.Value;
						break;
					}
				}
			}

			if (viewTemplate != null)
			{
				// Run this through the extension method in case it's really a DataTemplateSelector
				viewTemplate = viewTemplate.SelectDataTemplate(view, itemsView);

				// We have a template; turn it into a Forms view 
				var templateElement = viewTemplate.CreateContent() as View;

				// Make sure the Visual property is available when the renderer is created
				PropertyPropagationExtensions.PropagatePropertyChanged(null, templateElement, itemsView);

				var renderer = GetHandler(templateElement, itemsView.FindMauiContext());

				var element = renderer.VirtualView as VisualElement;

				// and set the view as its BindingContext
				element.BindingContext = view;

				return (renderer.ToPlatform(), element);
			}

			if (view is View mauiView)
			{
				// Make sure the Visual property is available when the renderer is created
				PropertyPropagationExtensions.PropagatePropertyChanged(null, mauiView, itemsView);

				// No template, and the EmptyView is a Maui view; use that
				// But we need to wrap it in a GeneralWrapperView so it can be measured and arranged
				var wrapperView = new GeneralWrapperView(mauiView, itemsView.FindMauiContext());
				wrapperView.Frame = mauiView.Bounds.ToCGRect();
				return (wrapperView, mauiView);
			}

			return (new UILabel { TextAlignment = UITextAlignment.Center, Text = $"{view}" }, null);
		}
	}
}