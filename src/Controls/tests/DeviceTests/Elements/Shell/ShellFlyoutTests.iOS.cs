using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Theory]
		[InlineData("Items", true)]
		[InlineData("FlyoutContent", true)]
		[InlineData("FlyoutContentTemplate", true)]
		[InlineData("Items", false)]
		[InlineData("FlyoutContent", false)]
		[InlineData("FlyoutContentTemplate", false)]
		public async Task FlyoutContentSizeCategoryChangeRefreshesFonts(string contentType, bool categoryChanged)
		{
			SetupBuilder(builder => builder.ConfigureMauiHandlers(handlers =>
				handlers.AddHandler<FlyoutFontTrackingLabel, FlyoutFontTrackingLabelHandler>()));

			var labels = new List<FlyoutFontTrackingLabel>();

			View CreateContent(string name)
			{
				var scaled = new FlyoutFontTrackingLabel { Text = name, FontSize = 20 };
				var unscaled = new FlyoutFontTrackingLabel
				{
					Text = $"{name} without scaling",
					FontSize = 20,
					FontAutoScalingEnabled = false
				};
				labels.Add(scaled);
				labels.Add(unscaled);

				return new VerticalStackLayout
				{
					new ContentView
					{
						Content = new VerticalStackLayout { scaled, unscaled }
					}
				};
			}

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				shell.FlyoutHeader = CreateContent("Header");
				shell.FlyoutFooter = CreateContent("Footer");

				if (contentType == "Items")
				{
					for (var i = 0; i < 2; i++)
					{
						var item = new FlyoutItem { Title = $"Item {i}", Items = { new ContentPage() } };
						Shell.SetItemTemplate(item, new DataTemplate(() => CreateContent(item.Title)));
						shell.Items.Add(item);
					}
				}
				else if (contentType == "FlyoutContent")
				{
					shell.FlyoutContent = CreateContent("Content");
				}
				else
				{
					shell.FlyoutContentTemplate = new DataTemplate(() => CreateContent("Templated content"));
				}
			},
			async shell =>
			{
				var renderer = (ShellRenderer)shell.Handler;
				await OpenFlyout(renderer);
				await AssertionExtensions.AssertEventually(() => labels.Count == (contentType == "Items" ? 8 : 6));

				foreach (var label in labels)
					await OnFrameSetToNotEmpty(label);

				shell.FlyoutIsPresented = false;
				var flyout = GetFlyoutPlatformView(renderer);
				await AssertionExtensions.AssertEventually(() => flyout.Frame.X < 0);

				var snapshots = labels.Select(label =>
				{
					var handler = Assert.IsType<FlyoutFontTrackingLabelHandler>(label.Handler);
					return (Label: label, Handler: handler, FontSize: handler.PlatformView.Font.PointSize,
						FontUpdates: handler.FontUpdateCount, MeasureInvalidations: label.MeasureInvalidationCount);
				}).ToArray();

				// Seed stale native fonts without changing managed properties or the device's settings.
				using var staleFont = UIFont.SystemFontOfSize(1);
				foreach (var snapshot in snapshots)
				{
					if (snapshot.Label.FontAutoScalingEnabled)
						snapshot.Handler.PlatformView.Font = staleFont;
				}

				var contentRenderer = renderer.ViewController.ChildViewControllers
					.OfType<ShellFlyoutContentRenderer>().Single();
				using var smallTraits = UITraitCollection.Create(UIContentSizeCategory.ExtraSmall);
				using var largeTraits = UITraitCollection.Create(UIContentSizeCategory.ExtraExtraExtraLarge);
				var previousTraits = categoryChanged
					? (contentRenderer.TraitCollection.PreferredContentSizeCategory == smallTraits.PreferredContentSizeCategory
						? largeTraits : smallTraits)
					: contentRenderer.TraitCollection;

#pragma warning disable CA1422 // Intentionally exercise the legacy callback, including when the fix's override is absent.
				contentRenderer.TraitCollectionDidChange(previousTraits);
#pragma warning restore CA1422

				foreach (var snapshot in snapshots)
				{
					if (categoryChanged && snapshot.Label.FontAutoScalingEnabled)
					{
						Assert.True(snapshot.Handler.FontUpdateCount > snapshot.FontUpdates, snapshot.Label.Text);
						Assert.True(snapshot.Label.MeasureInvalidationCount > snapshot.MeasureInvalidations, snapshot.Label.Text);
						Assert.Equal(snapshot.FontSize, snapshot.Handler.PlatformView.Font.PointSize);
					}
					else
					{
						Assert.Equal(snapshot.FontUpdates, snapshot.Handler.FontUpdateCount);
						Assert.Equal(snapshot.Label.FontAutoScalingEnabled ? staleFont.PointSize : snapshot.FontSize,
							snapshot.Handler.PlatformView.Font.PointSize);
					}
				}

				await OpenFlyout(renderer);
				Assert.Same(contentRenderer, renderer.ViewController.ChildViewControllers
					.OfType<ShellFlyoutContentRenderer>().Single());

				foreach (var snapshot in snapshots)
				{
					if (categoryChanged || !snapshot.Label.FontAutoScalingEnabled)
						Assert.Equal(snapshot.FontSize, snapshot.Handler.PlatformView.Font.PointSize);
				}
			});
		}

		sealed class FlyoutFontTrackingLabel : Label
		{
			public int MeasureInvalidationCount { get; private set; }

			protected override void InvalidateMeasureOverride()
			{
				MeasureInvalidationCount++;
				base.InvalidateMeasureOverride();
			}
		}

		sealed class FlyoutFontTrackingLabelHandler : LabelHandler
		{
			static readonly IPropertyMapper<ILabel, ILabelHandler> FontTrackingMapper = CreateFontTrackingMapper();

			public int FontUpdateCount { get; private set; }

			public FlyoutFontTrackingLabelHandler() : base(FontTrackingMapper)
			{
			}

			static IPropertyMapper<ILabel, ILabelHandler> CreateFontTrackingMapper()
			{
				var mapper = new PropertyMapper<ILabel, ILabelHandler>(LabelHandler.Mapper);
				mapper.AppendToMapping(nameof(ITextStyle.Font), (handler, view) =>
					((FlyoutFontTrackingLabelHandler)handler).FontUpdateCount++);
				return mapper;
			}
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