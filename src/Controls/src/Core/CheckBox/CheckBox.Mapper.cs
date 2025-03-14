using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class CheckBox : IRemappable
	{
		void IRemappable.RemapForControls()
		{
			RemappingHelper.RemapIfNeeded(typeof(VisualElement), VisualElement.RemapIfNeeded);

			CheckBoxHandler.Mapper.ReplaceMapping<ICheckBox, ICheckBoxHandler>(nameof(Color), MapColor);
		}

		internal static void MapColor(ICheckBoxHandler? handler, ICheckBox view)
		{
			handler?.UpdateValue(nameof(ICheckBox.Foreground));
		}
	}
}
