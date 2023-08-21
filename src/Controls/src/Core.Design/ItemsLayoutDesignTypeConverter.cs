// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Design
{
	public class ItemsLayoutDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public ItemsLayoutDesignTypeConverter()
		{
		}

		protected override string[] KnownValues
			=> new[] { "VerticalList", "HorizontalList", "VerticalGrid", "HorizontalGrid" };
	}
}