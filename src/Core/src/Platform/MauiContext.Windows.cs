using System;

namespace Microsoft.Maui
{
	public partial class MauiApplicationContext
	{
		public MauiApplicationContext(IServiceProvider services, UI.Xaml.Application application)
			: this(services)
		{
			Application = application ?? throw new ArgumentNullException(nameof(application));
		}

		public UI.Xaml.Application? Application { get; }
	}

	public partial class MauiContext
	{
		readonly WindowManager? _windowManager;

		public MauiContext(IServiceProvider services, UI.Xaml.Window window, IMauiContext? applicationContext = null)
			: this(services, applicationContext)
		{
			Window = window ?? throw new ArgumentNullException(nameof(window));
			_windowManager = new WindowManager(this);
		}

		public UI.Xaml.Window? Window { get; private set; }

		public WindowManager WindowManager => _windowManager ?? throw new InvalidOperationException("WindowManager Not Found");
	}
}