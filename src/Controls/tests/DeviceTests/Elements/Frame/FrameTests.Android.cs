using System;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameTests
	{
		[Fact(DisplayName = "Update Frame Content Test")]
		public async Task UpdateFrameContentTest()
		{
			SetupBuilder();

			var layout = new StackLayout();

#pragma warning disable CS0618 // Type or member is obsolete
			var frame = new Frame()
			{
				HeightRequest = 300,
				WidthRequest = 300,
				Content = new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = "Content"
				}
			};
#pragma warning restore CS0618 // Type or member is obsolete

			var button = new Button()
			{
				Text = "Remove Frame Content"
			};

			layout.Add(frame);
			layout.Add(button);

			var frameContent =
				await InvokeOnMainThreadAsync(async () =>
					await frame.ToPlatform(MauiContext).AttachAndRun(() =>
					{
						return frame.Content;
					})
				);

			Assert.NotNull(frameContent);

			var clicked = false;

			button.Clicked += delegate
			{
				frame.Content = null;
				clicked = true;
			};

			await PerformClick(button as IButton);

			Assert.True(clicked);

			Assert.Null(frame.Content);
		}

		[Fact]
		public async Task FrameContentAccountsForBorderThickness()
		{
			SetupBuilder();

			double contentSize = 50;
#pragma warning disable CS0618 // Type or member is obsolete
			var innerFrame = new Frame()
			{
				BorderColor = Colors.Black,
				BackgroundColor = Colors.Blue,
				CornerRadius = 0,
				Padding = new Thickness(0),
				Margin = new Thickness(0)
			};

			var outerFrame = new Frame()
			{
				Content = innerFrame,
				BorderColor = Colors.Black,
				CornerRadius = 0,
				Padding = new Thickness(0),
				Margin = new Thickness(0),
				WidthRequest = contentSize,
				HeightRequest = contentSize
			};
#pragma warning restore CS0618 // Type or member is obsolete

			var layout = new StackLayout()
			{
				Background = Colors.Purple,
				Children =
				{
					outerFrame
				}
			};

			// This tests that the border drawn fills the entire space it's expected to
			// There shouldn't be any white between the two frames
			await LayoutFrame(layout, outerFrame, 100, 100, () =>
			{
				return AssertionExtensions.AssertDoesNotContainColor(layout.ToPlatform(), Colors.White.ToPlatform(), MauiContext);
			});
		}

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler<ButtonHandler>(button)).PerformClick();
			});
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;
	}
}
