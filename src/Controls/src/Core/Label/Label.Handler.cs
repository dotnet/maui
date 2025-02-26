using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls;

partial class Label
{
	protected override IElementHandler? GetHandler(IMauiContext context)
	{
		RemapForControlsIfNeeded();
		return new LabelHandler();
	}

	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	protected override Type? GetHandlerType()
	{
		RemapForControlsIfNeeded();
		return typeof(LabelHandler);
	}

	protected override void RemapForControlsIfNeeded()
	{
		base.RemapForControlsIfNeeded();
		RemapForControlsIfNeeded<Label>(RemapForControls);
	}
}
