#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	internal interface IControlsElement : Maui.IElement
	{
		event EventHandler<HandlerChangingEventArgs>? HandlerChanging;
		event EventHandler? HandlerChanged;
	}
}
