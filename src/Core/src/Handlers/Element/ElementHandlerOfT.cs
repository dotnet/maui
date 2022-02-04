using System;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ElementHandler<TVirtualView, TPlatformView> : ElementHandler, IElementHandler
		where TVirtualView : class, IElement
		where TPlatformView : class
	{
		[HotReload.OnHotReload]
		internal static void OnHotReload()
		{
		}

		protected ElementHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		public new TPlatformView PlatformView
		{
			get => (TPlatformView?)base.PlatformView ?? throw new InvalidOperationException($"PlatformView cannot be null here");
			private set => base.PlatformView = value;
		}

		public new TVirtualView VirtualView
		{
			get => (TVirtualView?)base.VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null here");
			private protected set => base.VirtualView = value;
		}

		IElement? IElementHandler.VirtualView => base.VirtualView;

		object? IElementHandler.PlatformView => base.PlatformView;

		protected abstract TPlatformView CreatePlatformElement();

		protected virtual void ConnectHandler(TPlatformView nativeView)
		{
		}

		protected virtual void DisconnectHandler(TPlatformView nativeView)
		{
		}

		private protected override object OnCreatePlatformElement() =>
			CreatePlatformElement();

		private protected override void OnConnectHandler(object nativeView) =>
			ConnectHandler((TPlatformView)nativeView);

		private protected override void OnDisconnectHandler(object nativeView) =>
			DisconnectHandler((TPlatformView)nativeView);
	}
}