using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59925, "Font size does not change vertical height of Entry on iOS", PlatformAffected.Default)]
	public class Bugzilla59925 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		const int Delta = 1;
		Entry _entry;

		private void ChangeFontSize(int delta) 
		{
			_entry.FontSize += delta;
		}

		protected override void Init()
		{
			_entry = new Entry 
			{
				Text = "Hello World!"
			};

			var buttonBigger = new Button 
			{
				Text = "Bigger",
			};
			buttonBigger.Clicked += (x, o) => ChangeFontSize(Delta);

			var buttonSmaller = new Button 
			{
				Text = "Smaller"
			};
			buttonSmaller.Clicked += (x, o) => ChangeFontSize(-Delta);

			var stack = new StackLayout 
			{
				Children = {
					buttonBigger,
					buttonSmaller,
					_entry
				}
			};

			// Initialize ui here instead of ctor
			Content = stack;
		}

#if UITEST
		[Test]
		public void Issue123456Test ()
		{
			RunningApp.Screenshot ("I am at Issue 59925");
			RunningApp.WaitForElement (q => q.Marked ("Bigger"));
			RunningApp.Screenshot ("0");

			RunningApp.Tap ("Bigger");
			RunningApp.Screenshot("1");

			RunningApp.Tap ("Bigger");
			RunningApp.Screenshot("2");

			RunningApp.Tap ("Bigger");
			RunningApp.Screenshot("3");
		}
#endif
	}
}