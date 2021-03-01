using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.OS;

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public IMauiContext Context { get;  }
		public Bundle? SavedInstance { get; }

		internal ActivationState(Bundle? savedInstance, IMauiContext context)
		{
			SavedInstance = savedInstance;
			Context = context;
		}
	}
}
