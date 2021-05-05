using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12777, "[Bug] CarouselView NRE if item template is not specified",
		PlatformAffected.iOS)]
#if UITEST
	[Category(UITestCategories.Github10000)]
	[Category(UITestCategories.CarouselView)]
#endif
	public class Issue12777 : TestContentPage
	{
		public Issue12777()
		{
			BindingContext = new Issue12777ViewModel();
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = new Thickness(12),
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Without exceptions, the test has passed."
			};

			var carouselView = new CarouselView
			{
				AutomationId = "TestCarouselView"
			};

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue12777ViewModel.Items));

			layout.Children.Add(instructions);
			layout.Children.Add(carouselView);

			Content = layout;
		}

#if UITEST
		[Test]
		public void Issue12777Test()
		{
			RunningApp.WaitForElement("TestCarouselView");
			RunningApp.Screenshot("Test passed");
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class Issue12777Model
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue12777ViewModel : BindableObject
	{
		ObservableCollection<Issue12777Model> _items;

		public Issue12777ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<Issue12777Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		void LoadItems()
		{
			Items = new ObservableCollection<Issue12777Model>();

			var random = new Random();

			for (int n = 0; n < 5; n++)
			{
				Items.Add(new Issue12777Model
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}
		}
	}
}