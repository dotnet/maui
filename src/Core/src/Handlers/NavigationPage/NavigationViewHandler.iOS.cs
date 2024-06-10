using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Handlers;

public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, UIView>, IPlatformViewHandler
{
	public IStackNavigationView NavigationView => ((IStackNavigationView)VirtualView);

	public StackNavigationManager? StackNavigationManager { get; private set; }

	public NavigationManager? NavigationManager => MauiContext?.GetNavigationManager();

	public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

	PlatformNavigationController? _platformNavigationController;
	UIViewController? IPlatformViewHandler.ViewController => _platformNavigationController;

	protected override UIView CreatePlatformView()
	{
		_platformNavigationController ??= new PlatformNavigationController(this, navigationBarType: typeof(MauiNavigationBar));
		
		StackNavigationManager = CreateStackNavigationManager();

		if (_platformNavigationController.View is null)
		{
			throw new NullReferenceException("PlatformNavigationController.View is null.");
		}

		NavigationManager?.SetNavigationController(_platformNavigationController);

		return _platformNavigationController.View;
	}

	protected override void ConnectHandler(UIView platformView)
	{
		if (_platformNavigationController is null)
		{
			throw new NullReferenceException("PlatformNavigationController is null.");
		}

		StackNavigationManager?.Connect(VirtualView, _platformNavigationController, this);
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(UIView platformView)
	{
		StackNavigationManager?.Disconnect(VirtualView, _platformNavigationController!);
		_platformNavigationController?.Dispose();
		_platformNavigationController = null;
		base.DisconnectHandler(platformView);
	}

	void RequestNavigation(NavigationRequest request)
	{
		StackNavigationManager?.RequestNavigation(request);
	}

	public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
	{
		if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest nr)
		{
			platformHandler.NavigationStack = nr.NavigationStack;
			platformHandler.RequestNavigation(nr);
		}
		else
		{
			throw new InvalidOperationException("Args must be NavigationRequest");
		}
	}

	protected virtual StackNavigationManager CreateStackNavigationManager() =>
		new StackNavigationManager(MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null"));
}