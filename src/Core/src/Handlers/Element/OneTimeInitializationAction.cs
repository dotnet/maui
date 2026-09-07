using System;

namespace Microsoft.Maui;

internal sealed class OneTimeInitializationAction
{
	// Lazy currently provides the execute-once and wait-for-completion semantics this
	// primitive needs. Keep callers independent of that implementation detail.
	readonly Lazy<bool> _initialization;

	public OneTimeInitializationAction(Action initialization)
	{
		_ = initialization ?? throw new ArgumentNullException(nameof(initialization));
		_initialization = new Lazy<bool>(() =>
		{
			initialization();
			return true;
		});
	}

	public void InvokeOnce() => _ = _initialization.Value;
}
