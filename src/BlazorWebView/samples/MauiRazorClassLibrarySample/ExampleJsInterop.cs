using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MauiRazorClassLibrarySample
{
	// This class provides an example of how JavaScript functionality can be wrapped
	// in a .NET class for easy consumption. The associated JavaScript module is
	// loaded on demand when first needed.
	//
	// This class can be registered as scoped DI service and then injected into Razor
	// components for use.

	public class ExampleJsInterop : IAsyncDisposable
	{
		private readonly Lazy<Task<IJSObjectReference>> moduleTask;

		public ExampleJsInterop(IJSRuntime jsRuntime)
		{
			moduleTask = new(async () =>
			{
				try
				{
					return await jsRuntime.InvokeAsync<IJSObjectReference>(
						"import", "./_content/MauiRazorClassLibrarySample/exampleJsInterop.js");
				}
				catch (JSException ex)
				{
					throw new InvalidOperationException("Unable to import the example JavaScript module.", ex);
				}
			});
		}

		public async ValueTask<string> Prompt(string message)
		{
			try
			{
				var module = await moduleTask.Value;
				return await module.InvokeAsync<string>("showPrompt", message);
			}
			catch (JSException ex)
			{
				throw new InvalidOperationException("Unable to show the JavaScript prompt.", ex);
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (moduleTask.IsValueCreated)
			{
				var module = await moduleTask.Value;
				try
				{
					await module.DisposeAsync();
				}
				catch (JSDisconnectedException)
				{
					// The JavaScript context is already gone, so the module no longer needs disposal.
				}
			}
		}
	}
}
