using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Proves that a third-party handler can participate in MAUI Blazor static content hot reload
/// through <see cref="BlazorWebViewStaticContentHotReload"/> without any access to internals.
/// </summary>
[Collection(nameof(ExternalStaticContentHotReloadTests))]
public class ExternalStaticContentHotReloadTests
{
	private const string HotReloadScriptPath = "_framework/static-content-hot-reload.js";

	// Static content hot reload is gated on MetadataUpdater.IsSupported, which is only true when the
	// runtime is started with DOTNET_MODIFIABLE_ASSEMBLIES=debug. CI sets that for every xUnit work
	// item (see eng/helix.proj), so the enabled branch of these tests is the one that runs there.
	private static FakeExternalWebViewManager CreateManager() =>
		new(
			new ServiceCollection().BuildServiceProvider(),
			new NullFileProvider(),
			new JSComponentConfigurationStore(),
			new FakeExternalDispatcher());

	private static string RequestUri(string relativePath) =>
		new Uri(FakeExternalWebViewManager.AppBaseUri, relativePath).AbsoluteUri;

	[Fact]
	public async Task TryAttachToWebViewManager_ReportsWhetherItAttached()
	{
		await using var manager = CreateManager();

		var attachTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		if (MetadataUpdater.IsSupported)
		{
			Assert.NotNull(attachTask);
			await attachTask!;

			// The notifier root component is registered under a fixed selector, so removing it succeeds.
			await manager.RemoveRootComponentAsync("body::after");
		}
		else
		{
			Assert.Null(attachTask);
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => manager.RemoveRootComponentAsync("body::after"));
		}
	}

	[Fact]
	public async Task TryAttachToWebViewManager_IsIdempotentForTheSameManager()
	{
		await using var manager = CreateManager();

		var first = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		// A second attach must not throw "there is already a root component with selector 'body::after'".
		var second = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.Null(first);
			Assert.Null(second);
			return;
		}

		Assert.NotNull(second);
		await second!;

		// Only one registration exists, so exactly one removal succeeds.
		await manager.RemoveRootComponentAsync("body::after");
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => manager.RemoveRootComponentAsync("body::after"));
	}

	[Fact]
	public async Task TryAttachToWebViewManager_IsThreadSafeForConcurrentCalls()
	{
		if (!MetadataUpdater.IsSupported)
		{
			return;
		}

		await using var manager = CreateManager();
		const int callerCount = 8;
		using var barrier = new Barrier(callerCount);

		var calls = Enumerable.Range(0, callerCount)
			.Select(_ => Task.Run(() =>
			{
				Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(30)));
				return new[] { BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager)! };
			}))
			.ToArray();

		var attachTasks = (await Task.WhenAll(calls))
			.Select(result => result[0])
			.ToArray();

		Assert.All(attachTasks, task => Assert.Same(attachTasks[0], task));
		await Task.WhenAll(attachTasks);

		// Concurrent callers registered exactly one notifier.
		await manager.RemoveRootComponentAsync("body::after");
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => manager.RemoveRootComponentAsync("body::after"));
	}

	[Fact]
	public async Task TryAttachToWebViewManager_RetriesAfterFailedAttach()
	{
		if (!MetadataUpdater.IsSupported)
		{
			return;
		}

		await using var manager = CreateManager();

		await manager.AddRootComponentAsync(
			typeof(TestRootComponentA),
			"body::after",
			ParameterView.Empty);

		Assert.Throws<InvalidOperationException>(() =>
		{
			_ = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		});

		await manager.RemoveRootComponentAsync("body::after");

		var retry = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		Assert.NotNull(retry);
		await retry!;

		await manager.RemoveRootComponentAsync("body::after");
	}

	[Fact]
	public async Task TryAttachToWebViewManager_AttachesEachManagerIndependently()
	{
		await using var first = CreateManager();
		await using var second = CreateManager();

		var firstTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(first);
		var secondTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(second);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.Null(firstTask);
			Assert.Null(secondTask);
			return;
		}

		Assert.NotNull(firstTask);
		Assert.NotNull(secondTask);

		// Idempotence is scoped to a single manager, so each one really did get the notifier registered.
		await first.RemoveRootComponentAsync("body::after");
		await second.RemoveRootComponentAsync("body::after");
	}

	[Fact]
	public async Task TryAttachToWebViewManager_WaitsForDiscardedDetach()
	{
		await using var manager = CreateManager();

		var firstAttach = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		if (firstAttach is null)
		{
			return;
		}

		await firstAttach;

		_ = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);
		var secondAttach = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		Assert.NotNull(secondAttach);
		await secondAttach!;

		// Reattachment completed after removal and registered exactly one notifier.
		await manager.RemoveRootComponentAsync("body::after");
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => manager.RemoveRootComponentAsync("body::after"));
	}

	[Fact]
	public void TryAttachToWebViewManager_Throws_WhenManagerIsNull()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			_ = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(null!);
		});
	}

	[Fact]
	public async Task TryDetachFromWebViewManager_RemovesTheNotifier()
	{
		await using var manager = CreateManager();

		var attachTask = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		var detachTask = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.Null(attachTask);
			Assert.Null(detachTask);
			return;
		}

		Assert.NotNull(detachTask);
		await detachTask!;

		// The notifier is gone, so removing it again fails.
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => manager.RemoveRootComponentAsync("body::after"));
	}

	[Fact]
	public async Task TryDetachFromWebViewManager_ReturnsNull_WhenNothingWasAttached()
	{
		await using var manager = CreateManager();

		Assert.Null(BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager));
	}

	[Fact]
	public async Task TryDetachFromWebViewManager_IsIdempotent()
	{
		await using var manager = CreateManager();

		_ = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		var first = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);
		if (first is not null)
		{
			await first;
		}

		// A second detach has nothing left to remove and must not throw.
		Assert.Null(BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager));
	}

	[Fact]
	public async Task TryAttachToWebViewManager_CanReattachAfterDetach()
	{
		await using var manager = CreateManager();

		var firstAttach = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		var detach = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);
		if (detach is not null)
		{
			await detach;
		}

		var secondAttach = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.Null(firstAttach);
			Assert.Null(secondAttach);
			return;
		}

		Assert.NotNull(secondAttach);
		await secondAttach!;

		// The notifier really was registered again, so exactly one removal succeeds.
		await manager.RemoveRootComponentAsync("body::after");
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => manager.RemoveRootComponentAsync("body::after"));
	}

	[Fact]
	public void TryDetachFromWebViewManager_Throws_WhenManagerIsNull()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			_ = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(null!);
		});
	}

	[Fact]
	public async Task TryDetachFromWebViewManager_DoesNotFault_WhenNotifierIsAlreadyGone()
	{
		await using var manager = CreateManager();

		var attach = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		if (attach is null)
		{
			return;
		}

		await attach;

		// Stands in for renderer teardown pulling the notifier out from under an in-flight detach:
		// RemoveRootComponentAsync then reports that the selector is no longer registered.
		await manager.RemoveRootComponentAsync("body::after");

		var detach = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);

		Assert.NotNull(detach);
		await detach!;
		Assert.True(detach.IsCompletedSuccessfully);
	}

	[Fact]
	public async Task TryDetachFromWebViewManager_DoesNotFault_WhenDisposalFollowsImmediately()
	{
		var manager = CreateManager();
		_ = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);

		// The teardown ordering a handler could plausibly write: start the detach, then dispose without
		// waiting for it. The detach must still complete successfully, so discarding it cannot leave an
		// unobserved exception behind.
		var detach = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);
		await manager.DisposeAsync();

		if (detach is not null)
		{
			await detach;
			Assert.True(detach.IsCompletedSuccessfully);
		}
	}

	[Fact]
	public async Task TryDetachFromWebViewManager_DoesNotFault_AfterManagerIsDisposed()
	{
		var manager = CreateManager();
		_ = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		await manager.DisposeAsync();

		var detach = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);

		if (detach is not null)
		{
			await detach;
			Assert.True(detach.IsCompletedSuccessfully);
		}
	}

	[CollectionDefinition(nameof(ExternalStaticContentHotReloadTests), DisableParallelization = true)]
	public sealed class ExternalStaticContentHotReloadTestCollection
	{
	}

	[Fact]
	public async Task DiscardedDetach_FollowedByDisposal_ProducesNoUnobservedTaskException()
	{
		// This is the regression for the reported symptom rather than for detach itself. The in-box
		// handlers used to discard the detach task and dispose immediately; when the removal lost the
		// race it faulted, and because nothing observed the task the fault surfaced later as an
		// unobserved task exception. Faulted tasks only report as unobserved once finalized, which is
		// why this forces a collection rather than just awaiting.
		var unobserved = new List<Exception>();
		void OnUnobserved(object? sender, UnobservedTaskExceptionEventArgs e)
		{
			lock (unobserved)
			{
				unobserved.Add(e.Exception);
			}
		}

		TaskScheduler.UnobservedTaskException += OnUnobserved;
		try
		{
			for (var i = 0; i < 25; i++)
			{
				await RunDiscardedDetachTeardownAsync();
			}

			for (var i = 0; i < 3; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}
		finally
		{
			TaskScheduler.UnobservedTaskException -= OnUnobserved;
		}

		lock (unobserved)
		{
			Assert.Empty(unobserved);
		}
	}

	/// <summary>
	/// Reproduces the exact teardown shape the in-box handlers used to have, and that an external
	/// author could still write: discard the detach task, then dispose the manager.
	/// </summary>
	private static async Task RunDiscardedDetachTeardownAsync()
	{
		var manager = CreateManager();

		var attach = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(manager);
		if (attach is null)
		{
			await manager.DisposeAsync();
			return;
		}

		await attach;

		// Stands in for the renderer having already dropped the notifier, which is what makes the
		// removal throw. Without the guard in the seam, the discarded task below faults.
		await manager.RemoveRootComponentAsync("body::after");

		_ = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(manager);

		await manager.DisposeAsync();
	}

	[Fact]
	public async Task ExternalHandler_DetachesHotReload_OnTeardown()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		handler.StartWebViewCore();
		await handler.StopWebViewCoreAsync();

		Assert.Null(handler.WebViewManager);
		Assert.Equal(MetadataUpdater.IsSupported, handler.HotReloadDetachTask is not null);
	}

	[Fact]
	public async Task ExternalHandler_Teardown_IsIdempotent()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		handler.StartWebViewCore();
		await handler.StopWebViewCoreAsync();

		// A second teardown has nothing left to release and must not throw.
		await handler.StopWebViewCoreAsync();

		Assert.Null(handler.WebViewManager);
	}

	[Fact]
	public async Task ExternalHandler_CanRestartAfterTeardown()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		var first = handler.StartWebViewCore();
		await handler.StopWebViewCoreAsync();

		// A reconnected handler builds a new manager, which must attach cleanly.
		var second = handler.StartWebViewCore();

		Assert.NotSame(first, second);
		Assert.Equal(MetadataUpdater.IsSupported, handler.HotReloadAttachTask is not null);

		await handler.StopWebViewCoreAsync();
	}

	[Fact]
	public void ExternalHandler_AttachesHotReload_DuringStartup()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		var manager = handler.StartWebViewCore();

		Assert.NotNull(manager);
		Assert.Same(manager, handler.WebViewManager);
		Assert.Equal(MetadataUpdater.IsSupported, handler.HotReloadAttachTask is not null);
	}

	[Fact]
	public void TryGetUpdatedStaticContent_ReturnsFalse_ForUnknownContent()
	{
		var found = BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
			RequestUri("css/app.css"),
			out var content,
			out var contentType);

		Assert.False(found);
		Assert.Null(content);
		Assert.Null(contentType);
	}

	[Fact]
	public void TryGetUpdatedStaticContent_ServesHotReloadScript_WhenSupported()
	{
		var found = BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot,
			RequestUri(HotReloadScriptPath),
			out var content,
			out var contentType);

		if (!MetadataUpdater.IsSupported)
		{
			Assert.False(found);
			Assert.Null(content);
			return;
		}

		Assert.True(found);
		Assert.Equal("text/javascript", contentType);
		Assert.NotNull(content);

		// The caller owns the stream, so it must be readable and independently disposable.
		using (content!)
		{
			using var reader = new StreamReader(content, Encoding.UTF8);
			Assert.Contains("notifyCssUpdated", reader.ReadToEnd(), StringComparison.Ordinal);
		}
	}

	[Fact]
	public void TryGetUpdatedStaticContent_ReturnsAFreshStreamPerCall()
	{
		if (!MetadataUpdater.IsSupported)
		{
			return;
		}

		var uri = RequestUri(HotReloadScriptPath);

		Assert.True(BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot, uri, out var first, out _));
		Assert.True(BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
			FakeExternalWebViewManager.ContentRootRelativeToAppRoot, uri, out var second, out _));

		Assert.NotSame(first, second);

		// Disposing one must not affect the other; the caller owns each stream.
		first!.Dispose();
		Assert.NotEqual(0, second!.ReadByte());
		second.Dispose();
	}

	[Theory]
	[InlineData(null, "https://0.0.0.0/css/app.css")]
	[InlineData("wwwroot", null)]
	public void TryGetUpdatedStaticContent_Throws_ForNullArguments(string? contentRoot, string? requestUri)
	{
		Assert.Throws<ArgumentNullException>(() =>
			BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
				contentRoot!, requestUri!, out _, out _));
	}
}
