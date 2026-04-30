using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
		[Fact(DisplayName = "CanInvokeMappers returns false after window scope is disposed (teardown guard)")]
		public async Task CanInvokeMappers_AfterWindowScopeDisposed_ReturnsFalse()
		{
			var entry = new Entry();

			await InvokeOnMainThreadAsync(() =>
			{
				// Mirror production: create a window scope and assign it to MauiContext.
				var scope = ApplicationServices.CreateScope();
				var mauiContext = new MauiContext(scope.ServiceProvider);
				mauiContext.SetWindowScope(scope);
				var handler = CreateHandler<EntryHandler>(entry, mauiContext);

				// Before teardown, mappers should be allowed to run.
				Assert.True(handler.CanInvokeMappers());

				// Simulate window teardown: Window.Destroying() eventually calls
				// DisposeWindowScope(), which sets IsWindowScopeDisposed = true.
				mauiContext.DisposeWindowScope();

				// After disposal, CanInvokeMappers must return false.
				// Without this guard, mappers would run and attempt to resolve services
				// (e.g. IFontManager) from the disposed IServiceScope, causing
				// ObjectDisposedException (#34272).
				Assert.False(handler.CanInvokeMappers());

				((IViewHandler)handler).DisconnectHandler();
			});
		}
	}
}
