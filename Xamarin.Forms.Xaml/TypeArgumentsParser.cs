using System.Collections.Generic;
using System.Xml;

namespace Xamarin.Forms.Xaml
{
	static class TypeArgumentsParser
	{
		public static IList<XmlType> ParseExpression(string expression, IXmlNamespaceResolver resolver, IXmlLineInfo lineInfo)
		{
			var typeList = new List<XmlType>();
			while (!string.IsNullOrWhiteSpace(expression))
			{
				var match = expression;
				typeList.Add(Parse(match, ref expression, resolver, lineInfo));
			}
			return typeList;
		}

		static XmlType Parse(string match, ref string remaining, IXmlNamespaceResolver resolver, IXmlLineInfo lineinfo)
		{
			remaining = null;
			int parensCount = 0;
			int pos = 0;
			bool isGeneric = false;

			for (pos = 0; pos < match.Length; pos++)
			{
				if (match[pos] == '(')
				{
					parensCount++;
					isGeneric = true;
				}
				else if (match[pos] == ')')
					parensCount--;
				else if (match[pos] == ',' && parensCount == 0)
				{
					remaining = match.Substring(pos + 1);
					break;
				}
			}
			var type = match.Substring(0, pos).Trim();

			IList<XmlType> typeArguments = null;
			if (isGeneric)
			{
				typeArguments = ParseExpression(
					type.Substring(type.IndexOf('(') + 1, type.LastIndexOf(')') - type.IndexOf('(') - 1), resolver, lineinfo);
				type = type.Substring(0, type.IndexOf('('));
			}

			var split = type.Split(':');
			if (split.Length > 2)
				return null;

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

			var namespaceuri = resolver.LookupNamespace(prefix);
			if (namespaceuri == null)
				throw new XamlParseException($"No xmlns declaration for prefix '{prefix}'.", lineinfo, null);
			return new XmlType(namespaceuri, name, typeArguments);
		}
	}
}