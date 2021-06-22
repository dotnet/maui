using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{

	public interface IWindowFactory
	{
		public IWindow GetOrCreateWindow(WindowCreatingArgs args);
	}

	public class WindowFactory : IWindowFactory
	{
		public virtual IWindow GetOrCreateWindow(WindowCreatingArgs args)
		{
			IActivationState activationState = args.ActivationState!;
			IApplication application = args.Application!;
			IServiceProvider services = args.ServiceProvider!;

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
			var activationArgs = services.GetRequiredService<WindowCreatingArgs>();

			// set the activation state args for the Service Provider to use when creating a window
			activationArgs.ActivationState = activationState;

			var newWindow = services.GetRequiredService<IWindow>();
			application.AddWindow(newWindow);
			// clear out the args so we aren't holding any references to the bundle
			activationArgs.ActivationState = null;
			return newWindow;
		}
	}

	public class WindowCreatingArgs : IDisposable
	{
		public IActivationState? ActivationState { get; set; }
		public IApplication? Application { get; set; }
		public IServiceProvider? ServiceProvider { get; set; }

		public void Dispose()
		{
			Application = null;
			ServiceProvider = null;
			ActivationState = null;
		}
	}
}
