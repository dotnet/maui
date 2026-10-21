using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, false, true)]
		[InlineData(false, true, false)]
		[InlineData(false, true, true)]
		[InlineData(true, false, false)]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		[InlineData(true, true, true)]
		public async Task FlyoutCellAndTemplateRootPreserveButtonTrait(bool hasIcon, bool hasDescription, bool hasHint)
		{
			const string description = "Flyout destination";
			const string hint = "Opens the destination";

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				shell.ItemTemplate = new DataTemplate(() =>
				{
					var root = new Grid
					{
						ColumnDefinitions =
						{
							new ColumnDefinition { Width = GridLength.Auto },
							new ColumnDefinition { Width = GridLength.Star }
						}
					};
					var icon = new Image { WidthRequest = 24, HeightRequest = 24 };
					icon.SetBinding(Image.SourceProperty, nameof(FlyoutItem.FlyoutIcon));
					root.Add(icon);
					var title = new Label();
					title.SetBinding(Label.TextProperty, nameof(FlyoutItem.Title));
					root.Add(title, 1);

					if (hasDescription)
						SemanticProperties.SetDescription(root, description);
					if (hasHint)
						SemanticProperties.SetHint(root, hint);

					return root;
				});
				shell.Items.Add(new FlyoutItem
				{
					Title = "Destination",
					FlyoutIcon = hasIcon ? ImageSource.FromFile("white_tab.png") : null,
					Items = { new ShellContent { Content = new ContentPage() } }
				});
			}, async shell =>
			{
				var renderer = (ShellRenderer)shell.Handler;
				shell.FlyoutIsPresented = true;
				var flyout = renderer.ViewController.ChildViewControllers.OfType<ShellFlyoutContentRenderer>().Single();
				var table = flyout.View.FindDescendantView<UITableView>();
				Assert.NotNull(table);
				using var indexPath = NSIndexPath.FromRowSection(0, 0);
				await AssertEventually(() => table.CellAt(indexPath) is UIContainerCell);
				var cell = Assert.IsType<UIContainerCell>(table.CellAt(indexPath));
				var templateRoot = Assert.IsAssignableFrom<UIView>(cell.View.Handler.PlatformView);
				Assert.Contains(templateRoot, cell.ContentView.Subviews);

				Assert.True(cell.AccessibilityTraits.HasFlag(UIAccessibilityTrait.Button),
					"The enclosing flyout cell must retain the Button trait (issue #34913).");
				Assert.True(templateRoot.AccessibilityTraits.HasFlag(UIAccessibilityTrait.Button),
					"The flyout template native root must retain the Button trait (issue #34913).");

				// Template semantics map to its native root, not to the enclosing cell.
				if (hasDescription)
					Assert.Equal(description, templateRoot.AccessibilityLabel);
				if (hasHint)
					Assert.Equal(hint, templateRoot.AccessibilityHint);
			});
		}

#if !MACCATALYST
		[Theory]
		[InlineData(null)]
		[InlineData(0)]
		[InlineData(100)]
		public async Task FlyoutHeaderRendererHasTheRightHeight(int? topMargin)
		{
			var flyoutHeaderHeight = 250;
			var layout = new Grid() { HeightRequest = flyoutHeaderHeight };
			layout.Children.Add(new Button() { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill });

			if (topMargin is not null)
			{
				layout.Margin = new Thickness(0, topMargin.Value, 0, 0);
			}

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				shell.FlyoutHeader = layout;
				shell.FlyoutContent = new ScrollView() { Content = new Label() { Text = "FlyoutContent" } };
			},
			// RunShellTest's callback signature was changed from Func<Shell, ShellHandler, Task>
			// to Func<Shell, Task> so Android handler subclasses can inherit the test without
			// a platform-specific handler type leaking into the shared callback signature.
			// The handler is now accessed directly from shell.Handler inside the lambda.
			async shell =>
			{
				var handler = (ShellRenderer)shell.Handler;
				await OpenFlyout(handler);
				var flyout = GetFlyoutPlatformView(handler);
				var header = flyout.Subviews.OfType<ShellFlyoutHeaderContainer>().First();

				// The flyout header's height should be equal to the requested height + the top margin.
				// The safe area should not be accounted for.
				Assert.Equal(flyoutHeaderHeight + (topMargin ?? 0), header.Frame.Height);
			});
		}
#endif
	}
}