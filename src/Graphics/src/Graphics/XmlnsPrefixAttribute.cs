// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Graphics
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class XmlnsPrefixAttribute : Attribute
	{
		public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
		{
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
		}

		public string XmlNamespace { get; }
		public string Prefix { get; }
	}
}
