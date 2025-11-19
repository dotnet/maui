using System;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Maui.Controls.Sample;

public partial class BindableLayoutControlPage : NavigationPage
{
	private BindableLayoutViewModel _viewModel;
	public BindableLayoutControlPage()
	{
		_viewModel = new BindableLayoutViewModel();
		PushAsync(new BindableLayoutControlMainPage(_viewModel));
	}
}

public partial class BindableLayoutControlMainPage : ContentPage
{
	private BindableLayoutViewModel _viewModel;

	private class SimpleAlternatingSelector : DataTemplateSelector
	{
		private readonly DataTemplate _evenTemplate;
		private readonly DataTemplate _oddTemplate;

		public SimpleAlternatingSelector(Color evenColor, Color oddColor)
		{
			_evenTemplate = new DataTemplate(() =>
			{
				var label = new Label { TextColor = evenColor, FontAttributes = FontAttributes.Bold };
				label.SetBinding(Label.TextProperty, "Caption");
				return label;
			});
			_oddTemplate = new DataTemplate(() =>
			{
				var label = new Label { TextColor = oddColor, FontAttributes = FontAttributes.Italic, FontSize = 12 };
				label.SetBinding(Label.TextProperty, new Binding("Caption", stringFormat: "Sel {0}"));
				return label;
			});
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is BindableLayoutTestItem b)
				return b.Index % 2 == 0 ? _evenTemplate : _oddTemplate;
			return _evenTemplate;
		}
	}

	public BindableLayoutControlMainPage(BindableLayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		InitializeDirectApiState();

	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new BindableLayoutViewModel();
		await Navigation.PushAsync(new BindableLayoutOptionsPage(_viewModel));
	}

	private void RemoveItems_Clicked(object sender, EventArgs e)
	{
		var text = IndexEntry.Text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			_viewModel.RemoveLastItem();
		}
		else if (int.TryParse(text, out int index))
		{
			_viewModel.RemoveItemAtIndex(index);
		}

		IndexEntry.Text = string.Empty;
	}

	private void AddItems_Clicked(object sender, EventArgs e)
	{
		var text = IndexEntry.Text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			_viewModel.AddSequentialItem();
		}
		else if (int.TryParse(text, out int index))
		{
			_viewModel.AddItemAtIndex(index);
		}

		IndexEntry.Text = string.Empty;
	}

	public void OnGridLoaded(object sender, EventArgs e)
	{
		if (sender is not Grid grid)
			return;

		grid.ChildAdded += (_, _) => ArrangeGridItems(grid);
		grid.ChildRemoved += (_, _) => ArrangeGridItems(grid);

		ArrangeGridItems(grid);
	}

	private void ArrangeGridItems(Grid grid)
	{
		const int columns = 2;

		var children = grid.Children.OfType<View>().ToList();

		grid.RowDefinitions.Clear();
		grid.ColumnDefinitions.Clear();

		for (int i = 0; i < columns; i++)
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

		int totalRows = (int)Math.Ceiling((double)children.Count / columns);
		for (int i = 0; i < totalRows; i++)
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		for (int index = 0; index < children.Count; index++)
		{
			int row = index / columns;
			int column = index % columns;
			Microsoft.Maui.Controls.Grid.SetRow(children[index], row);
			Microsoft.Maui.Controls.Grid.SetColumn(children[index], column);
		}
	}
	private void ReplaceItems_Clicked(object sender, EventArgs e)
	{
		var text = IndexEntry.Text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			_viewModel.ReplaceItem();
		}
		else if (int.TryParse(text, out int index))
		{
			_viewModel.ReplaceItemAtIndex(index);
		}
		IndexEntry.Text = string.Empty;
	}

	private bool _setItemsSource;
	private void InitializeDirectApiState()
	{
		_setItemsSource = false;
	}

	private void OnSetItemsSourceClicked(object sender, EventArgs e)
	{
		var updatedItems = new ObservableCollection<Label>();

		foreach (var item in _viewModel.ItemsSource as IEnumerable)
		{
			updatedItems.Add(new Label
			{
				Text = $"{item} A",
				FontSize = 11,
				TextColor = Colors.Red,
			});
		}

		BindableLayout.SetItemsSource(MainStackBindableLayout, updatedItems);
		BindableLayout.SetItemsSource(MainFlexBindableLayout, updatedItems);
		BindableLayout.SetItemsSource(MainGridBindableLayout, updatedItems);

		_setItemsSource = !_setItemsSource;
		UpdateDirectSummary("Set ItemsSource");
	}


	private void OnSetItemTemplateClicked(object sender, EventArgs e)
	{
		BindableLayout.SetItemTemplateSelector(MainStackBindableLayout, null);
		BindableLayout.SetItemTemplateSelector(MainFlexBindableLayout, null);
		BindableLayout.SetItemTemplateSelector(MainGridBindableLayout, null);
		var template = new DataTemplate(() =>
		{
			var grid = new Grid { Padding = 4, BackgroundColor = Colors.LightGray };
			var label = new Label { TextColor = Colors.Blue, FontAttributes = FontAttributes.Bold };
			label.SetBinding(Label.TextProperty, "Caption");
			grid.Children.Add(label);
			return grid;
		});
		BindableLayout.SetItemTemplate(MainStackBindableLayout, template);
		BindableLayout.SetItemTemplate(MainFlexBindableLayout, template);
		BindableLayout.SetItemTemplate(MainGridBindableLayout, template);
		UpdateDirectSummary("Set ItemTemplate");
	}

	private void OnSetItemTemplateSelectorClicked(object sender, EventArgs e)
	{
		BindableLayout.SetItemTemplateSelector(MainStackBindableLayout, null);
		BindableLayout.SetItemTemplateSelector(MainFlexBindableLayout, null);
		BindableLayout.SetItemTemplateSelector(MainGridBindableLayout, null);
		var selector = new SimpleAlternatingSelector(Colors.Purple, Colors.Green);
		BindableLayout.SetItemTemplateSelector(MainStackBindableLayout, selector);
		BindableLayout.SetItemTemplateSelector(MainFlexBindableLayout, selector);
		BindableLayout.SetItemTemplateSelector(MainGridBindableLayout, selector);
		UpdateDirectSummary("Set ItemTemplateSelector");
	}

	private void OnSetEmptyViewClicked(object sender, EventArgs e)
	{
		BindableLayout.SetEmptyViewTemplate(MainStackBindableLayout, null);
		BindableLayout.SetEmptyView(MainStackBindableLayout, new Label { Text = "(Set EmptyView)", HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Gray });
		UpdateDirectSummary("Set EmptyView");
	}

	private void OnSetEmptyViewTemplateClicked(object sender, EventArgs e)
	{
		BindableLayout.SetEmptyView(MainStackBindableLayout, null);
		BindableLayout.SetEmptyView(MainFlexBindableLayout, null);
		BindableLayout.SetEmptyView(MainGridBindableLayout, null);
		var tmpl = new DataTemplate(() => new Label { Text = "(Set EmptyViewTemplate)", HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Green });
		BindableLayout.SetEmptyViewTemplate(MainStackBindableLayout, tmpl);
		BindableLayout.SetEmptyViewTemplate(MainFlexBindableLayout, tmpl);
		BindableLayout.SetEmptyViewTemplate(MainGridBindableLayout, tmpl);
		UpdateDirectSummary("Set EmptyViewTemplate");
	}

	private void OnClearAllClicked(object sender, EventArgs e)
	{
		BindableLayout.SetItemsSource(MainStackBindableLayout, null);
		BindableLayout.SetItemTemplate(MainStackBindableLayout, null);
		BindableLayout.SetItemTemplateSelector(MainStackBindableLayout, null);
		BindableLayout.SetEmptyView(MainStackBindableLayout, null);
		BindableLayout.SetEmptyViewTemplate(MainStackBindableLayout, null);
		BindableLayout.SetItemsSource(MainFlexBindableLayout, null);
		BindableLayout.SetItemTemplate(MainFlexBindableLayout, null);
		BindableLayout.SetItemTemplateSelector(MainFlexBindableLayout, null);
		BindableLayout.SetEmptyView(MainFlexBindableLayout, null);
		BindableLayout.SetEmptyViewTemplate(MainFlexBindableLayout, null);
		BindableLayout.SetItemsSource(MainGridBindableLayout, null);
		BindableLayout.SetItemTemplate(MainGridBindableLayout, null);
		BindableLayout.SetItemTemplateSelector(MainGridBindableLayout, null);
		BindableLayout.SetEmptyView(MainGridBindableLayout, null);
		BindableLayout.SetEmptyViewTemplate(MainGridBindableLayout, null);
		InitializeDirectApiState();
		UpdateDirectSummary("Clear All");
	}

	private void OnGetEmptyViewClicked(object sender, EventArgs e)
	{
		var ev = BindableLayout.GetEmptyView(MainStackBindableLayout);
		BindableLayout.GetEmptyView(MainFlexBindableLayout);
		;
		BindableLayout.GetEmptyView(MainGridBindableLayout);
		UpdateDirectSummary("Get EmptyView", extra: $"HasEmptyView={(ev != null)}");
	}

	private void OnGetEmptyViewTemplateClicked(object sender, EventArgs e)
	{
		var evt = BindableLayout.GetEmptyViewTemplate(MainStackBindableLayout);
		BindableLayout.GetEmptyViewTemplate(MainFlexBindableLayout);
		BindableLayout.GetEmptyViewTemplate(MainGridBindableLayout);
		UpdateDirectSummary("Get EmptyViewTemplate", extra: $"HasEmptyViewTemplate={(evt != null)}");
	}

	private void OnGetItemsSourceClicked(object sender, EventArgs e)
	{
		var src = BindableLayout.GetItemsSource(MainStackBindableLayout) as IEnumerable;
		BindableLayout.GetItemsSource(MainFlexBindableLayout);
		BindableLayout.GetItemsSource(MainGridBindableLayout);
		int count = src == null ? 0 : src.Cast<object>().Count();
		UpdateDirectSummary("Get ItemsSource", extra: $"Count={count}");
	}

	private void OnGetItemTemplateClicked(object sender, EventArgs e)
	{
		var it = BindableLayout.GetItemTemplate(MainStackBindableLayout);
		UpdateDirectSummary("Get ItemTemplate", extra: $"HasItemTemplate={(it != null)}");
	}

	private void OnGetItemTemplateSelectorClicked(object sender, EventArgs e)
	{
		var sel = BindableLayout.GetItemTemplateSelector(MainStackBindableLayout);
		UpdateDirectSummary("Get ItemTemplateSelector", extra: $"HasSelector={(sel != null)}");
	}

	private void UpdateDirectSummary(string action, string extra = null)
	{
		var ev = BindableLayout.GetEmptyView(MainStackBindableLayout);
		var evt = BindableLayout.GetEmptyViewTemplate(MainStackBindableLayout);
		var src = BindableLayout.GetItemsSource(MainStackBindableLayout) as IEnumerable;
		var it = BindableLayout.GetItemTemplate(MainStackBindableLayout);
		var sel = BindableLayout.GetItemTemplateSelector(MainStackBindableLayout);
		int count = src == null ? 0 : src.Cast<object>().Count();
		DirectApiSummaryLabel.Text = $"[{action}] EmptyView={(ev != null)} EmptyViewTemplate={(evt != null)} ItemsSourceCount={count} ItemTemplate={(it != null)} ItemTemplateSelector={(sel != null)}" + (extra != null ? " " + extra : "");
	}
}