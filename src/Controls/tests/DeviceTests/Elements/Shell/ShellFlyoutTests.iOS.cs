using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using CoreGraphics;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
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

		[Fact]
		public async Task FlyoutHeaderLabelsAreArrangedWhenBoundTextChangesWithoutResizingHeader()
		{
			var displayName = new Label { FontSize = 16 };
			var orgName = new Label { FontSize = 16 };
			displayName.SetBinding(Label.TextProperty, "DisplayName");
			orgName.SetBinding(Label.TextProperty, "OrgName");

			var layout = new Grid
			{
				Margin = Thickness.Zero,
				HeightRequest = 120,
				Children =
				{
					new VerticalStackLayout
					{
						WidthRequest = 180,
						Spacing = 6,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						Children = { displayName, orgName }
					}
				}
			};

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				shell.FlyoutHeader = layout;
				shell.BindingContext = new { DisplayName = default(string), OrgName = default(string) };
			},
			async shell =>
			{
				await OnFrameSetToNotEmpty(layout);

				var header = Assert.IsType<ShellFlyoutHeaderContainer>(layout.ToPlatform().Superview);
				var displayNameView = Assert.IsAssignableFrom<UILabel>(displayName.Handler.PlatformView);
				var orgNameView = Assert.IsAssignableFrom<UILabel>(orgName.Handler.PlatformView);
				Assert.NotNull(header.Window);
				header.Superview.LayoutIfNeeded();
				header.LayoutIfNeeded();
				var originalHeaderFrame = header.Frame;

				shell.BindingContext = new
				{
					DisplayName = "Jane Doe",
					OrgName = "ArcGIS Enterprise: ArcGIS Enterprise"
				};

				// Do not request layout or resize the header: process only the pending invalidation.
				header.LayoutIfNeeded();

				Assert.Equal("Jane Doe", displayNameView.Text);
				Assert.Equal("ArcGIS Enterprise: ArcGIS Enterprise", orgNameView.Text);
				Assert.Equal(originalHeaderFrame, header.Frame);

				var displayNameFrame = displayNameView.GetFrameRelativeTo(header);
				var orgNameFrame = orgNameView.GetFrameRelativeTo(header);
				Assert.True(displayNameFrame.Width > 0 && displayNameFrame.Height > 0,
					"The updated display name must have a nonempty native frame.");
				Assert.True(orgNameFrame.Width > 0 && orgNameFrame.Height > displayNameFrame.Height,
					"The updated organization name must wrap over multiple lines.");

				var expectedOrgSize = orgNameView.SizeThatFits(
					new CGSize(orgNameFrame.Width, double.PositiveInfinity));
				AssertionExtensions.CloseEnough((double)expectedOrgSize.Height, orgNameFrame.Height,
					epsilon: 1, message: "The organization label must display all wrapped lines.");

				Assert.True(displayNameFrame.Top >= 0 && orgNameFrame.Bottom <= header.Bounds.Height,
					"The updated labels must fit inside the unchanged header.");
				Assert.True(orgNameFrame.Top >= displayNameFrame.Bottom,
					"The updated header labels must not overlap.");
			});
		}
	}
}