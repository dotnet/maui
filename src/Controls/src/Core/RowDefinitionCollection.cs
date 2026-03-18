#nullable disable
using System.Collections.Generic;
namespace Microsoft.Maui.Controls
{
	/// <summary>A collection of <see cref="RowDefinition"/> objects that define the rows of a <see cref="Grid"/>.</summary>
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
