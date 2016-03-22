using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.WinRT
{
	public class TabSelectedEventArgs
		: EventArgs
	{
		public TabSelectedEventArgs(Page tab)
		{
			if (tab == null)
				throw new ArgumentNullException("tab");

			Tab = tab;
		}

		public Page Tab { get; private set; }
	}

	public class TabButton
		: Windows.UI.Xaml.Controls.Button
	{
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Click += OnClick;
		}

		void OnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			var page = DataContext as Page;
			if (page == null)
				return;

			var parent = page.RealParent as TabbedPage;
			if (parent == null)
				return;

			var renderer = Platform.GetRenderer(parent) as TabbedPageRenderer;
			if (renderer != null)
				renderer.OnTabSelected(page);
		}
	}

	public class TabsControl
		: ItemsControl
	{
	}
}