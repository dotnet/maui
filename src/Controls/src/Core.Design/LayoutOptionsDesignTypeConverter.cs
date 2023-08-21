// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Design
{
	public class LayoutOptionsDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public LayoutOptionsDesignTypeConverter()
		{
		}

		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new string[] { "Start", "Center", "End", "Fill", "StartAndExpand", "CenterAndExpand", "EndAndExpand", "FillAndExpand" };
	}
}