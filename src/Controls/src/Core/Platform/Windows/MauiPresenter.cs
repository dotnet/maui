using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MauiPresenter : Microsoft.UI.Xaml.Controls.ContentPresenter
	{
		public MauiPresenter()
		{
			Loaded += FormsPresenter_Loaded;
			Unloaded += FormsPresenter_Unloaded;
			SizeChanged += (s, e) =>
			{
				if (ActualWidth > 0 && ActualHeight > 0 && DataContext != null)
				{
					var page = (Page)DataContext;
					((Page)page.RealParent).ContainerArea = new Rect(0, 0, ActualWidth, ActualHeight);
				}
			};
		}

		void FormsPresenter_Loaded(object sender, RoutedEventArgs e)
			=> (DataContext as Page)?.SendAppearing();

		void FormsPresenter_Unloaded(object sender, RoutedEventArgs e)
			=> (DataContext as Page)?.SendDisappearing();
	}
}
