using ExampleFramework;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public static class ButtonExamples
    {
		[UIExample]
		public static Button SimpleButton() =>
			new Button()
			{
				Text = "Click Me",
				FontSize = 20,
				BorderWidth = 1,
			};
   }
}