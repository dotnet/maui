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
		// CanInvokeMappers that checks MauiContext.IsWindowScopeDisposed, preventing
		// disposed-scope access when property mappers run after teardown.
		[Fact(DisplayName = "UpdateIsFocused does not throw after window scope is disposed (teardown guard)")]
		public async Task UpdateIsFocused_WhenWindowScopeDisposed_DoesNotThrow()
		{
			var entry = new Entry();
			((IView)entry).IsFocused = true;

			await InvokeOnMainThreadAsync(() =>
			{
				// Use a real MauiContext (not ContextStub) so we can call DisposeWindowScope()
				// to simulate the window teardown state.
				var mauiContext = new MauiContext(ApplicationServices);
				var handler = CreateHandler<EntryHandler>(entry, mauiContext);

				// Simulate window teardown: Window.Destroying() eventually calls
				// DisposeWindowScope(), which sets IsWindowScopeDisposed = true.
				mauiContext.DisposeWindowScope();

				// Act: invoke UpdateIsFocused(false) via reflection, simulating the async
				// FocusManager.LostFocus callback that fires after window scope disposal.
				var updateIsFocused = typeof(ViewHandler).GetMethod(
					"UpdateIsFocused",
					BindingFlags.Instance | BindingFlags.NonPublic);

				// Should not throw ObjectDisposedException even though the scope is disposed.
				// CanInvokeMappers detects IsWindowScopeDisposed = true and skips mapper execution,
				// preventing access to services (e.g. IFontManager) from the disposed scope.
				updateIsFocused!.Invoke(handler, [false]);

				// UpdateIsFocused itself runs and updates IsFocused to false, but the IsFocused
				// property mapper is skipped because CanInvokeMappers returns false.
				Assert.False(entry.IsFocused);

				((IViewHandler)handler).DisconnectHandler();
			});
		}
	}
}
