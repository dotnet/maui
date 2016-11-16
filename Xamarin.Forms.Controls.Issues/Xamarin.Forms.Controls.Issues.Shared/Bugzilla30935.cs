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
	[Issue (IssueTracker.Bugzilla, 30935, "NullReferenceException in ViewRenderer<TView, TNativeView> (Xamarin.Forms.Platform.Android)")]
	public class Bugzilla30935 : TestContentPage
	{
		Entry _entry;
		protected override void Init ()
		{
			_entry = new Entry { AutomationId = "entry" };
			// Initialize ui here instead of ctor
			Content = new StackLayout { Children = { new Label {
						AutomationId = "IssuePageLabel",
						Text = "See if I'm here"
					},_entry
				}
			};
		}

		protected override void OnAppearing ()
		{
			_entry.Focus ();
			Content = null;
			base.OnAppearing ();
		}

#if UITEST
		[Test]
		public void Bugzilla30935DoesntThrowException ()
		{
			RunningApp.WaitForNoElement (q => q.Marked ("IssuePageLabel"));
			RunningApp.WaitForNoElement (q => q.Marked ("entry"));
		}
#endif
	}
}
