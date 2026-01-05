using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WASDKScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class ItemsViewExtensions
	{
		internal static FrameworkElement RealizeEmptyViewTemplate(object bindingContext, DataTemplate? emptyViewTemplate, IMauiContext mauiContext, ref View? mauiEmptyView)
		{
			if (emptyViewTemplate is null)
			{
				return new TextBlock
				{
					HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
					VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
					Text = bindingContext?.ToString() ?? string.Empty
				};
			}

			var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);
			var view = template.CreateContent() as View;
			if (view is not null)
			{
				view.BindingContext = bindingContext;
			}

			return RealizeEmptyView(view, mauiContext, ref mauiEmptyView);
		}

		internal static FrameworkElement RealizeEmptyView(View? view, IMauiContext mauiContext, ref View? mauiEmptyView)
		{
			mauiEmptyView = view ?? throw new ArgumentNullException(nameof(view));

			var handler = view.ToHandler(mauiContext);
			var platformView = handler.ContainerView ?? handler.PlatformView;

			return platformView as FrameworkElement ?? throw new InvalidOperationException("Unable to convert view to FrameworkElement");
		}

		internal static FrameworkElement RealizeHeaderFooterTemplate(object bindingContext, DataTemplate? template, IMauiContext mauiContext, ref View? mauiView)
		{
			if (template is null)
			{
				return new TextBlock
				{
					Text = bindingContext?.ToString() ?? string.Empty,
					Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 5)
				};
			}

			var dataTemplate = template.SelectDataTemplate(bindingContext, null);
			var view = dataTemplate.CreateContent() as View;
			if (view is not null)
			{
				view.BindingContext = bindingContext;
			}

			return RealizeHeaderFooterView(view, mauiContext, ref mauiView);
		}

		internal static FrameworkElement RealizeHeaderFooterView(View? view, IMauiContext mauiContext, ref View? mauiView)
		{
			mauiView = view ?? throw new ArgumentNullException(nameof(view));

			var handler = view.ToHandler(mauiContext);
			var platformView = handler.ContainerView ?? handler.PlatformView;

			return platformView as FrameworkElement ?? throw new InvalidOperationException("Unable to convert view to FrameworkElement");
		}

		internal static void UpdateVerticalScrollBarVisibility(
			UIElement control,
			ScrollBarVisibility scrollBarVisibility,
			ref WASDKScrollBarVisibility? defaultVerticalScrollVisibility)
		{
			if (scrollBarVisibility != ScrollBarVisibility.Default)
			{
				// If the value is changing to anything other than the default, record the default 
				if (defaultVerticalScrollVisibility is null)
				{
					defaultVerticalScrollVisibility = ScrollViewer.GetVerticalScrollBarVisibility(control);
				}
			}

			if (defaultVerticalScrollVisibility is null)
			{
				// If the default has never been recorded, then this has never been set to anything but the 
				// default value; there's nothing to do.
				return;
			}

			switch (scrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					ScrollViewer.SetVerticalScrollBarVisibility(control, WASDKScrollBarVisibility.Visible);
					break;
				case ScrollBarVisibility.Never:
					ScrollViewer.SetVerticalScrollBarVisibility(control, WASDKScrollBarVisibility.Hidden);
					break;
				case ScrollBarVisibility.Default:
					ScrollViewer.SetVerticalScrollBarVisibility(control, defaultVerticalScrollVisibility.Value);
					break;
			}
		}

		internal static void UpdateHorizontalScrollBarVisibility(
			UIElement control,
			ScrollBarVisibility scrollBarVisibility,
			ref WASDKScrollBarVisibility? defaultHorizontalScrollVisibility)
		{
			if (defaultHorizontalScrollVisibility is null)
			{
				defaultHorizontalScrollVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(control);
			}

			switch (scrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					ScrollViewer.SetHorizontalScrollBarVisibility(control, WASDKScrollBarVisibility.Visible);
					break;
				case ScrollBarVisibility.Never:
					ScrollViewer.SetHorizontalScrollBarVisibility(control, WASDKScrollBarVisibility.Hidden);
					break;
				case ScrollBarVisibility.Default:
					ScrollViewer.SetHorizontalScrollBarVisibility(control, defaultHorizontalScrollVisibility.Value);
					break;
			}
		}
	}
}
