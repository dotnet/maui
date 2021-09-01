#nullable enable
//
// XamlParser.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2013-2014 Microsoft.Maui.Controls, Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml
{
	static class XmlTypeXamlExtensions
	{
		public static T? GetTypeReference<T>(
			this XmlType xmlType,
			IEnumerable<XmlnsDefinitionAttribute> xmlnsDefinitions,
			string defaultAssemblyName,
			Func<(string typeName, string clrNamespace, string assemblyName), T> refFromTypeInfo)
			where T : class
		{
			var lookupAssemblies = new List<XmlnsDefinitionAttribute>();
			var namespaceURI = xmlType.NamespaceUri;
			var elementName = xmlType.Name;
			var typeArguments = xmlType.TypeArguments;

			foreach (var xmlnsDef in xmlnsDefinitions)
			{
				if (xmlnsDef.XmlNamespace != namespaceURI)
					continue;
				lookupAssemblies.Add(xmlnsDef);
			}

			if (lookupAssemblies.Count == 0)
			{
				XmlnsHelper.ParseXmlns(namespaceURI, out _, out var ns, out var asmstring, out _);
				asmstring ??= defaultAssemblyName;
				if (namespaceURI != null && ns != null)
					lookupAssemblies.Add(new XmlnsDefinitionAttribute(namespaceURI, ns) { AssemblyName = asmstring });
			}

			var lookupNames = new List<string>();
			if (elementName != "DataTemplate" && !elementName.EndsWith("Extension", StringComparison.Ordinal))
				lookupNames.Add(elementName + "Extension");
			lookupNames.Add(elementName);

			for (var i = 0; i < lookupNames.Count; i++)
			{
				var name = lookupNames[i];
				if (name.Contains(":"))
					name = name.Substring(name.LastIndexOf(':') + 1);
				if (typeArguments != null)
					name += "`" + typeArguments.Count; //this will return an open generic Type
				lookupNames[i] = name;
			}

			var potentialTypes = new List<(string typeName, string clrNamespace, string assemblyName)>();
			foreach (string typeName in lookupNames)
				foreach (XmlnsDefinitionAttribute xmlnsDefinitionAttribute in lookupAssemblies)
					potentialTypes.Add(new (typeName, xmlnsDefinitionAttribute.ClrNamespace, xmlnsDefinitionAttribute.AssemblyName));

			T? type = null;
			foreach (var typeInfo in potentialTypes)
				if ((type = refFromTypeInfo(typeInfo)) != null)
					break;

			return type;
		}
	}
}