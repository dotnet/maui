using System;
using Android.Content;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui
{
	internal partial class ScopedMauiContext : MauiContext, IScopedMauiContext
	{
		IAnimationManager? _animationManager;
		readonly IMauiContext _mauiContext;
		readonly IScopedMauiContext? _scopedMauiContext;
		readonly WeakReference<LayoutInflater>? _layoutInflater;
		readonly WeakReference<FragmentManager>? _fragmentManager;

		public ScopedMauiContext(IMauiContext mauiContext, IServiceProvider? services = null, Context? context = null, LayoutInflater? layoutInflater = null, FragmentManager? fragmentManager = null) :
			base(services ?? mauiContext.Services,
				context ?? mauiContext.Context!)
		{
			_mauiContext = mauiContext;
			_scopedMauiContext = _mauiContext as IScopedMauiContext;

			if (layoutInflater != null)
				_layoutInflater = new WeakReference<LayoutInflater>(layoutInflater);

			if (fragmentManager != null)
				_fragmentManager = new WeakReference<FragmentManager>(fragmentManager);
		}

		public LayoutInflater? LayoutInflater
		{
			get
			{
				if (_layoutInflater == null)
					return null;

				return _layoutInflater.TryGetTarget(out LayoutInflater? layoutInflater) ? layoutInflater : null;
			}
		}

		public FragmentManager? FragmentManager
		{
			get
			{
				if (_fragmentManager == null)
					return null;

				return _fragmentManager.TryGetTarget(out FragmentManager? fragmentManager) ? fragmentManager : null;
			}
		}

		IAnimationManager IScopedMauiContext.AnimationManager => _animationManager ??=
			(_scopedMauiContext?.AnimationManager ?? Services.GetRequiredService<IAnimationManager>());
	}
}
