using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class CheckBox
	{
		// TODO Make public for NET8
		internal static IPropertyMapper<ICheckBox, CheckBoxHandler> ControlsCheckBoxMapper = new PropertyMapper<CheckBox, CheckBoxHandler>(CheckBoxHandler.Mapper)
		{
			[nameof(CheckBox.Color)] = (handler, _) => handler?.UpdateValue(nameof(ICheckBox.Foreground))
		};

		internal new static void RemapForControls()
		{
			CheckBoxHandler.Mapper = ControlsCheckBoxMapper;
		}
	}
}
