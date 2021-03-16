using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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
