using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register(nameof(ToolbarForeground), typeof(Brush), typeof(TabsControl), new PropertyMetadata(default(Brush)));
		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register(nameof(ToolbarBackground), typeof(Brush), typeof(TabsControl), new PropertyMetadata(default(Brush)));

		public Brush ToolbarBackground
		{
			get { return (Brush)GetValue(ToolbarBackgroundProperty); }
			set { SetValue(ToolbarBackgroundProperty, value); }
		}

		public Brush ToolbarForeground
		{
			get { return (Brush)GetValue(ToolbarForegroundProperty); }
			set { SetValue(ToolbarForegroundProperty, value); }
		}
	}
}