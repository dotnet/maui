using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

internal sealed class TestRootComponentA : IComponent
{
	public void Attach(RenderHandle renderHandle)
	{
	}

	public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
}

internal sealed class TestRootComponentB : IComponent
{
	public void Attach(RenderHandle renderHandle)
	{
	}

	public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
}
