using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7780, "[Bug] CarouselView doesn't support RTL layout", PlatformAffected.iOS)]
	public class Issue7780 : TestContentPage
	{
		public Issue7780()
		{
#if APP
			Title = "Issue 7780";
			BindingContext = new Issue7780ViewModel();
#endif
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Margin = new Thickness(6),
				Text = "Change the CarouselView FlowDirection and verify that it works correctly."
			};

			var itemsLayout =
				new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					SnapPointsType = SnapPointsType.MandatorySingle,
					SnapPointsAlignment = SnapPointsAlignment.Center
				};

			var carouselView = new CarouselView
			{
				FlowDirection = FlowDirection.RightToLeft,
				ItemsLayout = itemsLayout,
				ItemTemplate = GetCarouselTemplate()
			};

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			var flowDirectionButton = new Button
			{
				Text = "Change FlowDirection"
			};

			var flowDirectionLabel = new Label
			{
				Text = carouselView.FlowDirection.ToString()
			};

			layout.Children.Add(instructions);
			layout.Children.Add(carouselView);
			layout.Children.Add(flowDirectionButton);
			layout.Children.Add(flowDirectionLabel);

			flowDirectionButton.Clicked += (sender, args) =>
			{
				if (carouselView.FlowDirection == FlowDirection.RightToLeft)
				{
					carouselView.FlowDirection = FlowDirection.LeftToRight;
					flowDirectionLabel.Text = "LeftToRight";
				}
				else
				{
					carouselView.FlowDirection = FlowDirection.RightToLeft;
					flowDirectionLabel.Text = "RightToLeft";
				}
			};

			Content = layout;
		}

		internal DataTemplate GetCarouselTemplate()
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
					Content = grid,
					HasShadow = false
				};

				frame.SetBinding(BackgroundColorProperty, new Binding("Color"));

				return frame;
			});
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7780Model
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7780ViewModel : BindableObject
	{
		ObservableCollection<Issue7780Model> _items;

		public Issue7780ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<Issue7780Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public void LoadItems()
		{
			Items = new ObservableCollection<Issue7780Model>();

			var random = new Random();

			for (int n = 0; n < 5; n++)
			{
				Items.Add(new Issue7780Model
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}
		}
	}
}
