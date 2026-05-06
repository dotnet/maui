using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui
{
	internal static class InternalElementHandlerExtensions
	{
		/// <summary>
		/// The handler is connecting for the first time to the element and mapping all properties.
		/// </summary>
		/// <param name="handler"></param>
		/// <returns></returns>
		internal static bool IsMappingProperties(this IElementHandler handler) =>
			(handler as IElementHandlerStateExhibitor)?.State.HasFlag(ElementHandlerState.MappingProperties) ?? false;

		/// <summary>
		/// Indicates whether the handler is connecting for the first time to the element and mapping all properties.
		/// </summary>
		/// <param name="handler"></param>
		/// <returns></returns>
		internal static bool IsConnectingHandler(this IElementHandler handler) =>
			(handler as IElementHandlerStateExhibitor)?.State.HasFlag(ElementHandlerState.Connecting) ?? false;

		/// <summary>
		/// Indicates whether the connected handler is now connecting to a new element and updating properties.
		/// </summary>
		/// <param name="handler"></param>
		/// <returns></returns>
		internal static bool IsReconnectingHandler(this IElementHandler handler) =>
			(handler as IElementHandlerStateExhibitor)?.State.HasFlag(ElementHandlerState.Reconnecting) ?? false;

		/// <summary>
		/// Indicates whether the handler is currently being disconnected from its element.
		/// While disconnecting, the platform view has already been released so property and command
		/// mappers must not be invoked.
		/// </summary>
		/// <param name="handler"></param>
		/// <returns></returns>
		internal static bool IsDisconnectingHandler(this IElementHandler handler) =>
			(handler as IElementHandlerStateExhibitor)?.State.HasFlag(ElementHandlerState.Disconnecting) ?? false;
	}
}
