using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 58645, "[iOS] NRE Thrown When ListView Items Are Replaced By Items With a Different Template", PlatformAffected.iOS)]
	public class Bugzilla58645 : TestContentPage
	{
		const string ButtonId = "button";
		ObservableCollection<string> Items { get; set; }

		protected override void Init()
		{
			Items = new ObservableCollection<string> { "Item 1A", "Item 2A", "Item 3A" };

			var myListView = new ListView
			{
				HasUnevenRows = true,
				ItemsSource = Items,
				ItemTemplate = new LayoutTemplateSelector
				{
					LayoutA = new DataTemplate(typeof(LayoutA)),
					LayoutB = new DataTemplate(typeof(LayoutB))
				}
			};

			var switchBtn = new Button
			{
				Text = "Switch Items",
				AutomationId = ButtonId,
				Command = new Command(() =>
				{
					Items.Clear();
					Items.Add("Item 1B");
				})
			};

			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "Tap the 'Switch Items' button. If the app does not crash, this test has passed." },
					switchBtn,
					myListView
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class LayoutA : ViewCell
		{
			public LayoutA()
			{
				var layout = new Grid
				{
					Padding = new Thickness(14),
					ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
				},
					RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto }
				}
				};

				var mainLabel = new Label();
				mainLabel.SetBinding(Label.TextProperty, ".");

				var sw = new Switch();
				layout.Children.Add(mainLabel, 0, 0);
				layout.Children.Add(sw, 2, 0);
				View = layout;
			}
		}

		[Preserve(AllMembers = true)]
		public class LayoutB : ViewCell
		{
			public LayoutB()
			{
				var layout = new Grid
				{
					Padding = new Thickness(14),
					ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
				},
					RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto }
				}
				};

				var mainLabel = new Label();
				mainLabel.SetBinding(Label.TextProperty, ".");

				var secondLabel = new Label
				{
					Text = "B"
				};

				layout.Children.Add(mainLabel, 0, 0);
				layout.Children.Add(secondLabel, 2, 0);
				View = layout;
			}
		}

		[Preserve(AllMembers = true)]
		public class LayoutTemplateSelector : DataTemplateSelector
		{
			public DataTemplate LayoutA { get; set; }
			public DataTemplate LayoutB { get; set; }

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				if (item == null)
					return LayoutA;

				var text = (string)item;
				return text.Contains("A") ? LayoutA : LayoutB;
			}
		}

#if UITEST
		[Test]
		public void Bugzilla58645Test()
		{
			RunningApp.WaitForElement(q => q.Marked(ButtonId));
			RunningApp.Tap(q => q.Marked(ButtonId));
		}
#endif
	}
}