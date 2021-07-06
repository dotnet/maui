#nullable enable

using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ModalStack _modalStack => MauiContext.Context!.ModalStack;
		IPageController CurrentPageController => _navModel.CurrentPage;

		partial void OnPageAttachedHandler()
		{
			MauiContext.Context!.SetBackButtonPressedHandler(OnBackButtonPressed);
		}

		public  Task<Page> PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();
			((IPageController)modal).SendDisappearing();
			var source = new TaskCompletionSource<Page>();

			var modalRenderer = modal.Handler as INativeViewHandler;
			if (modalRenderer != null)
			{
				// TODO. Need to implement animated
				_modalStack.Pop();
				source.TrySetResult(modal);
				CurrentPageController?.SendAppearing();
			}
			return source.Task;
		}

		public Task PushModalAsync(Page modal, bool animated)
		{
			CurrentPageController?.SendDisappearing();
			_navModel.PushModal(modal);

			var nativeView = modal.ToNative(MauiContext);
			_modalStack.Push(nativeView);

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == modal)
				((IPageController)modal).SendAppearing();

			return Task.CompletedTask;
		}

		bool OnBackButtonPressed()
		{
			Page root = _navModel.LastRoot;
			bool handled = root?.SendBackButtonPressed() ?? false;

			return handled;
		}
	}
}
