using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Picker = Microsoft.Maui.Controls.Picker;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CarouselSnapGallery : ContentPage
	{
		public CarouselSnapGallery()
		{
			//On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);

			var viewModel = new CarouselItemsGalleryViewModel(false, false);

			Title = $"CarouselView Snap Options";

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var snapPointsStack = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var snapPointsLabel = new Label { FontSize = 10, Text = "SnapPointsType:" };
			var snapPointsTypes = Enum.GetNames(typeof(SnapPointsType)).Select(b => b).ToList();

			var snapPointsTypePicker = new Microsoft.Maui.Controls.Picker
			{
				ItemsSource = snapPointsTypes,
				SelectedItem = snapPointsTypes[1]
			};

			snapPointsStack.Children.Add(snapPointsLabel);
			snapPointsStack.Children.Add(snapPointsTypePicker);

			layout.Children.Add(snapPointsStack);

			var snapPointsAlignmentsStack = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var snapPointsAlignmentsLabel = new Label { FontSize = 10, Text = "SnapPointsAlignment:" };
			var snapPointsAlignments = Enum.GetNames(typeof(SnapPointsAlignment)).Select(b => b).ToList();

			var snapPointsAlignmentPicker = new Picker
			{
				ItemsSource = snapPointsAlignments,
				SelectedItem = snapPointsAlignments[0]
			};

			snapPointsAlignmentsStack.Children.Add(snapPointsAlignmentsLabel);
			snapPointsAlignmentsStack.Children.Add(snapPointsAlignmentPicker);

			Grid.SetRow(snapPointsAlignmentsStack, 1);
			layout.Children.Add(snapPointsAlignmentsStack);

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.Mandatory,
				SnapPointsAlignment = SnapPointsAlignment.Start
			};

			var itemTemplate = GetCarouselTemplate();

			var carouselView = new CarouselView
			{
				ItemsSource = viewModel.Items,
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				BackgroundColor = Colors.LightGray,
				PeekAreaInsets = new Thickness(0, 0, 100, 0),
				Margin = new Thickness(12),
				AutomationId = "TheCarouselView"
			};

			Grid.SetRow(carouselView, 2);
			layout.Children.Add(carouselView);


			snapPointsTypePicker.SelectedIndexChanged += (sender, e) =>
			{
				if (carouselView.ItemsLayout is LinearItemsLayout linearItemsLayout)
				{
					Enum.TryParse(snapPointsTypePicker.SelectedItem.ToString(), out SnapPointsType snapPointsType);
					linearItemsLayout.SnapPointsType = snapPointsType;
				}
			};

			snapPointsAlignmentPicker.SelectedIndexChanged += (sender, e) =>
			{
				if (carouselView.ItemsLayout is LinearItemsLayout linearItemsLayout)
				{
					Enum.TryParse(snapPointsAlignmentPicker.SelectedItem.ToString(), out SnapPointsAlignment snapPointsAlignment);
					linearItemsLayout.SnapPointsAlignment = snapPointsAlignment;
				}
			};

			Content = layout;
			BindingContext = viewModel;
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

				var frame = new Border
				{
					Content = grid
				};

				frame.SetBinding(BackgroundColorProperty, new Binding("Color"));

				return frame;
			});
		}
	}
}