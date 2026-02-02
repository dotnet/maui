#nullable enable
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Device tests for https://github.com/dotnet/maui/issues/33187
	/// 
	/// Bug: On Android, when the app goes to background (e.g., notification tap scenario),
	/// Window.Destroying() is called which used to dispose the service provider.
	/// When the user returns to the app and navigates, ContentPage.UpdateHideSoftInputOnTapped
	/// would throw ObjectDisposedException trying to resolve services.
	/// 
	/// Fix: The fix adds #if !ANDROID to skip DisposeWindowScope() on Android since windows
	/// are reused in the single-Activity model.
	/// </summary>
	[Category(TestCategory.Window)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public class WindowTestsIssue33187 : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);

#if ANDROID || WINDOWS
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
#else
					handlers.AddHandler(typeof(NavigationPage), typeof(Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer));
#endif

					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
				});
			});
		}

		/// <summary>
		/// Key test for Issue #33187:
		/// 
		/// On Android, calling Window.Destroying() should NOT dispose the window scope.
		/// On other platforms, it SHOULD dispose the scope (since windows are independently created/destroyed).
		/// 
		/// This test verifies that calling DisposeWindowScope has the expected behavior on each platform
		/// by directly testing MauiContext.DisposeWindowScope.
		/// </summary>
		[Fact]
		public async Task DisposeWindowScopeWorksCorrectly()
		{
			SetupBuilder();

			var page = new ContentPage { Title = "Test Page" };
			var window = new Window(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(page);

				// Get MauiContext
				var mauiContext = handler.MauiContext as MauiContext;
				Assert.NotNull(mauiContext);

				// Get the internal _windowScope field
				var windowScopeField = typeof(MauiContext).GetField("_windowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.NotNull(windowScopeField);

				// The test stub doesn't set up window scope, so first set one up for testing
				var scope = mauiContext.Services.CreateScope();
				var setWindowScope = typeof(MauiContext).GetMethod("SetWindowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.NotNull(setWindowScope);
				setWindowScope.Invoke(mauiContext, new object[] { scope });

				// Verify window scope is now set
				var scopeBeforeDispose = windowScopeField.GetValue(mauiContext);
				Assert.NotNull(scopeBeforeDispose);

				// Call DisposeWindowScope - this is what Window.Destroying() does (or should skip on Android)
				var disposeWindowScope = typeof(MauiContext).GetMethod("DisposeWindowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.NotNull(disposeWindowScope);
				disposeWindowScope.Invoke(mauiContext, null);

				// Check if the window scope is null after disposal
				var scopeAfterDispose = windowScopeField.GetValue(mauiContext);
				
				// DisposeWindowScope should always clear the _windowScope field (on all platforms)
				// The key is that on Android, Window.Destroying() doesn't CALL DisposeWindowScope
				Assert.Null(scopeAfterDispose);
			});
		}

		/// <summary>
		/// Test that Destroying() has the expected behavior per platform.
		/// On Android with the fix, Destroying() should NOT call DisposeWindowScope.
		/// On other platforms, Destroying() SHOULD call DisposeWindowScope.
		/// </summary>
		[Fact]
#if ANDROID
		public async Task AndroidWindowDestroyingDoesNotDisposeWindowScope()
#else
		public async Task NonAndroidWindowDestroyingDisposesWindowScope()
#endif
		{
			SetupBuilder();

			var page = new ContentPage { Title = "Test Page" };
			var window = new Window(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(page);

				// Get MauiContext
				var mauiContext = handler.MauiContext as MauiContext;
				Assert.NotNull(mauiContext);

				// Get the internal _windowScope field
				var windowScopeField = typeof(MauiContext).GetField("_windowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.NotNull(windowScopeField);

				// The test stub doesn't set up window scope, so first set one up for testing
				var scope = mauiContext.Services.CreateScope();
				var setWindowScope = typeof(MauiContext).GetMethod("SetWindowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.NotNull(setWindowScope);
				setWindowScope.Invoke(mauiContext, new object[] { scope });

				// Verify window scope is now set
				var scopeBeforeDestroying = windowScopeField.GetValue(mauiContext);
				Console.WriteLine($"TEST: scopeBeforeDestroying is {(scopeBeforeDestroying == null ? "null" : "not null")}");
				Assert.NotNull(scopeBeforeDestroying);

				// Call Destroying() - this is what happens when Android activity goes through lifecycle
				var iWindow = (IWindow)window;
				Console.WriteLine("TEST: About to call Destroying()");
				iWindow.Destroying();
				Console.WriteLine("TEST: Called Destroying()");

				// Check if the window scope is null after Destroying
				var scopeAfterDestroying = windowScopeField.GetValue(mauiContext);
				Console.WriteLine($"TEST: scopeAfterDestroying is {(scopeAfterDestroying == null ? "null" : "not null")}");

#if ANDROID
				// On Android with the fix, the scope should NOT be disposed (still not null)
				// Note: The fix skips calling DisposeWindowScope() entirely on Android
				Assert.NotNull(scopeAfterDestroying);
#else
				// On other platforms, the scope IS disposed and set to null
				Assert.Null(scopeAfterDestroying);
#endif
			});
		}

		/// <summary>
		/// Test that basic navigation works without crashes.
		/// This validates the core navigation scenario from issue #33187.
		/// </summary>
		[Fact]
		public async Task NavigationPagePushPopDoesNotCrash()
		{
			SetupBuilder();

			var firstPage = new ContentPage { Title = "First" };
			var navigationPage = new NavigationPage(firstPage);
			var window = new Window(navigationPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				// Wait for page to be fully loaded  
				await OnLoadedAsync(firstPage);

				// Push a new page - this triggers ContentPage.NavigatedTo which calls UpdateHideSoftInputOnTapped
				var secondPage = new ContentPage { Title = "Second" };
				await navigationPage.PushAsync(secondPage);
				await OnLoadedAsync(secondPage);

				// Pop back - this also exercises navigation lifecycle
				await navigationPage.PopAsync();
				await OnLoadedAsync(navigationPage.CurrentPage);

				// If we got here, no ObjectDisposedException was thrown
				Assert.NotNull(navigationPage.CurrentPage);
				Assert.Equal("First", navigationPage.CurrentPage.Title);
			});
		}
	}
}
