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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Maui.Controls.Internals;
#if !__SOURCEGEN__
using Microsoft.Maui.Devices;
#endif

#if __SOURCEGEN__
#nullable disable
#endif

namespace Microsoft.Maui.Controls.Xaml
{
	static partial class XamlParser
	{
		public static void ParseXaml(RootNode rootNode, XmlReader reader)
		{
			var attributes = ParseXamlAttributes(reader, out IList<KeyValuePair<string, string>> xmlns);
			var prefixes = PrefixesToIgnore(xmlns);
			(rootNode.IgnorablePrefixes ?? (rootNode.IgnorablePrefixes = new List<string>())).AddRange(prefixes);
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
						if (reader.Name.IndexOf(".", StringComparison.Ordinal) != -1)
						{
							XmlName name;
							if (reader.Name.StartsWith(elementName + ".", StringComparison.Ordinal))
								name = new XmlName(reader.NamespaceURI, reader.Name.Substring(elementName.Length + 1));
							else //Attached BP
								name = new XmlName(reader.NamespaceURI, reader.LocalName);

							if (node.Properties.ContainsKey(name))
								throw new XamlParseException($"'{reader.Name}' is a duplicate property name.", (IXmlLineInfo)reader);

							INode prop = null;
							if (reader.IsEmptyElement)
								Debug.WriteLine($"Unexpected empty element '<{reader.Name} />'", (IXmlLineInfo)reader);
							else
								prop = ReadNode(reader);

							if (prop != null)
								node.Properties.Add(name, prop);
						}
						// 2. Xaml2009 primitives, x:Arguments, ...
						else if (reader.NamespaceURI == X2009Uri && reader.LocalName == "Arguments")
						{
							if (node.Properties.ContainsKey(XmlName.xArguments))
								throw new XamlParseException($"'x:Arguments' is a duplicate directive name.", (IXmlLineInfo)reader);

							var prop = ReadNode(reader);
							if (prop != null)
								node.Properties.Add(XmlName.xArguments, prop);
						}
						// 3. DataTemplate (should be handled by 4.)
						else if (node.XmlType.NamespaceUri == MauiUri &&
								 (node.XmlType.Name == "DataTemplate" || node.XmlType.Name == "ControlTemplate"))
						{
							if (node.Properties.ContainsKey(XmlName._CreateContent))
								throw new XamlParseException($"Multiple child elements in {node.XmlType.Name}", (IXmlLineInfo)reader);

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
			var nodes = new List<INode>();

			while (skipFirstRead || reader.Read())
			{
				skipFirstRead = false;

				INode node;
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
						var typeArguments = GetTypeArguments(attributes);

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
					case XmlNodeType.CDATA:
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

		internal static IList<XmlType> GetTypeArguments(XmlReader reader) => GetTypeArguments(ParseXamlAttributes(reader, out _));

		static IList<XmlType> GetTypeArguments(IList<KeyValuePair<XmlName, INode>> attributes)
		{
			return attributes.Any(kvp => kvp.Key == XmlName.xTypeArguments)
				? ((ValueNode)attributes.First(kvp => kvp.Key == XmlName.xTypeArguments).Value).Value as IList<XmlType>
				: null;
		}

		static IList<KeyValuePair<XmlName, INode>> ParseXamlAttributes(XmlReader reader, out IList<KeyValuePair<string, string>> xmlns)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			var attributes = new List<KeyValuePair<XmlName, INode>>();
			xmlns = new List<KeyValuePair<string, string>>();
			for (var i = 0; i < reader.AttributeCount; i++)
			{
				reader.MoveToAttribute(i);

				//skip xmlns
				if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
				{
					xmlns.Add(new KeyValuePair<string, string>(reader.LocalName, reader.Value));
					continue;
				}

				var namespaceUri = reader.NamespaceURI;
				if (reader.LocalName.IndexOf(".", StringComparison.Ordinal) != -1 && namespaceUri == "")
					namespaceUri = ((IXmlNamespaceResolver)reader).LookupNamespace("");
				var propertyName = ParsePropertyName(new XmlName(namespaceUri, reader.LocalName));

				if (propertyName.NamespaceURI == null && propertyName.LocalName == null)
					continue;

				object value = reader.Value;

				if (propertyName == XmlName.xTypeArguments)
					value = TypeArgumentsParser.ParseExpression((string)value, (IXmlNamespaceResolver)reader, (IXmlLineInfo)reader);

				var propertyNode = GetValueNode(value, reader);
				attributes.Add(new KeyValuePair<XmlName, INode>(propertyName, propertyNode));
			}
			reader.MoveToElement();
			return attributes;
		}

		public static XmlName ParsePropertyName(XmlName name)
		{
			if (name.NamespaceURI == X2006Uri)
			{
				switch (name.LocalName)
				{
					case "Key":
						return XmlName.xKey;
					case "Name":
						return XmlName.xName;
					case "Class":
						return XmlName.xClass;
					case "FieldModifier":
						return XmlName.xFieldModifier;
					default:
						Debug.WriteLine("Unhandled attribute {0}", name);
						return XmlName.Empty;
				}
			}

			if (name.NamespaceURI == X2009Uri)
			{
				switch (name.LocalName)
				{
					case "Key":
						return XmlName.xKey;
					case "Name":
						return XmlName.xName;
					case "TypeArguments":
						return XmlName.xTypeArguments;
					case "DataType":
						return XmlName.xDataType;
					case "Class":
						return XmlName.xClass;
					case "FieldModifier":
						return XmlName.xFieldModifier;
					case "FactoryMethod":
						return XmlName.xFactoryMethod;
					case "Arguments":
						return XmlName.xArguments;
					default:
						Debug.WriteLine("Unhandled attribute {0}", name);
						return XmlName.Empty;
				}
			}

			return name;
		}

		static IList<string> PrefixesToIgnore(IList<KeyValuePair<string, string>> xmlns)
		{
			var prefixes = new List<string>();
			foreach (var kvp in xmlns)
			{
				var prefix = kvp.Key;

				XmlnsHelper.ParseXmlns(kvp.Value, out _, out _, out _, out var targetPlatform);
				if (targetPlatform == null)
					continue;

//FIXME
#if !__SOURCEGEN__
				try
				{
					if (targetPlatform != DeviceInfo.Platform.ToString())
					{
						// Special case for Windows backward compatibility
						if (targetPlatform == "Windows" && DeviceInfo.Platform == DevicePlatform.WinUI)
							continue;

						prefixes.Add(prefix);
					}
				}
				catch (InvalidOperationException)
				{
					prefixes.Add(prefix);
				}
#endif
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
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			s_xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();

			foreach (var assembly in assemblies)
			{
				try
				{
					foreach (XmlnsDefinitionAttribute attribute in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute)))
					{
						s_xmlnsDefinitions.Add(attribute);
						attribute.AssemblyName = attribute.AssemblyName ?? assembly.FullName;
					}
				}
				catch (Exception ex)
				{
					// If we can't load the custom attribute for whatever reason from the assembly,
					// We can ignore it and keep going.
					Debug.WriteLine($"Failed to parse Assembly Attribute: {ex.ToString()}");
				}
			}
		}

#if !__SOURCEGEN__
		[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
#if !NETSTANDARD
		[RequiresDynamicCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
		public static Type GetElementType(XmlType xmlType, IXmlLineInfo xmlInfo, Assembly currentAssembly,
			out XamlParseException exception)
		{
			bool hasRetriedNsSearch = false;

		retry:
			if (s_xmlnsDefinitions == null)
				GatherXmlnsDefinitionAttributes();

			Type type = xmlType.GetTypeReference(
				s_xmlnsDefinitions,
				currentAssembly?.FullName,
				(typeInfo) =>
				{
					var t = Type.GetType($"{typeInfo.clrNamespace}.{typeInfo.typeName}, {typeInfo.assemblyName}");
					if (t is not null && t.IsPublicOrVisibleInternal(currentAssembly))
						return t;
					return null;
				});

			var typeArguments = xmlType.TypeArguments;
			exception = null;

			if (type == null)
			{
				// This covers the scenario where the AppDomain's loaded
				// assemblies might have changed since this method was first
				// called. This occurred during unit test runs and could
				// conceivably occur in the field. 
				if (!hasRetriedNsSearch)
				{
					hasRetriedNsSearch = true;
					s_xmlnsDefinitions = null;
					goto retry;
				}
			}

			if (type != null && typeArguments != null)
			{
				XamlParseException innerexception = null;
				var args = typeArguments.Select(delegate (XmlType xmltype)
				{
					var t = GetElementType(xmltype, xmlInfo, currentAssembly, out XamlParseException xpe);
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

				try
				{
					type = type.MakeGenericType(args);
				}
				catch (InvalidOperationException)
				{
					exception = new XamlParseException($"Type {type} is not a GenericTypeDefinition", xmlInfo);
				}
			}

			if (type == null)
				exception = new XamlParseException($"Type {xmlType.Name} not found in xmlns {xmlType.NamespaceUri}", xmlInfo);

			return type;
		}

		public static bool IsPublicOrVisibleInternal(this Type type, Assembly assembly)
		{
			if (type.IsPublic || type.IsNestedPublic)
				return true;
			if (type.Assembly == assembly)
				return true;
			if (type.Assembly.IsVisibleInternal(assembly))
				return true;
			return false;
		}

		public static bool IsVisibleInternal(this Assembly from, Assembly to) =>
			from.GetCustomAttributes<InternalsVisibleToAttribute>().Any(ca =>
				ca.AssemblyName.StartsWith(to.GetName().Name, StringComparison.InvariantCulture));
	}
}
