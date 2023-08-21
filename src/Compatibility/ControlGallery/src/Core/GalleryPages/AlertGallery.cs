//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class AlertGallery : ContentPage
	{
		public AlertGallery()
		{
			var lblResult = new Label { Text = "Result: ", AutomationId = "testResult" };

			var btn1 = new Button { Text = "Alert Override1", AutomationId = "test1" };
			btn1.Clicked += async (sender, e) =>
			{
				await DisplayAlert("TheAlertTitle", "TheAlertMessage", "TheCancelButton");
				;
			};

			var btn2 = new Button { Text = "Alert Override2", AutomationId = "test2" };
			btn2.Clicked += async (sender, e) =>
			{
				var result = await DisplayAlert("TheAlertTitle", "TheAlertMessage", "TheAcceptButton", "TheCancelButton");
				lblResult.Text = string.Format("Result: {0}", result);
			};

			Content = new StackLayout { Children = { lblResult, btn1, btn2 } };
		}
	}
}


