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
	/// <include file="../../docs/Microsoft.Maui.Controls/ContentPropertyAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.ContentPropertyAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ContentPropertyAttribute : Attribute
	{
		internal static string[] ContentPropertyTypes = { "Microsoft.Maui.Controls.ContentPropertyAttribute", "System.Windows.Markup.ContentPropertyAttribute" };

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPropertyAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ContentPropertyAttribute(string name) => Name = name;

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPropertyAttribute.xml" path="//Member[@MemberName='Name']/Docs/*" />
		public string Name { get; }
	}
}