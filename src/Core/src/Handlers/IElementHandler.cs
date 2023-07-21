using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IElementHandler
	{
		void SetMauiContext(IMauiContext mauiContext);

		void SetVirtualView(IElement view);

		void UpdateValue(string property);

		void Invoke(string command, object? args = null);

		void DisconnectHandler();

		object? PlatformView { get; }

		IElement? VirtualView { get; }

		IMauiContext? MauiContext { get; }

#pragma warning disable RS0016 // Add public types and members to the declared API

#if NETSTANDARD2_0
		void Disconnect(bool isDestroying);
		void Connect();
#else
		void Disconnect(bool isDestroying) { }
		void Connect() { }
#endif

#pragma warning restore RS0016 // Add public types and members to the declared API
	}
}