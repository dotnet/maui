using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Exposes the state of an element handler.
	/// </summary>
	[Flags]
	internal enum ElementHandlerState : byte
	{
		/// <summary>
		/// The handler is not connected to an element.
		/// </summary>
		Disconnected = 0x0,
		/// <summary>
		/// The handler is mapping all properties to the element.
		/// </summary>
		MappingProperties = 0x1,
		/// <summary>
		/// The handler is connecting for the first time to the element and mapping all properties.
		/// </summary>
		Connecting = MappingProperties | 0x2,
		/// <summary>
		/// The connected handler is now connecting to a new element and updating properties.
		/// </summary>
		Reconnecting = MappingProperties | 0x4,
		/// <summary>
		/// The handler is connected to an element.
		/// </summary>
		Connected = 0x8
	}
}
