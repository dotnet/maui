using Microsoft.VisualStudio.DesignTools.Extensibility.Metadata;

namespace Microsoft.Maui.Controls.Design
{
	internal class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable => new AttributeTableBuilder().CreateTable();
	}
}
