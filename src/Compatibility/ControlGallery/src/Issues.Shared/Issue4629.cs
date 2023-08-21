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

using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4629, "Picker/DatePicker/TimePicker on iOS iPad should NOT have word suggestions", PlatformAffected.iOS)]
	public class Issue4629 : ContentPage
	{
		public Issue4629()
		{
			var picker = new Picker()
			{
				ItemsSource = new List<string>()
				{
					"Apple",
					"Banana",
					"Peach"
				},
				HorizontalOptions = LayoutOptions.FillAndExpand
			};
			var datePicker = new DatePicker();
			var timePicker = new TimePicker();
			Content = new StackLayout()
			{
				Children = { picker, datePicker, timePicker },
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
		}
	}
}
