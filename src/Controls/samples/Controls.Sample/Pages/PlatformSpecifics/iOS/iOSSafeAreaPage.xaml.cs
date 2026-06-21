using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSSafeAreaPage : ContentPage
	{
		public iOSSafeAreaPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			this.SafeAreaEdges = Microsoft.Maui.SafeAreaEdges.None;
			(sender as Button)!.IsEnabled = false;
		}
	}
}
