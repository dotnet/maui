using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Threading.Tasks;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43469, "Calling DisplayAlert twice in WinRT causes a crash", PlatformAffected.WinRT)]
	public class Bugzilla43469 : TestContentPage
	{
		protected override void Init()
		{
			var button = new Button { Text = "Click to call DisplayAlert three times" };

			button.Clicked += (sender, args) =>
			{
				Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					await DisplayAlert("First", "Text", "Cancel");
				}));

				Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					await DisplayAlert("Second", "Text", "Cancel");
				}));

				Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					await DisplayAlert("Three", "Text", "Cancel");
				}));
			};

			Content = button;
		}
	}
}