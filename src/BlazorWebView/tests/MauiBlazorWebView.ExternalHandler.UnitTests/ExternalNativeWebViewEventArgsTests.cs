using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Proves that a third-party handler can surface its native control through
/// <see cref="BlazorWebViewInitializedEventArgs"/> on a target framework that has no built-in
/// MAUI BlazorWebView backend (and therefore no strongly typed <c>WebView</c> property).
/// </summary>
public class ExternalNativeWebViewEventArgsTests
{
	[Fact]
	public void NativeWebView_DefaultsToNull()
	{
		var args = new BlazorWebViewInitializedEventArgs();

		Assert.Null(args.NativeWebView);
	}

	[Fact]
	public void NativeWebView_IsSettableFromExternalAssembly()
	{
		var native = new FakeNativeWebView();

		var args = new BlazorWebViewInitializedEventArgs
		{
			NativeWebView = native,
		};

		Assert.Same(native, args.NativeWebView);
	}

	[Fact]
	public void ExternalHandler_PublishesNativeWebView_ThroughInitializedEvent()
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
		Assert.Same(handler.NativeControl, observed!.NativeWebView);
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
}
