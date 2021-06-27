using System;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIViewController;
#elif MONOANDROID
using NativeView = Android.App.Activity;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Window;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ElementHandler<TVirtualView, TNativeView> : ElementHandler
		where TVirtualView : class, IElement
#if !NETSTANDARD || IOS || ANDROID || WINDOWS
		where TNativeView : NativeView
#else
		where TNativeView : class
#endif
	{
		protected ElementHandler(PropertyMapper mapper)
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

		protected abstract TNativeView CreateNativeElement();

		protected virtual void SetupDefaults(TNativeView nativeView)
		{
		}

		protected virtual void ConnectHandler(TNativeView nativeView)
		{
		}

		protected virtual void DisconnectHandler(TNativeView nativeView)
		{
		}

		private protected override object OnCreateNativeElement() =>
			CreateNativeElement();

		private protected override void OnSetupDefaults(object nativeView) =>
			SetupDefaults((TNativeView)nativeView);

		private protected override void OnConnectHandler(object nativeView) =>
			ConnectHandler((TNativeView)nativeView);

		private protected override void OnDisconnectHandler(object nativeView) =>
			DisconnectHandler((TNativeView)nativeView);
	}
}