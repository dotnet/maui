using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 36681, "[A] NRE when Picker Replaces Page Content (pre-AppCompat only)", PlatformAffected.Android)]
	public class Bugzilla36681 : TestTabbedPage // or TestMasterDetailPage, etc ...
	{
		public class PickerPage : ContentPage
		{
			public Picker Picker { get; private set; }
			public Label Label { get; private set; }
			public PickerPage ()
			{
				Picker = new Picker { Title = "Select Item", AutomationId = "picker" };

				var items = new List<string> { "item", "item2", "item3", "item4" };
				foreach (var i in items)
					Picker.Items.Add (i);

				Picker.FocusChangeRequested += Picker_FocusChangeRequested;
				Picker.SelectedIndexChanged += Picker_SelectedIndexChanged;

				StackLayout stack = new StackLayout { Padding = 20 };
				stack.Children.Add (Picker);

				Content = stack;
			}

			void Picker_FocusChangeRequested (object sender, FocusRequestArgs e)
			{
				SwitchContent ();
			}

			void Picker_SelectedIndexChanged (object sender, EventArgs e)
			{
				SwitchContent ();
			}

			void SwitchContent ()
			{
				var x = Parent as TabbedPage;
				var y = x.CurrentPage as ContentPage;
				y.Content = new Label {
					Text = "Success!"
				};
				y.Padding = new Thickness (0, 20, 0, 0);
			}
		}

		protected override void Init ()
		{
			PickerPage pickerPage = new PickerPage { Title = "Picker Page" };
			Children.Add (pickerPage);
			Children.Add (new ContentPage { BackgroundColor = Color.Blue, Title = "Page 2" });
		}

#if UITEST && __ANDROID__
		[Test]
		public void Bugzilla36681Test ()
		{
				RunningApp.WaitForElement (q => q.Marked ("picker"));
				RunningApp.Tap ("picker");
				var ok = RunningApp.Query ("OK");
				if (ok.Length > 0 && ok[0].Id == "button1" ) { //only in pre-AppCompat; this is the culprit!
					// We check that the query has any results and that the first
					// result matched the id "button1" because the query reports a phantom OK button
					// on Android >= 6.0
					RunningApp.Tap ("OK");
				} else {
					RunningApp.WaitForElement (q => q.Marked ("item2"));
					RunningApp.Tap ("item2");
				}
				RunningApp.WaitForElement (q => q.Marked ("Success!"));
		}
#endif
	}
}
