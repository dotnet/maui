using System;
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
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : ViewHandler, IViewHandler
		where TVirtualView : class, IView
#if !NETSTANDARD || IOS || ANDROID || WINDOWS
		where TNativeView : NativeView
#else
		where TNativeView : class
#endif
	{
		[HotReload.OnHotReload]
		internal static void OnHotReload()
		{
			Defaults<ViewHandler<TVirtualView, TNativeView>>.HasSetDefaults = false;
		}

		protected ViewHandler(PropertyMapper mapper)
			: base(mapper)
		{
		}

		public new TNativeView NativeView
		{
			get => (TNativeView?)base.NativeView ?? throw new InvalidOperationException($"NativeView cannot be null here");
			private set => base.NativeView = value;
		}

		public new TVirtualView VirtualView
		{
			get => (TVirtualView?)base.VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null here");
			private protected set => base.VirtualView = value;
		}

		protected override bool HasSetDefaults
		{
			get => Defaults<ViewHandler<TVirtualView, TNativeView>>.HasSetDefaults;
			set => Defaults<ViewHandler<TVirtualView, TNativeView>>.HasSetDefaults = value;
		}

		public virtual void SetVirtualView(IView view) =>
			base.SetVirtualView(view);

		public sealed override void SetVirtualView(IElement view) =>
			SetVirtualView((IView)view);

		protected abstract TNativeView CreateNativeView();

		protected virtual void SetupDefaults(TNativeView nativeView)
		{
		}

		protected virtual void ConnectHandler(TNativeView nativeView)
		{
		}

		protected virtual void DisconnectHandler(TNativeView nativeView)
		{
		}

		private protected override NativeView OnCreateNativeView() =>
			CreateNativeView();

		private protected override void OnSetupDefaults(NativeView nativeView) =>
			SetupDefaults((TNativeView)nativeView);

		private protected override void OnConnectHandler(NativeView nativeView) =>
			ConnectHandler((TNativeView)nativeView);

		private protected override void OnDisconnectHandler(NativeView nativeView) =>
			DisconnectHandler((TNativeView)nativeView);
	}
}