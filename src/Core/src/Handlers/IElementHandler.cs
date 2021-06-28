namespace Microsoft.Maui
{
	public interface IElementHandler
	{
		void SetMauiContext(IMauiContext mauiContext);

		void SetVirtualView(IElement view);

		void UpdateValue(string property);

		void DisconnectHandler();

		object? NativeView { get; }

		IElement? VirtualView { get; }

		IMauiContext? MauiContext { get; }
	}
}