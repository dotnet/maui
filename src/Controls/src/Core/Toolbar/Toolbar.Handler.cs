using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls;

partial class Toolbar
{
	IElementHandler? IElement.GetElementHandler(IMauiContext context)
	{
		return null;
	}

	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	Type? IElement.GetElementHandlerType()
	{
		return null;
	}
}
