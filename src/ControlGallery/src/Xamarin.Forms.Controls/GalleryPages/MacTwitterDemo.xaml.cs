using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public partial class MacTwitterDemo : FlyoutPage
	{
		public MacTwitterDemo()
		{
			InitializeComponent();
			lstTweets.ItemsSource = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
		}
	}
}
