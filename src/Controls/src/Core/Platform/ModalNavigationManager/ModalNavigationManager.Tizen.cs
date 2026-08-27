#nullable enable

using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		TizenModalNavigationPlatform? _tizenPlatform;

		// The built-in Tizen presentation is itself an IModalNavigationPlatform, so the fallback path
		// exercises the same public contract an external backend implements. Created lazily because a
		// registered IModalNavigationPlatformFactory takes precedence and these members never run.
		TizenModalNavigationPlatform TizenPlatform =>
			_tizenPlatform ??= new TizenModalNavigationPlatform(this);

		Task SyncModalStackWhenPlatformIsReadyCoreAsync() =>
			SyncPlatformModalStackAsync();

		bool IsModalPlatformReadyCore => TizenPlatform.IsReady;

		partial void OnPageAttachedHandler() =>
			TizenPlatform.PageAttached();

		async Task<Page> PopModalPlatformCoreAsync(bool animated)
		{
			Page modal = CurrentPlatformModalPage;

			// Mirrors PopModalWithOverrideAsync: the framework owns the platform stack and removes the
			// modal before the dismissal, so CurrentPlatformPage already refers to the revealed page.
			_platformModalPages.Remove(modal);

			await TizenPlatform.PopModalAsync(modal, animated);
			return modal;
		}

		Task PushModalPlatformCoreAsync(Page modal, bool animated)
		{
			// Mirrors PushModalWithOverrideAsync. The add moved ahead of the platform call to match the
			// seam contract; it is equivalent because CurrentPage is computed from the requested stack,
			// not from _platformModalPages.
			_platformModalPages.Add(modal);

			return TizenPlatform.PushModalAsync(modal, animated);
		}
	}
}
