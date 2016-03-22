using Microsoft.Windows.Design;

namespace Xamarin.Forms.Xaml.Design
{
	class AttributeTableBuilder : Microsoft.Windows.Design.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			// Turn off validation of values, which doesn't work for OnPlatform/OnIdiom
			AddCustomAttributes(typeof (ArrayExtension).Assembly,
				new XmlnsSupportsValidationAttribute("http://xamarin.com/schemas/2014/forms", false));
		}
	}
}