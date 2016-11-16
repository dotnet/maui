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
	[Issue (IssueTracker.Bugzilla, 28575, "listview header set to null")]
	public class Bugzilla28575 : TestContentPage
	{
		readonly string _header = "Hello I am Header!!!!";

		protected override void Init ()
		{
			var listview = new ListView ();
			listview.Header = new Label () {
				Text = _header,
				TextColor = Color.Red,
#pragma warning disable 618
				XAlign = TextAlignment.Center
#pragma warning restore 618
			};

			var b = new Button () {
				Text = "Click",
				AutomationId = "btnClick"
					
			};
			b.Clicked += (sender, e) => listview.Header = null;	

			Content = new StackLayout {
				Children = {
					b,
					listview
				}
			};
		}

		#if UITEST
		[Test]
		public void Bugzilla28575Test ()
		{
			RunningApp.Screenshot ("I am at Bugzilla28575Test ");
			RunningApp.WaitForElement (q => q.Marked (_header));
			RunningApp.Tap (q => q.Marked ("Click"));
			RunningApp.WaitForNoElement (q => q.Marked (_header));
		}
		#endif
	}
}
