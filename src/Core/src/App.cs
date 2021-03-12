using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public abstract class App : IApplication
	{
		IServiceProvider? _serviceProvider;
		IMauiContext? _context;

		protected App()
		{
			if (Current != null)
				throw new InvalidOperationException($"Only one {nameof(App)} instance is allowed");

			Current = this;
		}

		public static App? Current { get; internal set; }

		public IServiceProvider? Services => _serviceProvider;

		public IMauiContext? Context => _context;

		// Move to abstract
		public virtual IAppHostBuilder CreateBuilder() => CreateDefaultBuilder();

		public event EventHandler? Created;

		public event EventHandler? Resumed;

		public event EventHandler? Paused;

		public event EventHandler? Stopped;

		public static IAppHostBuilder CreateDefaultBuilder()
		{
			var builder = new AppHostBuilder();

			builder.UseMauiHandlers();
			builder.UseFonts();

			return builder;
		}

		public abstract IWindow CreateWindow(IActivationState state);

		public virtual void OnCreated()
		{
			Created?.Invoke(this, EventArgs.Empty);
		}

		public virtual void OnResumed()
		{
			Resumed?.Invoke(this, EventArgs.Empty);
		}

		public virtual void OnPaused()
		{
			Paused?.Invoke(this, EventArgs.Empty);
		}

		public virtual void OnStopped()
		{
			Stopped?.Invoke(this, EventArgs.Empty);
		}

		internal void SetServiceProvider(IServiceProvider provider)
		{
			_serviceProvider = provider;
			SetHandlerContext(provider.GetService<IMauiContext>());
		}

		internal void SetHandlerContext(IMauiContext? context)
		{
			_context = context;
		}
	}
}
