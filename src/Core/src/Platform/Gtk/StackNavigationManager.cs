using System;
using System.Linq;

namespace Microsoft.Maui.Platform;

public class StackNavigationManager
{

	public StackNavigationManager(IMauiContext? mauiContext)
	{
		Context = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
	}

	public IStackNavigation? NavigationView { get; protected set; }

	NavigationView? PlatformView { get; set; }

	IMauiContext Context { get; }

	public void Connect(IStackNavigation virtualView, NavigationView platformView)
	{
		NavigationView = virtualView;
		PlatformView = platformView;

	}

	public void Disconnect(IStackNavigation virtualView, NavigationView platformView)
	{
		NavigationView = default;
		PlatformView = default;
	}

	[MissingMapper]
	public void NavigateTo(NavigationRequest request)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} must not be null");
		_ = NavigationView ?? throw new InvalidOperationException($"{nameof(NavigationView)} must not be null");


		if (request.NavigationStack.LastOrDefault() is { } view && view.ToPlatform(Context) is { } platformContent)
		{
			PlatformView.Content = platformContent;
		}

		NavigationView.NavigationFinished(request.NavigationStack);
	}

}