using System;
using Windows.UI.Xaml;
using Xamarin.Forms;

namespace Xamarin.Forms.Platform.UWP
{
	internal class FormsPresenter : Windows.UI.Xaml.Controls.ContentPresenter
	{
		public FormsPresenter()
		{
			Loaded += FormsPresenter_Loaded;
			Unloaded += FormsPresenter_Unloaded;
			SizeChanged += (s, e) =>
			{
				if (ActualWidth > 0 && ActualHeight > 0)
				{
					var page = (Page)DataContext;
					((IPageController)page.RealParent).ContainerArea = new Rectangle(0, 0, ActualWidth, ActualHeight);
				}
			};
		}

		void FormsPresenter_Loaded(object sender, RoutedEventArgs e)
		{
			var page = (IPageController)DataContext;
			page.SendAppearing();
		}

		void FormsPresenter_Unloaded(object sender, RoutedEventArgs e)
		{
			var page = (IPageController)DataContext;
			page.SendDisappearing();
		}
	}
}
