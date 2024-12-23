using System;
using System.Linq;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

using static System.String;

namespace Microsoft.Maui.Controls.SourceGen;

internal class KnownMarkups
{
	public static string ProvideValueForStaticExtension(IElementNode markupNode, Action<Diagnostic> reportDiagnostic, SourceGenContext context, out ITypeSymbol? returnType)
	{
		returnType = null;
		if (!markupNode.Properties.TryGetValue(new XmlName("", "Member"), out INode ntype))
				ntype = markupNode.CollectionItems[0];
			var member = ((ValueNode)ntype).Value as string;

			if (IsNullOrEmpty(member) || !member!.Contains("."))
			{
				//FIXME
				reportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null));
				return string.Empty;
			}

			var dotIdx = member.LastIndexOf('.');
			var typename = member.Substring(0, dotIdx);
			var membername = member.Substring(dotIdx + 1);

			var typeSymbol = typename.GetTypeSymbol(reportDiagnostic, context.Compilation, context.XmlnsCache, markupNode);
			if (typeSymbol == null)
			{
				//FIXME
				reportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, $"Type not found {typename}"));
				return string.Empty;
			}
			var field = typeSymbol.GetAllFields(membername, context).FirstOrDefault(f => f.IsStatic);
			var property = typeSymbol.GetAllProperties(membername, context).FirstOrDefault(p => p.IsStatic);

			//TODO handle enums, or contstants
			if (field == null && property == null)
			{
				//FIXME
				reportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, "Member not found"));
				return string.Empty;
			}
			if (field != null)
			{
				returnType = field.Type;
				return field.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType));
			}
			if (property != null)
			{
				returnType = property.Type;
				return property.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType));
			}

			//Should never happen
			return "null";
	}
}