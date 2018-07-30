//
// ContentPropertyAttribute.cs
//
// Author:
//       Stephane Delcroix <stephane@delcroix.org>
//
// Copyright (c) 2013 S. Delcroix
//

using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class ContentPropertyAttribute : Attribute
	{
		internal static string[] ContentPropertyTypes = { "Xamarin.Forms.ContentPropertyAttribute", "System.Windows.Markup.ContentPropertyAttribute" };

		public ContentPropertyAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}