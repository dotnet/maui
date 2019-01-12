using System;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1538, "Crash measuring empty ScrollView", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1538 : TestContentPage
	{
		ScrollView _sv;

		protected override void Init()
		{
			StackLayout sl = new StackLayout(){VerticalOptions = LayoutOptions.FillAndExpand};
			sl.Children.Add( _sv = new ScrollView(){HeightRequest=100} );
			Content = sl;

			AddContentDelayed ();
		}

		async void AddContentDelayed ()
		{
			await Task.Delay (1000);
			_sv.Content = new Label { Text = "Foo" };
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ScrollView)]
		public void MeasuringEmptyScrollViewDoesNotCrash()
		{
			Task.Delay(1000).Wait();
			RunningApp.WaitForElement("Foo");
		}
#endif
	}
}
