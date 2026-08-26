using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Proves that a third-party handler can surface its native control through
/// <see cref="BlazorWebViewInitializedEventArgs"/> on a target framework that has no built-in
/// MAUI BlazorWebView backend (and therefore no strongly typed <c>WebView</c> property).
/// </summary>
public class ExternalPlatformWebViewEventArgsTests
{
	[Fact]
	public void PlatformWebView_DefaultsToNull()
	{
		var args = new BlazorWebViewInitializedEventArgs();

		Assert.Null(args.PlatformWebView);
	}

	[Fact]
	public void PlatformWebView_IsSuppliedThroughConstructor()
	{
		var native = new FakeNativeWebView();

		var args = new BlazorWebViewInitializedEventArgs(native);

		Assert.Same(native, args.PlatformWebView);
	}

	[Fact]
	public void Constructor_Throws_WhenPlatformWebViewIsNull()
	{
		Assert.Throws<ArgumentNullException>(() => new BlazorWebViewInitializedEventArgs(null!));
	}

	[Fact]
	public void PlatformWebView_IsReadOnlyToSubscribers()
	{
		// A settable property would let one subscriber change what later subscribers observe, so the
		// public surface must expose a getter only.
		var property = typeof(BlazorWebViewInitializedEventArgs)
			.GetProperty(nameof(BlazorWebViewInitializedEventArgs.PlatformWebView));

		Assert.NotNull(property);
		Assert.True(property!.CanRead);
		Assert.False(property.CanWrite);
		Assert.Null(property.SetMethod);
	}

	[Fact]
	public void ExternalHandler_PublishesPlatformWebView_ThroughInitializedEvent()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		BlazorWebViewInitializedEventArgs? observed = null;
		blazorWebView.BlazorWebViewInitialized += (_, e) => observed = e;

		var initializingRaised = false;
		blazorWebView.BlazorWebViewInitializing += (_, _) => initializingRaised = true;

		handler.StartWebViewCore();

		Assert.True(initializingRaised);
		Assert.NotNull(observed);
		Assert.Same(handler.NativeControl, observed!.PlatformWebView);
	}

	[Fact]
	public void InitializedEvent_SenderIsTheBlazorWebView()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		object? sender = null;
		blazorWebView.BlazorWebViewInitialized += (s, _) => sender = s;

		handler.StartWebViewCore();

		Assert.Same(blazorWebView, sender);
	}

	[Fact]
	public void AllSubscribers_ObserveTheSamePlatformWebView()
	{
		var blazorWebView = new BlazorWebView();
		var handler = new FakeExternalBlazorWebViewHandler();
		handler.SetVirtualView(blazorWebView);

		object? first = null;
		object? second = null;
		blazorWebView.BlazorWebViewInitialized += (_, e) => first = e.PlatformWebView;
		blazorWebView.BlazorWebViewInitialized += (_, e) => second = e.PlatformWebView;

		handler.StartWebViewCore();

		Assert.Same(handler.NativeControl, first);
		Assert.Same(handler.NativeControl, second);
	}
}
