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
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(CheckBox)))
			{
				base.RemapForControls(remapped);

				// Register dependency: Command depends on CommandParameter for CanExecute evaluation
				// See https://github.com/dotnet/maui/issues/31939
				CommandProperty.DependsOn(CommandParameterProperty);

				CheckBoxHandler.Mapper.ReplaceMapping<ICheckBox, ICheckBoxHandler>(nameof(Color), MapColor);
			}
		}

		internal static void MapColor(ICheckBoxHandler handler, ICheckBox view)
		{
			handler?.UpdateValue(nameof(ICheckBox.Foreground));
		}
	}
}
