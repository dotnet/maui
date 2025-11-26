using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CheckBox)]
	public partial class CheckBoxTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CheckBox, CheckBoxHandler>();
				});
			});
		}

		[Theory("Checkbox Background Updates Correctly With BackgroundColor Property"
#if WINDOWS
			,Skip = "Failing"
#endif
			)]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		public async Task UpdatingCheckBoxBackgroundColorUpdatesBackground(string colorStr)
		{
			var color = Color.Parse(colorStr);

			var checkBox = new CheckBox
			{
				BackgroundColor = Colors.HotPink
			};

			checkBox.BackgroundColor = color;

			await ValidateHasColor<CheckBoxHandler>(checkBox, color);
		}

		[Fact]
		[Description("The Opacity property of a CheckBox should match with native Opacity")]
		public async Task VerifyCheckBoxOpacityProperty()
		{
			var checkBox = new CheckBox
			{
				Opacity = 0.35f
			};
			var expectedValue = checkBox.Opacity;

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

		[Fact]
		[Description("The IsVisible property of a CheckBox should match with native IsVisible")]
		public async Task VerifyCheckBoxIsVisibleProperty()
		{
			var checkBox = new CheckBox();
			checkBox.IsVisible = false;
			var expectedValue = checkBox.IsVisible;

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			await InvokeOnMainThreadAsync(async () =>
   			{
				   var isVisible = await GetPlatformIsVisible(handler);
				   Assert.Equal(expectedValue, isVisible);
			   });
		}
	}
}