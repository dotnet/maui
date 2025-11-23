namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void OnTestButtonClicked(object? sender, EventArgs e)
	{
		Dispatcher.DispatchAsync(async () =>
		{
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting test with one Task.Yield");
			StatusLabel.Text = "Testing (one Task.Yield)...";

			await Navigation.PushModalAsync(new ContentPage() { Content = new Label() { Text = "Hello!" } }, false);
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Modal pushed");

			await Task.Yield();
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] After Task.Yield");

			await Navigation.PopModalAsync();
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Modal popped");

			StatusLabel.Text = "Test completed!";
		});
	}

	private void OnTestButton2Clicked(object? sender, EventArgs e)
	{
		Dispatcher.DispatchAsync(async () =>
		{
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting test with two Task.Yield");
			StatusLabel.Text = "Testing (two Task.Yields)...";

			await Navigation.PushModalAsync(new ContentPage() { Content = new Label() { Text = "Hello!" } }, false);
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Modal pushed");

			await Task.Yield();
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] After first Task.Yield");

			await Task.Yield();
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] After second Task.Yield");

			await Navigation.PopModalAsync();
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Modal popped");

			StatusLabel.Text = "Test completed!";
		});
	}
}