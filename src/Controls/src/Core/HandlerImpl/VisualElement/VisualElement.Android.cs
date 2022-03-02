#nullable enable
using System;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		IDisposable? _loadedUnloadedToken;
		partial void HandlePlatformUnloadedLoaded()
		{
			_loadedUnloadedToken?.Dispose();
			_loadedUnloadedToken = null;
			if (Window != null &&
				Handler?.PlatformView is AView view)
			{
				if (view.IsLoaded())
				{
					OnLoadedCore();
					_loadedUnloadedToken = view.OnUnloaded(OnUnloadedCore);
				}
				else
				{
					OnUnloadedCore();
					_loadedUnloadedToken = view.OnLoaded(OnLoadedCore);
				}

			}
			else
			{
				OnUnloadedCore();
			}
		}
	}
}
