// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Design
{
	public class FontSizeDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public FontSizeDesignTypeConverter()
		{ }

		protected override string[] KnownValues
			=> new[] { "Default", "Micro", "Small", "Medium", "Large", "Body", "Header", "Title", "Subtitle", "Caption" };

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (KnownValues.Any(v => value?.ToString()?.Equals(v, StringComparison.Ordinal) ?? false))
				return true;

			if (double.TryParse(value?.ToString(), out var d))
				return true;

			return false;
		}
	}
}