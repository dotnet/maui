using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
{
	partial class ShellPageWrapper
	{
		Microsoft.UI.Xaml.Controls.ContentPresenter Root { get; }
		public ShellPageWrapper()
		{
			InitializeComponent();
			Root = new Microsoft.UI.Xaml.Controls.ContentPresenter()
			{
				HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch,
				VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch
			};

			this.Content = Root;
		}

		public Page Page { get; set; }
		protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			LoadPage();
		}

		protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			Root.Content = null;
		}

		public void LoadPage()
		{
			if (Page != null)
			{
				var container = Page.ToNative(Page.FindMauiContextOnParent());
				Root.Content = container;
				container.Loaded -= OnPageLoaded;
				container.Loaded += OnPageLoaded; 
			}
		}

		void OnPageLoaded(object sender, RoutedEventArgs e)
		{
			var frameworkElement = sender as FrameworkElement;
			Page.Layout(new Rectangle(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
		}
	}
}
