﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28827, "[Android] Group Header/Footer set for all Items when IsGrouped is True for ObservableCollection", PlatformAffected.Android)]
public partial class Issue28827 : ContentPage
{
	Issue28827CollectionViewViewModel _viewModel;
	public Issue28827()
	{
		InitializeComponent();
		BindingContext = _viewModel = new Issue28827CollectionViewViewModel();

	}

	void OnGroupHeaderTemplateChanged(object sender, CheckedChangedEventArgs e)
	{
		if (GroupHeaderTemplateNone.IsChecked)
		{
			_viewModel.GroupHeaderTemplate = null;
		}
		else if (GroupHeaderTemplateGrid.IsChecked)
		{
			_viewModel.GroupHeaderTemplate = new DataTemplate(() =>
			{
				return new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10),
					Children =
					{
							new Label
							{
								Text = "Group Header Template (Grid View)",
								FontSize = 18,
								FontAttributes = FontAttributes.Bold,
								HorizontalOptions = LayoutOptions.Center,
								VerticalOptions = LayoutOptions.Center,
								TextColor = Colors.Green
							}
					}
				};
			});
		}
	}

	void OnGroupFooterTemplateChanged(object sender, CheckedChangedEventArgs e)
	{
		if (GroupFooterTemplateNone.IsChecked)
		{
			_viewModel.GroupFooterTemplate = null;
		}
		else if (GroupFooterTemplateGrid.IsChecked)
		{
			_viewModel.GroupFooterTemplate = new DataTemplate(() =>
			{
				return new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10),
					Children =
					{
							new Label
							{
								Text = "Group Footer Template (Grid View)",
								FontSize = 18,
								FontAttributes = FontAttributes.Bold,
								HorizontalOptions = LayoutOptions.Center,
								VerticalOptions = LayoutOptions.Center,
								TextColor = Colors.Red
							}
					}
				};
			});
		}
	}

	void OnIsGroupedChanged(object sender, CheckedChangedEventArgs e)
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

	void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;

		if (radioButton == ItemsSourceGroupedList)
		{
			_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
		}
		else if (radioButton == ItemsSourceObservableCollection)
		{
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT;
		}

	}
}

public class Issue28827CollectionViewViewModel : INotifyPropertyChanged
{
	DataTemplate _groupHeaderTemplate;
	DataTemplate _groupFooterTemplate;
	DataTemplate _itemTemplate;
	bool _isGrouped = false;
	ItemsSourceType _itemsSourceType = ItemsSourceType.ObservableCollectionT;
	List<Issue28827Grouping<string, Issue28827ItemModel>> _groupedList;
	ObservableCollection<Issue28827ItemModel> _observableCollection;

	public event PropertyChangedEventHandler PropertyChanged;

	public Issue28827CollectionViewViewModel()
	{
		LoadItems();
		ItemTemplate = new DataTemplate(() =>
		{
			var stackLayout = new StackLayout
			{
				Padding = new Thickness(10),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			var label = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};
			label.SetBinding(Label.TextProperty, "Caption");
			stackLayout.Children.Add(label);
			return stackLayout;
		});

		GroupHeaderTemplate = new DataTemplate(() =>
		{
			var stackLayout = new StackLayout
			{
				BackgroundColor = Colors.LightGray
			};
			var label = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 18
			};
			label.SetBinding(Label.TextProperty, "Key");
			stackLayout.Children.Add(label);
			return stackLayout;
		});
	}

	void LoadItems()
	{
		_observableCollection = new ObservableCollection<Issue28827ItemModel>
			{
				new Issue28827ItemModel { Caption = "Item 1" },
				new Issue28827ItemModel { Caption = "Item 2" },
				new Issue28827ItemModel { Caption = "Item 3" }
			};

		// Create a grouped list with sample data
		_groupedList = new List<Issue28827Grouping<string, Issue28827ItemModel>>
			{
				new Issue28827Grouping<string, Issue28827ItemModel>("Group A", new List<Issue28827ItemModel>
				{
					new Issue28827ItemModel { Caption = "Group A - Item 1" },
					new Issue28827ItemModel { Caption = "Group A - Item 2" }
				}),
				new Issue28827Grouping<string, Issue28827ItemModel>("Group B", new List<Issue28827ItemModel>
				{
					new Issue28827ItemModel { Caption = "Group B - Item 1" },
					new Issue28827ItemModel { Caption = "Group B - Item 2" }
				})
			};
	}

	public DataTemplate GroupHeaderTemplate
	{
		get => _groupHeaderTemplate;
		set { _groupHeaderTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate GroupFooterTemplate
	{
		get => _groupFooterTemplate;
		set { _groupFooterTemplate = value; OnPropertyChanged(); }
	}

	public DataTemplate ItemTemplate
	{
		get => _itemTemplate;
		set { _itemTemplate = value; OnPropertyChanged(); }
	}

	public bool IsGrouped
	{
		get => _isGrouped;
		set { _isGrouped = value; OnPropertyChanged(); }
	}

	public ItemsSourceType ItemsSourceType
	{
		get => _itemsSourceType;
		set
		{
			if (_itemsSourceType != value)
			{
				_itemsSourceType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ItemsSource));
			}
		}
	}

	public object ItemsSource
	{
		get
		{
			return ItemsSourceType switch
			{
				ItemsSourceType.GroupedListT => _groupedList,
				ItemsSourceType.ObservableCollectionT => _observableCollection,
				_ => null
			};
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		if (propertyName == nameof(IsGrouped))
		{
			OnPropertyChanged(nameof(ItemsSource));
		}

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

internal class Issue28827Grouping<TKey, TItem> : List<TItem>
{
	public TKey Key { get; }

	public Issue28827Grouping(TKey key, List<TItem> items) : base(items)
	{
		Key = key;
	}

	public override string ToString()
	{
		return Key?.ToString() ?? base.ToString();
	}
}

internal class Issue28827ItemModel
{
	public string Caption { get; set; }

	public override string ToString()
	{
		return !string.IsNullOrEmpty(Caption) ? Caption : base.ToString();
	}
}

public enum ItemsSourceType
{
	None,
	ListT,
	ObservableCollectionT,
	GroupedListT
}