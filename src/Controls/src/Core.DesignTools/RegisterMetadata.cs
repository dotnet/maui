using Microsoft.VisualStudio.DesignTools.Extensibility.Metadata;

namespace Microsoft.Maui.Controls.Core.Design
{
	internal class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
			=> new AttributeTableBuilder().CreateTable();
	}
}
