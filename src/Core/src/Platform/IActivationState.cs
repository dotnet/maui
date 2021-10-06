using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IActivationState
	{
		IMauiContext Context { get; }
		IReadOnlyDictionary<string, string?> State { get; }
	}

	public interface IPersistedState
	{
		IDictionary<string, string?> State { get; }
	}
}