using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 3273, "Drag and drop reordering not firing CollectionChanged", PlatformAffected.UWP)]
	public class Issue3273 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		public class SortableListView : ListView
		{
		}

		protected override void Init ()
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
				var log = $"<{DateTime.Now.ToLongTimeString()}> {e.Action} action fired.";
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

#if UITEST
		[Test]
		public void Issue3273Test()
		{
			RunningApp.WaitForElement("Move items");
			RunningApp.Tap("Move items");
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
