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
		static CheckBox() => RemapForControls();

		private new static void RemapForControls()
		{
			VisualElement.RemapForControls();

			CheckBoxHandler.Mapper.ReplaceMapping<ICheckBox, ICheckBoxHandler>(nameof(Color), MapColor);
		}

		internal static void MapColor(ICheckBoxHandler handler, ICheckBox view)
		{
			handler?.UpdateValue(nameof(ICheckBox.Foreground));
		}
	}
}
