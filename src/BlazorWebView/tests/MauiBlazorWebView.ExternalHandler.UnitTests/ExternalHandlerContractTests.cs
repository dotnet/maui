using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.WebView.Maui;

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
	}

	[Fact]
	public void StaticContentHotReloadSeam_IsPublic()
	{
		var type = typeof(BlazorWebViewStaticContentHotReload);

		Assert.True(type.IsPublic);
		Assert.True(type.IsAbstract && type.IsSealed);
	}
}
