using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewTests
	{
		// Regression test for https://github.com/dotnet/maui/issues/34272
		// An ObjectDisposedException was thrown when closing a Windows app that had style triggers
		// bound to IsFocused, because FocusManager.LostFocus fired asynchronously AFTER the
		// IServiceScope was disposed during window teardown. The fix adds a guard in
		// UpdateIsFocused that checks whether the MAUI window handler is still connected.
		[Fact(DisplayName = "UpdateIsFocused skips update when MAUI window handler is absent (teardown guard)")]
		public async Task UpdateIsFocused_WhenMauiWindowAbsent_IsFocusedUnchanged()
		{

			var entry = new Entry();
			((IView)entry).IsFocused = true;

			await InvokeOnMainThreadAsync(() =>
			{
				// In the device test context (ContextStub), GetOptionalPlatformWindow() returns
				// a WinUI window, but GetWindow() returns null because no MAUI window is
				// registered in Application.Windows. This mirrors the post-teardown state:
				// after Window.Destroying() disconnects the MAUI window handler, GetWindow()
				// can no longer find a matching entry, so the guard triggers.
				var handler = CreateHandler<EntryHandler>(entry);

				// Act: invoke UpdateIsFocused(false) via reflection, simulating the async
				// FocusManager.LostFocus callback that fires after window scope disposal.
				var updateIsFocused = typeof(ViewHandler).GetMethod(
					"UpdateIsFocused",
					BindingFlags.Instance | BindingFlags.NonPublic);

				updateIsFocused!.Invoke(handler, [false]);

				// Assert: the teardown guard should have detected the absent MAUI window
				// handler and returned early, leaving IsFocused unchanged.
				// Without the fix, IsFocused would be set to false, triggering style triggers
				// that re
				//
				// solve IFontManager from the disposed IServiceScope → ObjectDisposedException.
				Assert.True(entry.IsFocused);

				((IViewHandler)handler).DisconnectHandler();
			});
		}
	}
}
