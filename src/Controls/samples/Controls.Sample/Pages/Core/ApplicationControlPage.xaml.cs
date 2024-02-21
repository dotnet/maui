using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class ApplicationControlPage
	{
		public ApplicationControlPage()
		{
			InitializeComponent();
		}

		void OnTerminateClicked(object sender, EventArgs e)
		{
			Application.Current!.Quit();
		}
	}
}