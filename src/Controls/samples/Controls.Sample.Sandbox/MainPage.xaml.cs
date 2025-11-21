using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public ObservableCollection<string> MyItemList { get; set; }

	public MainPage()
	{
		InitializeComponent();
		
		MyItemList = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
			"Item 5"
		};
		
		BindingContext = this;
		
		// Subscribe to SizeChanged on the CollectionView
		TestCollectionView.SizeChanged += (sender, args) =>
		{
			Console.WriteLine($"[MainPage] ✓ CollectionView SizeChanged: Width={TestCollectionView.Width}, Height={TestCollectionView.Height}");
		};
		
		// Subscribe to ChildAdded to track when items are added
		TestCollectionView.ChildAdded += (sender, args) =>
		{
			Console.WriteLine($"[MainPage] CollectionView ChildAdded: {args.Element}");
			if (args.Element is View childView)
			{
				childView.SizeChanged += (s, e) =>
				{
					Console.WriteLine($"[MainPage] ✓ CollectionView child SizeChanged: Width={childView.Width}, Height={childView.Height}");
					
					// Compare heights
					if (TestCollectionView.Height > 0 && childView.Height > 0)
					{
						if (Math.Abs(TestCollectionView.Height - childView.Height) < 1)
						{
							Console.WriteLine($"[MainPage] ✅ SUCCESS: CollectionView height ({TestCollectionView.Height}) matches child height ({childView.Height})");
						}
						else
						{
							Console.WriteLine($"[MainPage] ❌ FAILURE: CollectionView height ({TestCollectionView.Height}) does NOT match child height ({childView.Height})");
						}
					}
				};
			}
		};
	}
}