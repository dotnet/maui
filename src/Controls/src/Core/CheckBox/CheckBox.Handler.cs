using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls;

partial class CheckBox
{
	protected override IElementHandler? GetHandler(IMauiContext context)
	{
		RemapForControlsIfNeeded();
		return new CheckBoxHandler();
	}

	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	protected override Type? GetHandlerType()
	{
		RemapForControlsIfNeeded();
		return typeof(CheckBoxHandler);
	}

	protected override void RemapForControlsIfNeeded()
	{
		base.RemapForControlsIfNeeded();
		RemapForControlsIfNeeded<CheckBox>(RemapForControls);
	}
}
