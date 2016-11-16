using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 29247, "iOS Device.OpenUri breaks with encoded params", PlatformAffected.iOS )]
	public class Bugzilla29247 : TestContentPage
	{
		protected override void Init ()
		{
			Content = new StackLayout {
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
					},
					new Button {
						Text = "With encoded Params (Breaks)",
						AutomationId = "btnOpenUri2",
						Command = new Command (() => Device.OpenUri (new Uri ("http://www.bing.com/search?q=xamarin%20bombs%20on%20this")))
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
		[Test]
		[Ignore("Fails on ios 7.1")]
		public void Bugzilla29247Test ()
		{
			RunningApp.Tap (q => q.Marked ("btnOpenUri1"));
		}

		[Test]
		[Ignore("Fails on ios 7.1")]
		public void Bugzilla29247EncodedParamsTest ()
		{
			RunningApp.Tap (q => q.Marked ("btnOpenUri2"));
		}

		[Test]
		[Ignore("Fails on ios 7.1")]
		public void Bugzilla29247DecodeParamsTest ()
		{
			RunningApp.Tap (q => q.Marked ("btnOpenUri3"));
		}
		#endif
	}
}
