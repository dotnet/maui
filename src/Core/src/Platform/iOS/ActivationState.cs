using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public ActivationState(IMauiContext context, IReadOnlyDictionary<string, string?> restoredState)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			State = restoredState;
		}

		public IMauiContext Context { get; }
		public IReadOnlyDictionary<string, string?> State { get; }
	}

	public class PersistedState : IPersistedState
	{
		public PersistedState()
		{
			State = new Dictionary<string, string?>();
		}

		public IDictionary<string, string?> State { get; }
	}
}