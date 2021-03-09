using Microsoft.Windows.Design.Metadata;

namespace Microsoft.Maui.Controls.Core.Design
{
	internal class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable { get { return new AttributeTableBuilder().CreateTable(); } }
	}
}
