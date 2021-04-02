using System;
#if __IOS__
using NativeView = UIKit.UIView;
#elif __MACOS__
using NativeView = AppKit.NSView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView> : ViewHandler
		where TVirtualView : class, IView
	{
		TVirtualView? _virtualView;
		NativeView? _nativeView;

		public new TVirtualView? VirtualView
		{
			get => _virtualView;
			private set => SetVirtualViewCore(value);
		}

		private protected override void SetVirtualViewCore(IView? virtualView)
		{
			_virtualView = (TVirtualView?)virtualView;
			base.SetVirtualViewCore(virtualView);
		}

		public new NativeView? View
		{
			get => _nativeView;
			private set => SetNativeViewCore(value);
		}

		private protected override void SetNativeViewCore(object? nativeView)
		{
			_nativeView = (NativeView?)nativeView;
			base.SetNativeViewCore(nativeView);
		}
	}

	public abstract partial class ViewHandler<TVirtualView, TNativeView> : ViewHandler<TVirtualView>,
		IViewHandler
		where TVirtualView : class, IView
#if !NETSTANDARD || IOS || ANDROID || WINDOWS
		where TNativeView : NativeView
#else
		where TNativeView : class
#endif
	{
		protected readonly PropertyMapper _defaultMapper;
		protected PropertyMapper _mapper;
		static bool HasSetDefaults;
		private TNativeView? _nativeView;

		protected ViewHandler(PropertyMapper mapper)
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
		}

		protected abstract TNativeView CreateNativeView();

		public new TNativeView? View
		{
			get => _nativeView; 
			private set => SetNativeViewCore(value);
		}

		private protected override void SetNativeViewCore(object? nativeView)
		{
			_nativeView = (TNativeView?)nativeView;
			base.SetNativeViewCore(nativeView);
		}

		public IServiceProvider? Services => MauiContext?.Services;

		public override void SetVirtualView(IView view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (VirtualView?.Handler != null)
				VirtualView.Handler = null;

			bool setupNativeView = VirtualView == null;

			SetVirtualViewCore(view);
			View ??= CreateNativeView();

			if (setupNativeView && View != null)
			{
				ConnectHandler(View);
			}

			if (!HasSetDefaults)
			{
				if (View != null)
				{
					SetupDefaults(View);
				}

				HasSetDefaults = true;
			}

			_mapper = _defaultMapper;

			if (VirtualView is IPropertyMapperView imv)
			{
				var map = imv.GetPropertyMapperOverrides();
				var instancePropertyMapper = map as PropertyMapper<TVirtualView>;
				if (map != null && instancePropertyMapper == null)
				{
				}
				if (instancePropertyMapper != null)
				{
					instancePropertyMapper.Chained = _defaultMapper;
					_mapper = instancePropertyMapper;
				}
			}

			_mapper.UpdateProperties(this, VirtualView);
		}

		protected virtual void ConnectHandler(TNativeView nativeView)
		{
			ConnectHandler(nativeView);
		}

		protected virtual void DisconnectHandler(TNativeView nativeView)
		{
			DisconnectHandler(nativeView);
		}

		void IViewHandler.DisconnectHandler()
		{
			if (View != null && VirtualView != null)
				DisconnectHandler(View);

			if (VirtualView != null)
				VirtualView.Handler = null;

			SetVirtualViewCore(null);
		}

		public override void UpdateValue(string property)
			=> _mapper?.UpdateProperty(this, VirtualView, property);

		protected virtual void SetupDefaults(TNativeView nativeView) { }
	}
}