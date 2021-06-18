#nullable enable
using System;
using System.Runtime.CompilerServices;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : ViewHandler,
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

		[HotReload.OnHotReload]
		static void OnHotReload()
		{
			HasSetDefaults = false;
		}

		protected ViewHandler(PropertyMapper mapper)
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
		}

		protected abstract TNativeView CreateNativeView();

		public new TNativeView NativeView
		{
			get => (TNativeView?)base.NativeView ?? throw new InvalidOperationException($"NativeView cannot be null here");
			private set => base.NativeView = value;
		}

		public override void SetVirtualView(IView view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (base.VirtualView == view)
				return;

			if (base.VirtualView?.Handler != null && base.VirtualView.Handler != this)
				VirtualView.Handler = null;

			bool setupNativeView = base.VirtualView == null;

			VirtualView = (TVirtualView)view;
			NativeView = (TNativeView?)base.NativeView ?? CreateNativeView();

			if (VirtualView.Handler != this)
				VirtualView.Handler = this;

			if (setupNativeView)
			{
				ConnectHandler(NativeView);
			}

			if (!HasSetDefaults)
			{
				if (NativeView != null)
				{
					SetupDefaults(NativeView);
				}

				HasSetDefaults = true;
			}

			_mapper = _defaultMapper;

			if (VirtualView is IPropertyMapperView imv)
			{
				var map = imv.GetPropertyMapperOverrides();
				var instancePropertyMapper = map as PropertyMapper<TVirtualView>;

				if (instancePropertyMapper != null)
				{
					instancePropertyMapper.Chained = _defaultMapper;
					_mapper = instancePropertyMapper;
				}
			}

			_mapper.UpdateProperties(this, VirtualView);
		}

		void IViewHandler.DisconnectHandler()
		{
			if (base.NativeView != null && base.VirtualView != null)
				DisconnectHandler(NativeView);
		}

		protected virtual void ConnectHandler(TNativeView nativeView)
		{
			base.ConnectHandler(nativeView);
		}

		protected virtual void DisconnectHandler(TNativeView nativeView)
		{
			base.DisconnectHandler(nativeView);
		}

		public override void UpdateValue(string property)
		{
			if (base.VirtualView == null)
				return;

			_mapper?.UpdateProperty(this, VirtualView, property);
		}

		protected virtual void SetupDefaults(TNativeView nativeView) { }


		public new TVirtualView VirtualView
		{
			get => (TVirtualView?)base.VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null here");
			private protected set => base.VirtualView = value;
		}

		IView? IViewHandler.VirtualView
		{
			get => base.VirtualView;
		}

		object? IViewHandler.NativeView
		{
			get => base.NativeView;
		}
	}
}