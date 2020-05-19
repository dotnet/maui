using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 52700, "[iOS] Recycled cell should respect selection style set to none", PlatformAffected.iOS)]
	public class Bugzilla52700 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		const string Instructions = "On iOS, all three of the following ListViews should not change background color upon selection. If the background of the row changes color, this test fails.";
		const string ListView1 = "Custom Cell with Context Actions";
		const string ListView2 = "Custom Cell + RecycleElement";
		const string ListView3 = "Custom Cell + RetainElement";

		public class NoSelectionViewCell : ViewCell
		{
			public Label label { get; set; }

			public NoSelectionViewCell ()
			{
				label = new Label ();
				label.SetBinding (Label.TextProperty, ".");
				View = label;
			}
		}

		public class NoSelectionViewCellWithContextActions : NoSelectionViewCell
		{
			public NoSelectionViewCellWithContextActions ()
			{
				label = new Label ();
				label.SetBinding (Label.TextProperty, ".");
				View = label;

				var delete = new MenuItem { Text = "Delete" };
				ContextActions.Add (delete);
			}
		}

		protected override void Init ()
		{
			var label = new Label { Text = Instructions };
			var selectionLabel = new Label { Text = "<< THIS changes when row selected >>" };
			var listView = new ListView { ItemTemplate = new DataTemplate (typeof (NoSelectionViewCellWithContextActions)), ItemsSource = Enumerable.Range (0, 9), Header = ListView1 };
			var listView2 = new ListView (ListViewCachingStrategy.RecycleElement) { ItemTemplate = new DataTemplate (typeof (NoSelectionViewCell)), ItemsSource = Enumerable.Range (10, 19), Header = ListView2 };
			var listView3 = new ListView { ItemTemplate = new DataTemplate (typeof (NoSelectionViewCell)), ItemsSource = Enumerable.Range (20, 29), Header = ListView3 };

			listView.ItemSelected += (sender, e) => {
				selectionLabel.Text = DateTime.Now.ToLocalTime ().ToString ();
			};

			listView2.ItemSelected += (sender, e) => {
				selectionLabel.Text = DateTime.Now.ToLocalTime ().ToString ();
			};

			listView3.ItemSelected += (sender, e) => {
				selectionLabel.Text = DateTime.Now.ToLocalTime ().ToString ();
			};

			Content = new StackLayout { Children = { label, selectionLabel, listView, listView2, listView3 } };
		}
	}
}
