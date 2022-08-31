#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellSectionHandler : ElementHandler<ShellSection, ShellSectionStackManager>, IDisposable
	{
		bool _disposedValue;
		Page? _dummyPage;

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

		public ShellSectionHandler() : base(Mapper, CommandMapper)
		{
		}

		~ShellSectionHandler()
		{
			Dispose(disposing: false);
		}

		protected override ShellSectionStackManager CreatePlatformElement()
		{
			return new ShellSectionStackManager();
		}

		public static void MapCurrentItem(ShellSectionHandler handler, ShellSection item)
		{
			handler.SyncNavigationStack(false);
		}

		public static void RequestNavigation(ShellSectionHandler handler, IStackNavigation view, object? arg3)
		{
			if (arg3 is NavigationRequest nr)
			{
				handler.PlatformView.RequestNavigation(nr);
			}
			else
			{
				throw new InvalidOperationException("Args must be NavigationRequest");
			}
		}

		protected override void ConnectHandler(ShellSectionStackManager platformView)
		{
			if (VirtualView is IShellSectionController shellSectionController)
			{
				shellSectionController.NavigationRequested += OnNavigationRequested;
			}

			platformView.Connect(VirtualView);
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ShellSectionStackManager platformView)
		{
			if (VirtualView is IShellSectionController shellSectionController)
			{
				shellSectionController.NavigationRequested -= OnNavigationRequested;
			}

			platformView.Disconnect();
			base.DisconnectHandler(platformView);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					var platformView = PlatformView;
					foreach (var item in VirtualView.Items)
					{
						if (item.Handler is IDisposable thandler)
						{
							thandler.Dispose();
						}
					}
					(this as IElementHandler)?.DisconnectHandler();
					platformView?.Dispose();
				}

				_disposedValue = true;
			}
		}

		void OnNavigationRequested(object? sender, NavigationRequestedEventArgs e)
		{
			SyncNavigationStack(e.Animated);
		}

		void SyncNavigationStack(bool animated)
		{
			if (_dummyPage == null)
				_dummyPage = new DummyPage();

			List<IView> pageStack = new List<IView>()
			{
				// TODO. It is dummy root page to sync navigation stack
				_dummyPage
			};

			for (var i = 1; i < VirtualView.Navigation.NavigationStack.Count; i++)
			{
				pageStack.Add(VirtualView.Navigation.NavigationStack[i]);
			}

			(VirtualView as IStackNavigation).RequestNavigation(new NavigationRequest(pageStack, animated));
		}
	}

	class DummyPage : Page { }
}
