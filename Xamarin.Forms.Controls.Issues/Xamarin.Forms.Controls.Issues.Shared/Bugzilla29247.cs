using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29247, "iOS Device.OpenUri breaks with encoded params", PlatformAffected.iOS, issueTestNumber: 1)]
#if UITEST
	// this doesn't fail on Uwp but it leaves a browser window open and breaks later tests
	[Category(UITestCategories.UwpIgnore)]
#endif
	public class Bugzilla29247 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = "Welcome to Xamarin Forms!"
					},
					new Button {
						Text = "Without Params (Works)",
						AutomationId = "btnOpenUri1",
						Command = new Command (() => Device.OpenUri (new Uri ("http://www.bing.com")))
					}
				}
			};
		}

#if UITEST
		protected override bool Isolate => true;

		[Test]
		public void Bugzilla29247Test ()
		{
			RunningApp.WaitForElement(q => q.Marked("btnOpenUri1"));
			RunningApp.Tap (q => q.Marked ("btnOpenUri1"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29247, "iOS Device.OpenUri breaks with encoded params 2", PlatformAffected.iOS, issueTestNumber: 2)]
#if UITEST
	// This one isn't failing on UWP but it opens a browser window
	// and causes the rest to fail
	[Category(UITestCategories.UwpIgnore)]
#endif
	public class Bugzilla29247_2 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = "Welcome to Xamarin Forms!"
					},
					new Button {
						Text = "With encoded Params (Breaks)",
						AutomationId = "btnOpenUri2",
						Command = new Command (() => Device.OpenUri (new Uri ("http://www.bing.com/search?q=xamarin%20bombs%20on%20this")))
					}
				}
			};
		}

#if UITEST
		protected override bool Isolate => true;

		[Test]
		public void Bugzilla29247EncodedParamsTest ()
		{
			RunningApp.WaitForElement(q => q.Marked("btnOpenUri2"));
			RunningApp.Tap (q => q.Marked ("btnOpenUri2"));
		}

#endif
	}

#if UITEST
	// This one isn't failing on UWP but it opens a browser window
	// and causes the rest to fail
	[Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29247, "iOS Device.OpenUri breaks with encoded params 3", PlatformAffected.iOS, issueTestNumber: 3)]
	public class Bugzilla29247_3 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = "Welcome to Xamarin Forms!"
					},
					new Button {
						Text = "With decoded Params (Breaks)",
						AutomationId = "btnOpenUri3",
						Command = new Command (() => Device.OpenUri (new Uri ("http://www.bing.com/search?q=xamarin bombs on this")))
					}
				}
			};
		}

#if UITEST
		protected override bool Isolate => true;

		[Test]
		public void Bugzilla29247DecodeParamsTest ()
		{
			RunningApp.WaitForElement(q => q.Marked("btnOpenUri3"));
			RunningApp.Tap (q => q.Marked ("btnOpenUri3"));
		}
#endif
	}
}
