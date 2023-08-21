// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class ILRootNode : RootNode
	{
		public ILRootNode(XmlType xmlType, TypeReference typeReference, IXmlNamespaceResolver nsResolver, int linenumber = -1, int lineposition = -1) : base(xmlType, nsResolver, linenumber: linenumber, lineposition: lineposition)
		{
			TypeReference = typeReference;
		}

		public TypeReference TypeReference { get; private set; }
	}
}