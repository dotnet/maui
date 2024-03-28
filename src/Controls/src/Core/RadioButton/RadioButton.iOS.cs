#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			// We want to modify the text in the content property only if
			// the content is a string. In that case we want to apply
			// the text values from the radio button
			if (radioButton.Content is string
				&& handler.VirtualView.PresentedContent is Element element
				&& FindLabel(element) is Label label)
			{
				label.FontFamily = radioButton.FontFamily;
				label.CharacterSpacing = radioButton.CharacterSpacing;
				label.TextColor = radioButton.TextColor;
				label.TextTransform = radioButton.TextTransform;
				label.FontAttributes = radioButton.FontAttributes;
				label.FontSize = radioButton.FontSize;
			}

			if (radioButton.ResolveControlTemplate() == null)
				radioButton.ControlTemplate = DefaultTemplate;

			RadioButtonHandler.MapContent(handler, radioButton);
		}

		private static Label FindLabel(Element parent)
		{
			if (parent is Label label)
			{
				return label;
			}

			foreach (var child in parent.LogicalChildrenInternal)
			{
				var foundChild = FindLabel(child);
				if (foundChild != null)
				{
					return foundChild;
				}
			}
			
			return null;
		}
	}
}