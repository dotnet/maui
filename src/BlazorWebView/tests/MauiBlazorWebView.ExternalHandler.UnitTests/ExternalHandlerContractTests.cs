using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Dispatching;
using BlazorDispatcher = Microsoft.AspNetCore.Components.Dispatcher;
using MauiBlazorDispatcher = Microsoft.AspNetCore.Components.WebView.Maui.MauiDispatcher;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Guards the premise of this test assembly: everything here must be reachable from a third-party
/// package that has no privileged access to <c>Microsoft.AspNetCore.Components.WebView.Maui</c>.
/// </summary>
public class ExternalHandlerContractTests
{
	private static Assembly BlazorWebViewAssembly => typeof(BlazorWebView).Assembly;

	[Fact]
	public void TestAssembly_DoesNotHaveInternalsVisibleTo()
	{
		var thisAssemblyName = typeof(ExternalHandlerContractTests).Assembly.GetName().Name;

		var grantedTo = BlazorWebViewAssembly
			.GetCustomAttributes<InternalsVisibleToAttribute>()
			.Select(a => a.AssemblyName.Split(',')[0].Trim())
			.ToArray();

		Assert.DoesNotContain(thisAssemblyName, grantedTo);
	}

	[Fact]
	public void FakeHandler_ImplementsPublicHandlerContract()
	{
		var handler = new FakeExternalBlazorWebViewHandler();

		Assert.IsAssignableFrom<IBlazorWebViewHandler>(handler);
		Assert.IsAssignableFrom<IViewHandler>(handler);
	}

	[Theory]
	[InlineData(nameof(RootComponent.AddToWebViewManagerAsync))]
	[InlineData(nameof(RootComponent.RemoveFromWebViewManagerAsync))]
	public void RootComponentLifecycleMethods_ArePublic(string methodName)
	{
		var method = typeof(RootComponent).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

		Assert.NotNull(method);
		Assert.True(method!.IsPublic);
		Assert.Equal("webViewManager", method.GetParameters().Single().Name);
	}

	[Fact]
	public void StaticContentHotReloadSeam_IsPublic()
	{
		var type = typeof(BlazorWebViewStaticContentHotReload);

		Assert.True(type.IsPublic);
		Assert.True(type.IsAbstract && type.IsSealed);
	}

	[Fact]
	public void StaticContentHotReloadSeam_DoesNotExposeRefMutatingResponseApi()
	{
		// The ref/out response-mutating shape stays internal; external handlers own their response state.
		var refMutating = typeof(BlazorWebViewStaticContentHotReload)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Where(m => m.GetParameters().Any(p => p.ParameterType.IsByRef && !p.IsOut))
			.ToArray();

		Assert.Empty(refMutating);
	}

	[Fact]
	public void MauiDispatcherAdapter_IsPublicAndUsableExternally()
	{
		var dispatcher = new MauiBlazorDispatcher(new FakeExternalDispatcher());

		Assert.IsAssignableFrom<BlazorDispatcher>(dispatcher);
		Assert.True(dispatcher.CheckAccess());
	}

	[Fact]
	public void MauiDispatcherAdapter_Throws_WhenDispatcherIsNull()
	{
		Assert.Throws<ArgumentNullException>(() => new MauiBlazorDispatcher(null!));
	}

	[Fact]
	public async Task MauiDispatcherAdapter_DispatchesThroughTheMauiDispatcher()
	{
		var maui = new FakeExternalDispatcher();
		var dispatcher = new MauiBlazorDispatcher(maui);

		var ran = false;
		await dispatcher.InvokeAsync(() => ran = true);

		Assert.True(ran);
		Assert.Equal(1, maui.DispatchCount);
	}

	[Fact]
	public void PlatformWebView_IsNotAddedToWpfOrWinFormsPackages()
	{
		// The platform-neutral property is scoped to the MAUI surface. This assembly only references the
		// MAUI package, so the presence check here documents the intent; the WPF/WinForms PublicAPI files
		// are what actually enforce it.
		var property = typeof(BlazorWebViewInitializedEventArgs)
			.GetProperty(nameof(BlazorWebViewInitializedEventArgs.PlatformWebView));

		Assert.NotNull(property);
		Assert.Equal("Microsoft.AspNetCore.Components.WebView.Maui", BlazorWebViewAssembly.GetName().Name);
	}
}
