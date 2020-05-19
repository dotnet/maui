using System;
using System.Collections.Generic;

using System.Maui;

namespace System.Maui.Controls
{
	public partial class MacTwitterDemo : MasterDetailPage
	{
		public MacTwitterDemo()
		{
			InitializeComponent();
			lstTweets.ItemsSource = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
		}
	}
}
