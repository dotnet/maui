#nullable disable
using System.Collections.Generic;
namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.DefinitionCollection{T}"/> for <see cref="Microsoft.Maui.Controls.ColumnDefinition"/>s.</summary>
	public sealed class ColumnDefinitionCollection : DefinitionCollection<ColumnDefinition>
	{
		public ColumnDefinitionCollection() : base()
		{
		}

		public ColumnDefinitionCollection(params ColumnDefinition[] definitions) : base(definitions)
		{
		}

		internal ColumnDefinitionCollection(List<ColumnDefinition> definitions, bool copy) : base(definitions, copy)
		{
		}
	}
}
