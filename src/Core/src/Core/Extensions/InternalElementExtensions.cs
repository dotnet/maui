using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui
{
	internal static class InternalElementExtensions
	{
		/// <summary>
		/// The handler is connecting for the first time to the element and mapping all properties.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		internal static bool IsMappingProperties(this IElement element) =>
			(element.Handler as IElementHandlerStateExhibitor)?.State.HasFlag(ElementHandlerState.MappingProperties) ?? false;

		/// <summary>
		/// Indicates whether the handler is connecting for the first time to the element and mapping all properties.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		internal static bool IsConnectingHandler(this IElement element) =>
			(element.Handler as IElementHandlerStateExhibitor)?.State.HasFlag(ElementHandlerState.Connecting) ?? false;

		/// <summary>
		/// Indicates whether the connected handler is now connecting to a new element and updating properties.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		internal static bool IsReconnectingHandler(this IElement element) =>
			(element.Handler as IElementHandlerStateExhibitor)?.State.HasFlag(ElementHandlerState.Reconnecting) ?? false;
	}
}
