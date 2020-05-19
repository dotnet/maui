using Microsoft.Windows.Design.Metadata;

namespace System.Maui.Core.Design
{
	internal class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable { get { return new AttributeTableBuilder ().CreateTable (); } }
	}
}
