#nullable disable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
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

				return ((UIView)renderer.PlatformView, element);
			}

			if (view is View formsView)
			{
				// Make sure the Visual property is available when the renderer is created
				PropertyPropagationExtensions.PropagatePropertyChanged(null, formsView, itemsView);

				// No template, and the EmptyView is a Forms view; use that
				var renderer = GetHandler(formsView, itemsView.FindMauiContext());
				var element = renderer.VirtualView as VisualElement;

				return ((UIView)renderer.PlatformView, element);
			}

			return (new UILabel { TextAlignment = UITextAlignment.Center, Text = $"{view}" }, null);
		}
	}
}