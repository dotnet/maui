#nullable enable

using System.Threading.Tasks;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// The in-box Tizen modal presentation, written against the public
	/// <see cref="IModalNavigationPlatform"/> seam.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the framework's own consumer of the extensibility contract: everything Tizen needs to
	/// present and dismiss a modal is expressed through <see cref="IModalNavigationHost"/>, with no
	/// access to <c>ModalNavigationManager</c> internals. An external backend can be written the same
	/// way, and this type is the reference for doing so.
	/// </para>
	/// <para>
	/// It is used when no <see cref="IModalNavigationPlatformFactory"/> is registered. A registered
	/// factory takes precedence and this type is never constructed.
	/// </para>
	/// </remarks>
	internal sealed class TizenModalNavigationPlatform : IModalNavigationPlatform
	{
		readonly IModalNavigationHost _host;

		public TizenModalNavigationPlatform(IModalNavigationHost host)
		{
			_host = host;
		}

		NavigationStack ModalStack => _host.MauiContext.GetModalStack();

		public bool IsReady => true;

		public void PageAttached() =>
			_host.MauiContext.GetPlatformWindow().SetBackButtonPressedHandler(OnBackButtonPressed);

		public async Task PushModalAsync(Page modal, bool animated)
		{
			// The framework has already put the modal on IModalNavigationHost.PlatformModalStack, which
			// does not affect CurrentPage, so this still observes the page that is going away.
			(_host.CurrentPage as IPageController)?.SendDisappearing();

			var nativeView = modal.ToPlatform(_host.MauiContext);

			await ModalStack.Push(nativeView, animated);

			// Verify that the modal is still on the stack
			if (_host.CurrentPage == modal)
				((IPageController)modal).SendAppearing();
		}

		public async Task PopModalAsync(Page modal, bool animated)
		{
			((IPageController)modal).SendDisappearing();

			// A modal with no platform handler was never realized, so there is nothing to dismiss. This
			// is also what makes the method idempotent for a modal the platform already tore down.
			if (modal.Handler is IPlatformViewHandler handler)
			{
				await ModalStack.Pop(animated);
				(_host.CurrentPage as IPageController)?.SendAppearing();
				handler.Dispose();
			}
		}

		// The navigation stack and the platform window are owned by the window's platform side, not by
		// this instance, so there is nothing of our own to release.
		public void Dispose()
		{
		}

		bool OnBackButtonPressed() =>
			_host.CurrentPage?.SendBackButtonPressed() ?? false;
	}
}
