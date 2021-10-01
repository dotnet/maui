using System;
using System.Collections.Generic;
using Android.OS;

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public ActivationState(IMauiContext context, IReadOnlyDictionary<string, string?> state)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			State = state;
		}

		public IMauiContext Context { get; }

		public IReadOnlyDictionary<string, string?> State { get; }
	}
}