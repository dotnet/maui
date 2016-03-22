using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 25662, "Setting IsEnabled does not disable SwitchCell")]
    public class Bugzilla25662 : ContentPage
    {
		class MySwitch : SwitchCell
		{
			public MySwitch ()
			{
				IsEnabled = false;
			}
		}


		public Bugzilla25662 ()
		{
			var list = new ListView {
				ItemsSource = new[] {
					"One", "Two", "Three"
				},
				ItemTemplate = new DataTemplate (typeof (MySwitch))
			};

			Content = list;
			Title = "My page";

		}
    }
}
