using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
				});
			});
		}

		[Fact("Button Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();
				var button = new Button
				{
					Text = "Test"
				};
				layout.Add(button);

				var handler = CreateHandler<LayoutHandler>(layout);
				handlerReference = new WeakReference(button.Handler);
				platformViewReference = new WeakReference(button.Handler.PlatformView);
			});

			Assert.NotNull(handlerReference);
			Assert.NotNull(platformViewReference);

			// Several GCs required on iOS
			for (int i = 0; i < 5; i++)
			{
				if (!handlerReference.IsAlive && !platformViewReference.IsAlive)
					break;
				await Task.Yield();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			Assert.False(handlerReference.IsAlive, "Handler should not be alive!");
			Assert.False(platformViewReference.IsAlive, "PlatformView should not be alive!");
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<ButtonHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text };
			var handler = await CreateHandlerAsync<ButtonHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "LineBreakMode Initializes Correctly")]
		[InlineData(LineBreakMode.MiddleTruncation)]
		[InlineData(LineBreakMode.HeadTruncation)]
		[InlineData(LineBreakMode.TailTruncation)]
		[InlineData(LineBreakMode.WordWrap)]
		[InlineData(LineBreakMode.CharacterWrap)]
		[InlineData(LineBreakMode.NoWrap)]
		public async Task LineBreakModeInitializesCorrectly(LineBreakMode lineBreakMode)
		{
			var xplatLineBreakMode = lineBreakMode;

			var button = new Button()
			{
				LineBreakMode = xplatLineBreakMode
			};

			var expectedValue = xplatLineBreakMode.ToPlatform();

			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(expectedValue, GetPlatformLineBreakMode(handler));
			});
		}
	}
}