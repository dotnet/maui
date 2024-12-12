using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24961, "[iOS] CollectionView with header and complex item will have the wrong position after a refresh with a RefreshView", PlatformAffected.iOS)]
public partial class Issue24961 : ContentPage
{
	 private MainPageModel _pageModel;
	public Issue24961()
	{
		InitializeComponent();
		this.BindingContext = _pageModel = new MainPageModel();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
        _pageModel.IsRefreshed = false;
	}
}

public class MainPageModel : INotifyPropertyChanged
{
    private bool _isRefreshing;

    private bool _isRefreshed;
    public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;

	// Define the command property
    public ICommand RefreshCommand { get; set;}

    // Property for IsRefreshing
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing != value)
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }
    }

    public bool IsRefreshed
    {
        get => _isRefreshed;
        set
        {
            if (_isRefreshed != value)
            {
                _isRefreshed = value;
                OnPropertyChanged(nameof(IsRefreshed));
            }
        }
    }

    // Command method for Refresh
    public async Task RefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            IsRefreshed =false;
            await InitItems();
        }
        finally
        {
            IsRefreshing = false;
            IsRefreshed = true;
        }
    }

    public MainPageModel()
    {
        _ = InitItems();
		ConfigureCommand();
    }

	private void ConfigureCommand()
	{
		RefreshCommand = new Command(async () => await RefreshAsync());
	}

    // Initialize Items collection
    public async Task InitItems()
    {
        await Task.Delay(2000);

        // Updating UI on the UI thread if necessary
        
        Application.Current?.Dispatcher.Dispatch(() =>
        {
            Items.Clear();
            foreach (var itemViewModel in GetItems())
            {
                Items.Add(itemViewModel);
            }
        });
    }

    // Generate a list of items (for demonstration purposes)
    private List<ItemViewModel> GetItems()
    {
        return new List<ItemViewModel>
        {
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2", TextLine3 = "3" },
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2" },
            new ItemViewModel { TextLine1 = "1" },
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2", TextLine3 = "3" },
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2" },
            new ItemViewModel { TextLine1 = "1" },
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2", TextLine3 = "3" },
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2" },
            new ItemViewModel { TextLine1 = "1" },
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2", TextLine3 = "3" },
            new ItemViewModel { TextLine1 = "1", TextLine2 = "2" },
            new ItemViewModel { TextLine1 = "1" }
        };
    }

    // Raise PropertyChanged event
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ItemViewModel : INotifyPropertyChanged
{
    private string _textLine1;
    private string _textLine2;
    private string _textLine3;

    // Implement INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    // Property for TextLine1
    public string TextLine1
    {
        get => _textLine1;
        set
        {
            if (_textLine1 != value)
            {
                _textLine1 = value;
                OnPropertyChanged(nameof(TextLine1));
                OnPropertyChanged(nameof(ShowTextLine1)); // Notify that ShowTextLine1 has changed
            }
        }
    }

    // Property for TextLine2
    public string TextLine2
    {
        get => _textLine2;
        set
        {
            if (_textLine2 != value)
            {
                _textLine2 = value;
                OnPropertyChanged(nameof(TextLine2));
                OnPropertyChanged(nameof(ShowTextLine2)); // Notify that ShowTextLine2 has changed
            }
        }
    }

    // Property for TextLine3
    public string TextLine3
    {
        get => _textLine3;
        set
        {
            if (_textLine3 != value)
            {
                _textLine3 = value;
                OnPropertyChanged(nameof(TextLine3));
                OnPropertyChanged(nameof(ShowTextLine3)); // Notify that ShowTextLine3 has changed
            }
        }
    }

    // Computed property to show if TextLine1 has a value
    public bool ShowTextLine1 => !string.IsNullOrWhiteSpace(TextLine1);

    // Computed property to show if TextLine2 has a value
    public bool ShowTextLine2 => !string.IsNullOrWhiteSpace(TextLine2);

    // Computed property to show if TextLine3 has a value
    public bool ShowTextLine3 => !string.IsNullOrWhiteSpace(TextLine3);

    // Raise PropertyChanged event
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}