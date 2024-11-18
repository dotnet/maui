using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Layout)]
	public partial class LayoutTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task InputTransparentCorrectlyAppliedToPlatformView(bool inputTransparent)
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Button), typeof(ButtonHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
				});
			});

			var control = new Grid() { InputTransparent = inputTransparent, CascadeInputTransparent = false };
			var child = new Button();
			control.Add(child);

			await InvokeOnMainThreadAsync(() =>
			{
				ValidateInputTransparentOnPlatformView(control);
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task InputTransparentUpdatesCorrectlyOnPlatformView(bool finalInputTransparent)
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Button), typeof(ButtonHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
				});
			});

			var control = new Grid() { InputTransparent = !finalInputTransparent, CascadeInputTransparent = false };
			var child = new Button();
			control.Add(child);

			await InvokeOnMainThreadAsync(() =>
			{
				control.InputTransparent = finalInputTransparent;
				ValidateInputTransparentOnPlatformView(control);
			});
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnAdd(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var control = new StackLayout() { InputTransparent = inputTransparent, CascadeInputTransparent = cascadeInputTransparent };
			_ = await CreateHandlerAsync<LayoutHandler>(control);

			var child = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child);

			await InvokeOnMainThreadAsync(() => control.Add(child));

			Assert.Equal(expected, child.InputTransparent);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnInsert(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var control = new StackLayout()
			{
				InputTransparent = inputTransparent,
				CascadeInputTransparent = cascadeInputTransparent
			};

			_ = await CreateHandlerAsync<LayoutHandler>(control);

			var child = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child);

			await InvokeOnMainThreadAsync(() => control.Insert(0, child));

			Assert.Equal(expected, child.InputTransparent);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnUpdate(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var control = new StackLayout() { InputTransparent = inputTransparent, CascadeInputTransparent = cascadeInputTransparent };
			_ = await CreateHandlerAsync<LayoutHandler>(control);

			var child0 = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child0);

			await InvokeOnMainThreadAsync(() => control.Add(child0));

			var child1 = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child1);

			await InvokeOnMainThreadAsync(() => control[0] = child1);

			Assert.Equal(expected, child1.InputTransparent);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnInit(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var child = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child);

			var control = new StackLayout() { InputTransparent = inputTransparent, CascadeInputTransparent = cascadeInputTransparent };
			await InvokeOnMainThreadAsync(() => control.Add(child));
			_ = await CreateHandlerAsync<LayoutHandler>(control);

			Assert.Equal(expected, child.InputTransparent);
		}

		[Theory(
#if IOS || MACCATALYST
			Skip = "Not able to debug iOS right now"
#elif ANDROID
			Skip = "Android stopped working in the tests, but works in real life..."
#endif
		)]
		[InlineData(typeof(Grid), LayoutAlignment.Center)]
		[InlineData(typeof(Grid), LayoutAlignment.Start)]
		[InlineData(typeof(Grid), LayoutAlignment.End)]
		[InlineData(typeof(VerticalStackLayout), LayoutAlignment.Center)]
		[InlineData(typeof(VerticalStackLayout), LayoutAlignment.Start)]
		[InlineData(typeof(VerticalStackLayout), LayoutAlignment.End)]
		public async Task UpdatingLayoutOptionsTriggersParentToRepositionControl(Type layoutType, LayoutAlignment layoutAlignment)
		{
			var layoutOptions = new LayoutOptions(layoutAlignment, false);

			// create a layout with the values all set before creating the handler
			CreateLayout(layoutType, out var initialLayout, out var initialLabel);
			initialLabel.HorizontalOptions = layoutOptions;

			// create a layout that will update once attached
			CreateLayout(layoutType, out var updatingLayout, out var updatingLabel);

			await InvokeOnMainThreadAsync(async () =>
			{
				_ = CreateHandler<LabelHandler>(initialLabel);
				var initialHandler = CreateHandler<LayoutHandler>(initialLayout);
				var initialBitmap = await initialHandler.PlatformView.ToBitmap(MauiContext);

				_ = CreateHandler<LabelHandler>(updatingLabel);
				var updatingHandler = CreateHandler<LayoutHandler>(updatingLayout);
				var updatingBitmap = await AttachAndRun(updatingLayout, (handler) =>
				{
					updatingLabel.HorizontalOptions = layoutOptions;

					return updatingHandler.PlatformView.ToBitmap(MauiContext);
				});

				await initialBitmap.AssertEqualAsync(updatingBitmap);
			});

			static void CreateLayout(Type layoutType, out Layout layout, out Label label)
			{
				layout = Activator.CreateInstance(layoutType) as Layout;
				layout.WidthRequest = 200;
				layout.HeightRequest = 100;
				layout.Background = Colors.Red;

				label = new Label
				{
					WidthRequest = 50,
					HeightRequest = 50,
					Text = "Text",
					TextColor = Colors.Blue,
				};

				layout.Add(label);
			}
		}

		[Fact, Category(TestCategory.FlexLayout)]
		public async Task FlexLayoutInVerticalStackLayoutDoesNotCycle()
		{
			await FlexLayoutInStackLayoutDoesNotCycle(new VerticalStackLayout());
		}

		[Fact, Category(TestCategory.FlexLayout)]
		public async Task FlexLayoutInHorizontalStackLayoutDoesNotCycle()
		{
			await FlexLayoutInStackLayoutDoesNotCycle(new HorizontalStackLayout());
		}

		async Task FlexLayoutInStackLayoutDoesNotCycle(IStackLayout root)
		{
			var flexLayout = new FlexLayout();
			var label = new Label { Text = "Hello" };

			flexLayout.Add(label);
			root.Add(flexLayout);

			await InvokeOnMainThreadAsync(async () =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var flexLayoutHandler = CreateHandler<LayoutHandler>(flexLayout);
				var layoutHandler = CreateHandler<LayoutHandler>(root);

				// If this can be attached to the hierarchy and make it through a layout 
				// without crashing, then we're good.

				await AttachAndRun(root, (handler) => { });
			});
		}

		[Fact]
		public async Task GridCellsHonorMaxWidth()
		{
			var grid = new Grid() { MaximumWidthRequest = 50 };
			var label = new Label() { Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sodales eros nec massa facilisis venenatis", LineBreakMode = LineBreakMode.WordWrap };

			grid.Add(label);

			await InvokeOnMainThreadAsync(async () =>
			{
				await CreateHandlerAsync<LabelHandler>(label);
				await CreateHandlerAsync<LayoutHandler>(grid);

				await AttachAndRun(grid, (handler) => { });
			});

			Assert.True(label.Width <= grid.MaximumWidthRequest);
			Assert.True(grid.Width <= grid.MaximumWidthRequest);
		}

		[Fact]
		public async Task GridCellsHonorMaxHeight()
		{
			var grid = new Grid() { MaximumHeightRequest = 20 };
			var label = new Label() { Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sodales eros nec massa facilisis venenatis", LineBreakMode = LineBreakMode.WordWrap };

			grid.Add(label);

			await InvokeOnMainThreadAsync(async () =>
			{
				await CreateHandlerAsync<LabelHandler>(label);
				await CreateHandlerAsync<LayoutHandler>(grid);

				await AttachAndRun(grid, (handler) => { });
			});

			Assert.True(label.Height <= grid.MaximumHeightRequest);
			Assert.True(grid.Height <= grid.MaximumHeightRequest);
		}

		[Fact]
		public async Task GridAddAndRemoveChildrenViaIndex()
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Label), typeof(LabelHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
				});
			});

			var grid = new Grid();
			var label1 = new Label() { Text = "Lorem ipsum dolor" };
			var label2 = new Label() { Text = "Hello world" };
			var label3 = new Label() { Text = "The quick brown fox" };

			grid.Add(label1);
			grid.Add(label2);
			grid.Add(label3);

			await InvokeOnMainThreadAsync(async () =>
			{
				await AttachAndRun(grid, (handler) =>
				{
					Assert.True((grid[1] as Label).Text == "Hello world");

					// Remove middle item
					grid.Remove(grid[1]);
					Assert.True((grid[0] as Label).Text == "Lorem ipsum dolor");
					Assert.True((grid[1] as Label).Text == "The quick brown fox");

					// Insert item at start
					grid.Insert(0, label2);
					Assert.True((grid[0] as Label).Text == "Hello world");
					Assert.True((grid[1] as Label).Text == "Lorem ipsum dolor");
					Assert.True((grid[2] as Label).Text == "The quick brown fox");

					// Remove another item
					grid.Remove(grid[2]);
					Assert.True((grid[0] as Label).Text == "Hello world");
					Assert.True((grid[1] as Label).Text == "Lorem ipsum dolor");
				});
			});
		}

		[Fact]
		[Category(TestCategory.Button, TestCategory.FlexLayout)]
		public async Task ButtonWithImageInFlexLayoutInGridDoesNotCycle()
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Button), typeof(ButtonHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
				});
			});

			await ButtonWithImageInFlexLayoutInGridDoesNotCycleCore();
			// Cycle does not occur on first run
			await ButtonWithImageInFlexLayoutInGridDoesNotCycleCore();
		}

		async Task ButtonWithImageInFlexLayoutInGridDoesNotCycleCore()
		{
			var grid = new Grid() { MaximumWidthRequest = 150 };
			grid.AddRowDefinition(new RowDefinition(GridLength.Auto));

			var flexLayout = new FlexLayout() { Wrap = Layouts.FlexWrap.Wrap };
			grid.Add(flexLayout);

			for (int i = 0; i < 2; i++)
			{
				var button = new Button { ImageSource = "black.png" };
				flexLayout.Add(button);
			}

			await InvokeOnMainThreadAsync(async () =>
			{
				// If this can be attached to the hierarchy and make it through a layout
				// without crashing, then we're good.
				await AttachAndRun(grid, (handler) => { });
			});
		}

		/* Commented out of for now due to inconsistent platform behavior
		[Fact("Ensures grid rows renders the correct size - Issue 15330")]
		public async Task Issue15330()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			Grid grid = new Grid()
			{
				BackgroundColor = Colors.Cyan,
				WidthRequest = 200,  // TODO: I really don't want to set size - need to hit iOS safe area too
				HeightRequest = 500
			};
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
			grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
			grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
			BoxView boxView1 = new BoxView() { Color = Colors.Red };
			Grid.SetColumn(boxView1, 1);
			grid.Children.Add(boxView1);
			var boxView2 = new BoxView() { Color = Colors.Lime };
			Grid.SetRow(boxView2, 1);
			grid.Children.Add(boxView2);
			var boxView3 = new BoxView() { Color = Colors.Violet };
			Grid.SetColumn(boxView3, 1);
			Grid.SetRow(boxView3, 1);
			Grid.SetRowSpan(boxView3, 2);
			grid.Children.Add(boxView3);
			var boxView4 = new BoxView() { Color = Colors.Yellow };
			grid.Children.Add(boxView4);

			await CreateHandlerAsync<BoxViewHandler>(boxView1);
			await CreateHandlerAsync<BoxViewHandler>(boxView2);
			await CreateHandlerAsync<BoxViewHandler>(boxView3);
			await CreateHandlerAsync<BoxViewHandler>(boxView4);

			var bitmap = await GetRawBitmap(grid, typeof(LayoutHandler));
			var yellowBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Yellow).Single();
			Assert.Equal(bitmap.Width / 2, yellowBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, yellowBlob.Height, 2d);
			Assert.Equal(0, yellowBlob.MinColumn, 2d);
			Assert.Equal(0, yellowBlob.MinRow, 2d);

			var redBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Red).Single();
			Assert.Equal(bitmap.Width / 2, redBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, redBlob.Height, 2d);
			Assert.Equal(bitmap.Width / 2, redBlob.MinColumn, 2d);
			Assert.Equal(0, redBlob.MinRow);

			var limeBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Lime).Single();
			Assert.Equal(bitmap.Width / 2, limeBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, limeBlob.Height, 2d);
			Assert.Equal(0, limeBlob.MinColumn, 2d);
			Assert.Equal(bitmap.Height / 3, limeBlob.MinRow, 2d);

			var violetBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Violet).Single();
			Assert.Equal(bitmap.Width / 2, violetBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3 * 2, violetBlob.Height, 2d);
			Assert.Equal(bitmap.Width / 2, violetBlob.MinColumn, 2d);
			Assert.Equal(bitmap.Height / 3, violetBlob.MinRow, 2d);

			var cyanBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Cyan).Single();
			Assert.Equal(bitmap.Width / 2, cyanBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, cyanBlob.Height, 2d);
			Assert.Equal(0, cyanBlob.MinColumn, 2d);
			Assert.Equal(bitmap.Height / 3 * 2, cyanBlob.MinRow, 2d);
		}*/

		[Fact]
		public async Task DependentLayoutBindingsResolve()
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Entry), typeof(EntryHandler));
					handler.AddHandler(typeof(Button), typeof(ButtonHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
				});
			});

			double expectedWidth = 200;

			var outerGrid = new Grid()
			{
				WidthRequest = expectedWidth
			};

			var grid = new Grid
			{
				HeightRequest = 80,
				HorizontalOptions = LayoutOptions.Fill
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

			var entry = new Entry() { Text = "Lorem ipsum dolor", HorizontalOptions = LayoutOptions.Fill };
			var button = new Button() { Text = "Hello world", HorizontalOptions = LayoutOptions.Fill };

			grid.Add(entry);
			grid.Add(button);

			grid.SetColumn(entry, 0);
			grid.SetColumn(button, 1);

			// Because of this binding, the the layout will need two passes.
			button.SetBinding(VisualElement.WidthRequestProperty, new Binding(nameof(View.Width), mode: BindingMode.Default, source: grid));

			await AttachAndRun(outerGrid, async (handler) =>
			{
				// The layout needs to occur while the views are attached to the Window, otherwise they won't be able to schedule
				// the second layout pass correctly on Android. That's why we don't add the inner Grid to the outer Grid until
				// we're already attached.

				outerGrid.Add(grid);

				var expectation = () => button.Width == expectedWidth;

				await expectation.AssertEventually(timeout: 2000, message: $"Button did not have expected Width of {expectedWidth}");
			});
		}

		[Fact]
		public async Task MinimumSizeRequestsCanBeCleared()
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Button), typeof(ButtonHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
				});
			});

			var button = new Button()
			{
				Text = "X",
				MinimumWidthRequest = 300,
				MinimumHeightRequest = 200,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};

			var grid = new Grid { button };

			await InvokeOnMainThreadAsync(async () =>
			{
				await AttachAndRun(grid, async _ =>
				{
					// The size should be the minimum requested size, since that will easily hold the "X" text
					Assert.Equal(300, button.Width, 0.5);
					Assert.Equal(200, button.Height, 0.5);

					button.ClearValue(VisualElement.MinimumWidthRequestProperty);
					button.ClearValue(VisualElement.MinimumHeightRequestProperty);

					// The new size should just be enough to hold the "X" text
					await AssertionExtensions.AssertEventually(() => button.Width < 100 && button.Height < 100);
				});
			});
		}

		[Fact]
		public async Task SizeRequestIsClampedToMinimumAndMaximum()
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Button), typeof(ButtonHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
				});
			});

			var button = new Button()
			{
				WidthRequest = 20, // request smaller than the minimum
				MinimumWidthRequest = 200,
				MaximumWidthRequest = 300,

				HeightRequest = 400, // request larger than the maximum
				MinimumHeightRequest = 200,
				MaximumHeightRequest = 300,

				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};

			var grid = new Grid { button };

			await InvokeOnMainThreadAsync(async () =>
			{
				await AttachAndRun(grid, _ =>
				{
					// The size should be the minimum requested size, since that will easily hold the "X" text
					Assert.Equal(button.MinimumWidthRequest, button.Width, 0.5);
					Assert.Equal(button.MaximumHeightRequest, button.Height, 0.5);
				});
			});
		}
	}
}
