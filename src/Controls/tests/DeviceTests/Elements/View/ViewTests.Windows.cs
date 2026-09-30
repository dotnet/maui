using System;
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
		// Timing regression for https://github.com/dotnet/maui/issues/34272:
		// The previous fix set IsWindowScopeDisposed *after* _windowScope.Dispose() returned,
		// so CanInvokeMappers() still returned true *during* teardown. A late
		// FocusManager.LostFocus / IsFocused style-trigger mapper that ran while the scope
		// was being disposed could still reach the already-being-torn-down services and throw
		// ObjectDisposedException. This test reproduces that exact timing by observing
		// CanInvokeMappers() from *inside* the scope's Dispose() call.
		[Fact(DisplayName = "CanInvokeMappers returns false DURING window scope disposal (timing guard)")]
		public async Task CanInvokeMappers_DuringWindowScopeDispose_ReturnsFalse()
		{
			var entry = new Entry();
      {
        bool? canInvokeMappersDuringDispose = null;
        EntryHandler capturedHandler = null;

        var innerScope = ApplicationServices.CreateScope();

        // Wrap the real scope so we can observe CanInvokeMappers() mid-Dispose.
        var spyScope = new SpyServiceScope(innerScope, onDispose: () =>
        {
          canInvokeMappersDuringDispose = capturedHandler == null ? (bool?)null : capturedHandler.CanInvokeMappers();
        });

        var mauiContext = new MauiContext(innerScope.ServiceProvider);
        mauiContext.SetWindowScope(spyScope);
        var handler = CreateHandler<EntryHandler>(entry, mauiContext);
        capturedHandler = handler;

        Assert.True(handler.CanInvokeMappers());

        // DisposeWindowScope() must set the flag BEFORE calling spyScope.Dispose(),
        // so that any mapper invoked from within Dispose() is already blocked.
        mauiContext.DisposeWindowScope();

        // canInvokeMappersDuringDispose was captured inside spyScope.Dispose().
        // If true, IsWindowScopeDisposed was still false at that point — meaning
        // a late FocusManager.LostFocus mapper could have run against the
        // already-being-torn-down scope, reproducing the #34272 race.
        Assert.False(canInvokeMappersDuringDispose,
          "CanInvokeMappers() must return false during _windowScope.Dispose() — " +
          "not just after — to prevent style-trigger mappers from resolving " +
          "services off a scope that is actively being torn down (#34272).");

        ((IViewHandler)handler).DisconnectHandler();
      };
    }

		class SpyServiceScope : IServiceScope
		{
			readonly IServiceScope _inner;
			readonly Action _onDispose;

			public SpyServiceScope(IServiceScope inner, Action onDispose)
			{
				_inner = inner;
				_onDispose = onDispose;
			}

			public IServiceProvider ServiceProvider => _inner.ServiceProvider;

			public void Dispose()
			{
				_onDispose();
				_inner.Dispose();
			}
		}
	}
}