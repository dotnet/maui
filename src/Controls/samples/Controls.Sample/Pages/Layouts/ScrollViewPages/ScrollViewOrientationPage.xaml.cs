using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.ScrollViewPages
{
	public partial class ScrollViewOrientationPage
	{
		public ScrollViewOrientationPage()
		{
			InitializeComponent();
		}

		public void OrientationSelectedIndexChanged(object sender, EventArgs args)
		{
			ScrollViewer.Orientation = (ScrollOrientation)Orientation.SelectedIndex;
		}

		protected override void OnAppearing()
		{
			Orientation.SelectedIndex = 0;
			base.OnAppearing();
		}
	}
}