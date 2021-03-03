using Microsoft.Windows.Design.Metadata;

namespace Microsoft.Maui.Controls.Xaml.Design
{
	class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable => new AttributeTableBuilder().CreateTable();
	}
}