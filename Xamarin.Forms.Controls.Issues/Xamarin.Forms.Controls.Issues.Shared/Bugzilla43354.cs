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
	[Issue(IssueTracker.Bugzilla, 43354, "Button command being set after IsEnabled enables the button", PlatformAffected.All)]
	public class Bugzilla43354 : TestContentPage
	{
		protected override void Init()
		{
			var buttonIsEnabledSetFirst = new Button
			{
				Text = "Click to display an alert",
				IsEnabled = false,
				Command = new Command(() => DisplayAlert("Test", "Message", "Cancel")),
			};

			var buttonIsEnabledSetSecond = new Button
			{
				Text = "Click to enable/disable button",
				Command = new Command(() =>
				{
					if (buttonIsEnabledSetFirst.IsEnabled)
						buttonIsEnabledSetFirst.IsEnabled = false;
					else
						buttonIsEnabledSetFirst.IsEnabled = true;
				})
			};

			var buttonSetCommandToNull = new Button
			{
				Text = "Click to set first button's command to null",
				Command = new Command(() => buttonIsEnabledSetFirst.Command = null)
			};

			Content = Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
						buttonIsEnabledSetFirst,
						buttonIsEnabledSetSecond,
						buttonSetCommandToNull
				}
			};
		}
	}
}
