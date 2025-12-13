using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ManualTests.ViewModels;

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
