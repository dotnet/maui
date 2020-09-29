using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4748, "Setting SelectedItem property of GTK ListView does not reflected in UI", PlatformAffected.Default)]
	public class Issue4748 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout();
			var lastItem = "Item3";
			var listView = new ListView
			{
				ItemsSource = new List<string>
				{
					"Item1",
					"Item2",
					lastItem
				}
			};

			var button = new Button
			{
				Text = "Select last item",
				AutomationId = "SelectLastItem"
			};

			button.Clicked += (_, __) => listView.SelectedItem = lastItem;
			layout.Children.Add(listView);
			layout.Children.Add(button);

			Content = layout;
		}

	}
}