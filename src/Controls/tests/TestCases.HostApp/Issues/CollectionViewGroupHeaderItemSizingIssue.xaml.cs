using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.ManualTest, 1, "CollectionView group header size changes with ItemSizingStrategy", PlatformAffected.iOS)]
public partial class CollectionViewGroupHeaderItemSizingIssue : ContentPage
{
	public CollectionViewGroupHeaderItemSizingIssue()
	{
		InitializeComponent();
		BindingContext = new GroupedAnimalsViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		// Capture initial header size after a short delay
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1000), () =>
		{
			CaptureHeaderSize("OnAppearing");
		});
	}

	private void OnSwitchToMeasureFirstItem(object sender, EventArgs e)
	{
		Console.WriteLine("=== SWITCHING ItemSizingStrategy ===");
		Console.WriteLine($"Before: {TestCollectionView.ItemSizingStrategy}");

		TestCollectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
		StatusLabel.Text = $"ItemSizingStrategy: {TestCollectionView.ItemSizingStrategy}";

		Console.WriteLine($"After: {TestCollectionView.ItemSizingStrategy}");

		// Capture header size after layout updates
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
		{
			CaptureHeaderSize("AfterSwitch");
		});
	}

	private void CaptureHeaderSize(string context)
	{
		Console.WriteLine($"=== HEADER SIZE CAPTURE: {context} ===");

		// Try to find the first group header element
		// This is platform-specific, so we'll rely on console logs for now
		// The actual measurement will be done in the UI test using Appium

		Console.WriteLine($"ItemSizingStrategy: {TestCollectionView.ItemSizingStrategy}");
		Console.WriteLine("=== END HEADER SIZE CAPTURE ===");

		HeaderSizeLabel.Text = $"Header Size: Check console for {context}";
	}
}

public class GroupedAnimalsViewModel
{
	public ObservableCollection<TestAnimalGroup> Animals { get; set; }

	public GroupedAnimalsViewModel()
	{
		Animals = new ObservableCollection<TestAnimalGroup>
		{
			new TestAnimalGroup("Bears")
			{
				new TestAnimal { Name = "Grizzly Bear", Location = "North America", ImageUrl = "bear.jpg" },
				new TestAnimal { Name = "Polar Bear", Location = "Arctic", ImageUrl = "bear.jpg" },
			},
			new TestAnimalGroup("Monkeys")
			{
				new TestAnimal { Name = "Baboon", Location = "Africa", ImageUrl = "monkey.jpg" },
				new TestAnimal { Name = "Capuchin Monkey", Location = "South America", ImageUrl = "monkey.jpg" },
				new TestAnimal { Name = "Spider Monkey", Location = "Central America", ImageUrl = "monkey.jpg" },
			},
			new TestAnimalGroup("Elephants")
			{
				new TestAnimal { Name = "African Elephant", Location = "Africa", ImageUrl = "elephant.jpg" },
				new TestAnimal { Name = "Asian Elephant", Location = "Asia", ImageUrl = "elephant.jpg" },
			}
		};
	}
}

public class TestAnimalGroup : ObservableCollection<TestAnimal>
{
	public string Name { get; set; }

	public TestAnimalGroup(string name) : base()
	{
		Name = name;
	}
}

public class TestAnimal
{
	public string Name { get; set; }
	public string Location { get; set; }
	public string ImageUrl { get; set; }
}
