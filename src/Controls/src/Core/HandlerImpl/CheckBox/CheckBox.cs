namespace Microsoft.Maui.Controls
{
	public partial class CheckBox
	{
		// TODO Make public for NET7
		internal static IPropertyMapper<ICheckBox, CheckBoxHandler> ControlsCheckBoxMapper = new PropertyMapper<CheckBox, CheckBoxHandler>(CheckBoxHandler.Mapper)
		{

		};

		internal new static void RemapForControls()
		{
			CheckBoxHandler.Mapper = ControlsCheckBoxMapper;
		}
	}
}
