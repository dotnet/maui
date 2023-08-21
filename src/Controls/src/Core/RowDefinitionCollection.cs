// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
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
	}
}
