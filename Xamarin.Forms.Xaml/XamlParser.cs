//
// XamlParser.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2013-2014 Xamarin, Inc
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml
{
	static class XamlParser
	{
		public const string XFUri = "http://xamarin.com/schemas/2014/forms";
		public const string X2006Uri = "http://schemas.microsoft.com/winfx/2006/xaml";
		public const string X2009Uri = "http://schemas.microsoft.com/winfx/2009/xaml";
		public const string McUri = "http://schemas.openxmlformats.org/markup-compatibility/2006";

		public static void ParseXaml(RootNode rootNode, XmlReader reader)
		{
			IList<KeyValuePair<string, string>> xmlns;
			var attributes = ParseXamlAttributes(reader, out xmlns);
			var prefixes = PrefixesToIgnore(xmlns);
			(rootNode.IgnorablePrefixes ?? (rootNode.IgnorablePrefixes=new List<string>())).AddRange(prefixes);
			rootNode.Properties.AddRange(attributes);
			ParseXamlElementFor(rootNode, reader);
		}

		static void ParseXamlElementFor(IElementNode node, XmlReader reader)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element);

			var elementName = reader.Name;
			var isEmpty = reader.IsEmptyElement;

			if (isEmpty)
				return;

			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.EndElement:
						Debug.Assert(reader.Name == elementName); //make sure we close the right element
						return;
					case XmlNodeType.Element:
						// 1. Property Element.
						if (reader.Name.Contains("."))
						{
							XmlName name;
							if (reader.Name.StartsWith(elementName + ".", StringComparison.Ordinal))
								name = new XmlName(reader.NamespaceURI, reader.Name.Substring(elementName.Length + 1));
							else //Attached DP
								name = new XmlName(reader.NamespaceURI, reader.LocalName);

							var prop = ReadNode(reader);
							if (prop != null)
								node.Properties.Add(name, prop);
						}
						// 2. Xaml2009 primitives, x:Arguments, ...
						else if (reader.NamespaceURI == X2009Uri && reader.LocalName == "Arguments")
						{
							var prop = ReadNode(reader);
							if (prop != null)
								node.Properties.Add(XmlName.xArguments, prop);
						}
						// 3. DataTemplate (should be handled by 4.)
						else if (node.XmlType.NamespaceUri == XFUri &&
						         (node.XmlType.Name == "DataTemplate" || node.XmlType.Name == "ControlTemplate"))
						{
							var prop = ReadNode(reader, true);
							if (prop != null)
								node.Properties.Add(XmlName._CreateContent, prop);
						}
						// 4. Implicit content, implicit collection, or collection syntax. Add to CollectionItems, resolve case later.
						else
						{
							var item = ReadNode(reader, true);
							if (item != null)
								node.CollectionItems.Add(item);
						}
						break;
					case XmlNodeType.Whitespace:
						break;
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
						if (node.CollectionItems.Count == 1 && node.CollectionItems[0] is ValueNode)
							((ValueNode)node.CollectionItems[0]).Value += reader.Value.Trim();
						else
							node.CollectionItems.Add(new ValueNode(reader.Value.Trim(), (IXmlNamespaceResolver)reader));
						break;
					default:
						Debug.WriteLine("Unhandled node {0} {1} {2}", reader.NodeType, reader.Name, reader.Value);
						break;
				}
			}
		}

		static INode ReadNode(XmlReader reader, bool nested = false)
		{
			var skipFirstRead = nested;
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			var name = reader.Name;
			List<INode> nodes = new List<INode>();
			INode node = null;

			while (skipFirstRead || reader.Read())
			{
				skipFirstRead = false;

				switch (reader.NodeType)
				{
					case XmlNodeType.EndElement:
						Debug.Assert(reader.Name == name);
						if (nodes.Count == 0) //Empty element
							return null;
						if (nodes.Count == 1)
							return nodes[0];
						return new ListNode(nodes, (IXmlNamespaceResolver)reader, ((IXmlLineInfo)reader).LineNumber,
							((IXmlLineInfo)reader).LinePosition);
					case XmlNodeType.Element:
						var isEmpty = reader.IsEmptyElement && reader.Name == name;
						var elementName = reader.Name;
						var elementNsUri = reader.NamespaceURI;
						var elementXmlInfo = (IXmlLineInfo)reader;
						IList<KeyValuePair<string, string>> xmlns;

						var attributes = ParseXamlAttributes(reader, out xmlns);
						var prefixes = PrefixesToIgnore(xmlns);

						IList<XmlType> typeArguments = null;
						if (attributes.Any(kvp => kvp.Key == XmlName.xTypeArguments))
						{
							typeArguments =
								((ValueNode)attributes.First(kvp => kvp.Key == XmlName.xTypeArguments).Value).Value as IList<XmlType>;
						}

						node = new ElementNode(new XmlType(elementNsUri, elementName, typeArguments), elementNsUri,
							reader as IXmlNamespaceResolver, elementXmlInfo.LineNumber, elementXmlInfo.LinePosition);
						((IElementNode)node).Properties.AddRange(attributes);
						(node.IgnorablePrefixes ?? (node.IgnorablePrefixes = new List<string>())).AddRange(prefixes);

						ParseXamlElementFor((IElementNode)node, reader);
						nodes.Add(node);
						if (isEmpty || nested)
							return node;
						break;
					case XmlNodeType.Text:
						node = new ValueNode(reader.Value.Trim(), (IXmlNamespaceResolver)reader, ((IXmlLineInfo)reader).LineNumber,
							((IXmlLineInfo)reader).LinePosition);
						nodes.Add(node);
						break;
					case XmlNodeType.Whitespace:
						break;
					default:
						Debug.WriteLine("Unhandled node {0} {1} {2}", reader.NodeType, reader.Name, reader.Value);
						break;
				}
			}
			throw new XamlParseException("Closing PropertyElement expected", (IXmlLineInfo)reader);
		}

		static IList<KeyValuePair<XmlName, INode>> ParseXamlAttributes(XmlReader reader, out IList<KeyValuePair<string,string>> xmlns)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			var attributes = new List<KeyValuePair<XmlName, INode>>();
			xmlns = new List<KeyValuePair<string, string>>();
			for (var i = 0; i < reader.AttributeCount; i++)
			{
				reader.MoveToAttribute(i);

				//skip xmlns
				if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/") {
					xmlns.Add(new KeyValuePair<string, string>(reader.LocalName, reader.Value));
					continue;
				}

				var namespaceUri = reader.NamespaceURI;
				if (reader.LocalName.Contains(".") && namespaceUri == "")
					namespaceUri = ((IXmlNamespaceResolver)reader).LookupNamespace("");
				var propertyName = new XmlName(namespaceUri, reader.LocalName);

				object value = reader.Value;

				if (reader.NamespaceURI == X2006Uri)
				{
					switch (reader.Name) {
					case "x:Key":
						propertyName = XmlName.xKey;
						break;
					case "x:Name":
						propertyName = XmlName.xName;
						break;
					case "x:Class":
					case "x:FieldModifier":
						continue;
					default:
						Debug.WriteLine("Unhandled attribute {0}", reader.Name);
						continue;
					}
                }

				if (reader.NamespaceURI == X2009Uri)
				{
					switch (reader.Name) {
					case "x:Key":
						propertyName = XmlName.xKey;
						break;
					case "x:Name":
						propertyName = XmlName.xName;
						break;
					case "x:TypeArguments":
						propertyName = XmlName.xTypeArguments;
						value = TypeArgumentsParser.ParseExpression((string)value, (IXmlNamespaceResolver)reader, (IXmlLineInfo)reader);
						break;
					case "x:DataType":
						propertyName = XmlName.xDataType;
						break;
					case "x:Class":
					case "x:FieldModifier":
						continue;
					case "x:FactoryMethod":
						propertyName = XmlName.xFactoryMethod;
						break;
					case "x:Arguments":
						propertyName = XmlName.xArguments;
						break;
					default:
						Debug.WriteLine("Unhandled attribute {0}", reader.Name);
						continue;
					}
				}

				var propertyNode = GetValueNode(value, reader);
				attributes.Add(new KeyValuePair<XmlName, INode>(propertyName, propertyNode));
			}
			reader.MoveToElement();
			return attributes;
		}

		static IList<string> PrefixesToIgnore(IList<KeyValuePair<string, string>> xmlns)
		{
			var prefixes = new List<string>();
			foreach (var kvp in xmlns) {
				var prefix = kvp.Key;

				string typeName = null, ns = null, asm = null, targetPlatform = null;
				XmlnsHelper.ParseXmlns(kvp.Value, out typeName, out ns, out asm, out targetPlatform);
				if (targetPlatform == null)
					continue;
				try {
					if (targetPlatform != Device.RuntimePlatform)
					{
						// Special case for Windows backward compatibility
						if (targetPlatform == "Windows" &&
						    (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.WinRT))
							continue;
						
						prefixes.Add(prefix);
					}
				} catch (InvalidOperationException) {
					prefixes.Add(prefix);
				}
			}
			return prefixes;
		}

		static IValueNode GetValueNode(object value, XmlReader reader)
		{
			var valueString = value as string;
			if (valueString != null && valueString.Trim().StartsWith("{}", StringComparison.Ordinal))
			{
				return new ValueNode(valueString.Substring(2), (IXmlNamespaceResolver)reader, ((IXmlLineInfo)reader).LineNumber,
					((IXmlLineInfo)reader).LinePosition);
			}
			if (valueString != null && valueString.Trim().StartsWith("{", StringComparison.Ordinal))
			{
				return new MarkupNode(valueString.Trim(), reader as IXmlNamespaceResolver, ((IXmlLineInfo)reader).LineNumber,
					((IXmlLineInfo)reader).LinePosition);
			}
			return new ValueNode(value, (IXmlNamespaceResolver)reader, ((IXmlLineInfo)reader).LineNumber,
				((IXmlLineInfo)reader).LinePosition);
		}

		static IList<XmlnsDefinitionAttribute> s_xmlnsDefinitions;

		static void GatherXmlnsDefinitionAttributes()
		{
			//this could be extended to look for [XmlnsDefinition] in all assemblies
			var assemblies = new [] {
				typeof(View).GetTypeInfo().Assembly,
				typeof(XamlLoader).GetTypeInfo().Assembly,
			};

			s_xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();

			foreach (var assembly in assemblies)
				foreach (XmlnsDefinitionAttribute attribute in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute))) {
					s_xmlnsDefinitions.Add(attribute);
					attribute.AssemblyName = attribute.AssemblyName ?? assembly.FullName;
				}
		}

		public static Type GetElementType(XmlType xmlType, IXmlLineInfo xmlInfo, Assembly currentAssembly,
			out XamlParseException exception)
		{
			if (s_xmlnsDefinitions == null)
				GatherXmlnsDefinitionAttributes();

			var namespaceURI = xmlType.NamespaceUri;
			var elementName = xmlType.Name;
			var typeArguments = xmlType.TypeArguments;
			exception = null;

			var lookupAssemblies = new List<XmlnsDefinitionAttribute>();
			var lookupNames = new List<string>();

			foreach (var xmlnsDef in s_xmlnsDefinitions) {
				if (xmlnsDef.XmlNamespace != namespaceURI)
					continue;
				lookupAssemblies.Add(xmlnsDef);
			}

			if (lookupAssemblies.Count == 0) {
				string ns, asmstring, _;
				XmlnsHelper.ParseXmlns(namespaceURI, out _, out ns, out asmstring, out _);
				lookupAssemblies.Add(new XmlnsDefinitionAttribute(namespaceURI, ns) {
					AssemblyName = asmstring ?? currentAssembly.FullName
				});
			}

			lookupNames.Add(elementName);
			lookupNames.Add(elementName + "Extension");

			for (var i = 0; i < lookupNames.Count; i++)
			{
				var name = lookupNames[i];
				if (name.Contains(":"))
					name = name.Substring(name.LastIndexOf(':') + 1);
				if (typeArguments != null)
					name += "`" + typeArguments.Count; //this will return an open generic Type
				lookupNames[i] = name;
			}

			Type type = null;
			foreach (var asm in lookupAssemblies) {
				foreach (var name in lookupNames)
					if ((type = Type.GetType($"{asm.ClrNamespace}.{name}, {asm.AssemblyName}")) != null)
						break;
				if (type != null)
					break;
			}

			if (type != null && typeArguments != null)
			{
				XamlParseException innerexception = null;
				var args = typeArguments.Select(delegate(XmlType xmltype)
				{
					XamlParseException xpe;
					var t = GetElementType(xmltype, xmlInfo, currentAssembly, out xpe);
					if (xpe != null)
					{
						innerexception = xpe;
						return null;
					}
					return t;
				}).ToArray();
				if (innerexception != null)
				{
					exception = innerexception;
					return null;
				}
				type = type.MakeGenericType(args);
			}

			if (type == null)
				exception = new XamlParseException($"Type {elementName} not found in xmlns {namespaceURI}", xmlInfo);

			return type;
		}
	}
}