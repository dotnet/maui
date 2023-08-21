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

using System.Diagnostics;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2292, "DatePicker not shown when .Focus() is called", PlatformAffected.WinPhone)]
	public class Issue2292 : ContentPage
	{
		public Issue2292()
		{
			var datePicker = new DatePicker();
			var datePickerBtn = new Button
			{
				Text = "Click me to call .Focus on DatePicker"
			};

			datePickerBtn.Clicked += (sender, args) =>
			{
				datePicker.Focus();
			};

			var datePickerBtn2 = new Button
			{
				Text = "Click me to call .Unfocus on DatePicker"
			};

			datePickerBtn2.Clicked += (sender, args) =>
			{
				datePicker.Unfocus();
			};

			Content = new StackLayout
			{
				Children = {
					datePickerBtn,
					datePickerBtn2,
					datePicker,
				}
			};
		}
	}
}
