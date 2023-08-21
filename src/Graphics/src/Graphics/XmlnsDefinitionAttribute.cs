// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Maui.Graphics
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[DebuggerDisplay("{XmlNamespace}, {ClrNamespace}")]
	internal sealed class XmlnsDefinitionAttribute : Attribute
	{
		public string XmlNamespace { get; }
		public string ClrNamespace { get; }

		public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
		{
			ClrNamespace = clrNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(clrNamespace));
		}
	}
}
