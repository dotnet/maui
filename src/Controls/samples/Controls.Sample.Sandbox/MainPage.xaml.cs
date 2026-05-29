using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	private ObservableCollection<string> _items;
	private int _scrollCount = 0;
	
	public MainPage()
	{
		InitializeComponent();
		
		// Initialize data source
		_items = new ObservableCollection<string>
		{
			"Item 1: Baboon",
			"Item 2: Capuchin Monkey",
			"Item 3: Blue Monkey",
			"Item 4: Squirrel Monkey",
			"Item 5: Golden Lion Tamarin"
		};
		
		TestCarouselView.ItemsSource = _items;
		TestCarouselView.CurrentItem = _items[0];
		
		// Subscribe to CurrentItemChanged event
		TestCarouselView.CurrentItemChanged += OnCurrentItemChanged;
		
		// Display handler info after layout
		TestCarouselView.Loaded += (s, e) =>
		{
			var handlerType = TestCarouselView.Handler?.GetType().Name ?? "Unknown";
			HandlerLabel.Text = $"Handler: {handlerType}";
			
			Console.WriteLine("========== VERTICAL CAROUSELVIEW CENTERING TEST ==========");
			Console.WriteLine($"Handler Type: {handlerType}");
			Console.WriteLine($"Initial CurrentItem: {TestCarouselView.CurrentItem}");
			Console.WriteLine($"Initial Position: {TestCarouselView.Position}");
			Console.WriteLine($"Total Items: {_items.Count}");
			Console.WriteLine($"Orientation: Vertical");
			Console.WriteLine($"Loop: {TestCarouselView.Loop}");
			Console.WriteLine("");
			Console.WriteLine("INSTRUCTIONS:");
			Console.WriteLine("1. Tap 'Scroll to Next Item' button repeatedly");
			Console.WriteLine("2. Test scrolling past last item (Item 5) to see if it loops to Item 1");
			Console.WriteLine("3. Watch CurrentItem updates in loop scenario");
			Console.WriteLine("==========================================================");
		};
	}
	
	private void OnScrollButtonClicked(object sender, EventArgs e)
	{
		_scrollCount++;
		var targetIndex = _scrollCount % _items.Count;
		
		Console.WriteLine("");
		Console.WriteLine($"========== SCROLL #{_scrollCount} ==========");
		Console.WriteLine($"Current Position: {TestCarouselView.Position}");
		Console.WriteLine($"Current Item: {TestCarouselView.CurrentItem}");
		Console.WriteLine($"Scrolling to index: {targetIndex} ({_items[targetIndex]})");
		
		if (_scrollCount > _items.Count)
		{
			Console.WriteLine("⚠️ TESTING LOOP BEHAVIOR - scrolling past end of collection");
		}
		
		TestCarouselView.ScrollTo(targetIndex, position: ScrollToPosition.Center, animate: true);
		
		// Wait for scroll animation and log results
		Task.Run(async () =>
		{
			await Task.Delay(1500);
			MainThread.BeginInvokeOnMainThread(() =>
			{
				Console.WriteLine($"After scroll - Position: {TestCarouselView.Position}, CurrentItem: {TestCarouselView.CurrentItem}");
				
				if (TestCarouselView.CurrentItem?.ToString() == _items[targetIndex])
				{
					Console.WriteLine("✅ CurrentItem updated correctly");
				}
				else
				{
					Console.WriteLine($"❌ CurrentItem mismatch! Expected: {_items[targetIndex]}, Got: {TestCarouselView.CurrentItem}");
				}
				Console.WriteLine("==========================================");
			});
		});
	}
	
	private void OnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
	{
		Console.WriteLine("");
		Console.WriteLine("========== CurrentItemChanged EVENT ==========");
		Console.WriteLine($"Previous: {e.PreviousItem ?? "null"}");
		Console.WriteLine($"Current: {e.CurrentItem ?? "null"}");
		Console.WriteLine($"Position: {TestCarouselView.Position}");
		Console.WriteLine("==============================================");
		
		if (e.CurrentItem != null)
		{
			CurrentItemLabel.Text = $"CurrentItem: {e.CurrentItem}";
			Console.WriteLine($"✅ CurrentItem updated to: {e.CurrentItem}");
		}
		else
		{
			CurrentItemLabel.Text = "CurrentItem: (null)";
			Console.WriteLine("⚠️ WARNING: CurrentItem is null");
		}
	}
}