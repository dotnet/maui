using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public partial class MauiContext
	{
		public MauiContext(IServiceProvider services, UI.Xaml.Window window) : this(services)
		{
			Window = window ?? throw new ArgumentNullException(nameof(window));
		}

		public UI.Xaml.Window? Window
		{
			get;
			private set;
		}
	}
}
