using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6472, "[Bug][iOS] listview / observable collection throwing native error on load", PlatformAffected.iOS)]
	public class Issue6472 : TestContentPage
	{
		const string ListViewAutomationId = "TheListview";
		const string ClearButtonAutomationId = "ClearButton";
		const string UiThreadButtonAutomationId = "UiThreadButton";
		const string OtherThreadButtonAutomationId = "OtherThreadButton";
		const string AddItemsAddRangeButtonAutomationId = "AddItemsAddRangeButton";

		class testData
		{
			public int recordId { get; set; }
			public string recordText { get; set; }
		}

		static class staticData
		{
			public static ObservableCollection<testData> TestCollection = new ObservableCollection<testData>();
			public async static void testPopulate()
			{
				await Task.Run(() =>
				{
					TestCollection.Clear();
					for (int i = 0; i < 11; i++)
					{
						var tdn = new testData
						{
							recordId = i,
							recordText = i.ToString()
						};
						TestCollection.Add(tdn);
					}
				});
			}
		}

		protected override void Init()
		{
			var clearButton = new Button
			{
				Text = "Clear collection",
				AutomationId = ClearButtonAutomationId
			};

			clearButton.Clicked += (s, a) =>
			{
				staticData.TestCollection.Clear();
			};

			var buttonAddElementUiThreadButton = new Button
			{
				Text = "Add item from UI thread",
				AutomationId = UiThreadButtonAutomationId
			};

			buttonAddElementUiThreadButton.Clicked += (s, a) =>
			{
				staticData.TestCollection.Add(new testData
				{
					recordId = 1,
					recordText = "Just one"
				});

				staticData.TestCollection.Add(new testData
				{
					recordId = 2,
					recordText = "Just two"
				});

				staticData.TestCollection.Add(new testData
				{
					recordId = 3,
					recordText = "Just three"
				});
			};

			var buttonAddElementOtherThreadButton = new Button
			{
				Text = "Add item from other thread",
				AutomationId = OtherThreadButtonAutomationId
			};

			buttonAddElementOtherThreadButton.Clicked += (s, a) =>
			{
				Task.Run(() =>
				{
					staticData.TestCollection.Add(new testData
					{
						recordId = 42,
						recordText = "THE answer"
					});

					staticData.TestCollection.Add(new testData
					{
						recordId = 1337,
						recordText = "1337 HaxX0r"
					});
				});
			};

			var listView = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var labelAccount = new Label
					{
						Margin = new Thickness(10, 0),
						VerticalTextAlignment = TextAlignment.Center,
						HorizontalTextAlignment = TextAlignment.Start,
						LineBreakMode = LineBreakMode.NoWrap,
					};
					labelAccount.FontSize = 18;
					labelAccount.SetBinding(Label.TextProperty, "recordText");

#pragma warning disable CS0618 // Type or member is obsolete
					var stackAccountLayout = new StackLayout
					{
						Orientation = StackOrientation.Vertical,
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.StartAndExpand,
						Children = { labelAccount }
					};
#pragma warning restore CS0618 // Type or member is obsolete
					return new ViewCell { View = stackAccountLayout };
				}),
				AutomationId = ListViewAutomationId
			};

			var stack = new StackLayout();
			stack.Children.Add(buttonAddElementOtherThreadButton);
			stack.Children.Add(buttonAddElementUiThreadButton);
			stack.Children.Add(clearButton);
			stack.Children.Add(listView);


			Content = stack;
			listView.ItemsSource = staticData.TestCollection;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			staticData.testPopulate();
		}
	}
}
