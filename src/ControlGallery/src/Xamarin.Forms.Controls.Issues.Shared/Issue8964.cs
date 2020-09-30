using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8964, "Adding an item to the beginning of the bound ItemSource causes the carousel to skip sometimes", PlatformAffected.Android)]
	public class Issue8964 : TestContentPage // or TestFlyoutPage, etc ...
	{
		object _currentItem;
		int _counter;
		Label _lbl;

		protected override void Init()
		{
			ItemSourceUnderTest = new ObservableCollection<ModelIssue8964>(GetCarouselItems());
			var lbl = new Label
			{
				Text = "Scroll to the previous item until see the Item with counter 6, since we are inserting items on the start of the collection the position should be  the same"
			};
			CarouselViewUnderTest = new CarouselView
			{
				HeightRequest = 250,
				IsSwipeEnabled = true,
				ItemsSource = ItemSourceUnderTest,
				ItemTemplate = GetCarouselTemplate(),
				CurrentItem = _currentItem,
				AutomationId = "carouseView",
				Loop = false
			};
			CarouselViewUnderTest.CurrentItemChanged += CarouselViewUnderTestCurrentItemChanged;
			CarouselViewUnderTest.PositionChanged += CarouselViewUnderTest_PositionChanged;

			_lbl = new Label
			{
				Text = $"Item Position - {CarouselViewUnderTest.Position}"
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = { lbl, CarouselViewUnderTest, _lbl, }
			};
		}

		public ObservableCollection<ModelIssue8964> ItemSourceUnderTest { get; set; }
		public CarouselView CarouselViewUnderTest { get; set; }

		void CarouselViewUnderTest_PositionChanged(object sender, PositionChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"PositionChanged {CarouselViewUnderTest.Position}");
			_lbl.Text = $"Item Position - {e.CurrentPosition}";
		}

		void CarouselViewUnderTestCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"CurrentItemChanged {CarouselViewUnderTest.Position}");
			_counter++;
			ItemSourceUnderTest.Insert(0, new ModelIssue8964 { Name = $"Counter {_counter}", Color = Color.Red, Index = _counter });
		}

		DataTemplate GetCarouselTemplate()
		{
			return new DataTemplate(() =>
			{
				var grid = new Grid();

				var info = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(6)
				};

				info.SetBinding(Label.TextProperty, new Binding("Name"));

				grid.Children.Add(info);

				var frame = new Frame
				{
					CornerRadius = 12,
					Content = grid,
					HasShadow = false,
					Margin = new Thickness(12)
				};

				frame.SetBinding(BackgroundColorProperty, new Binding("Color"));

				return frame;
			});
		}

		List<ModelIssue8964> GetCarouselItems()
		{
			var random = new Random();

			var items = new List<ModelIssue8964>();

			for (int n = 0; n < 5; n++)
			{
				items.Add(new ModelIssue8964
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = DateTime.Now.AddDays(n).ToString("D"),
					Index = _counter
				});
				_counter++;
			}

			_currentItem = items[4];

			return items;
		}

#if UITEST
		[Test]
		public void Issue8964Test()
		{
			RunningApp.WaitForElement(q => q.Marked($"Item Position - 4"));
			var rect = RunningApp.Query("carouseView")[0].Rect;
			SwipePreviousItem(rect);
			RunningApp.WaitForElement(q => q.Marked($"Item Position - 4"));
			SwipePreviousItem(rect);
			RunningApp.WaitForElement(q => q.Marked($"Item Position - 4"));
			SwipePreviousItem(rect);
			RunningApp.WaitForElement(q => q.Marked($"Item Position - 4"));
			SwipePreviousItem(rect);
			RunningApp.WaitForElement(q => q.Marked($"Item Position - 4"));
			SwipePreviousItem(rect);
			RunningApp.WaitForElement(q => q.Marked($"Item Position - 4"));
			RunningApp.WaitForElement(q => q.Marked($"Counter 6"));

		}

		void SwipePreviousItem(UITest.Queries.AppRect rect)
		{
#if __ANDROID__
			RunningApp.DragCoordinates(rect.X + 10, rect.Y, rect.X + rect.Width - 10, rect.Y);
#else
			RunningApp.SwipeLeftToRight("carouseView");
#endif
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ViewModelIssue8964
	{
		public ViewModelIssue8964()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class ModelIssue8964
	{
		public Color Color { get; set; }
		public string Name { get; set; }
		public int Index { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}