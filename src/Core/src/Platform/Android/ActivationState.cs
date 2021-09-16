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

	public class RestoredState : IRestoredState
	{
		public RestoredState(IMauiContext context, Bundle? savedInstance = null)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			SavedInstance = savedInstance;
		}

		public IMauiContext Context { get; }

		public Bundle? SavedInstance { get; }
	}

	public class SaveableState : ISaveableState
	{
		public SaveableState(IMauiContext context, Bundle? bundle = null, PersistableBundle? persistableBundle)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			Bundle = bundle;
			PersistableBundle = persistableBundle;
		}

		public IMauiContext Context { get; }

		public Bundle? Bundle { get; }
		public PersistableBundle? PersistableBundle { get; }
	}
}