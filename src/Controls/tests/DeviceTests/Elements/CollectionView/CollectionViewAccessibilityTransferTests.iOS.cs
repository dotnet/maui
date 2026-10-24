using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.CollectionView)]
public class CollectionViewAccessibilityTransferTests : ControlsHandlerTestBase
{
	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public async Task MeasurementCellContentPreservesButtonTraitOwnership(bool transferContent, bool developerAssignedButtonTrait)
	{
		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CollectionView, CollectionViewHandler>();
				handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
			});
		});

		var content = new VerticalStackLayout();
		SemanticProperties.SetDescription(content, "Item");
		var collectionView = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			ItemTemplate = new DataTemplate(() => content)
		};

		await CreateHandlerAsync<CollectionViewHandler>(collectionView);
		await InvokeOnMainThreadAsync(() =>
		{
			var platformView = content.ToPlatform(MauiContext);
			if (developerAssignedButtonTrait)
				platformView.AccessibilityTraits |= UIAccessibilityTrait.Button;

			using var measurementCell = new VerticalCell(CGRect.Empty);
			using var displayedCell = new VerticalCell(CGRect.Empty);
			measurementCell.Bind(collectionView.ItemTemplate, "Item", collectionView);
			Assert.Equal(UIAccessibilityTrait.Button, platformView.AccessibilityTraits & UIAccessibilityTrait.Button);

			TemplatedCell cell = measurementCell;
			if (transferContent)
			{
				displayedCell.UseContent(measurementCell);
				cell = displayedCell;
			}

			collectionView.SelectionMode = SelectionMode.None;
			cell.UpdateAccessibilityTraits(collectionView);

			Assert.Equal(developerAssignedButtonTrait ? UIAccessibilityTrait.Button : (UIAccessibilityTrait)0,
				platformView.AccessibilityTraits & UIAccessibilityTrait.Button);
		});
	}
}
