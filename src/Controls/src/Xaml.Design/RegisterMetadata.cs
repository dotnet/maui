// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.VisualStudio.DesignTools.Extensibility.Metadata;

namespace Microsoft.Maui.Controls.Xaml.Design
{
	class RegisterMetadata : IProvideAttributeTable
	{
		public AttributeTable AttributeTable => new AttributeTableBuilder().CreateTable();
	}
}