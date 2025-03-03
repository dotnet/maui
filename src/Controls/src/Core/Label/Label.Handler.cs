using System;

namespace Microsoft.Maui.Controls;

partial class Label
{
	protected override IElementHandler? GetHandler(IMauiContext context)
	{
		RemapForControlsIfNeeded();
		return new LabelHandler();
	}

	protected override Type? GetHandlerType()
	{
		return typeof(LabelHandler);
	}

	protected override void RemapForControlsIfNeeded()
	{
		base.RemapForControlsIfNeeded();
		RemapForControlsIfNeeded<Label>(RemapForControls);
	}
}
