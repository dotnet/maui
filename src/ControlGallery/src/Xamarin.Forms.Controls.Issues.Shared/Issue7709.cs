using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7709, " On android, not showing keyboard when changing placeholder in entry focused event", PlatformAffected.Android)]
	public class Issue7709 : TestContentPage
	{
		public Issue7709()
		{
			var stack = new StackLayout();

			var entry = new Entry();
			entry.Placeholder = "Before focus";
			entry.Focused += Entry_Focused;

			stack.Children.Add(entry);

			Content = stack;
		}

		void Entry_Focused(object sender, FocusEventArgs e)
		{
			var entry = sender as Entry;
			entry.Placeholder = "After focus";
		}

		protected override void Init()
		{

		}
	}
}
