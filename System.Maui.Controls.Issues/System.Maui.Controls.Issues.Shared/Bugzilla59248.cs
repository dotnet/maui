using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59248, "[UWP] ItemTapped event is not fired when keyboard Enter Pressed on ListView", PlatformAffected.UWP)]
	public class Bugzilla59248 : TestContentPage
	{
		protected override void Init()
		{
			var selectedItem = new Label { Text = "SelectedItem" };
			var list = new ListView
			{
				ItemsSource = new string[] { "A", "B", "C" },
				ItemTemplate = new DataTemplate(() =>
				{
					var view = new ViewCell();
					view.View = new StackLayout
					{
						Children =
						{
							new Label { Text = "Label" },
							new Button { Text = "Click for alert", Command = new Command(() => DisplayAlert("Clicked the button in the listview item", "Ok", "Cancel"))}
						}
					};
					return view;
				})
			};
			list.ItemTapped += List_ItemTapped;
			list.ItemSelected += (s, e) =>
			{
				selectedItem.Text = list.SelectedItem == null ? "None" : list.SelectedItem.ToString();
			};

			Content = new StackLayout
			{
				Children =
				{
					list,
					selectedItem
				}
			};
		}

		private void List_ItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e.Item != null)
				DisplayAlert("Tapped: " + e.Item, "Ok", "Cancel");
		}
	}
}