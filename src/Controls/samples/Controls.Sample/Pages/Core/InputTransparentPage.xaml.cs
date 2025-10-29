using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class InputTransparentPage
	{
		public InputTransparentPage()
		{
			InitializeComponent();
		}

		void ClickFail(object sender, EventArgs e)
		{
			Debug.WriteLine("Failure; You shouldn't have been able to interact with that.");
			DisplayAlertAsync("Failure", "You shouldn't have been able to interact with that.", "OK");
		}

		void ClickSuccess(object sender, EventArgs e)
		{
			Debug.WriteLine("Success; That should have worked, and it did!");
			DisplayAlertAsync("Success", "That should have worked, and it did!", "OK");
		}
	}
}