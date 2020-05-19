using Microsoft.Windows.Design.Metadata;

namespace Xamarin.Forms.Core.Design
{
	internal class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable { get { return new AttributeTableBuilder ().CreateTable (); } }
	}
}
