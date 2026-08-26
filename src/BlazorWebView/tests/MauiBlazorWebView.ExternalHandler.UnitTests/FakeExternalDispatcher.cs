using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.MauiBlazorWebView.ExternalHandler.UnitTests;

/// <summary>
/// Stands in for the MAUI <see cref="IDispatcher"/> an external backend resolves from its services and
/// hands to <see cref="AspNetCore.Components.WebView.Maui.MauiDispatcher"/>. It runs everything inline.
/// </summary>
internal sealed class FakeExternalDispatcher : IDispatcher
{
	public bool IsDispatchRequired => false;

	public int DispatchCount { get; private set; }

	public bool Dispatch(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		DispatchCount++;
		action();
		return true;
	}

	public bool DispatchDelayed(TimeSpan delay, Action action) => Dispatch(action);

	public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
}
