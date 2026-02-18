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
		static CheckBox()
		{
			// Register dependency: Command depends on CommandParameter for CanExecute evaluation
			// See https://github.com/dotnet/maui/issues/31939
			CommandProperty.DependsOn(CommandParameterProperty);

			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(CheckBox), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			CheckBoxHandler.Mapper.ReplaceMapping<ICheckBox, ICheckBoxHandler>(nameof(Color), MapColor);
		}

		internal static void MapColor(ICheckBoxHandler handler, ICheckBox view)
		{
			handler?.UpdateValue(nameof(ICheckBox.Foreground));
		}
	}
}
