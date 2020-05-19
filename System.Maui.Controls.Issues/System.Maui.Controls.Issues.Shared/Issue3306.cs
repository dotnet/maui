using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
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
