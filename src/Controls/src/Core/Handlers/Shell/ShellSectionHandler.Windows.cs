using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using WFrame = Microsoft.UI.Xaml.Controls.Frame;


namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellSectionHandler : ElementHandler<ShellSection, WFrame>, IAppearanceObserver
	{
		public static PropertyMapper<ShellSection, ShellSectionHandler> Mapper =
				new PropertyMapper<ShellSection, ShellSectionHandler>(ElementMapper)
				{
					[nameof(ShellSection.Title)] = MapTitle,
					[nameof(ShellSection.CurrentItem)] = MapCurrentItem,
				};

		public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper =
				new CommandMapper<ShellSection, ShellSectionHandler>(ElementCommandMapper)
				{

					[nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
				};

		StackNavigationManager? _navigationManager;
		WeakReference? _lastShell;

		public ShellSectionHandler() : base(Mapper, CommandMapper)
		{
		}

		protected override WFrame CreatePlatformElement()
		{
			_navigationManager = CreateNavigationManager();
			return new WFrame();
		}
		public static void MapTitle(ShellSectionHandler handler, ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shellItemHandler = shellItem?.Handler as ShellItemHandler;
			shellItemHandler?.UpdateTitle();
		}

		public static void MapCurrentItem(ShellSectionHandler handler, ShellSection item)
		{
			handler.SyncNavigationStack(false, null);
		}

		ShellSection? _shellSection;
		public override void SetVirtualView(Maui.IElement view)
		{
			if (_shellSection != null)
			{
				((IShellSectionController)_shellSection).NavigationRequested -= OnNavigationRequested;

				((IShellSectionController)_shellSection).ItemsCollectionChanged -= OnItemsCollectionChanged;

				if (_lastShell?.Target is IShellController shell)
				{
					shell.RemoveAppearanceObserver(this);
				}

				foreach (var item in _shellSection.Items)
				{
					if (item is ShellContent shellContent)
					{
						shellContent.PropertyChanged -= OnShellContentPropertyChanged;
					}
				}
				_lastShell = null;
			}

			// If we've already connected to the navigation manager
			// then we need to make sure to disconnect and connect up to 
			// the new incoming virtual view
			if (_navigationManager?.NavigationView != null &&
				_navigationManager.NavigationView != view)
			{
				_navigationManager.Disconnect(_navigationManager.NavigationView, PlatformView);

				if (view is IStackNavigation stackNavigation)
				{
					_navigationManager.Connect(stackNavigation, PlatformView);
				}
			}

			base.SetVirtualView(view);

			_shellSection = (ShellSection)view;
			if (_shellSection != null)
			{
				((IShellSectionController)_shellSection).NavigationRequested += OnNavigationRequested;

				((IShellSectionController)_shellSection).ItemsCollectionChanged += OnItemsCollectionChanged;

				var shell = _shellSection.FindParentOfType<Shell>() as IShellController;
				if (shell != null)
				{
					_lastShell = new WeakReference(shell);
					shell.AddAppearanceObserver(this, _shellSection);
				}

				foreach (var item in _shellSection.Items)
				{
					if (item is ShellContent shellContent)
					{
						shellContent.PropertyChanged += OnShellContentPropertyChanged;
					}
				}
			}
		}

		void OnNavigationRequested(object? sender, NavigationRequestedEventArgs e)
		{
			SyncNavigationStack(e.Animated, e);
		}

		void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (_shellSection is null)
				return;

			if (_shellSection.Parent is ShellItem shellItem && shellItem.Handler is ShellItemHandler shellItemHandler)
			{
				shellItemHandler.MapMenuItems();
			}

			if (e.OldItems is not null)
			{
				foreach (ShellContent item in e.OldItems)
				{
					item.PropertyChanged -= OnShellContentPropertyChanged;
				}
			}

			if (e.NewItems is not null)
			{
				foreach (ShellContent item in e.NewItems)
				{
					item.PropertyChanged += OnShellContentPropertyChanged;
				}
			}
		}

		void OnShellContentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender is not ShellContent shellContent)
				return;

			if (e.PropertyName == nameof(ShellContent.Title))
			{
				shellContent.UpdateTitle();
			}
		}

		void SyncNavigationStack(bool animated, NavigationRequestedEventArgs? e)
		{
			// Current Item might transition to null while visibility is adjusting on shell
			// so we just ignore this and eventually when shell knows
			// the next current item it will request to sync again
			if (VirtualView.CurrentItem == null || MauiContext is null)
				return;

			// This is used to assign the ShellContentHandler to ShellContent
			_ = VirtualView.CurrentItem.ToPlatform(MauiContext);

			List<IView> pageStack = new List<IView>()
			{
				(VirtualView.CurrentItem as IShellContentController).GetOrCreateContent()
			};

			// PopToRoot in the xplat code fires before the navigation stack has been updated
			// Once we get shell all converted over to newer navigation APIs this will all be a bit
			// less leaky
			if (e?.RequestType != NavigationRequestType.PopToRoot)
			{
				for (var i = 1; i < VirtualView.Navigation.NavigationStack.Count; i++)
				{
					pageStack.Add(VirtualView.Navigation.NavigationStack[i]);
				}
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

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			// I realize this is empty but it's necessary to register the active section as 
			// an appearance observer so that shell fires appearance changes when shell section changes
		}
	}
}
