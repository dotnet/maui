using System;

namespace Microsoft.Maui.Controls;

partial class Toolbar
{
	IElementHandler? IElement.GetElementHandler(IMauiContext context) => null;
	Type? IElement.GetElementHandlerType() => null;
}
