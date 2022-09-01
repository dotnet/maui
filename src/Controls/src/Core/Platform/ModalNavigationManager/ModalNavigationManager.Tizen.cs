#nullable enable

using System.Threading.Tasks;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		NavigationStack _modalStack => WindowMauiContext.GetModalStack();
		IPageController CurrentPageController => _navModel.CurrentPage;

		partial void OnPageAttachedHandler()
		{
			WindowMauiContext.GetPlatformWindow().SetBackButtonPressedHandler(OnBackButtonPressed);
		}

		public async Task<Page> PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();
			((IPageController)modal).SendDisappearing();

			var modalRenderer = modal.Handler as IPlatformViewHandler;
			if (modalRenderer != null)
			{
				await _modalStack.Pop(animated);
				CurrentPageController?.SendAppearing();
				(modal.Handler as IPlatformViewHandler)?.Dispose();
			}
			return modal;
		}

		public async Task PushModalAsync(Page modal, bool animated)
		{
			CurrentPageController?.SendDisappearing();
			_navModel.PushModal(modal);

			var nativeView = modal.ToPlatform(WindowMauiContext);

			await _modalStack.Push(nativeView, animated);

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == modal)
				((IPageController)modal).SendAppearing();
		}

		bool OnBackButtonPressed()
		{
			Page root = _navModel.LastRoot;
			bool handled = root?.SendBackButtonPressed() ?? false;

			return handled;
		}
	}
}
