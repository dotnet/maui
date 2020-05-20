using System;
using System.Maui.Core.Controls;
using System.Windows;

namespace System.Maui.Platform {
	public partial class ButtonRenderer
	{
		protected override MauiButton CreateView()
		{
			var control = new MauiButton();
			control.Click += HandleButtonClick;
			return control;
		}

		protected override void DisposeView(MauiButton nativeView)
		{
			nativeView.Click -= HandleButtonClick;
			base.DisposeView(nativeView);
		}

		void HandleButtonClick(object sender, RoutedEventArgs e)
		{
			VirtualView.Clicked();
		}
	}
}
