using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests
	{
		static ControlsModalWrapper GetSheetWrapper(Page sheet) =>
			Assert.IsType<ControlsModalWrapper>((sheet.Handler as IPlatformViewHandler)?.ViewController?.ParentViewController);

		static void AssertPresentedAsBottomSheet(Page sheet)
		{
			var wrapper = GetSheetWrapper(sheet);
			Assert.Equal(UIModalPresentationStyle.PageSheet, wrapper.ModalPresentationStyle);
			Assert.NotNull(wrapper.SheetPresentationController);
			Assert.NotNull(wrapper.PresentingViewController);
		}

		static bool IsBottomSheetOnScreen(Window window)
		{
			var controller = (window.Page?.Handler as IPlatformViewHandler)?.ViewController;
			var root = controller?.View?.Window?.RootViewController;
			return controller?.PresentedViewController is ControlsModalWrapper || root?.PresentedViewController is ControlsModalWrapper;
		}

		// UIKit doesn't notify its delegate about programmatic dismissals, so the user's swipe is simulated by dismissing natively and
		// sending the notification UIKit sends after a swipe.
		static async Task DismissBottomSheetAsUser(Page sheet)
		{
			var wrapper = GetSheetWrapper(sheet);
			var presentationController = wrapper.PresentationController!;
			await wrapper.PresentingViewController!.DismissViewControllerAsync(false);
			wrapper.DidDismiss(presentationController);
		}

		static async Task AssertNativeSheetState(Page sheet, int expectedStops, int expectedIndex, bool dismissible)
		{
			var wrapper = GetSheetWrapper(sheet);
			var sheetController = wrapper.SheetPresentationController!;

			await AssertEventually(() => sheetController.Detents.Length == expectedStops);

			var expectedIdentifier = (sheetController.Detents[expectedIndex].ValueForKey(new NSString("identifier")) as NSString)?.ToString();
			await AssertEventually(() =>
				(sheetController.ValueForKey(new NSString("selectedDetentIdentifier")) as NSString)?.ToString() == expectedIdentifier);

			Assert.Equal(!dismissible, wrapper.ModalInPresentation);
			Assert.Equal(BottomSheet.GetHasDragHandle(sheet), sheetController.PrefersGrabberVisible);
		}

		// The user dragging the sheet to another detent: UIKit changes the selection and notifies the delegate.
		static Task SelectDetentAsUser(Page sheet, int index)
		{
			var wrapper = GetSheetWrapper(sheet);
			var sheetController = wrapper.SheetPresentationController!;
			sheetController.SetValueForKey(sheetController.Detents[index].ValueForKey(new NSString("identifier")), new NSString("selectedDetentIdentifier"));
			wrapper.DidChangeSelectedDetentIdentifier(sheetController);
			return Task.CompletedTask;
		}
	}
}
