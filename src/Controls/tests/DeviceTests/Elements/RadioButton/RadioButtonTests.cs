using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RadioButton)]
	public partial class RadioButtonTests : ControlsHandlerTestBase
	{
#if WINDOWS
		[Theory(DisplayName = "IsChecked Initializes Correctly")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task IsCheckedInitializesCorrectly(bool isChecked)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<RadioButton, RadioButtonHandler>();
				});
			});


			await InvokeOnMainThreadAsync(async () =>
			{
				bool xplatIsChecked = isChecked;
				var radioButton = new RadioButton() { IsChecked = xplatIsChecked };
				bool expectedValue = isChecked;
				var layoutFirst = new VerticalStackLayout();
				var rdFirst = new RadioButton { GroupName = "FirstGroup", IsChecked = xplatIsChecked };
				layoutFirst.Add(rdFirst);
				layoutFirst.Add(new RadioButton { GroupName = "FirstGroup" });
				layoutFirst.Add(new RadioButton { GroupName = "FirstGroup" });
				var layoutSecond = new VerticalStackLayout();
				layoutSecond.Add(new RadioButton { GroupName = "SecondGroup" });
				var rdSecond = new RadioButton { GroupName = "SecondGroup", IsChecked = xplatIsChecked };
				layoutSecond.Add(rdSecond);
				layoutSecond.Add(new RadioButton { GroupName = "SecondGroup" });
				var layout = new VerticalStackLayout
			{
				layoutFirst,
				layoutSecond
			};
				var valuesFirst = await GetValueAsync(rdFirst, (handler) => { return new { ViewValue = rdFirst.IsChecked, PlatformViewValue = GetNativeIsChecked(handler as RadioButtonHandler) }; });
				var valuesSecond = await GetValueAsync(rdSecond, (handler) => { return new { ViewValue = rdSecond.IsChecked, PlatformViewValue = GetNativeIsChecked(handler as RadioButtonHandler) }; });
				Assert.Equal(xplatIsChecked, valuesFirst.ViewValue);
				Assert.Equal(expectedValue, valuesFirst.PlatformViewValue);
				Assert.Equal(xplatIsChecked, valuesSecond.ViewValue);
				Assert.Equal(expectedValue, valuesSecond.PlatformViewValue);
			});
		}
#endif

		[Fact]
		[Category(TestCategory.RadioButton)]
		public async Task RadioButtonHandlerShouldNotLeak()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Shape, ShapeViewHandler>();
					handlers.AddHandler<ContentPresenter, ContentViewHandler>();
					handlers.AddHandler<RadioButton, RadioButtonHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});

			WeakReference radioButtonHandlerRef = null;
			WeakReference radioButtonPlatformRef = null;
			WeakReference layoutHandlerRef = null;
			WeakReference layoutPlatformRef = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var page = new ContentPage();
				var layout = new Grid();
				var radioButton = new RadioButton();
				radioButton.Content = "Dog";
				radioButton.Value = "dog";

				layout.SetValue(RadioButtonGroup.GroupNameProperty, "group");
				layout.SetValue(RadioButtonGroup.SelectedValueProperty, "dog");
				layout.Add(radioButton);
				page.Content = layout;

				var handler = CreateHandler<LayoutHandler>(layout);
				radioButtonHandlerRef = new WeakReference(radioButton.Handler);
				radioButtonPlatformRef = new WeakReference(radioButton.Handler.PlatformView);
				layoutHandlerRef = new WeakReference(handler);
				layoutPlatformRef = new WeakReference(handler.PlatformView);
			});

			await AssertionExtensions.WaitForGC(radioButtonHandlerRef, layoutHandlerRef, layoutPlatformRef, radioButtonPlatformRef);
		}
	}
}