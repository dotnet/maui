using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	class WindowFactory
	{
		public static IWindow GetOrCreateWindow(
			IActivationState activationState, 
			IApplication application,
			IServiceProvider services)
		{

#if __ANDROID__
			string? id = activationState.SavedInstance?.GetString(MauiAppCompatActivity.MauiAppCompatActivity_WindowId);
#else
			string? id = null;

#endif
			int count = application.Windows.Count;
			for (var i = 0; i < count; i++)
			{
				IWindow window = application.Windows[i];
				if (window.Id == id)
					return window;
			}

			// retrieve the args used to create new windows
			var activationArgs = services.GetRequiredService<StartupActivationState>();
			// set the activation state args for the Service Provider to use when creating a window
			activationArgs.ActivationState = activationState;
			var newWindow = services.GetRequiredService<IWindow>();
			// clear out the args so we aren't holding any references to the bundle
			activationArgs.ActivationState = null;
			return newWindow;
		}
	}

	class StartupActivationState : IDisposable
	{
		public IActivationState? ActivationState { get; set; }

		public void Dispose()
		{
			ActivationState = null;
		}
	}
}
