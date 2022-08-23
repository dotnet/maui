using System;
using ElmSharp;
using Microsoft.Maui.HotReload;

namespace Microsoft.Maui.Platform
{
	public class ContainerView : Box, IReloadHandler
	{
		readonly IMauiContext? _context;

		EvasObject? _mainView;
		IElement? _view;

		public ContainerView(IMauiContext context) : this(context.GetPlatformParent(), context)
		{
		}

		public ContainerView(EvasObject parent, IMauiContext context) : base(parent)
		{
			_context = context;
		}

		public EvasObject? MainView
		{
			get => _mainView;
			set
			{
				if (_mainView != null)
				{
					UnPack(_mainView);
				}

				_mainView = value;

				if (_mainView != null)
				{
					_mainView.SetAlignment(-1, -1);
					_mainView.SetWeight(1, 1);
					PackEnd(_mainView);
				}
			}
		}

		public IElement? CurrentView
		{
			get => _view;
			set => SetView(value);
		}

		void SetView(IElement? view, bool forceRefresh = false)
		{
			if (view == _view && !forceRefresh)
				return;

			_view = view;

			if (_view is IHotReloadableView ihr)
			{
				ihr.ReloadHandler = this;
				MauiHotReloadHelper.AddActiveView(ihr);
			}

			MainView = null;

			if (_view != null)
			{
				_ = _context ?? throw new ArgumentNullException(nameof(_context));
				MainView = _view.ToPlatform(_context);
			}
		}

		public void Reload() => SetView(CurrentView, true);
	}
}