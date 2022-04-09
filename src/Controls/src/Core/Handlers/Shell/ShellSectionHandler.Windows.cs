#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using WFrame = Microsoft.UI.Xaml.Controls.Frame;


namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellSectionHandler : ElementHandler<ShellSection, WFrame>
	{
		public static PropertyMapper<ShellSection, ShellSectionHandler> Mapper =
				new PropertyMapper<ShellSection, ShellSectionHandler>(ElementMapper)
				{
					[nameof(ShellSection.CurrentItem)] = MapCurrentItem,
				};

		public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper =
				new CommandMapper<ShellSection, ShellSectionHandler>(ElementCommandMapper)
				{

					[nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
				};

		StackNavigationManager? _navigationManager;

		public ShellSectionHandler() : base(Mapper, CommandMapper)
		{
		}

		protected override WFrame CreatePlatformElement()
		{
			_navigationManager = CreateNavigationManager();
			return new WFrame();
		}

		public static void MapCurrentItem(ShellSectionHandler handler, ShellSection item)
		{
			handler.SyncNavigationStack(false);
		}

		ShellSection? _shellSection;
		public override void SetVirtualView(Maui.IElement view)
		{
			if(_shellSection != null)
			{
				((IShellSectionController)_shellSection).NavigationRequested -= OnNavigationRequested;
			}

			base.SetVirtualView(view);

			_shellSection = (ShellSection)view;
			if (_shellSection != null)
			{
				((IShellSectionController)_shellSection).NavigationRequested += OnNavigationRequested;
			}
		}

		void OnNavigationRequested(object? sender, NavigationRequestedEventArgs e)
		{
			SyncNavigationStack(e.Animated);
		}

		void SyncNavigationStack(bool animated)
		{
			List<IView> pageStack = new List<IView>()
			{
				(VirtualView.CurrentItem as IShellContentController).GetOrCreateContent()
			};

			for (var i = 1; i < VirtualView.Navigation.NavigationStack.Count; i++)
			{
				pageStack.Add(VirtualView.Navigation.NavigationStack[i]);
			}

			// The point of this is to push the shell navigation over to using the INavigationStack
			// work flow. Ideally we rewrite all the push/pop/etc.. parts inside ShellSection.cs
			// to just use INavigationStack but that will be easier once all platforms are using
			// ShellHandler
			(VirtualView as IStackNavigation)
				.RequestNavigation(new NavigationRequest(pageStack, animated));
		}

		// this should move to a factory method
		protected virtual StackNavigationManager CreateNavigationManager() =>
			_navigationManager ??= new StackNavigationManager(MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null"));

		protected override void ConnectHandler(WFrame platformView)
		{
			_navigationManager?.Connect(VirtualView, platformView);
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(WFrame platformView)
		{
			_navigationManager?.Disconnect(VirtualView, platformView);
			base.DisconnectHandler(platformView);
		}

		public static void RequestNavigation(ShellSectionHandler handler, IStackNavigation view, object? arg3)
		{
			if (arg3 is NavigationRequest nr)
			{
				handler._navigationManager?.NavigateTo(nr);
			}
			else
			{
				throw new InvalidOperationException("Args must be NavigationRequest");
			}
		}
	}
}
