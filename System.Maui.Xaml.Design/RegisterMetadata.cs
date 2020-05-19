using Microsoft.Windows.Design.Metadata;

namespace System.Maui.Xaml.Design
{
	class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable => new AttributeTableBuilder().CreateTable();
	}
}