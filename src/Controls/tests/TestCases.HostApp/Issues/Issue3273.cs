using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3273, "Drag and drop reordering not firing CollectionChanged", PlatformAffected.UWP)]
	public class Issue3273 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		public class SortableListView : ListView
		{
		}

		protected override void Init()
		{
			var statusLabel = new Label();
			var actionLabel = new Label();
			var Items = new ObservableCollection<string>
			{
				"drag",
				"and",
				"drop",
				"me",
				"please"
			};
			BindingContext = Items;

			Items.CollectionChanged += (_, e) =>
			{
				statusLabel.Text = "Success";
				var log = $"<{DateTime.Now.ToString("T")}> {e.Action} action fired.";
				actionLabel.Text += $"{log}{Environment.NewLine}";
				System.Diagnostics.Debug.WriteLine(log);
			};
			Items.RemoveAt(4);
			Items.Move(0, 1);

			var listView = new SortableListView();
			listView.SetBinding(ListView.ItemsSourceProperty, ".");

			Content = new StackLayout
			{
				Children = {
					statusLabel,
					new Button {
						AutomationId = "Move items",
						Text = "Move items",
						Command = new Command(() =>
						{
							actionLabel.Text = string.Empty;
							statusLabel.Text = "Failed";
							Items.Move(0, 1);
						})
					},
					listView,
					new ListView(),
					actionLabel
				}
			};
		}
	}
}
