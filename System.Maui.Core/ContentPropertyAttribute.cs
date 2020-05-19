//
// ContentPropertyAttribute.cs
//
// Author:
//       Stephane Delcroix <stephane@delcroix.org>
//
// Copyright (c) 2013 S. Delcroix
//

using System;

namespace System.Maui
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class ContentPropertyAttribute : Attribute
	{
		internal static string[] ContentPropertyTypes = { "System.Maui.ContentPropertyAttribute", "System.Windows.Markup.ContentPropertyAttribute" };

		public ContentPropertyAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}