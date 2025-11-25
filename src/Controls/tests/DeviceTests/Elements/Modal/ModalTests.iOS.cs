using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Theory]
		[ClassData(typeof(PageTypes))]
		public async Task PushModalUsingTransparencies(Page rootPage, Page modalPage)
		{
			SetupBuilder();

			var expected = Colors.Red;

			rootPage.BackgroundColor = expected;
			modalPage.BackgroundColor = Colors.Transparent;

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (handler) =>
				{
					var currentPage = rootPage.GetCurrentPage();
					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					Assert.Single(currentPage.Navigation.ModalStack);

					var rootView = handler.PlatformView;
					Assert.NotNull(rootView);

					var currentView = currentPage.Handler.PlatformView as UIView;
					Assert.NotNull(currentView);
					Assert.NotNull(currentView.Window);
				});
		}

		[Fact(DisplayName = "Reusing FormSheet Modal Does Not Shrink")]
		public async Task ReusingFormSheetModalDoesNotShrink()
		{
			SetupBuilder();

			var rootPage = new ContentPage
			{
				Content = new Label { Text = "Root Page" }
			};

			var modalPage = new ContentPage
			{
				Content = new Label { Text = "Modal Page", AutomationId = "ModalLabel" }
			};

			// Set FormSheet presentation style
			modalPage.On<iOS>().SetModalPresentationStyle(Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.FormSheet);

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (handler) =>
				{
					// First push
					await rootPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					var modalHandler = modalPage.Handler as IPlatformViewHandler;
					var firstWidth = modalHandler?.ViewController?.View?.Frame.Width ?? 0;
					var firstHeight = modalHandler?.ViewController?.View?.Frame.Height ?? 0;

					// Pop the modal
					await rootPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					// Second push (reusing the same page)
					await rootPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					modalHandler = modalPage.Handler as IPlatformViewHandler;
					var secondWidth = modalHandler?.ViewController?.View?.Frame.Width ?? 0;
					var secondHeight = modalHandler?.ViewController?.View?.Frame.Height ?? 0;

					// Pop the modal
					await rootPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					// Third push (reusing the same page again)
					await rootPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					modalHandler = modalPage.Handler as IPlatformViewHandler;
					var thirdWidth = modalHandler?.ViewController?.View?.Frame.Width ?? 0;
					var thirdHeight = modalHandler?.ViewController?.View?.Frame.Height ?? 0;

					await rootPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					// Verify sizes are consistent (within tolerance for floating point)
					const double tolerance = 1.0; // Allow small floating point differences
					
					Assert.True(firstWidth > 0, "First width should be greater than 0");
					Assert.True(firstHeight > 0, "First height should be greater than 0");

					// The widths and heights should be approximately equal across all pushes
					Assert.True(Math.Abs(firstWidth - secondWidth) < tolerance, $"Second push width ({secondWidth}) differs from first ({firstWidth})");
					Assert.True(Math.Abs(firstHeight - secondHeight) < tolerance, $"Second push height ({secondHeight}) differs from first ({firstHeight})");
					Assert.True(Math.Abs(firstWidth - thirdWidth) < tolerance, $"Third push width ({thirdWidth}) differs from first ({firstWidth})");
					Assert.True(Math.Abs(firstHeight - thirdHeight) < tolerance, $"Third push height ({thirdHeight}) differs from first ({firstHeight})");
				});
		}
	}
}
