using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 33870, "[W] Crash when the ListView Selection is set to null", PlatformAffected.WinRT)]
	public class Bugzilla33870 : TestContentPage
	{
		public class Section : ObservableCollection<string>
		{
			public Section (string title, IEnumerable<string> items = null)
				: this (items ?? new List<string> ())
			{
				Title = title;
			}

			public Section (IEnumerable<string> items)
				: base (items)
			{ }

			public string Title { get; set; }
		}

		protected override void Init ()
		{
			var source = new ObservableCollection<Section> {
				new Section("SECTION 1") {
					"ITEM 1",
					"ITEM 2",
				},
				new Section("SECTION 2") {
					"ITEM 3",
					"CLEAR SELECTION",
				}
			};

			var listview = new ListView {
				ItemsSource = source,
				IsGroupingEnabled = true,
				GroupDisplayBinding = new Binding ("Title"),
			};

			var label = new Label { Text = "Tap CLEAR SELECTION. If the app does not crash and no item is selected, the test has passed." };

			listview.ItemSelected += (sender, args) =>
			{
				string selecteditem = args.SelectedItem?.ToString ();
				label.Text = selecteditem;
				if (selecteditem == "CLEAR SELECTION") {
					label.Text = "cleared";
					((ListView) sender).SelectedItem = null;
				}
			};

			var stack = new StackLayout {
				Children = {
					label,
					listview
				}
			};

			Content = stack;
		}
	}
}
