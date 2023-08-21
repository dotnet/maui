// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Design
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;

	public class KeyboardDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public KeyboardDesignTypeConverter()
		{
		}

		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new[]
			{
				"Plain",
				"Chat",
				"Default",
				"Email",
				"Numeric",
				"Telephone",
				"Text",
				"Url"
			};
	}
}