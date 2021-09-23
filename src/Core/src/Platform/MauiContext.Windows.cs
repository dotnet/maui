using System;

namespace Microsoft.Maui
{
	public partial class MauiContext
	{
		WindowManager? _windowManager;
		public MauiContext(IServiceProvider services, UI.Xaml.Window window)
			: this(services)
		{
			Window = window ?? throw new ArgumentNullException(nameof(window));
			_windowManager = new WindowManager(this);
		}

		public UI.Xaml.Window? Window { get; private set; }

		public WindowManager WindowManager => _windowManager ?? throw new InvalidOperationException("WindowManager Not Found");
	}
}