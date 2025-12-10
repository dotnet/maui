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
				label.SetBinding(Label.TextProperty, new Binding("Caption", stringFormat: "Set {0}"));
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

	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new BindableLayoutViewModel();
		ReinitializeBindableLayouts();
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
		_viewModel.OnGridLoaded(sender, e);
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

	private void OnResetClicked(object sender, EventArgs e)
	{
		ReinitializeBindableLayouts();
	}

	private void ReinitializeBindableLayouts()
	{
		MainStackBindableLayout.Children.Clear();
		MainFlexBindableLayout.Children.Clear();
		MainGridBindableLayout.Children.Clear();

		MainStackBindableLayout.SetBinding(BindableLayout.ItemsSourceProperty, new Binding(nameof(BindableLayoutViewModel.ItemsSource)));
		MainFlexBindableLayout.SetBinding(BindableLayout.ItemsSourceProperty, new Binding(nameof(BindableLayoutViewModel.ItemsSource)));
		MainGridBindableLayout.SetBinding(BindableLayout.ItemsSourceProperty, new Binding(nameof(BindableLayoutViewModel.ItemsSource)));

		MainStackBindableLayout.SetBinding(BindableLayout.ItemTemplateProperty, new Binding(nameof(BindableLayoutViewModel.ItemTemplate)));
		MainFlexBindableLayout.SetBinding(BindableLayout.ItemTemplateProperty, new Binding(nameof(BindableLayoutViewModel.ItemTemplate)));
		MainGridBindableLayout.SetBinding(BindableLayout.ItemTemplateProperty, new Binding(nameof(BindableLayoutViewModel.ItemTemplate)));

		MainStackBindableLayout.SetBinding(BindableLayout.ItemTemplateSelectorProperty, new Binding(nameof(BindableLayoutViewModel.ItemTemplateSelector)));
		MainFlexBindableLayout.SetBinding(BindableLayout.ItemTemplateSelectorProperty, new Binding(nameof(BindableLayoutViewModel.ItemTemplateSelector)));
		MainGridBindableLayout.SetBinding(BindableLayout.ItemTemplateSelectorProperty, new Binding(nameof(BindableLayoutViewModel.ItemTemplateSelector)));

		MainStackBindableLayout.SetBinding(BindableLayout.EmptyViewProperty, new Binding(nameof(BindableLayoutViewModel.StackEmptyView)));
		MainFlexBindableLayout.SetBinding(BindableLayout.EmptyViewProperty, new Binding(nameof(BindableLayoutViewModel.FlexEmptyView)));
		MainGridBindableLayout.SetBinding(BindableLayout.EmptyViewProperty, new Binding(nameof(BindableLayoutViewModel.GridEmptyView)));

		MainStackBindableLayout.SetBinding(BindableLayout.EmptyViewTemplateProperty, new Binding(nameof(BindableLayoutViewModel.EmptyViewTemplate)));
		MainFlexBindableLayout.SetBinding(BindableLayout.EmptyViewTemplateProperty, new Binding(nameof(BindableLayoutViewModel.EmptyViewTemplate)));
		MainGridBindableLayout.SetBinding(BindableLayout.EmptyViewTemplateProperty, new Binding(nameof(BindableLayoutViewModel.EmptyViewTemplate)));

		ArrangeIfGrid(MainGridBindableLayout);

		UpdateDirectSummary("Reinitialize BindableLayouts");
	}


	private void OnSetItemsSourceClicked(object sender, EventArgs e)
	{

		ApplySetItemsSource(MainStackBindableLayout);
		ApplySetItemsSource(MainFlexBindableLayout);
		ApplySetItemsSource(MainGridBindableLayout);
		ArrangeIfGrid(MainGridBindableLayout);
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
			var label = new Label { TextColor = Colors.Green, FontAttributes = FontAttributes.Bold };
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
		BindableLayout.SetItemTemplate(MainStackBindableLayout, null);
		BindableLayout.SetItemTemplate(MainFlexBindableLayout, null);
		BindableLayout.SetItemTemplate(MainGridBindableLayout, null);

		var selector = new SimpleAlternatingSelector(Colors.Purple, Colors.Green);
		_viewModel.ItemTemplateSelector = selector;

		ApplySetItemTemplateSelector(MainStackBindableLayout);
		ApplySetItemTemplateSelector(MainFlexBindableLayout);
		ApplySetItemTemplateSelector(MainGridBindableLayout);

		ArrangeIfGrid(MainGridBindableLayout);
		UpdateDirectSummary("Set ItemTemplateSelector");
	}

	private void OnSetEmptyViewClicked(object sender, EventArgs e)
	{
		BindableLayout.SetEmptyViewTemplate(MainStackBindableLayout, null);
		BindableLayout.SetEmptyViewTemplate(MainFlexBindableLayout, null);
		BindableLayout.SetEmptyViewTemplate(MainGridBindableLayout, null);
		BindableLayout.SetEmptyView(MainStackBindableLayout, new Label { Text = "(Set EmptyView)", HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Gray });
		BindableLayout.SetEmptyView(MainFlexBindableLayout, new Label { Text = "(Set EmptyView)", HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Gray });
		BindableLayout.SetEmptyView(MainGridBindableLayout, new Label { Text = "(Set EmptyView)", HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Gray });
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

	private void OnGetEmptyViewClicked(object sender, EventArgs e)
	{
		var ev = BindableLayout.GetEmptyView(MainStackBindableLayout);
		BindableLayout.GetEmptyView(MainFlexBindableLayout);
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

	public void ApplySetItemsSource(Layout layout)
	{
		if (layout == null) return;
		BindableLayout.SetItemsSource(layout, _viewModel.ItemsSource as IEnumerable);
	}

	public void ApplySetItemTemplateSelector(Layout layout)
	{
		if (layout == null) return;
		BindableLayout.SetItemTemplate(layout, null);
		BindableLayout.SetItemTemplateSelector(layout, _viewModel.ItemTemplateSelector);
	}

	public void ApplySetEmptyView(Layout layout)
	{
		if (layout == null) return;
		object emptyView = layout switch
		{
			VerticalStackLayout => _viewModel.StackEmptyView,
			FlexLayout => _viewModel.FlexEmptyView,
			Grid => _viewModel.GridEmptyView,
			_ => _viewModel.StackEmptyView ?? _viewModel.FlexEmptyView ?? _viewModel.GridEmptyView
		};
		BindableLayout.SetEmptyView(layout, emptyView);
	}

	public void ApplySetEmptyViewTemplate(Layout layout)
	{
		if (layout == null) return;
		BindableLayout.SetEmptyViewTemplate(layout, _viewModel.EmptyViewTemplate);
	}

	public void ArrangeIfGrid(Layout layout)
	{
		if (layout is Grid grid)
		{
			OnGridLoaded(grid, EventArgs.Empty);
		}
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