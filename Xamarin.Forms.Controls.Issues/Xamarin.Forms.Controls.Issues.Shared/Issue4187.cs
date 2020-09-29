using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using System.Threading;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4187, "Picker list shows up, when focus is set on other controls", PlatformAffected.Android)]
	public class Issue4187 : TestCarouselPage
	{
		protected override void Init()
		{
			var items = new List<Issue4187Model>
			{
				new Issue4187Model
				{
					Label = "Label 1",
					PickerSource = new List<string> {"Flower", "Harvest", "Propagation", "Vegetation"},
					Text = "Text 1",
					Date = DateTime.Now
				},
				new Issue4187Model
				{
					Label = "Label 2",
					PickerSource = new List<string> {"1", "2", "3", "4"},
					Text = "Text 2",
					Date = DateTime.Now.AddDays(1)
				}
			};
			var listView = new ListView
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HasUnevenRows = true,
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() => GetViewCell())
			};

			var tableView = new TableView
			{
				BackgroundColor = Color.Wheat,
				HasUnevenRows = true,
				RowHeight = 100
			};
			tableView.Root = new TableRoot
			{
				new TableSection
				{
					GetViewCell(),
					GetViewCell(),
				}
			};
			tableView.BindingContext = new Issue4187Model
			{
				Label = "Label 1",
				PickerSource = new List<string> { "Flower", "Harvest", "Propagation", "Vegetation" },
				Text = "Text 1",
				Date = DateTime.Now
			};

			Children.Add(new ContentPage
			{
				Content = new StackLayout
				{
					Children = {
						GenerateNewPicker(),
						listView
					}
				}
			});

			Children.Add(new ContentPage
			{
				BackgroundColor = Color.Red,
				Content = new StackLayout
				{
					Children = { GenerateNewPicker(), tableView }
				}
			});

			Children.Add(new ContentPage
			{
				BackgroundColor = Color.Blue,
				Content = new StackLayout
				{
					Children = { GenerateNewPicker() }
				}
			});
		}

		static ViewCell GetViewCell()
		{
			var label = new Label { Text = "Status" };
			label.SetBinding(Label.TextProperty, new Binding(nameof(Issue4187Model.Label)));
			var picker = new Picker();
			picker.SetBinding(Picker.ItemsSourceProperty, new Binding(nameof(Issue4187Model.PickerSource)));

			var datePicker = new DatePicker();
			datePicker.SetBinding(DatePicker.DateProperty, new Binding(nameof(Issue4187Model.Date)));

			var entry = new Entry();
			entry.SetBinding(Entry.TextProperty, new Binding(nameof(Issue4187Model.Text)));

			return new ViewCell
			{
				View = new StackLayout
				{
					BackgroundColor = Color.Pink,
					Children = {
						label,
						picker,
						datePicker,
						new TimePicker(),
						entry
					}
				}
			};
		}

		Picker GenerateNewPicker()
		{
			var picker = new Picker() { ClassId = "PickerEditText" };
			for (int i = 1; i < 100; i++)
				picker.Items.Add($"item {i}");
			return picker;
		}

		[Preserve(AllMembers = true)]
		class Issue4187Model
		{
			public string Label { get; set; }
			public List<string> PickerSource { get; set; }
			public string Text { get; set; }
			public DateTime Date { get; set; }
		}

#if UITEST && __ANDROID__

		UITest.Queries.AppResult[] GetPickerEditText(UITest.IApp RunningApp) =>
			RunningApp.Query(q => q.TextField()).Where(x => x.Class.Contains("PickerEditText")).ToArray();

		[Test]
		public void Issue4187Test()
		{
			RunningApp.WaitForElement("Text 1");
			UITest.Queries.AppResult[] fields = RunningApp.Query(q => q.TextField());

			Assert.AreEqual(7, GetPickerEditText(RunningApp).Length, "picker count");
			TapOnPicker(1);
			Assert.IsTrue(DialogIsOpened(), "#1");
			RunningApp.Tap("Text 2");
			Assert.IsFalse(DialogIsOpened(), "#2");
			TapOnPicker(3);
			Assert.IsTrue(DialogIsOpened(), "#3");
			RunningApp.Tap("Text 1");
			Assert.IsFalse(DialogIsOpened(), "#5");

			// Carousel - first page
			RunningApp.Back();
			RunningApp.ScrollUp();
			TapOnPicker(0);
			Assert.IsTrue(DialogIsOpened(), "Carousel - #1");

			// Red page
			RunningApp.SwipeRightToLeft();
			Assert.IsFalse(DialogIsOpened(), "Carousel - #2");
			TapOnPicker(0);
			Assert.IsTrue(DialogIsOpened(), "Carousel - #3");

			// Blue page
			RunningApp.SwipeRightToLeft();
			Assert.IsFalse(DialogIsOpened(), "Carousel - #4");
			TapOnPicker(0);
			Assert.IsTrue(DialogIsOpened(), "Carousel - #5");
		}

		void TapOnPicker(int index)
		{
			var picker = GetPickerEditText(RunningApp)[index];
			var location = picker.Rect;
			RunningApp.TapCoordinates(location.X + 10, location.Y + location.Height / 2);
		}

		bool DialogIsOpened()
		{
			RunningApp.WaitForElement(q => q.Class("FrameLayout"));
			var frameLayouts = RunningApp.Query(q => q.Class("FrameLayout"));
			foreach (var layout in frameLayouts)
			{
				if (layout.Rect.X > 0 && layout.Rect.Y > 0 && layout.Description.Contains(@"id/content"))
				{
					// close dialog
					RunningApp.Back();
					Thread.Sleep(1500);
					return true;
				}
			}
			return false;
		}
#endif
	}
}