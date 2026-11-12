using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

sealed class XamlTypeConflictAnalyzer
{
	readonly ProjectItem _projectItem;
	readonly SourceText _sourceText;
	readonly Compilation _compilation;
	readonly AssemblyAttributes _xmlns;
	readonly CancellationToken _cancellationToken;
	readonly INamedTypeSymbol? _systemType;
	readonly INamedTypeSymbol? _bindablePropertyType;
	readonly string? _targetPlatform;
	readonly bool _prunePlatform;
	readonly Dictionary<(XmlType type, bool expandToExtension), INamedTypeSymbol[]> _typeCache = new();
	readonly Dictionary<(INamedTypeSymbol owner, string propertyName, bool attached), TypeValueKind> _valueKindCache = new();
	readonly List<IAssemblySymbol> _assemblies = new();
	readonly Dictionary<string, IAssemblySymbol?> _assemblyCache = new(StringComparer.OrdinalIgnoreCase);
	readonly ImmutableArray<Diagnostic>.Builder _diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
	readonly HashSet<(TextSpan span, string namespaceUri, string name)> _reportedReferences = new();

	XamlTypeConflictAnalyzer(ProjectItem projectItem, SourceText sourceText, Compilation compilation, AssemblyAttributes xmlns, CancellationToken cancellationToken)
	{
		_projectItem = projectItem;
		_sourceText = sourceText;
		_compilation = compilation;
		_xmlns = xmlns;
		_cancellationToken = cancellationToken;
		_systemType = compilation.GetTypeByMetadataName("System.Type");
		_bindablePropertyType = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableProperty");
		var inflator = projectItem.Inflator;
		_targetPlatform = GetTargetPlatform(projectItem.TargetFramework);
		_prunePlatform = inflator != 0 && (inflator & XamlInflator.Runtime) == 0;
		_assemblies.Add(compilation.Assembly);
		foreach (var reference in compilation.References)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
				_assemblies.Add(assembly);
		}
	}

	internal sealed record Document(ProjectItem ProjectItem, SourceText SourceText, XDocument Xml);

	internal static Document? ParseDocument((XamlProjectItemForCB? item, AssemblyAttributes xmlns) input, CancellationToken cancellationToken)
	{
		var (item, xmlns) = input;
		if (item?.Root is null || item.ProjectItem is null || item.SourceText is null)
			return null;

		var resolver = CreateNamespaceManager(xmlns);
		using var reader = XmlReader.Create(new StringReader(item.SourceText.ToString()),
			new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment },
			new XmlParserContext(resolver.NameTable, resolver, null, XmlSpace.None));
		var document = XDocument.Load(reader, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
		cancellationToken.ThrowIfCancellationRequested();
		return new Document(item.ProjectItem, item.SourceText, document);
	}

	internal static ImmutableArray<Diagnostic> Analyze((Document? document, AssemblyAttributes xmlns, Compilation compilation) input, CancellationToken cancellationToken)
	{
		var (document, xmlns, compilation) = input;
		if (document?.Xml.Root is null || XamlDiagnosticHelpers.IsSuppressed(document.ProjectItem.NoWarn, Descriptors.ConflictingXmlnsType.Id))
			return ImmutableArray<Diagnostic>.Empty;

		var analyzer = new XamlTypeConflictAnalyzer(document.ProjectItem, document.SourceText, compilation, xmlns, cancellationToken);
		analyzer.VisitElement(document.Xml.Root, CreateNamespaceManager(xmlns), new HashSet<string>(StringComparer.Ordinal));
		return analyzer._diagnostics.ToImmutable();
	}

	static XmlNamespaceManager CreateNamespaceManager(AssemblyAttributes xmlns)
	{
		var manager = new XmlNamespaceManager(new NameTable());
		manager.AddNamespace("__f__", XamlParser.MauiUri);
		manager.AddNamespace("__g__", XamlParser.MauiGlobalUri);
		manager.AddNamespace("", XamlParser.DefaultImplicitUri);
		foreach (var prefix in xmlns.XmlnsPrefixes)
			manager.AddNamespace(prefix.Prefix, prefix.XmlNamespace);
		return manager;
	}

	void VisitElement(XElement element, XmlNamespaceManager resolver, HashSet<string> ignoredNamespaces)
	{
		_cancellationToken.ThrowIfCancellationRequested();
		resolver.PushScope();
		foreach (var attribute in element.Attributes().Where(a => a.IsNamespaceDeclaration))
			resolver.AddNamespace(attribute.Name.LocalName == "xmlns" ? "" : attribute.Name.LocalName, attribute.Value);

		var ignored = element.Attribute(XName.Get("Ignorable", XamlParser.McUri));
		if (ignored is not null)
		{
			ignoredNamespaces = new HashSet<string>(ignoredNamespaces, StringComparer.Ordinal);
			foreach (var prefix in ignored.Value.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
				if (resolver.LookupNamespace(prefix) is string uri)
					ignoredNamespaces.Add(uri);
		}

		if (!ignoredNamespaces.Contains(element.Name.NamespaceName) && !IsExcludedPlatform(element))
		{
			var typeArgumentsAttribute = element.Attributes().FirstOrDefault(a =>
				(a.Name.NamespaceName == XamlParser.X2009Uri || a.Name.NamespaceName == XamlParser.X2006Uri) &&
				a.Name.LocalName == "TypeArguments");
			IList<XmlType>? arguments = null;
			if (typeArgumentsAttribute is not null)
			{
				try
				{
					arguments = TypeArgumentsParser.ParseExpression(typeArgumentsAttribute.Value, resolver, (IXmlLineInfo)typeArgumentsAttribute);
				}
				catch (XamlParseException)
				{
					// The existing XAML parser reports malformed type expressions.
				}
			}

			var name = element.Name.LocalName;
			var dot = name.IndexOf('.');
			if (dot >= 0)
				name = name.Substring(0, dot);

			var xmlType = new XmlType(element.Name.NamespaceName, name, arguments);
			INamedTypeSymbol? elementType = null;
			if (typeArgumentsAttribute is null || arguments is not null)
			{
				ReportIfAmbiguous(xmlType, GetNameSpan(element, dot >= 0));
				elementType = ResolveUniqueType(xmlType);
			}

			if (arguments is not null)
			{
				var value = GetAttributeValue(typeArgumentsAttribute!);
				ReportTypeArguments(arguments, value, 0, value.Value.Length, resolver);
			}

			foreach (var attribute in element.Attributes())
			{
				if (attribute.IsNamespaceDeclaration || attribute == typeArgumentsAttribute ||
					ignoredNamespaces.Contains(attribute.Name.NamespaceName))
					continue;
				VisitAttribute(element, attribute, elementType, xmlType, resolver);
			}

			if (dot >= 0)
			{
				var propertyName = element.Name.LocalName.Substring(dot + 1);
				var parent = element.Parent;
				var constructorElement = parent?.Name.NamespaceName == element.Name.NamespaceName &&
					parent.Name.LocalName == name ? parent : null;
				var kind = GetValueKind(elementType, xmlType, propertyName, attached: true, constructorElement);
				VisitTextValues(element, kind, resolver, isPropertyElement: true);
			}
			else
			{
				var contentProperty = elementType?.GetContentPropertyName(context: null);
				var kind = GetValueKind(elementType, xmlType, contentProperty, attached: false);
				VisitTextValues(element, kind, resolver);
			}

			foreach (var child in element.Elements())
				VisitElement(child, resolver, ignoredNamespaces);
		}
		resolver.PopScope();
	}

	void VisitAttribute(XElement element, XAttribute attribute, INamedTypeSymbol? elementType, XmlType elementXmlType, XmlNamespaceManager resolver)
	{
		var name = attribute.Name.LocalName;
		var namespaceUri = attribute.Name.NamespaceName;
		if (namespaceUri.Length == 0 && name == "Default" && IsOnPlatform(element) && GetMatchingPlatformBranch(element) is not null)
			return;
		var isXamlDirective = namespaceUri == XamlParser.X2009Uri || namespaceUri == XamlParser.X2006Uri;
		var kind = isXamlDirective && name == "DataType" ? TypeValueKind.Type : TypeValueKind.None;

		var dot = name.IndexOf('.');
		if (!isXamlDirective && dot >= 0)
		{
			var ownerName = name.Substring(0, dot);
			var ownerNamespace = string.IsNullOrEmpty(namespaceUri) ? resolver.LookupNamespace("") ?? "" : namespaceUri;
			var owner = new XmlType(ownerNamespace, ownerName, null);
			ReportIfAmbiguous(owner, GetNameSpan(attribute, ownerOnly: true));
			kind = GetValueKind(ResolveUniqueType(owner), owner, name.Substring(dot + 1), attached: true);
		}
		else if (!isXamlDirective)
			kind = GetValueKind(elementType, elementXmlType, name, attached: false, element);

		var value = GetAttributeValue(attribute);
		var trimmed = value.Value.Trim();
		if (trimmed.StartsWith("{}", StringComparison.Ordinal))
			return;
		if (trimmed.StartsWith("{", StringComparison.Ordinal))
		{
			AnalyzeMarkup(value, resolver, element, kind);
			return;
		}
		if (kind != TypeValueKind.None)
			ReportTypeValue(trimmed, value, 0, value.Value.Length, resolver, kind);
	}

	void VisitTextValues(XElement element, TypeValueKind kind, XmlNamespaceManager resolver, bool isPropertyElement = false)
	{
		var analyzeExpressions = isPropertyElement && (_projectItem.Inflator & XamlInflator.SourceGen) != 0;
		if (kind == TypeValueKind.None && !analyzeExpressions)
			return;
		foreach (var text in element.Nodes().OfType<XText>())
		{
			var value = GetTextValue(text);
			var trimmed = value.Value.Trim();
			if (trimmed.Length == 0 || trimmed.StartsWith("{}", StringComparison.Ordinal))
				continue;
			if (analyzeExpressions && trimmed.StartsWith("{", StringComparison.Ordinal))
				AnalyzeMarkup(value, resolver, element, kind, expressionsOnly: true);
			else if (kind != TypeValueKind.None)
				ReportTypeValue(trimmed, value, 0, value.Value.Length, resolver, kind);
		}
	}

	TypeValueKind GetValueKind(INamedTypeSymbol? owner, XmlType ownerXmlType, string? propertyName, bool attached,
		XElement? constructorElement = null, ParsedMarkup? constructorMarkup = null)
	{
		if (propertyName is null)
			return TypeValueKind.None;
		if (ownerXmlType.NamespaceUri == XamlParser.X2009Uri || ownerXmlType.NamespaceUri == XamlParser.X2006Uri)
		{
			if ((ownerXmlType.Name == "Type" || ownerXmlType.Name == "TypeExtension") && propertyName == "TypeName")
				return TypeValueKind.Type;
			if ((ownerXmlType.Name == "Static" || ownerXmlType.Name == "StaticExtension") && propertyName == "Member")
				return TypeValueKind.StaticMember;
		}

		if (owner is null)
			return TypeValueKind.None;
		if (owner.ToFQDisplayString() == "global::Microsoft.Maui.Controls.Xaml.DataTemplateExtension" && propertyName == "TypeName")
			return TypeValueKind.Type;

		var parameterKind = GetConstructorValueKind(owner, propertyName, constructorElement, constructorMarkup);
		if (parameterKind is not null)
			return parameterKind.Value;

		var key = (owner, propertyName, attached);
		if (_valueKindCache.TryGetValue(key, out var cached))
			return cached;
		var result = GetValueKindCore(owner, propertyName, attached);
		_valueKindCache[key] = result;
		return result;
	}

	TypeValueKind GetValueKindCore(INamedTypeSymbol owner, string propertyName, bool attached)
	{
		foreach (var property in owner.GetAllProperties(propertyName, context: null))
		{
			var kind = KindFor(property.Type);
			if (kind != TypeValueKind.None)
				return kind;
			if (UsesTypeConverter(property.GetAttributes()))
				return TypeValueKind.Type;
		}

		if (attached)
		{
			foreach (var method in owner.GetAllMethods("Get" + propertyName, context: null))
				if (method.IsStatic)
				{
					var kind = KindFor(method.ReturnType);
					if (kind != TypeValueKind.None)
						return kind;
					if (UsesTypeConverter(method.GetAttributes()))
						return TypeValueKind.Type;
				}
			foreach (var method in owner.GetAllMethods("Set" + propertyName, context: null))
				if (method.IsStatic && method.Parameters.Length == 2)
				{
					var kind = KindFor(method.Parameters[1].Type);
					if (kind != TypeValueKind.None)
						return kind;
				}
		}

		return TypeValueKind.None;
	}

	TypeValueKind? GetConstructorValueKind(INamedTypeSymbol owner, string propertyName, XElement? element, ParsedMarkup? markup)
	{
		if (element is null && markup is null)
			return null;
		if (element is not null && (element.Attributes().Any(a =>
				(a.Name.NamespaceName == XamlParser.X2009Uri || a.Name.NamespaceName == XamlParser.X2006Uri) &&
				a.Name.LocalName == "FactoryMethod") ||
			element.Elements().Any(e =>
				(e.Name.NamespaceName == XamlParser.X2009Uri || e.Name.NamespaceName == XamlParser.X2006Uri) &&
				e.Name.LocalName == "Arguments")))
			return null;

		bool HasProperty(string name) =>
			markup?.Properties.Any(p => p.Name == name) == true ||
			element?.Attribute(name) is not null ||
			element?.Elements().Any(e => e.Name.NamespaceName == element.Name.NamespaceName &&
				e.Name.LocalName == element.Name.LocalName + "." + name) == true;

		static string? ParameterName(IParameterSymbol parameter) =>
			parameter.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass?.ToFQDisplayString() == "global::Microsoft.Maui.Controls.ParameterAttribute")
				?.ConstructorArguments.FirstOrDefault().Value as string;

		foreach (var constructor in owner.GetConstructors(context: null))
		{
			if (constructor.Parameters.Length == 0 ||
				!constructor.Parameters.All(p => ParameterName(p) is string name && HasProperty(name)))
				continue;
			var parameter = constructor.Parameters.FirstOrDefault(p => ParameterName(p) == propertyName);
			if (parameter is not null)
			{
				var kind = KindFor(parameter.Type);
				if (kind != TypeValueKind.None)
					return kind;
				if (UsesTypeConverter(parameter.GetAttributes()))
					return TypeValueKind.Type;
				return TypeValueKind.None;
			}
			break;
		}
		return null;
	}

	TypeValueKind KindFor(ITypeSymbol? type)
	{
		if (type is not null && SymbolEqualityComparer.Default.Equals(type, _systemType))
			return TypeValueKind.Type;
		if (type is not null && SymbolEqualityComparer.Default.Equals(type, _bindablePropertyType))
			return TypeValueKind.BindableMember;
		return TypeValueKind.None;
	}

	static bool UsesTypeConverter(IEnumerable<AttributeData> attributes) =>
		attributes.Any(a => a.AttributeClass?.ToFQDisplayString() == "global::System.ComponentModel.TypeConverterAttribute" &&
			a.ConstructorArguments.Length > 0 &&
			a.ConstructorArguments[0].Value is INamedTypeSymbol converter &&
			converter.ToFQDisplayString() == "global::Microsoft.Maui.Controls.TypeTypeConverter");

	void ReportTypeValue(string value, ValueSource source, int start, int end, XmlNamespaceManager resolver, TypeValueKind kind)
	{
		if (string.IsNullOrWhiteSpace(value))
			return;
		var typeName = value.Trim();
		if (kind == TypeValueKind.StaticMember || kind == TypeValueKind.BindableMember)
		{
			var dot = typeName.LastIndexOf('.');
			if (dot <= 0 || (kind == TypeValueKind.BindableMember && typeName.IndexOf('.') != dot))
				return;
			typeName = typeName.Substring(0, dot);
		}

		try
		{
			var type = TypeArgumentsParser.ParseSingle(typeName, resolver, new XmlLineInfo());
			var literalStart = source.Value.IndexOf(typeName, start, end - start, StringComparison.Ordinal);
			if (literalStart >= 0)
			{
				start = literalStart;
				end = literalStart + typeName.Length;
			}
			ReportTypeArguments(new[] { type }, source, start, end, resolver, expandToExtension: kind != TypeValueKind.StaticMember);
		}
		catch (XamlParseException)
		{
			// Invalid type values are diagnosed by the existing XAML parser or inflator.
		}
	}

	void AnalyzeMarkup(ValueSource source, XmlNamespaceManager resolver, XElement element, TypeValueKind expectedKind, bool expressionsOnly = false)
	{
		var expression = source.Value.Trim();
		if (expression.Length < 3 || expression[expression.Length - 1] != '}')
			return;
		var expressionStart = source.Value.IndexOf(expression, StringComparison.Ordinal);
		var classification = CSharpExpressionHelpers.ClassifyExpression(expression,
			name => GetCandidates(new XmlType(resolver.LookupNamespace("") ?? "", name, null)).Length > 0,
			name => ResolveUniqueType(new XmlType(element.Name.NamespaceName, element.Name.LocalName, null)) is INamedTypeSymbol type &&
				MemberResolver.HasMember(type, name));
		if (classification.IsExpression)
		{
			var innerStart = expression.StartsWith("{=", StringComparison.Ordinal) ? 2 : 1;
			var code = expression.Substring(innerStart, expression.Length - innerStart - 1);
			foreach (var match in CSharpExpressionHelpers.GetXmlnsPrefixMatches(code))
			{
				var prefix = match.Groups[1].Value;
				if (prefix == "x" || prefix == "global" || resolver.LookupNamespace(prefix) is not string uri)
					continue;
				var start = expressionStart + innerStart + match.Index;
				ReportIfAmbiguous(new XmlType(uri, match.Groups[2].Value, null), source.GetSpan(start, start + match.Length));
			}
			return;
		}
		if (expressionsOnly)
			return;

		ParsedMarkup? markup = null;
		try
		{
			var context = new MarkupParseContext(expression);
			var remaining = expression;
			if (new ReferenceMarkupParser().ParseExpression(ref remaining, context) is ParsedMarkup parsed &&
				string.IsNullOrWhiteSpace(remaining))
				markup = parsed;
		}
		catch (Exception exception) when (exception is XamlParseException or ArgumentException or IndexOutOfRangeException)
		{
			// Malformed markup remains the responsibility of the existing XAML parser.
		}
		if (markup is not null)
			VisitMarkup(markup, expressionStart, source, resolver, expectedKind);
	}

	void VisitMarkup(ParsedMarkup markup, int expressionStart, ValueSource source, XmlNamespaceManager resolver, TypeValueKind expectedKind = TypeValueKind.None)
	{
		var parts = markup.Name.Split(':');
		if (parts.Length > 2)
			return;
		var prefix = parts.Length == 2 ? parts[0] : "";
		var name = parts[parts.Length - 1];
		var namespaceUri = resolver.LookupNamespace(prefix);
		if (namespaceUri is null)
			return;

		IList<XmlType>? arguments = null;
		foreach (var property in markup.Properties)
		{
			if (!IsTypeArguments(property.Name, resolver))
				continue;
			try
			{
				if (property.Literal is not null)
					arguments = TypeArgumentsParser.ParseExpression(property.Literal, resolver, new XmlLineInfo());
				else if (property.Nested is not null && GetXTypeName(property.Nested, resolver) is string typeName)
					arguments = new[] { TypeArgumentsParser.ParseSingle(typeName, resolver, new XmlLineInfo()) };
			}
			catch (XamlParseException)
			{
				// The existing XAML parser reports malformed type arguments.
			}
		}

		var extension = new XmlType(namespaceUri, name + "Extension", arguments);
		var xmlType = GetCandidates(extension).Length == 1
			? extension
			: new XmlType(namespaceUri, name, arguments);
		ReportIfAmbiguous(xmlType, source.GetSpan(expressionStart + markup.NameStart, expressionStart + markup.NameStart + markup.Name.Length));
		var extensionType = ResolveUniqueType(xmlType);
		var contentProperty = extensionType?.GetContentPropertyName(context: null);
		var extensionName = extensionType?.ToFQDisplayString();
		var isMauiMarkup = namespaceUri == XamlParser.MauiUri || namespaceUri == XamlParser.MauiGlobalUri;
		var isOnPlatform = isMauiMarkup && extensionName == "global::Microsoft.Maui.Controls.Xaml.OnPlatformExtension";
		var isOnIdiom = isMauiMarkup && extensionName == "global::Microsoft.Maui.Controls.Xaml.OnIdiomExtension";
		var hasTargetPlatform = isOnPlatform && _targetPlatform is not null;
		var platformValue = hasTargetPlatform
			? markup.Properties.FirstOrDefault(p => p.Name == _targetPlatform) ??
			  markup.Properties.FirstOrDefault(p => p.Name == "Default") ??
			  markup.Properties.FirstOrDefault(p => p.Name is null)
			: null;
		foreach (var property in markup.Properties)
		{
			if (_prunePlatform && hasTargetPlatform && !ReferenceEquals(property, platformValue) && !IsTypeArguments(property.Name, resolver))
				continue;
			var start = expressionStart + property.Start;
			var end = expressionStart + property.End;
			var propertyName = property.Name ?? contentProperty;
			var kind = GetValueKind(extensionType, xmlType, propertyName, attached: false, constructorMarkup: markup);
			if (kind == TypeValueKind.None && (isOnIdiom || (isOnPlatform && (!hasTargetPlatform || ReferenceEquals(property, platformValue)))))
				kind = expectedKind;
			if (property.Name is string qualifiedProperty && qualifiedProperty.LastIndexOf('.') is int dot && dot > 0)
			{
				var ownerName = qualifiedProperty.Substring(0, dot);
				var ownerParts = ownerName.Split(':');
				if (ownerParts.Length <= 2 && resolver.LookupNamespace(ownerParts.Length == 2 ? ownerParts[0] : "") is string ownerNamespace)
				{
					var owner = new XmlType(ownerNamespace, ownerParts[ownerParts.Length - 1], null);
					var ownerStart = source.Value.IndexOf(ownerName, start, end - start, StringComparison.Ordinal);
					if (ownerStart >= 0)
						ReportIfAmbiguous(owner, source.GetSpan(ownerStart, ownerStart + ownerName.Length));
					kind = GetValueKind(ResolveUniqueType(owner), owner, qualifiedProperty.Substring(dot + 1), attached: true);
				}
			}
			if (property.Nested is not null)
			{
				VisitMarkup(property.Nested, expressionStart, source, resolver, kind);
				continue;
			}
			if (property.Literal is null)
				continue;
			if (property.Name is not null)
			{
				var equals = source.Value.IndexOf('=', start, end - start);
				if (equals >= 0)
					start = equals + 1;
			}
			if (IsTypeArguments(property.Name, resolver))
			{
				if (arguments is not null)
					ReportTypeArguments(arguments, source, start, end, resolver);
				continue;
			}
			if (kind != TypeValueKind.None)
				ReportTypeValue(property.Literal, source, start, end, resolver, kind);
		}
	}

	static bool IsTypeArguments(string? name, XmlNamespaceManager resolver)
	{
		if (name is null)
			return false;
		var parts = name.Split(':');
		return parts.Length == 2 && parts[1] == "TypeArguments" &&
			(resolver.LookupNamespace(parts[0]) == XamlParser.X2009Uri || resolver.LookupNamespace(parts[0]) == XamlParser.X2006Uri);
	}

	static string? GetXTypeName(ParsedMarkup markup, XmlNamespaceManager resolver)
	{
		var parts = markup.Name.Split(':');
		var prefix = parts.Length == 2 ? parts[0] : "";
		var name = parts[parts.Length - 1];
		var uri = resolver.LookupNamespace(prefix);
		if ((uri != XamlParser.X2009Uri && uri != XamlParser.X2006Uri) || (name != "Type" && name != "TypeExtension"))
			return null;
		return markup.Properties.FirstOrDefault(p => p.Name is null || p.Name == "TypeName")?.Literal;
	}

	INamedTypeSymbol? ResolveUniqueType(XmlType type)
	{
		var matches = GetCandidates(type);
		if (matches.Length != 1)
			return null;
		var symbol = matches[0];
		if (type.TypeArguments is null || !symbol.IsGenericType)
			return symbol;
		var arguments = type.TypeArguments.Select(ResolveUniqueType).ToArray();
		if (arguments.Any(a => a is null) || arguments.Length != symbol.Arity)
			return symbol;
		return symbol.Construct(arguments!);
	}

	enum TypeValueKind
	{
		None,
		Type,
		StaticMember,
		BindableMember
	}

	bool IsExcludedPlatform(XElement element)
	{
		if (!_prunePlatform || _targetPlatform is null || element.Parent is not XElement parent || !IsOnPlatform(parent) ||
			element.Name.NamespaceName != XamlParser.MauiUri)
			return false;
		var matchingBranch = GetMatchingPlatformBranch(parent);
		return element.Name.LocalName switch
		{
			"On" => !ReferenceEquals(element, matchingBranch),
			"OnPlatform.Default" => matchingBranch is not null,
			_ => false
		};
	}

	static bool IsOnPlatform(XElement element) =>
		element.Name.NamespaceName == XamlParser.MauiUri && element.Name.LocalName == "OnPlatform";

	XElement? GetMatchingPlatformBranch(XElement element)
	{
		if (!_prunePlatform || _targetPlatform is null)
			return null;
		var branch = element.Elements(XName.Get("On", XamlParser.MauiUri)).FirstOrDefault(branch =>
			((string?)branch.Attribute("Platform"))?.Split(',').Any(platform => platform.Trim() == _targetPlatform) == true);
		if (branch?.Attribute("Value") is not null || branch?.Nodes().Any(node =>
			node is XText text ? !string.IsNullOrWhiteSpace(text.Value) :
			node is XElement child && !child.Name.LocalName.Contains('.') &&
			!(child.Name.NamespaceName == XamlParser.X2009Uri && child.Name.LocalName == "Arguments")) == true)
			return branch;
		return null;
	}

	static string? GetTargetPlatform(string? targetFramework) =>
		targetFramework is null ? null :
			targetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) >= 0 ? "Android" :
			targetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) >= 0 ? "iOS" :
			targetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) >= 0 ? "MacCatalyst" :
			targetFramework.IndexOf("-macos", StringComparison.OrdinalIgnoreCase) >= 0 ? "macOS" : null;

	void ReportTypeArguments(IEnumerable<XmlType> arguments, ValueSource source, int start, int end, XmlNamespaceManager resolver, bool expandToExtension = true)
	{
		var cursor = start;
		foreach (var argument in arguments)
			ReportTypeArgument(argument, source, end, resolver, ref cursor, expandToExtension);
	}

	void ReportTypeArgument(XmlType type, ValueSource source, int end, XmlNamespaceManager resolver, ref int cursor, bool expandToExtension)
	{
		if (FindTypeName(type, source, end, resolver, ref cursor) is TextSpan span)
			ReportIfAmbiguous(type, span, expandToExtension);
		if (type.TypeArguments is not null)
			foreach (var argument in type.TypeArguments)
				ReportTypeArgument(argument, source, end, resolver, ref cursor, expandToExtension: true);
	}

	TextSpan? FindTypeName(XmlType type, ValueSource source, int end, XmlNamespaceManager resolver, ref int cursor)
	{
		var text = source.Value;
		while (cursor < end)
		{
			var match = text.IndexOf(type.Name, cursor, end - cursor, StringComparison.Ordinal);
			if (match < 0)
				return null;
			var start = match;
			while (start > 0 && IsNameCharacter(text[start - 1]))
				start--;
			var finish = match + type.Name.Length;
			if (finish < end && IsNameCharacter(text[finish]))
			{
				cursor = finish;
				continue;
			}
			var token = text.Substring(start, finish - start);
			var colon = token.IndexOf(':');
			var prefix = colon >= 0 ? token.Substring(0, colon) : "";
			var localName = colon >= 0 ? token.Substring(colon + 1) : token;
			cursor = finish;
			if (localName == type.Name && resolver.LookupNamespace(prefix) == type.NamespaceUri)
				return source.GetSpan(start, finish);
		}
		return null;
	}

	static bool IsNameCharacter(char c) =>
		char.IsLetterOrDigit(c) || char.IsSurrogate(c) || c is '_' or '-' or '.' or ':' or '\u00b7' ||
		char.GetUnicodeCategory(c) is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.ConnectorPunctuation;

	TextSpan GetNameSpan(XObject node, bool ownerOnly = false)
	{
		var info = (IXmlLineInfo)node;
		var start = _sourceText.Lines[info.LineNumber - 1].Start + info.LinePosition - 1;
		if (_sourceText[start] == '<')
			start++;
		var end = start;
		while (end < _sourceText.Length && IsNameCharacter(_sourceText[end]))
			end++;
		if (ownerOnly)
		{
			var qualifiedName = _sourceText.ToString(TextSpan.FromBounds(start, end));
			var dot = qualifiedName.LastIndexOf('.');
			if (dot >= 0)
				end = start + dot;
		}
		return TextSpan.FromBounds(start, end);
	}

	ValueSource GetAttributeValue(XAttribute attribute)
	{
		var name = GetNameSpan(attribute);
		var start = name.End;
		while (start < _sourceText.Length && _sourceText[start] != '"' && _sourceText[start] != '\'')
			start++;
		var quote = _sourceText[start++];
		var end = start;
		while (end < _sourceText.Length && _sourceText[end] != quote)
			end++;
		return ValueSource.Create(attribute.Value, _sourceText.ToString(TextSpan.FromBounds(start, end)), start, isAttribute: true);
	}

	ValueSource GetTextValue(XText text)
	{
		var info = (IXmlLineInfo)text;
		var start = _sourceText.Lines[info.LineNumber - 1].Start + info.LinePosition - 1;
		var contents = _sourceText.ToString();
		if (text is XCData && contents.IndexOf("<![CDATA[", start, StringComparison.Ordinal) == start)
			start += "<![CDATA[".Length;
		var end = text is XCData
			? contents.IndexOf("]]>", start, StringComparison.Ordinal)
			: contents.IndexOf('<', start);
		if (end < 0)
			end = contents.Length;
		return ValueSource.Create(text.Value, contents.Substring(start, end - start), start, isAttribute: false, isCData: text is XCData);
	}

	void ReportIfAmbiguous(XmlType type, TextSpan span, bool expandToExtension = true)
	{
		var matches = GetCandidates(type, expandToExtension);
		if (matches.Length < 2 || !_reportedReferences.Add((span, type.NamespaceUri, type.Name)))
			return;

		var names = matches.Select(t => $"{t.ToFQDisplayString()} ({t.ContainingAssembly.Identity.Name})")
			.OrderBy(n => n, StringComparer.Ordinal);
		var location = Location.Create(_projectItem.AdditionalText.Path, span, _sourceText.Lines.GetLinePositionSpan(span));
		_diagnostics.Add(Diagnostic.Create(Descriptors.ConflictingXmlnsType, location, type.NamespaceUri, type.Name, string.Join(", ", names)));
	}

	INamedTypeSymbol[] GetCandidates(XmlType type, bool expandToExtension = true)
	{
		var key = (type, expandToExtension);
		if (_typeCache.TryGetValue(key, out var cached))
			return cached;

		var name = type.Name.Split(':').Last();
		var candidates = type.GetTypeReferences(_xmlns.XmlnsDefinitions, _compilation.AssemblyName ?? "",
			info =>
			{
				if (string.IsNullOrEmpty(info.clrNamespace) || info.clrNamespace.StartsWith("http", StringComparison.Ordinal))
					return null;
				if (string.IsNullOrEmpty(info.assemblyName))
					return null;
				if (!_assemblyCache.TryGetValue(info.assemblyName, out var assembly))
				{
					assembly = _assemblies.FirstOrDefault(a =>
						AssemblyIdentityComparer.Default.ReferenceMatchesDefinition(info.assemblyName, a.Identity));
					_assemblyCache.Add(info.assemblyName, assembly);
				}
				var symbol = assembly?.GetTypeByMetadataName($"{info.clrNamespace}.{info.typeName}");
				if (symbol is null || (symbol.DeclaredAccessibility != Accessibility.Public &&
					!_xmlns.InternalsVisible.Contains(symbol.ContainingAssembly, SymbolEqualityComparer.Default)))
					return null;
				var resolvedName = info.typeName.Split('`')[0];
				if (resolvedName == name + "Extension" && symbol.IsStatic)
					return null;
				return symbol;
			}, expandToExtension)
			.Where(t => t is not null)
			.Distinct(SymbolEqualityComparer.Default)
			.Cast<INamedTypeSymbol>()
			.ToArray();
		_typeCache[key] = candidates;
		return candidates;
	}

	sealed class ValueSource(string value, int[] offsets, TextSpan rawSpan, bool accurate)
	{
		public string Value { get; } = value;

		public TextSpan GetSpan(int start, int end) =>
			accurate ? TextSpan.FromBounds(offsets[start], offsets[end]) : rawSpan;

		public static ValueSource Create(string value, string raw, int rawStart, bool isAttribute, bool isCData = false)
		{
			var offsets = new int[value.Length + 1];
			var position = 0;
			var valuePosition = 0;
			while (position < raw.Length && valuePosition < value.Length)
			{
				var start = position;
				string decoded;
				if (raw[position] == '&' && !isCData)
				{
					var semicolon = raw.IndexOf(';', position + 1);
					if (semicolon < 0 || !TryDecodeEntity(raw.Substring(position + 1, semicolon - position - 1), out decoded))
						return new ValueSource(value, offsets, new TextSpan(rawStart, raw.Length), accurate: false);
					position = semicolon + 1;
				}
				else if (raw[position] == '\r')
				{
					position++;
					if (position < raw.Length && raw[position] == '\n')
						position++;
					decoded = isAttribute ? " " : "\n";
				}
				else
				{
					var character = raw[position++];
					decoded = isAttribute && (character == '\n' || character == '\t') ? " " : character.ToString();
				}

				if (valuePosition + decoded.Length > value.Length ||
					!string.Equals(value.Substring(valuePosition, decoded.Length), decoded, StringComparison.Ordinal))
					return new ValueSource(value, offsets, new TextSpan(rawStart, raw.Length), accurate: false);
				for (var i = 0; i < decoded.Length; i++)
					offsets[valuePosition + i] = rawStart + start;
				valuePosition += decoded.Length;
				offsets[valuePosition] = rawStart + position;
			}
			return new ValueSource(value, offsets, new TextSpan(rawStart, raw.Length), accurate: position == raw.Length && valuePosition == value.Length);
		}

		static bool TryDecodeEntity(string entity, out string decoded)
		{
			decoded = entity switch
			{
				"amp" => "&",
				"lt" => "<",
				"gt" => ">",
				"quot" => "\"",
				"apos" => "'",
				_ => ""
			};
			if (decoded.Length != 0)
				return true;
			if (!entity.StartsWith("#", StringComparison.Ordinal))
				return false;
			var hex = entity.StartsWith("#x", StringComparison.Ordinal);
			var number = entity.Substring(hex ? 2 : 1);
			if (!int.TryParse(number, hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out var codePoint) ||
				codePoint < 0 || codePoint > 0x10ffff || (codePoint >= 0xd800 && codePoint <= 0xdfff))
				return false;
			decoded = char.ConvertFromUtf32(codePoint);
			return true;
		}
	}

	sealed record ParsedMarkup(string Name, int NameStart, IReadOnlyList<ParsedProperty> Properties);
	sealed record ParsedProperty(string? Name, string? Literal, ParsedMarkup? Nested, int Start, int End);

	sealed class MarkupParseContext(string expression) : IServiceProvider
	{
		public string Expression { get; } = expression;
		public object? GetService(Type serviceType) => serviceType == typeof(MarkupParseContext) ? this : null;
	}

	public sealed class ReferenceMarkupParser : MarkupExpressionParser, IExpressionParser
	{
		public object Parse(string match, ref string remaining, IServiceProvider serviceProvider)
		{
			var context = (MarkupParseContext)serviceProvider.GetService(typeof(MarkupParseContext))!;
			var position = context.Expression.Length - remaining.Length;
			var nameStart = context.Expression.LastIndexOf(match, position - 1, StringComparison.Ordinal);
			var properties = new List<ParsedProperty>();
			if (remaining.StartsWith("}", StringComparison.Ordinal))
			{
				remaining = remaining.Substring(1);
				return new ParsedMarkup(match, nameStart, properties);
			}

			Property property;
			do
			{
				var start = context.Expression.Length - remaining.Length;
				property = ParseProperty(serviceProvider, ref remaining);
				var end = context.Expression.Length - remaining.Length;
				properties.Add(new ParsedProperty(property.name, property.strValue, property.value as ParsedMarkup, start, end));
			}
			while (!property.last);
			return new ParsedMarkup(match, nameStart, properties);
		}
	}
}
