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
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3306, "[WPF] TimePicker with short time pattern shows verbose time", PlatformAffected.WPF)]
	public class Issue3306 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout();

			TimePicker timePicker = new TimePicker();
			timePicker.Time = DateTime.Now.TimeOfDay;

			TimePicker timePicker2 = new TimePicker();
			timePicker2.Time = DateTime.Now.TimeOfDay;
			timePicker2.Format = "t";

			stack.Children.Add(timePicker);
			stack.Children.Add(timePicker2);
			Content = stack;
		}
	}
}
