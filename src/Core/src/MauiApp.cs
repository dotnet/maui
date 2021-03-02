using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public abstract class MauiApp : App
	{
		public abstract IWindow CreateWindow(IActivationState state);

		public MauiApp()
		{
			
		}
	}
}
