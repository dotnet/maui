using System;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class MultiWindowPage : BasePage
	{
		public MultiWindowPage()
		{
			InitializeComponent();
		}

		void OnNewWindowClicked(object sender, EventArgs e)
		{
			Application.Current.OpenWindow(new Window(new MultiWindowPage()));
		}
	}
}