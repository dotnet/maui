// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls.StyleSheets
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
	sealed class StylePropertyAttribute : Attribute
	{
		public string CssPropertyName { get; }
		public string BindablePropertyName { get; }
		public Type TargetType { get; }
		public Type PropertyOwnerType { get; set; }
		public BindableProperty BindableProperty { get; set; }
		public bool Inherited { get; set; } = false;


		public StylePropertyAttribute(string cssPropertyName, Type targetType, string bindablePropertyName)
		{
			CssPropertyName = cssPropertyName;
			BindablePropertyName = bindablePropertyName;
			TargetType = targetType;
		}
	}
}