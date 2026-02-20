using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28334, "[Windows] EmptyViewTemplate not displayed when ItemsSource is set to Null", PlatformAffected.UWP)]
public partial class Issue28334 : ContentPage
{
	private Issue28334CollectionViewViewModel _viewModel;
	public Issue28334()
	{
		InitializeComponent();
		this.BindingContext = _viewModel = new Issue28334CollectionViewViewModel();
	}
	private void OnItemSourceButtonClick(object sender, EventArgs e)
	{
		_viewModel.ItemsSource = null;
	}

	private void OnEmptyViewChangeButtonClick(object sender, EventArgs e)
	{
		_viewModel.EmptyView = "EmptyView";
	}

	private void OnEmptyViewTemplateChangedButtonClick(object sender, EventArgs e)
	{
		_viewModel.EmptyViewTemplate = new DataTemplate(() =>
		{
			Grid grid = new Grid();
			grid.Children.Add(new Label
			{
				Text = "EmptyViewTemplate",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Colors.Blue
			});

			return grid;
		});
	}

	private void OnReAddItemSourceButtonClick(object sender, EventArgs e)
	{
		_viewModel.ItemsSource = new List<Issue28334ItemModel>
			{
				new Issue28334ItemModel { Name = "Item 1", Description = "Description 1" },
				new Issue28334ItemModel { Name = "Item 2", Description = "Description 2" },
				new Issue28334ItemModel { Name = "Item 3", Description = "Description 3" }
			};
	}
}

public class Issue28334CollectionViewViewModel : INotifyPropertyChanged
{
	private List<Issue28334ItemModel> _itemsSource;
	public List<Issue28334ItemModel> ItemsSource
	{
		get => _itemsSource;
		set
		{
			_itemsSource = value;
			OnPropertyChanged();
		}
	}

	private object _emptyView;

	public object EmptyView
	{
		get => _emptyView;
		set { _emptyView = value; OnPropertyChanged(); }
	}

	private DataTemplate _emptyViewTemplate;

	public DataTemplate EmptyViewTemplate
	{
		get => _emptyViewTemplate;
		set { _emptyViewTemplate = value; OnPropertyChanged(); }
	}

	public Issue28334CollectionViewViewModel()
	{
		ItemsSource = new List<Issue28334ItemModel>
			{
				new Issue28334ItemModel { Name = "Item 1", Description = "Description 1" },
				new Issue28334ItemModel { Name = "Item 2", Description = "Description 2" },
				new Issue28334ItemModel { Name = "Item 3", Description = "Description 3" }
			};
	}

	public event PropertyChangedEventHandler PropertyChanged;

	private void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class Issue28334ItemModel
{
	public string Name { get; set; }
	public string Description { get; set; }

	public override string ToString()
	{
		return $"{Name} - {Description}";
	}
}