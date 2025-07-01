#nullable disable
using System.Collections.Generic;
namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinitionCollection.xml" path="Type[@FullName='Microsoft.Maui.Controls.RowDefinitionCollection']/Docs/*" />
	public sealed class RowDefinitionCollection : DefinitionCollection<RowDefinition>
	{
		public RowDefinitionCollection() : base()
		{
		}

		public RowDefinitionCollection(params RowDefinition[] definitions) : base(definitions)
		{
		}

		internal RowDefinitionCollection(List<RowDefinition> definitions, bool copy) : base(definitions, copy)
		{
		}
	}
}
