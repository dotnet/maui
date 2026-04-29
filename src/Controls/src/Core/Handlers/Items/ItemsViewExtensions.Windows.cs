using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WASDKScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WASDKScrollingScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollingScrollBarVisibility;	

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class ItemsViewExtensions
	{
		internal static FrameworkElement RealizeEmptyViewTemplate(object? bindingContext, DataTemplate? emptyViewTemplate, IMauiContext mauiContext, ref View? mauiEmptyView)
		{
			if (emptyViewTemplate is null)
			{
				return CreateDefaultEmptyViewTextBlock(bindingContext);
			}

			var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);

			var view = template.CreateContent() as View;
			if (view is not null)
			{
				view.BindingContext = bindingContext;
			}

			return RealizeEmptyView(view, mauiContext, ref mauiEmptyView);
		}

		static TextBlock CreateDefaultEmptyViewTextBlock(object? bindingContext)
		{
			return new TextBlock
			{
				HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
				VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
				Text = bindingContext?.ToString() ?? string.Empty
			};
		}
		internal static FrameworkElement RealizeEmptyView(View? view, IMauiContext mauiContext, ref View? mauiEmptyView)
		{
			mauiEmptyView = view ?? throw new ArgumentNullException(nameof(view));

			var handler = view.ToHandler(mauiContext);
			var platformView = handler.ContainerView ?? handler.PlatformView;

			return platformView as FrameworkElement ?? throw new InvalidOperationException("Unable to convert view to FrameworkElement");
		}

		internal static FrameworkElement RealizeHeaderFooterTemplate(object? bindingContext, DataTemplate? template, IMauiContext mauiContext, ref View? mauiView)
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

		/// <summary>
		/// Applies MAUI margin and layout options to a WinUI element that hosts a MAUI View
		/// (e.g., CollectionView header/footer placed in a WinUI ContentControl).
		/// The cross-platform layout system never applies these because the element
		/// is not inside a MAUI layout.
		/// </summary>
		internal static void ApplyMauiLayoutProperties(View? mauiView, FrameworkElement? platformElement)
		{
			if (mauiView is null || platformElement is null)
			{
				return;
			}

			// Apply MAUI margin as WinUI margin
			var margin = mauiView.Margin;
			platformElement.Margin = new Microsoft.UI.Xaml.Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom);

			// Map HorizontalOptions/VerticalOptions to WinUI alignment
			platformElement.UpdateHorizontalOptions(mauiView);
			platformElement.UpdateVerticalOptions(mauiView);
		}
	}
}
