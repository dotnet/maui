using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31351, "[WinUI] Custom CollectionView does not work on ScrollTo", PlatformAffected.All)]
public partial class Issue31351 : ContentPage
{
    private readonly Issue31351CustomCollectionView<Issue31351StartupPageModel.DisplayItem> _customCollectionView;
    private readonly Issue31351StartupPageModel _viewModel;

    public Issue31351()
    {
        _viewModel = new Issue31351StartupPageModel();
        BindingContext = _viewModel;

        var scrollButton = new Button
        {
            Text = "Scroll to Mid",
            AutomationId = "Issue31351ScrollButton"
        };
        scrollButton.Clicked += (s, e) =>
        {
            _viewModel.Scroll.Execute(null);
        };

        var scrollButton2 = new Button
        {
            Text = "Scroll to Top",
            AutomationId = "Issue31351TopScrollButton"
        };
        scrollButton2.HeightRequest = 50;
        scrollButton2.Clicked += (s, e) =>
        {
            _customCollectionView.ScrollTo(0, position: ScrollToPosition.Center, animate: true);
        };

        var statusLabel = new Label
        {
            Text = "Ready - Tap buttons to test ScrollTo functionality",
            AutomationId = "Issue31351StatusLabel",
            BackgroundColor = Colors.LightYellow,
            Padding = new Thickness(10),
            HorizontalTextAlignment = TextAlignment.Center
        };

        _customCollectionView = new Issue31351CustomCollectionView<Issue31351StartupPageModel.DisplayItem>
        {
            AutomationId = "Issue31351CollectionView",
            Margin = 10,
            BackgroundColor = Colors.LightGray
        };
        _customCollectionView.SetBinding(Issue31351CustomCollectionView<Issue31351StartupPageModel.DisplayItem>.CustomItemsSourceProperty,
            new Binding(nameof(Issue31351StartupPageModel.DisplayItems), BindingMode.TwoWay, source: _viewModel));
        _customCollectionView.SetBinding(Issue31351CustomCollectionView<Issue31351StartupPageModel.DisplayItem>.CustomSelectedItemProperty,
            new Binding(nameof(Issue31351StartupPageModel.SelectedDisplayItem), BindingMode.TwoWay, source: _viewModel));
        _customCollectionView.SetBinding(Issue31351CustomCollectionView<Issue31351StartupPageModel.DisplayItem>.InvalidateCommandProperty,
            new Binding(nameof(Issue31351StartupPageModel.InvalidateTreeviewCommand), BindingMode.OneWayToSource, source: _viewModel));


        _customCollectionView.Scrolled += (sender, e) =>
        {
            statusLabel.Text = $"Scrolled: First={e.FirstVisibleItemIndex}, Center={e.CenterItemIndex}, Last={e.LastVisibleItemIndex}";
        };


        var layoutGrid = new Grid();
        layoutGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        layoutGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        layoutGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        layoutGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        layoutGrid.Add(scrollButton, 0, 0);
        layoutGrid.Add(scrollButton2, 0, 1);
        layoutGrid.Add(statusLabel, 0, 2);
        layoutGrid.Add(_customCollectionView, 0, 3);

        Content = layoutGrid;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing.Execute(null);
    }
}

internal class Issue31351StartupPageModel:INotifyPropertyChanged
{
	#region *// Display data for custom control
	public class DisplayItem : ITreeviewItem
	{
		public string Title { get; set; }
	}

	public List<DisplayItem> DisplayItems { get; set; } = new();

	DisplayItem _displayItem;
	public DisplayItem SelectedDisplayItem 
	{
		get { return _displayItem; }
		set { _displayItem = value; OnPropertyChanged("SelectedDisplayItem"); }
	}

	public Action InvalidateTreeviewCommand { get; set; }
	#endregion

	public class DataItem
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public List<DataItem> data = new();

	public Issue31351StartupPageModel()
	{
		#region *// Generate test data
		for (int i = 0; i < 100; i++)
		{
			data.Add(new DataItem { Id = i, Name = $"Item {i}" });
		}
		#endregion
	}
	public event PropertyChangedEventHandler PropertyChanged; 
	public void OnPropertyChanged(string name)
	{ 
		if (this.PropertyChanged != null) 
			this.PropertyChanged(this, new PropertyChangedEventArgs(name)); 
	}

	public ICommand OnAppearing => new Command(() =>
	{
		DisplayItems.Clear();
		foreach (var dataItem in data)
		{
			DisplayItems.Add(new DisplayItem { Title = dataItem.Name });
		}

		SelectedDisplayItem = DisplayItems[5];
		InvalidateTreeviewCommand?.Invoke();
	});

	public ICommand Scroll => new Command(() =>
	{
		SelectedDisplayItem = DisplayItems[50];
		InvalidateTreeviewCommand?.Invoke();
	});
}

public interface ITreeviewItem
{
	string Title { get; set; }
}

internal class Issue31351CustomCollectionView<T> : CollectionView where T : ITreeviewItem
{
	#region *// Bindable properties

	#region *// Item source
	public static readonly BindableProperty CustomItemsSourceProperty = BindableProperty.Create(
		nameof(CustomItemsSource), 
		typeof(List<T>), 
		typeof(Issue31351CustomCollectionView<T>), 
		null);

	public List<T> CustomItemsSource
	{
		get { return (List<T>)GetValue(CustomItemsSourceProperty); }
		set { SetValue(CustomItemsSourceProperty, value); }
	}
	#endregion

	#region *// InvalidateCommand
	public static BindableProperty InvalidateCommandProperty = BindableProperty.Create(
		nameof(InvalidateCommand), 
		typeof(Action), 
		typeof(Issue31351CustomCollectionView<T>), 
		null, 
		BindingMode.OneWayToSource);

	public Action InvalidateCommand
	{
		get { return (Action)GetValue(InvalidateCommandProperty); }
		set { SetValue(InvalidateCommandProperty, value); }
	}
	#endregion

	#region *// Selected item
	public static readonly BindableProperty CustomSelectedItemProperty = BindableProperty.Create(
		nameof(CustomSelectedItem), 
		typeof(ITreeviewItem), 
		typeof(Issue31351CustomCollectionView<T>), 
		null);

	public ITreeviewItem CustomSelectedItem
	{
		get { return (ITreeviewItem)GetValue(CustomSelectedItemProperty); }
		set { SetValue(CustomSelectedItemProperty, value); }
	}
	#endregion

	#endregion

	public class CollectionViewItem
	{
		public string Title { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is not CollectionViewItem)
				return false;
			return ((CollectionViewItem)this).Title.Equals(((CollectionViewItem)obj).Title, StringComparison.Ordinal);
		}
		
		public override int GetHashCode()
		{
			return Title?.GetHashCode(StringComparison.Ordinal) ?? 0;
		}
	}
	
	private List<CollectionViewItem> CollectionViewItems = new();

	public Issue31351CustomCollectionView()
	{
		ItemTemplate = new DataTemplate(() => {
			var label = new Label();
			label.SetBinding(Label.TextProperty, nameof(CollectionViewItem.Title));
			label.Padding = new Thickness(10);
			label.BackgroundColor = Colors.White;

			var border = new Border
			{
				Content = label,
				BackgroundColor = Colors.White,
				Stroke = Colors.Gray,
				StrokeThickness = 1,
				Margin = new Thickness(2)
			};
			
			return border;
		});

		SelectionMode = SelectionMode.Single;

		InvalidateCommand = new Action(() =>
		{
			Invalidate();
		});
	}

	private void Invalidate()
	{
		CollectionViewItems.Clear();
		
		if (CustomItemsSource != null)
		{
			foreach (var itemSource in CustomItemsSource)
			{
				CollectionViewItems.Add(new CollectionViewItem { Title = itemSource.Title });
			}
		}

		ItemsSource = CollectionViewItems;
		if (CustomSelectedItem != null)
		{
			var selectedCollectionViewItem = CollectionViewItems.FirstOrDefault(o => o.Title == CustomSelectedItem.Title);

			ScrollTo(selectedCollectionViewItem, position: ScrollToPosition.Center, animate: true);
			SelectedItem = selectedCollectionViewItem;
		}
				
	}
}