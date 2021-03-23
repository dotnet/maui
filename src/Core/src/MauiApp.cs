using System;

namespace Microsoft.Maui
{
	public abstract class MauiApp : App
	{
		protected MauiApp()
		{
			if (Current != null)
				throw new InvalidOperationException($"Only one {nameof(App)} instance is allowed.");

			Current = this;
		}

		public static MauiApp? Current { get; internal set; }

		public abstract IWindow CreateWindow(IActivationState state);
	}
}