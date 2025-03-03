using System;

namespace Microsoft.Maui.Controls;

partial class CheckBox
{
	protected override IElementHandler? GetHandler(IMauiContext context)
	{
		RemapForControlsIfNeeded();
		return new CheckBoxHandler();
	}

	protected override Type? GetHandlerType()
	{
		return typeof(CheckBoxHandler);
	}

	protected override void RemapForControlsIfNeeded()
	{
		base.RemapForControlsIfNeeded();
		RemapForControlsIfNeeded<CheckBox>(RemapForControls);
	}
}
