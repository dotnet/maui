#nullable disable
using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	static class TypeArgumentsParser
	{
		public static IList<XmlType> ParseExpression(string expression, IXmlNamespaceResolver resolver, IXmlLineInfo lineInfo)
		{
			if (string.IsNullOrWhiteSpace(expression))
				throw new XamlParseException("A type expression is required.", lineInfo);

			var typeList = new List<XmlType>();
			while (expression != null)
			{
				var match = expression;
				typeList.Add(Parse(match, ref expression, resolver, lineInfo));
				if (expression != null && string.IsNullOrWhiteSpace(expression))
					throw new XamlParseException($"Invalid type expression '{match}'.", lineInfo);
			}
			return typeList;
		}

		public static XmlType ParseSingle(string expression, IXmlNamespaceResolver resolver, IXmlLineInfo lineInfo)
		{
			if (string.IsNullOrWhiteSpace(expression))
				throw new XamlParseException("A type expression is required.", lineInfo);

			string remaining = null;
			XmlType type = Parse(expression, ref remaining, resolver, lineInfo);
			if (type is null || remaining != null)
			{
				throw new XamlParseException($"Invalid type expression or more than one type declared in '{expression}'", lineInfo, null);
			}

			return type;
		}

		public static XmlType ParseTypeExtension(ElementNode node, IXmlLineInfo lineInfo)
		{
			if ((node.XmlType.NamespaceUri != XamlParser.X2006Uri && node.XmlType.NamespaceUri != XamlParser.X2009Uri)
				|| (node.XmlType.Name != "Type" && node.XmlType.Name != "TypeExtension"))
				throw new XamlParseException("Only x:Type can be used as a type argument.", lineInfo);

			INode typeNameNode = null;
			if (node.Properties.Count == 1 && node.CollectionItems.Count == 0)
			{
				if (!node.Properties.TryGetValue(new XmlName(null, "TypeName"), out typeNameNode))
					node.Properties.TryGetValue(new XmlName("", "TypeName"), out typeNameNode);
			}
			else if (node.Properties.Count == 0 && node.CollectionItems.Count == 1)
				typeNameNode = node.CollectionItems[0];

			if (typeNameNode is not ValueNode { Value: string typeName })
				throw new XamlParseException("x:Type requires a type name.", lineInfo);

			return ParseSingle(typeName, node.NamespaceResolver, lineInfo);
		}

		static XmlType Parse(string match, ref string remaining, IXmlNamespaceResolver resolver, IXmlLineInfo lineinfo)
		{
			remaining = null;
			int parensCount = 0;
			int bracesCount = 0;
			int pos = 0;

			for (pos = 0; pos < match.Length; pos++)
			{
				if (match[pos] == '(')
					parensCount++;
				else if (match[pos] == ')')
				{
					if (--parensCount < 0)
						throw new XamlParseException($"Invalid type expression '{match}'.", lineinfo);
				}
				else if (match[pos] == '{')
					bracesCount++;
				else if (match[pos] == '}')
				{
					if (--bracesCount < 0)
						throw new XamlParseException($"Invalid type expression '{match}'.", lineinfo);
				}
				else if (match[pos] == ',' && parensCount == 0 && bracesCount == 0)
				{
					remaining = match.Substring(pos + 1);
					break;
				}
			}
			if (parensCount != 0 || bracesCount != 0)
				throw new XamlParseException($"Invalid type expression '{match}'.", lineinfo);

			var type = match.Substring(0, pos).Trim();
			if (type.Length == 0)
				throw new XamlParseException($"Invalid type expression '{match}'.", lineinfo);

			if (type[0] == '{')
				return ParseTypeExtension(type, resolver, lineinfo);

			IList<XmlType> typeArguments = null;
			var openBracket = type.IndexOf("(", StringComparison.Ordinal);
			if (openBracket >= 0)
			{
				if (type[type.Length - 1] != ')')
					throw new XamlParseException($"Invalid type expression '{type}'.", lineinfo);
				typeArguments = ParseExpression(
					type.Substring(openBracket + 1, type.Length - openBracket - 2), resolver, lineinfo);
				type = type.Substring(0, openBracket).Trim();
			}

			var split = type.Split(':');
			if (split.Length > 2 || string.IsNullOrWhiteSpace(type))
				throw new XamlParseException($"Invalid type expression '{type}'.", lineinfo);

			string prefix, name;
			if (split.Length == 2)
			{
				prefix = split[0];
				name = split[1];
			}
			else
			{
				prefix = "";
				name = split[0];
			}

			if (name.Length == 0 || (split.Length == 2 && prefix.Length == 0))
				throw new XamlParseException($"Invalid type expression '{type}'.", lineinfo);
			foreach (var character in type)
				if (char.IsWhiteSpace(character) || character == '{' || character == '}')
					throw new XamlParseException($"Invalid type expression '{type}'.", lineinfo);

			var namespaceuri = resolver.LookupNamespace(prefix);
			if (namespaceuri == null)
				throw new XamlParseException($"No xmlns declaration for prefix '{prefix}'.", lineinfo, null);
			return new XmlType(namespaceuri, name, typeArguments);
		}

		static XmlType ParseTypeExtension(string expression, IXmlNamespaceResolver resolver, IXmlLineInfo lineInfo)
		{
			if (expression[expression.Length - 1] != '}')
				throw new XamlParseException($"Invalid type expression '{expression}'.", lineInfo);

			var contents = expression.Substring(1, expression.Length - 2).Trim();
			var separator = 0;
			while (separator < contents.Length && !char.IsWhiteSpace(contents[separator]))
				separator++;
			if (separator == contents.Length)
				throw new XamlParseException("x:Type requires a type name.", lineInfo);

			var extensionName = contents.Substring(0, separator).Split(':');
			if (extensionName.Length > 2)
				throw new XamlParseException($"Invalid type expression '{expression}'.", lineInfo);
			var prefix = extensionName.Length == 2 ? extensionName[0] : "";
			var name = extensionName[extensionName.Length - 1];
			var namespaceUri = resolver.LookupNamespace(prefix);
			if (namespaceUri == null)
				throw new XamlParseException($"No xmlns declaration for prefix '{prefix}'.", lineInfo);
			if ((namespaceUri != XamlParser.X2006Uri && namespaceUri != XamlParser.X2009Uri)
				|| (name != "Type" && name != "TypeExtension"))
				throw new XamlParseException("Only x:Type can be used as a type argument.", lineInfo);

			var typeName = contents.Substring(separator).Trim();
			var equals = FindTopLevelEquals(typeName);
			if (equals >= 0)
			{
				if (typeName.Substring(0, equals).Trim() != "TypeName")
					throw new XamlParseException($"Invalid x:Type expression '{expression}'.", lineInfo);
				typeName = typeName.Substring(equals + 1).Trim();
			}

			if (typeName.Length > 0 && (typeName[0] == '\'' || typeName[0] == '"'))
			{
				if (typeName.Length < 2 || typeName[typeName.Length - 1] != typeName[0])
					throw new XamlParseException($"Invalid x:Type expression '{expression}'.", lineInfo);
				typeName = typeName.Substring(1, typeName.Length - 2);
			}
			return ParseSingle(typeName, resolver, lineInfo);
		}

		static int FindTopLevelEquals(string expression)
		{
			int parenthesesCount = 0;
			int bracesCount = 0;
			char quote = '\0';
			for (var i = 0; i < expression.Length; i++)
			{
				var current = expression[i];
				if (quote != '\0')
				{
					if (current == quote)
						quote = '\0';
					continue;
				}

				if (current == '\'' || current == '"')
					quote = current;
				else if (current == '(')
					parenthesesCount++;
				else if (current == ')')
					parenthesesCount--;
				else if (current == '{')
					bracesCount++;
				else if (current == '}')
					bracesCount--;
				else if (current == '=' && parenthesesCount == 0 && bracesCount == 0)
					return i;
			}

			return -1;
		}
	}
}