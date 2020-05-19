using System;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public class AlertGallery : ContentPage
	{
		public AlertGallery ()
		{
			var lblResult = new Label { Text = "Result: ", AutomationId = "testResult" };

			var btn1 = new Button { Text = "Alert Override1", AutomationId = "test1" };
			btn1.Clicked += async (sender, e) => {
				await DisplayAlert ("TheAlertTitle", "TheAlertMessage", "TheCancelButton");;
			};

			var btn2 = new Button { Text = "Alert Override2", AutomationId = "test2" };
			btn2.Clicked += async (sender, e) => {
				var result = await DisplayAlert ("TheAlertTitle", "TheAlertMessage", "TheAcceptButton", "TheCancelButton");
				lblResult.Text = string.Format("Result: {0}", result);
			};

			Content =  new StackLayout {  Children = { lblResult, btn1, btn2 }};
		}
	}
}


