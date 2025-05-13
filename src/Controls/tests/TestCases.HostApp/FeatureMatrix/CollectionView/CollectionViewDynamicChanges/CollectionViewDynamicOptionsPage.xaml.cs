namespace Maui.Controls.Sample;

public partial class CollectionViewDynamicOptionsPage : ContentPage
{
	private CollectionViewViewModel _viewModel;

	public CollectionViewDynamicOptionsPage(CollectionViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnEmptyViewChanged(object sender, CheckedChangedEventArgs e)
	{
		if (EmptyViewNone.IsChecked)
		{
			_viewModel.EmptyView = null;
		}
		else if (EmptyViewString.IsChecked)
		{
			_viewModel.EmptyView = "No Items Available(String)";
			_viewModel.IsEmptyViewStringSelected = true;
		}
		else if (EmptyViewGrid.IsChecked)
		{
			Grid grid = new Grid
			{
				BackgroundColor = Colors.LightGray,
				Padding = new Thickness(10),
			};
			grid.Children.Add(new Label
			{
				Text = "No Items Available(Grid View)",

				TextColor = Colors.Blue
			});
			_viewModel.EmptyView = grid;
			_viewModel.IsEmptyViewGridSelected = true;

		}
	}

	private void OnHeaderChanged(object sender, CheckedChangedEventArgs e)
	{
		if (HeaderNone.IsChecked)
		{
			_viewModel.Header = null;
		}
		else if (HeaderString.IsChecked)
		{
			_viewModel.Header = "Default Header String";
			_viewModel.IsHeaderStringSelected = true;
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
				Text = "Default Header Grid",
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Colors.Blue
			});
			_viewModel.Header = grid;
			_viewModel.IsHeaderGridSelected = true;
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
			_viewModel.Footer = "Default Footer String";
			_viewModel.IsFooterStringSelected = true;
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
				Text = "Default Footer Grid",
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Colors.Red
			});
			_viewModel.Footer = grid;
			_viewModel.IsFooterGridSelected = true;
		}
	}
	private void OnEmptyViewTemplateChanged(object sender, CheckedChangedEventArgs e)
	{
		if (EmptyViewTemplateNone.IsChecked)
		{
			_viewModel.EmptyViewTemplate = null;
		}
		else if (EmptyViewTemplateGrid.IsChecked)
		{
			_viewModel.EmptyViewTemplate = new DataTemplate(() =>
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10)
				};
				grid.Children.Add(new Label
				{
					Text = "No Template Items Available(Grid View)",
					TextColor = Colors.Blue
				});
				return grid;

			});
			_viewModel.IsEmptyViewTemplateSelected = true;
		}
	}

	private void OnHeaderTemplateChanged(object sender, CheckedChangedEventArgs e)
	{
		if (HeaderTemplateNone.IsChecked)
		{
			_viewModel.HeaderTemplate = null;
			_viewModel.IsHeaderTemplateViewSelected = false;

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
					Text = "Default HeaderTemplate",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Blue,
					AutomationId = "HeaderTemplateLabel"
				});
				return grid;

			});
			_viewModel.IsHeaderTemplateViewSelected = true;
		}
	}

	private void OnFooterTemplateChanged(object sender, CheckedChangedEventArgs e)
	{
		if (FooterTemplateNone.IsChecked)
		{
			_viewModel.FooterTemplate = null;
			_viewModel.IsFooterTemplateViewSelected = false;
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
					Text = "Default FooterTemplate",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Green,
					AutomationId = "FooterTemplateLabel"
				});
				return grid;
			});
			_viewModel.IsFooterTemplateViewSelected = true;
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
					Text = "Default GroupHeaderTemplate",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Green,
					AutomationId = "GroupHeaderTemplateLabel"
				});
				return grid;
			});
			_viewModel.IsGroupHeaderTemplateViewSelected = true;
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
					Text = "Default GroupFooterTemplate",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Red,
					AutomationId = "GroupFooterTemplateLabel"
				});
				return grid;
			});
			_viewModel.IsGroupFooterTemplateViewSelected = true;
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
			_viewModel.IsItemTemplateSelected = true;
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

	private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;
		if (radioButton == ItemsSourceObservableCollection5)
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
		else if (radioButton == ItemsSourceGroupedList)
			_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
		else if (radioButton == ItemsSourceNone)
			_viewModel.ItemsSourceType = ItemsSourceType.None;
	}
}
