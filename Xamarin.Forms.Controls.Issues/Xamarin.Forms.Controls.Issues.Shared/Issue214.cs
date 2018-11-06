using System;
using System.Linq;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 214, "TextCell DetailColor change not immediate", PlatformAffected.iOS)]
	public class Issue214 : ContentPage
	{
		public Issue214 ()
		{
			var items = Enumerable.Range (0, 50).Select (i => new TextCell {
				Text = i.ToString (),
				Detail = i.ToString ()
			}).ToList ();

			var tableSection = new TableSection("First Section");
			foreach (TextCell cell in items) {
				tableSection.Add (cell);
			}

			var tableRoot = new TableRoot () {
				tableSection
			};

			var tableLayout = new TableView {
				Root = tableRoot
			};

			tableLayout.Intent = TableIntent.Data;
			Content = tableLayout;
		}
	}
}
