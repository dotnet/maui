#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class CheckBox
	{
		public static IPropertyMapper<ICheckBox, CheckBoxHandler> ControlsCheckBoxMapper = new PropertyMapper<CheckBox, CheckBoxHandler>(CheckBoxHandler.Mapper)
		{
			[nameof(Color)] = (handler, view) =>
			{
				handler?.UpdateValue(nameof(ICheckBox.Foreground));
			}
		};

		internal new static void RemapForControls()
		{
			CheckBoxHandler.Mapper = ControlsCheckBoxMapper;
		}
	}
}
