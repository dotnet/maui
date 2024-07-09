using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 9580, "[Bug] CollectionView - iOS - Crash when adding first item to empty item group",
		PlatformAffected.iOS)]
	public class Issue9580 : TestContentPage
	{
		const string Success = "Success";
		const string Test9580 = "9580";

		protected override void Init()
		{
			var layout = new StackLayout();

			var cv = new CollectionView
			{
				IsGrouped = true
			};

			var groups = new ObservableCollection<_9580Group>()
			{
				new _9580Group() { Name = "One" }, new _9580Group(){ Name = "Two" }, new _9580Group(){ Name = "Three" },
				new _9580Group() { Name = "Four" }, new _9580Group(){ Name = "Five" }, new _9580Group(){ Name = "Six" }
			};

			cv.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label() { Margin = new Thickness(5, 0, 0, 0) };
				label.SetBinding(Label.TextProperty, new Binding("Text"));
				return label;
			});

			cv.GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Name"));
				return label;
			});

			cv.ItemsSource = groups;

			var instructions = new Label { Text = $"Tap the '{Test9580}' button. The application doesn't crash, this test has passed." };

			var result = new Label { };

			var button = new Button { AutomationId = Test9580, Text = Test9580 };
			button.Clicked += (sender, args) =>
			{
				groups[0].Add(new _9580Item { Text = "An Item" });
				result.Text = Success;
			};

			layout.Children.Add(instructions);
			layout.Children.Add(result);
			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;
		}

		class _9580Item
		{
			public string Text { get; set; }
		}

		class _9580Group : ObservableCollection<_9580Item>
		{
			public string Name { get; set; }
		}
	}
}
