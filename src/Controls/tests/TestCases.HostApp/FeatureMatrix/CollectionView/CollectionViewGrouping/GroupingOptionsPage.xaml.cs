using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Maui.Controls.Sample.CollectionViewGalleries;

namespace Maui.Controls.Sample
{
	public partial class GroupingOptionsPage : ContentPage
	{
		private CollectionViewViewModel _viewModel;
		public GroupingOptionsPage(CollectionViewViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}
		private void ApplyButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}

		private void OnHeaderChanged(object sender, CheckedChangedEventArgs e)
		{
			if (HeaderNone.IsChecked)
			{
				_viewModel.Header = null;
			}
			else if (HeaderString.IsChecked)
			{
				_viewModel.Header = "CollectionView Header(String)";
			}
			else if (HeaderGrid.IsChecked)
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10)
				};
				grid.Children.Add(new Label
				{
					Text = "CollectionView Header(Grid View)",
					FontSize = 18,

					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Blue
				});
				_viewModel.Header = grid;
			}
		}

		private void OnFooterChanged(object sender, CheckedChangedEventArgs e)
		{
			if (FooterNone.IsChecked)
			{
				_viewModel.Footer = null;
			}
			else if (FooterString.IsChecked)
			{
				_viewModel.Footer = "CollectionView Footer(String)";
			}
			else if (FooterGrid.IsChecked)
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10)
				};
				grid.Children.Add(new Label
				{
					Text = "CollectionView Footer(Grid View)",
					FontSize = 18,

					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Red
				});
				_viewModel.Footer = grid;
			}
		}

		private void OnHeaderTemplateChanged(object sender, CheckedChangedEventArgs e)
		{
			if (HeaderTemplateNone.IsChecked)
			{
				_viewModel.HeaderTemplate = null;
			}
			else if (HeaderTemplateGrid.IsChecked)
			{
				_viewModel.HeaderTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "Header Template(Grid View)",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Blue
					});
					return grid;
				});
			}
		}

		private void OnFooterTemplateChanged(object sender, CheckedChangedEventArgs e)
		{
			if (FooterTemplateNone.IsChecked)
			{
				_viewModel.FooterTemplate = null;
			}
			else if (FooterTemplateGrid.IsChecked)
			{
				_viewModel.FooterTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "Footer Template(Grid View)",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Green
					});
					return grid;
				});
			}
		}

		private void OnGroupHeaderTemplateChanged(object sender, CheckedChangedEventArgs e)
		{
			if (GroupHeaderTemplateNone.IsChecked)
			{
				_viewModel.GroupHeaderTemplate = null;
			}
			else if (GroupHeaderTemplateGrid.IsChecked)
			{
				_viewModel.GroupHeaderTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "Group Header Template(Grid View)",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Green
					});
					return grid;
				});
			}
		}

		private void OnGroupFooterTemplateChanged(object sender, CheckedChangedEventArgs e)
		{
			if (GroupFooterTemplateNone.IsChecked)
			{
				_viewModel.GroupFooterTemplate = null;
			}
			else if (GroupFooterTemplateGrid.IsChecked)
			{
				_viewModel.GroupFooterTemplate = new DataTemplate(() =>
				{
					Grid grid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};
					grid.Children.Add(new Label
					{
						Text = "Group Footer Template(Grid View)",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Red
					});
					return grid;
				});
			}
		}

		private void OnIsGroupedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (IsGroupedFalse.IsChecked)
			{
				_viewModel.IsGrouped = false;
			}
			else if (IsGroupedTrue.IsChecked)
			{
				_viewModel.IsGrouped = true;
			}
		}

		private void OnItemTemplateChanged(object sender, CheckedChangedEventArgs e)
		{
			if (ItemTemplateNone.IsChecked)
			{
				_viewModel.ItemTemplate = null;
			}
			else if (ItemTemplateBasic.IsChecked)
			{
				_viewModel.ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Caption"));
					return label;
				});
			}
			else if (ItemTemplateGrid.IsChecked)
			{
				_viewModel.ItemTemplate = ExampleTemplates.PhotoTemplate();
			}
			else if (ItemTemplateSelector.IsChecked)
			{
				// DataTemplateSelector
				var template1 = new DataTemplate(() =>
				{
					var label = new Label
					{
						Text = "Template 1",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Black,
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};

					return label;
				});

				var template2 = new DataTemplate(() =>
				{
					var label = new Label
					{
						Text = "Template 2",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						TextColor = Colors.Black,
						BackgroundColor = Colors.LightGray,
						Padding = new Thickness(10)
					};

					return label;
				});

				_viewModel.ItemTemplate = new CollectionViewViewModel.CustomDataTemplateSelector
				{
					Template1 = template1,
					Template2 = template2
				};
			}
		}

		private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!(sender is RadioButton radioButton) || !e.Value)
				return;
			if (radioButton == ItemsSourceList)
				_viewModel.ItemsSourceType = ItemsSourceType.ListT;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType = ItemsSourceType.None;
		}

		private void OnCanMixGroupsChanged(object sender, CheckedChangedEventArgs e)
		{
			if (CanMixGroupsTrue.IsChecked)
			{
				_viewModel.CanMixGroups = true;
			}
			else if (CanMixGroupsFalse.IsChecked)
			{
				_viewModel.CanMixGroups = false;
			}
		}

		private void OnCanReorderItemsChanged(object sender, CheckedChangedEventArgs e)
		{
			if (CanReorderItemsTrue.IsChecked)
			{
				_viewModel.CanReorderItems = true;
			}
			else if (CanReorderItemsFalse.IsChecked)
			{
				_viewModel.CanReorderItems = false;
			}
		}
	}
}