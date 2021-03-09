using System;
using Android.OS;

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public ActivationState(IMauiContext context, Bundle? savedInstance = null)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			SavedInstance = savedInstance;
		}

		public IMauiContext Context { get; }

		public Bundle? SavedInstance { get; }
	}
}