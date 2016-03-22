using Microsoft.Windows.Design.Metadata;

namespace Xamarin.Forms.Xaml.Design
{
	class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable => new AttributeTableBuilder().CreateTable();
	}
}