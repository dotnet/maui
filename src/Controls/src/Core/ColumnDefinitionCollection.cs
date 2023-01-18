#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinitionCollection.xml" path="Type[@FullName='Microsoft.Maui.Controls.ColumnDefinitionCollection']/Docs/*" />
	public sealed class ColumnDefinitionCollection : DefinitionCollection<ColumnDefinition>
	{
		public ColumnDefinitionCollection() : base()
		{
		}

		public ColumnDefinitionCollection(params ColumnDefinition[] definitions) : base(definitions)
		{
		}
	}
}
