using System;
using System.Collections.Generic;

namespace Xamarin.Platform
{
	public abstract class MauiApp : App
	{
		public abstract IWindow GetWindowFor(IActivationState state);

		public MauiApp()
		{
			
		}
	}
}
