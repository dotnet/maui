using System;
using Microsoft.Maui.HotReload;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public class ContainerView : ViewGroup, IReloadHandler
	{
		readonly IMauiContext? _context;

		IElement? _view;

		public ContainerView(IMauiContext context)
		{
			_context = context;
		}

		public IElement? CurrentView
		{
			get => _view;
			set => SetView(value);
		}

		public NView? CurrentPlatformView { get; private set; }

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

			Children.Clear();
			CurrentPlatformView = null;

			if (_view != null)
			{
				_ = _context ?? throw new ArgumentNullException(nameof(_context));
				var nativeView = _view.ToPlatform(_context);
				nativeView.WidthSpecification = Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent;
				nativeView.HeightSpecification = Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent;
				Children.Add(nativeView);
				CurrentPlatformView = nativeView;
			}
		}

		public void Reload() => SetView(CurrentView, true);
	}
}
