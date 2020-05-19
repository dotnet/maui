using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1801, "[Enhancement] Add SelectionMode property to ListView", PlatformAffected.All)]
	public class Issue1801
		: TestContentPage
	{
		protected override void Init()
		{
			var list = new ListView { SelectionMode = ListViewSelectionMode.Single };
//			var list = new ListView { SelectionMode = ListViewSelectionMode.None };
			list.ItemsSource = new [] { "A1", "A2", "A3", "A4" };
			var label = new Label { Text = "SelectionMode == " + list.SelectionMode };
			var toggle = new Switch { IsToggled = list.SelectionMode == ListViewSelectionMode.Single };
			var log = new Editor { HeightRequest = 200 };

			var stackLayout = new StackLayout();
			stackLayout.Children.Add(label);
			stackLayout.Children.Add(list);
			stackLayout.Children.Add(toggle);
			stackLayout.Children.Add(new Label { Text = "Event log" });
			stackLayout.Children.Add(log);

			toggle.Toggled += (_, b) =>
			{
				list.SelectionMode = b.Value ? ListViewSelectionMode.Single : ListViewSelectionMode.None;
				label.Text = "SelectionMode == " + list.SelectionMode;
			};

			Func<ListView, object, string, object> logEvent = (_, item, e) =>
			{
				var line = $"Item '{item}' {e}. SelectedItem = '{list.SelectedItem}'";
				if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.Tizen)
					// Android/Tizen scroll to show the last line so append to the log to make sure this line is visible
					log.Text += line + "\n";
				else
					// iOS/UWP don't scroll to show the last line so prepend instead to make sure this line is visible
					log.Text = line + "\n" + log.Text;
				return null;
			};
			EventHandler<SelectedItemChangedEventArgs> itemSelected = (l, e) => logEvent((ListView)l, e.SelectedItem, "selected");
			EventHandler<ItemTappedEventArgs> itemTapped = (l, e) => logEvent((ListView)l, e.Item, "tapped");

			list.ItemSelected += itemSelected;
			list.ItemTapped += itemTapped;

			stackLayout.Padding = new Thickness(0, 20, 0, 0);
			Content = stackLayout;
		}
	}
}
