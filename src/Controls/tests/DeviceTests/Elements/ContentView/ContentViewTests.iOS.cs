using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Foundation;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using ContentView = Microsoft.Maui.Controls.ContentView;

namespace Microsoft.Maui.DeviceTests
{
	public class FocusableContentView : ContentView
	{
	}

	public class FocusablePlatformContentView : Microsoft.Maui.Platform.ContentView
	{
		public FocusablePlatformContentView()
		{
			UserInteractionEnabled = true;
		}

		public override bool CanBecomeFocused => true;
		public override bool CanBecomeFirstResponder => true;
	}

	public class FocusableContentViewHandler : ContentViewHandler
	{
		protected override Microsoft.Maui.Platform.ContentView CreatePlatformView() =>
			new FocusablePlatformContentView();
	}

	public class TestFocusUpdateContext : UIFocusUpdateContext
	{
		public TestFocusUpdateContext(UIView nextFocusedView, UIView previouslyFocusedView)
			: base(NSObjectFlag.Empty)
		{
			Next = nextFocusedView;
			Previous = previouslyFocusedView;
		}

		UIView Next { get; }
		UIView Previous { get; }

		public override UIView NextFocusedView => Next;
		public override UIView PreviouslyFocusedView => Previous;
	}

	public partial class ContentViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Subviews.Length;
		}

		static int GetContentChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Subviews[0].Subviews.Length;
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewHasExpectedSize()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			contentView.Content = label;

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewRespondsWhenViewAdded()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(0, size.Width);
			Assert.Equal(0, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ContentViewHandler>(contentView);

				contentView.Content = label;

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, updatedSize.Width);
			Assert.Equal(100, updatedSize.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewRespondsWhenViewRemoved()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			contentView.Content = label;

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ContentViewHandler>(contentView);

				contentView.Content = null;

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(0, updatedSize.Width);
			Assert.Equal(0, updatedSize.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewRespondsWhenViewUpdated()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			contentView.Content = label;

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ContentViewHandler>(contentView);

				label.HeightRequest = 300;

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, updatedSize.Width);
			Assert.Equal(300, updatedSize.Height);
		}

		[Fact]
		public async Task DidUpdateFocusDoesNotResetCustomParentFocusState()
		{
			SetupBuilder();

			int parentUnfocusedCount = 0;
			Entry templatedEntry = null;
			FocusableContentView contentView = null;

			contentView = new FocusableContentView
			{
				ControlTemplate = new ControlTemplate(() =>
				{
					templatedEntry = new Entry
					{
						Text = "Templated Inner Entry"
					};

					return new VerticalStackLayout
					{
						Children =
						{
							templatedEntry
						}
					};
				})
			};

			contentView.Unfocused += (_, _) => parentUnfocusedCount++;

			var page = new ContentPage
			{
				Content = contentView
			};

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				await AssertEventually(() => templatedEntry is not null);

				// Simulate a vendor control that has already mirrored its inner Entry focus
				// to the parent IsFocused property before the platform focus update arrives.
				((Microsoft.Maui.IView)contentView).IsFocused = true;

				var platformView = (FocusablePlatformContentView)contentView.ToPlatform();
				var context = new TestFocusUpdateContext(templatedEntry.ToPlatform(), new UIView());

				platformView.DidUpdateFocus(context, new UIFocusAnimationCoordinator());

				Assert.True(contentView.IsFocused);
				Assert.Equal(0, parentUnfocusedCount);
			});
		}
	}
}
