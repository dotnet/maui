using ExampleFramework;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public static class CheckBoxExamples
    {
		[UIExample]
		public static CheckBox SimpleCheckBox() =>
			new CheckBox();

		[UIExample]
		public static CheckBox SimpleCheckedCheckBox() =>
			new CheckBox()
			{
				IsChecked = true
			};
	}
}