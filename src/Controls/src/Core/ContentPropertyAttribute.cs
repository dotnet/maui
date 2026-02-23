#nullable disable
//
// ContentPropertyAttribute.cs
//
// Author:
//       Stephane Delcroix <stephane@delcroix.org>
//
// Copyright (c) 2013 S. Delcroix
//

using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Indicates the property used as the content property in XAML.</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ContentPropertyAttribute : Attribute
	{
		internal static string[] ContentPropertyTypes = { "Microsoft.Maui.Controls.ContentPropertyAttribute", "System.Windows.Markup.ContentPropertyAttribute" };

		/// <summary>Creates a new <see cref="ContentPropertyAttribute"/> with the specified property name.</summary>
		public ContentPropertyAttribute(string name) => Name = name;

		/// <summary>Gets the name of the content property.</summary>
		public string Name { get; }
	}
}