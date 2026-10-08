using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

[Category(TestCategory.BlazorWebView)]
public class BlazorWebViewLifecycleTests : Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
{
	[Fact]
	public async Task DisconnectUnsubscribesRootComponentsWithoutClearingThem()
	{
		EnsureHandlerCreated(additionalCreationActions: builder =>
		{
			builder.Services.AddMauiBlazorWebView();
		});

		await InvokeOnMainThreadAsync(() =>
		{
			// No HostPage isolates subscription cleanup from WebViewManager disposal.
			var view = new BlazorWebView();
			var component = new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app" };
			view.RootComponents.Add(component);
			var handler = CreateHandler<BlazorWebViewHandler>(view);

			try
			{
				Assert.Single(GetHandlerSubscriptions(view.RootComponents));
				((IElementHandler)handler).DisconnectHandler();

				Assert.Empty(GetHandlerSubscriptions(view.RootComponents));
				Assert.Same(component, Assert.Single(view.RootComponents));

				handler.SetVirtualView(view);
				Assert.Same(handler, Assert.Single(GetHandlerSubscriptions(view.RootComponents)).Target);
				Assert.Same(component, Assert.Single(view.RootComponents));
			}
			finally
			{
				((IElementHandler)handler).DisconnectHandler();
			}
		});
	}

	static Delegate[] GetHandlerSubscriptions(RootComponentsCollection components)
	{
		var field = typeof(ObservableCollection<RootComponent>).GetField(
			nameof(INotifyCollectionChanged.CollectionChanged), BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(field);
		var subscription = (NotifyCollectionChangedEventHandler)field.GetValue(components);
		return Array.FindAll(subscription?.GetInvocationList() ?? Array.Empty<Delegate>(),
			callback => callback.Target is BlazorWebViewHandler);
	}
}
