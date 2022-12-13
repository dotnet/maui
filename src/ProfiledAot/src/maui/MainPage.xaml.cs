namespace maui;

public partial class MainPage : ContentPage
{
	int count = 0;
	bool _isLoaded;
	bool _startCompleted;

	public MainPage()
	{
		InitializeComponent();

		if (Application.Current.MainPage is not AppFlyoutPage)
		{
			Start();
			this.Loaded += OnMainPageLoaded;
		}
	}

	private async void Start()
	{
		CounterBtn.Text = await CommonMethods.Invoke();
		_startCompleted= true;
		LoadFlyoutPage();
	}

	void OnMainPageLoaded(object sender, EventArgs e)
	{
		_isLoaded = true;
		LoadFlyoutPage();
	}

	async void LoadFlyoutPage()
	{
		if (_isLoaded && _startCompleted)
		{
			await Task.Delay(500);
			Application.Current.MainPage = new AppFlyoutPage();
		}
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}