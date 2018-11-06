using System;

using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue (IssueTracker.Bugzilla, 27085, "EntryCell has no possibility to hide keyboard on iOS")]
	public class Bugzilla27085 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			var tableview = new TableView ();
			
			var section = new TableSection ("Settings");
			section.Add (new TextCell { Text = "TextCell" });
			section.Add (new TextCell { Text = "TextCell" });
			section.Add (new EntryCell { Text = "EntryCell", Keyboard = Keyboard.Numeric });
			section.Add (new EntryCell { Text = "EntryCell", Keyboard = Keyboard.Numeric });
			var root = new TableRoot ("Main");
			root.Add (section);
			
			tableview.Root = root;
				
			Content = tableview;
		}
	}
}
