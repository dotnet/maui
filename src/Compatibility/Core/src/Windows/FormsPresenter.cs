using System;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class FormsPresenter : Microsoft.UI.Xaml.Controls.ContentPresenter
	{
		public FormsPresenter()
		{
			Loaded += FormsPresenter_Loaded;
			Unloaded += FormsPresenter_Unloaded;
			SizeChanged += (s, e) =>
			{
				if (ActualWidth > 0 && ActualHeight > 0 && DataContext != null)
				{
					var page = (Page)DataContext;
					((Page)page.RealParent).ContainerArea = new Rectangle(0, 0, ActualWidth, ActualHeight);
				}
			};
		}

		void FormsPresenter_Loaded(object sender, RoutedEventArgs e)
			=> (DataContext as Page)?.SendAppearing();

		void FormsPresenter_Unloaded(object sender, RoutedEventArgs e)
			=> (DataContext as Page)?.SendDisappearing();
	}
}
