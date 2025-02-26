using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls;

partial class Element
{
	/// <summary>
	/// Create the Handler for the Element.
	/// </summary>
	protected virtual IElementHandler? GetHandler(IMauiContext context)
	{
		return null;
	}

	/// <summary>
	/// Create the type of the Handler for the Element.
	/// </summary>
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	protected virtual Type? GetHandlerType()
	{
		return null;
	}

	protected virtual void RemapForControlsIfNeeded()
	{
		RemapForControlsIfNeeded<Element>(RemapForControlsCore);
	}

	IElementHandler? IElement.GetElementHandler(IMauiContext context) => GetHandler(context);

	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	Type? IElement.GetElementHandlerType() => GetHandlerType();

	private static readonly HashSet<Type> s_remappedTypes = new ();
	protected internal static void RemapForControlsIfNeeded<T>(Action remapForControls)
	{
		if (s_remappedTypes.Add(typeof(T)))
		{
			remapForControls();
		}
	}
}
