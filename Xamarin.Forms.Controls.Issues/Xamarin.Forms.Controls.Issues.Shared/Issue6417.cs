using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6417, "NRE WPF/GTK ListView", PlatformAffected.WPF)]
	public class Issue6417 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			var listView = new ListView();
			listView.ItemsSource = new[] { "a", "b", "c" };
			listView.ItemTemplate = new DataTemplate(typeof(Issue6417ContextActionsCell));
			listView.ItemTapped += async (sender, e) =>
			{
				await DisplayAlert("Tapped", e.Item + " row was tapped", "OK");
				Debug.WriteLine("Tapped: " + e.Item);
				((ListView)sender).SelectedItem = null;
			};

			stack.Children.Add(listView);
			Content = stack;
		}
	}

	public class Issue6417ContextActionsCell : ViewCell
	{
		public Issue6417ContextActionsCell()
		{
			var label1 = new Label { Text = "Label 1" };
			label1.SetBinding(Label.TextProperty, new Binding("."));
			var hint = Device.RuntimePlatform == Device.iOS ? "Tip: swipe left for context action" : "Tip: long press for context action";
			var label2 = new Label { Text = hint };
			var grid = new Grid() { BackgroundColor = Color.Gray };
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.Children.Add(label1, 0, 0);
			grid.Children.Add(label2, 0, 1);

			var moreAction = new MenuItem { Text = "More" };
			moreAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			moreAction.Clicked += (sender, e) =>
			{
				var mi = ((MenuItem)sender);
				Debug.WriteLine("More Context Action clicked: " + mi.CommandParameter);
			};

			var deleteAction = new MenuItem { Text = "Delete", IsDestructive = true };
			deleteAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			deleteAction.Clicked += (sender, e) =>
			{
				var mi = ((MenuItem)sender);
				Debug.WriteLine("Delete Context Action clicked: " + mi.CommandParameter);
			};

			View = grid;

			ContextActions.Add(moreAction);
			ContextActions.Add(deleteAction);
		}
	}
}