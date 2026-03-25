using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Generates a single <c>UpdateComponent()</c> partial method containing accumulated
/// <c>if (__version == N)</c> patch blocks from successive XAML Hot Reload edits.
/// </summary>
/// <remarks>
/// <para>
/// Per the spec, the generated method chains sequential <c>if</c> blocks (not <c>else if</c>):
/// <code>
/// internal void UpdateComponent()
/// {
///     if (__version == 0) { /* v0→v1 patch */ __version = 1; }
///     if (__version == 1) { /* v1→v2 patch */ __version = 2; }
/// }
/// </code>
/// A v0 instance chains through ALL patches; a fresh instance (whose <c>InitializeComponent</c>
/// sets <c>__version</c> to the latest) skips them all.
/// </para>
/// <para>
/// Property value encoding strategy (in priority order):
/// <list type="bullet">
/// <item><c>string</c> — direct string literal.</item>
/// <item><c>bool</c> — <c>true</c> / <c>false</c> literal.</item>
/// <item><c>int</c>, <c>double</c>, <c>float</c>, <c>decimal</c> — numeric literal.</item>
/// <item>All other types — <c>TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString("value")</c>.</item>
/// <item>Property not found on type — return (skip patch).</item>
/// </list>
/// </para>
/// </remarks>
static class UpdateComponentCodeWriter
{
	static readonly string NewLine = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n";

	/// <summary>
	/// Generates the code for a single <c>if (__version == fromVersion) { ... __version = toVersion; }</c> block.
	/// Returns <see langword="null"/> when <paramref name="diff"/> contains no changes.
	/// </summary>
	/// <param name="newIds">ID dictionary for added nodes (from the new tree). May be null for tests.</param>
	public static string? GeneratePatchBody(
		XamlTreeDiff diff,
		int fromVersion,
		int toVersion,
		INamedTypeSymbol rootType,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		Dictionary<ElementNode, string>? newIds = null)
	{
		if (diff.IsEmpty)
			return null;

		using var codeWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { NewLine = NewLine };

		// Emit diff summary as a comment for diagnostics
		EmitDiffSummary(codeWriter, diff);

		// Emit child list changes FIRST (before property changes, since property diffs use post-change IDs)
		int changeIdx = 0;
		int addedCounter = 0;
		foreach (var change in diff.ChildListChanges)
		{
			EmitChildListChange(codeWriter, change, changeIdx++, ref addedCounter, newIds, compilation, xmlnsCache, typeCache);
			codeWriter.WriteLine();
		}

		int compIdx = 0;
		foreach (var nodeDiff in diff.NodeChanges)
		{
			bool isRoot = string.IsNullOrEmpty(nodeDiff.NodeId);

			if (isRoot)
			{
				INamedTypeSymbol? rootNodeType = null;
				if (nodeDiff.NodeXmlType is { } xmlTypeRoot)
					xmlTypeRoot.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out rootNodeType);

				foreach (var propDiff in nodeDiff.PropertyChanges)
				{
					EmitRootPropertyChange(codeWriter, propDiff, rootNodeType ?? rootType, compilation, xmlnsCache, typeCache);
				}
				codeWriter.WriteLine();
				continue;
			}

			var varName = $"__uc_{compIdx++}";
			codeWriter.WriteLine($"if (!global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.TryGet(this, \"{nodeDiff.NodeId}\", out var {varName}))");
			codeWriter.Indent++;
			codeWriter.WriteLine("return;");
			codeWriter.Indent--;

			INamedTypeSymbol? nodeType = null;
			string castPrefix;
			if (nodeDiff.NodeXmlType is { } xmlType
				&& xmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out nodeType)
				&& nodeType != null)
			{
				var fqName = nodeType.ToFQDisplayString();
				castPrefix = $"(({fqName}){varName}!).";
			}
			else
			{
				castPrefix = $"({varName} as global::Microsoft.Maui.Controls.BindableObject)?.";
			}

			foreach (var propDiff in nodeDiff.PropertyChanges)
			{
				EmitPropertyChange(codeWriter, castPrefix, propDiff, nodeType, varName, compilation, xmlnsCache, typeCache, rootType);
			}

			codeWriter.WriteLine();
		}

		codeWriter.WriteLine($"__version = {toVersion};");

		codeWriter.Flush();
		return codeWriter.InnerWriter.ToString();
	}

	/// <summary>
	/// Assembles a complete <c>UpdateComponent()</c> source file from accumulated patch bodies.
	/// Each patch body becomes an <c>if (__version == N) { ... }</c> block inside the single method.
	/// </summary>
	public static string? GenerateUpdateComponent(
		INamedTypeSymbol rootType,
		string accessModifier,
		List<string> allPatchBodies,
		int startVersion = 0)
	{
		if (allPatchBodies.Count == 0)
			return null;

		using var codeWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { NewLine = NewLine };

		codeWriter.WriteLine(GeneratorHelpers.AutoGeneratedHeaderText);
		codeWriter.WriteLine("#nullable enable");
		codeWriter.WriteLine("#pragma warning disable CS0219 // Variable is assigned but its value is never used");
		codeWriter.WriteLine();
		codeWriter.WriteLine($"namespace {rootType.ContainingNamespace};");
		codeWriter.WriteLine();
		codeWriter.WriteLine($"{accessModifier} partial class {rootType.Name}");

		using (PrePost.NewBlock(codeWriter))
		{
			codeWriter.WriteLine($"[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
			codeWriter.WriteLine($"internal void UpdateComponent()");

			using (PrePost.NewBlock(codeWriter))
			{
				// Emit each patch as a sequential if (__version == N) block
				for (int i = 0; i < allPatchBodies.Count; i++)
				{
					var body = allPatchBodies[i];
					var version = startVersion + i;
					codeWriter.WriteLine($"if (__version == {version})");
					using (PrePost.NewBlock(codeWriter))
					{
						WriteIndentedBody(codeWriter, body);
					}
				}

				codeWriter.WriteLine("return;");
			}
		}

		codeWriter.Flush();
		return codeWriter.InnerWriter.ToString();
	}

	/// <summary>
	/// Backward-compatible overload: generates a single-patch UpdateComponent() from a diff.
	/// Used by unit tests that test a single diff.
	/// </summary>
	public static string? GenerateUpdateComponent(
		INamedTypeSymbol rootType,
		string accessModifier,
		XamlTreeDiff diff,
		int fromVersion,
		int toVersion,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache)
	{
		var patchBody = GeneratePatchBody(diff, fromVersion, toVersion, rootType, compilation, xmlnsCache, typeCache);
		if (patchBody == null)
			return null;

		return GenerateUpdateComponent(rootType, accessModifier, new List<string> { patchBody }, startVersion: fromVersion);
	}

	static void WriteIndentedBody(IndentedTextWriter codeWriter, string body)
	{
		var lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		foreach (var line in lines)
		{
			if (!string.IsNullOrWhiteSpace(line))
				codeWriter.WriteLine(line.TrimStart('\t'));
			else if (line.Length > 0)
				codeWriter.WriteLine();
		}
	}

	static void EmitChildListChange(
		IndentedTextWriter codeWriter,
		ChildListChangeDiff change,
		int changeIdx,
		ref int addedCounter,
		Dictionary<ElementNode, string>? newIds,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache)
	{
		bool isRoot = string.IsNullOrEmpty(change.ParentNodeId);

		if (isRoot)
		{
			codeWriter.WriteLine("// Root-level child list change not supported — fallback");
			codeWriter.WriteLine("return;");
			return;
		}

		// Look up parent from registry
		var parentVar = $"__rp_{changeIdx}";
		codeWriter.WriteLine($"if (!global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.TryGet(this, \"{change.ParentNodeId}\", out var {parentVar}))");
		codeWriter.Indent++;
		codeWriter.WriteLine("return;");
		codeWriter.Indent--;

		// Cast to Layout to access Children
		var layoutVar = $"__rl_{changeIdx}";
		codeWriter.WriteLine($"var {layoutVar} = {parentVar} as global::Microsoft.Maui.Controls.Layout;");
		codeWriter.WriteLine($"if ({layoutVar} == null) return;");

		// Save references to all retained children by their old node IDs
		int retainedIdx = 0;
		for (int i = 0; i < change.NewChildren.Count; i++)
		{
			var entry = change.NewChildren[i];
			if (entry.Kind != ChildChangeKind.Retained)
				continue;
			var childVar = $"__rc_{changeIdx}_{retainedIdx++}";
			codeWriter.WriteLine($"if (!global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.TryGet(this, \"{entry.OldNodeId}\", out var {childVar}))");
			codeWriter.Indent++;
			codeWriter.WriteLine("return;");
			codeWriter.Indent--;
		}

		// Clear children
		codeWriter.WriteLine($"{layoutVar}.Clear();");

		// Re-add retained children and create+add new children in new order
		retainedIdx = 0;
		for (int i = 0; i < change.NewChildren.Count; i++)
		{
			var entry = change.NewChildren[i];
			if (entry.Kind == ChildChangeKind.Retained)
			{
				var childVar = $"__rc_{changeIdx}_{retainedIdx++}";
				codeWriter.WriteLine($"{layoutVar}.Add((global::Microsoft.Maui.IView){childVar}!);");
			}
			else // Added
			{
				var newElement = entry.NewElement!;
				EmitNewElement(codeWriter, newElement, layoutVar, entry.NewNodeId, newIds, ref addedCounter, compilation, xmlnsCache, typeCache);
			}
		}

		// With stable IDs, retained children keep their old IDs — no re-registration needed.
		// (Old position-based IDs required ReRoot on reorder; stable IDs don't.)

		// Unregister removed children and their entire subtrees (individual calls since flat IDs
		// don't support prefix-based subtree removal).
		foreach (var removedId in change.RemovedNodeIds)
		{
			codeWriter.WriteLine($"global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Unregister(this, \"{removedId}\");");
		}
	}

	/// <summary>
	/// Emits code that creates a new element, sets its simple properties, recursively creates
	/// its children, adds them, and registers everything in the component registry.
	/// Uses the <paramref name="newIds"/> dictionary to look up IDs for each node.
	/// </summary>
	static void EmitNewElement(
		IndentedTextWriter codeWriter,
		ElementNode element,
		string parentLayoutVar,
		string nodeId,
		Dictionary<ElementNode, string>? newIds,
		ref int addedCounter,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache)
	{
		// Resolve XmlType → C# type
		if (!element.XmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var typeSymbol)
			|| typeSymbol == null)
		{
			codeWriter.WriteLine($"// Cannot resolve type '{element.XmlType.Name}' — fallback");
			codeWriter.WriteLine("return;");
			return;
		}

		var fqName = typeSymbol.ToFQDisplayString();
		var idx = addedCounter++;
		var varName = $"__na_{idx}";

		// Create instance
		codeWriter.WriteLine($"var {varName} = new {fqName}();");

		// Set simple properties
		foreach (var kvp in element.Properties)
		{
			if (kvp.Key.NamespaceURI == "x")
				continue; // skip x:Name, x:Class, etc.
			if (kvp.Value is not ValueNode valueNode)
				continue; // skip complex properties (already validated by CanCreateIncrementally)

			var propName = kvp.Key.LocalName;
			var rawValue = valueNode.Value?.ToString() ?? string.Empty;
			var valueExpr = BuildValueExpression(rawValue, typeSymbol, propName);

			if (valueExpr != null)
				codeWriter.WriteLine($"{varName}.{propName} = {valueExpr};");
		}

		// Recursively create children (only for Layout containers that support Add())
		if (element.CollectionItems.Count > 0)
		{
			bool hasElementChildren = false;
			for (int i = 0; i < element.CollectionItems.Count; i++)
			{
				if (element.CollectionItems[i] is ElementNode)
				{ hasElementChildren = true; break; }
			}

			if (hasElementChildren)
			{
				// Only Layout types support Add(); for non-Layout containers (ContentView, ScrollView, etc.)
				// fall back to full reload to avoid invalid casts.
				if (!InheritsFrom(typeSymbol, "global::Microsoft.Maui.Controls.Layout"))
				{
					codeWriter.WriteLine($"// Non-layout container '{typeSymbol.Name}' with children — fallback");
					codeWriter.WriteLine("return;");
					return;
				}

				var childLayoutVar = $"__nal_{idx}";
				codeWriter.WriteLine($"var {childLayoutVar} = (global::Microsoft.Maui.Controls.Layout){varName};");

				for (int i = 0; i < element.CollectionItems.Count; i++)
				{
					if (element.CollectionItems[i] is ElementNode childElement)
					{
						string childNodeId;
						if (newIds != null && newIds.TryGetValue(childElement, out var childId))
							childNodeId = childId;
						else
							childNodeId = $"{nodeId}_{i}"; // fallback
						EmitNewElement(codeWriter, childElement, childLayoutVar, childNodeId, newIds, ref addedCounter, compilation, xmlnsCache, typeCache);
					}
				}
			}
		}

		// Add to parent layout
		codeWriter.WriteLine($"{parentLayoutVar}.Add((global::Microsoft.Maui.IView){varName});");

		// Register in component registry
		codeWriter.WriteLine($"global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Register(this, \"{nodeId}\", {varName});");
	}

	static void EmitRootPropertyChange(
		IndentedTextWriter codeWriter,
		PropertyDiff propDiff,
		INamedTypeSymbol rootType,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache)
	{
		var propName = propDiff.PropertyName.LocalName;

		if (propDiff.Kind == PropertyDiffKind.Clear)
		{
			var bpFieldName = $"{propName}Property";
			var bp = FindStaticField(rootType, bpFieldName);
			if (bp != null)
			{
				codeWriter.WriteLine($"this.ClearValue({rootType.ToFQDisplayString()}.{bpFieldName});");
				return;
			}
			codeWriter.WriteLine($"// Property '{propName}' cleared on root — fallback to runtime reload");
			codeWriter.WriteLine("return;");
			return;
		}

		if (propDiff.NewNode != null)
		{
			if (TryEmitMarkupNodeChange(codeWriter, propDiff, rootType, "this", isRoot: true, compilation, xmlnsCache, typeCache, rootType))
				return;
			codeWriter.WriteLine($"// Complex root property '{propName}' ({propDiff.NewNode.GetType().Name}) — not supported");
			codeWriter.WriteLine("return;");
			return;
		}

		var rawValue = propDiff.NewValue ?? string.Empty;
		var valueExpr = BuildValueExpression(rawValue, rootType, propName);

		if (valueExpr == null)
		{
			codeWriter.WriteLine($"// Cannot encode root '{propName}' = \"{EscapeString(rawValue)}\" inline — fallback");
			codeWriter.WriteLine("return;");
			return;
		}

		codeWriter.WriteLine($"this.{propName} = {valueExpr};");
	}

	static void EmitPropertyChange(
		IndentedTextWriter codeWriter,
		string castPrefix,
		PropertyDiff propDiff,
		INamedTypeSymbol? nodeType,
		string varName,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType)
	{
		var propName = propDiff.PropertyName.LocalName;

		if (propDiff.Kind == PropertyDiffKind.Clear)
		{
			// Use ClearValue via the BindableObject API when we know the bindable property field
			if (nodeType != null)
			{
				// Look for a static BindableProperty field named {PropName}Property
				var bpFieldName = $"{propName}Property";
				var bp = FindStaticField(nodeType, bpFieldName);
				if (bp != null)
				{
					codeWriter.WriteLine($"({varName} as global::Microsoft.Maui.Controls.BindableObject)?.ClearValue({nodeType.ToFQDisplayString()}.{bpFieldName});");
					return;
				}
			}
			codeWriter.WriteLine($"// Property '{propName}' cleared — fallback to runtime reload");
			codeWriter.WriteLine("return;");
			return;
		}

		// Kind == Set
		if (propDiff.NewNode != null)
		{
			if (nodeType != null && TryEmitMarkupNodeChange(codeWriter, propDiff, nodeType, varName, isRoot: false, compilation, xmlnsCache, typeCache, rootType))
				return;
			codeWriter.WriteLine($"// Complex property '{propName}' ({propDiff.NewNode.GetType().Name}) — not supported");
			codeWriter.WriteLine("return;");
			return;
		}
		var rawValue = propDiff.NewValue ?? string.Empty;
		var valueExpr = BuildValueExpression(rawValue, nodeType, propName);

		if (valueExpr == null)
		{
			// Cannot encode value inline — use runtime fallback
			codeWriter.WriteLine($"// Cannot encode '{propName}' = \"{EscapeString(rawValue)}\" inline — fallback to runtime reload");
			codeWriter.WriteLine("return;");
			return;
		}

		codeWriter.WriteLine($"{castPrefix}{propName} = {valueExpr};");
	}

	// -----------------------------------------------------------------------
	// MarkupNode handling (expressions, DynamicResource, StaticResource)
	// -----------------------------------------------------------------------

	/// <summary>
	/// Attempts to emit code for a MarkupNode property change.
	/// Returns <c>true</c> if code was emitted, <c>false</c> if the node type is unsupported.
	/// </summary>
	static bool TryEmitMarkupNodeChange(
		IndentedTextWriter codeWriter,
		PropertyDiff propDiff,
		INamedTypeSymbol ownerType,
		string targetAccessor,
		bool isRoot,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType)
	{
		if (propDiff.NewNode is not MarkupNode markupNode)
			return false;

		var markupString = markupNode.MarkupString;
		var propName = propDiff.PropertyName.LocalName;

		// Try {DynamicResource Key}
		if (TryParseDynamicResource(markupString, out var drKey))
			return TryEmitDynamicResource(codeWriter, drKey, propName, ownerType, targetAccessor, isRoot);

		// Try {StaticResource Key}
		if (TryParseStaticResource(markupString, out var srKey))
			return TryEmitStaticResource(codeWriter, srKey, propName, ownerType, targetAccessor, isRoot);

		// Everything else: try as C# expression.
		// Explicit expressions ({= ...}) and implicit expressions (operators, method calls, member access)
		// are detected by IsExpression. Bare identifiers like {FirstName} are not — but if we have
		// an x:DataType and the identifier is NOT a known markup extension, treat as expression.
		if (IsExpressionForUC(markupString))
			return TryEmitExpressionBinding(codeWriter, markupString, propName, ownerType, targetAccessor, isRoot, compilation, xmlnsCache, typeCache, rootType, markupNode);

		return false;
	}

	/// <summary>
	/// Determines if a markup string should be treated as a C# expression in the UC context.
	/// More permissive than <c>CSharpExpressionHelpers.IsExpression</c> because bare identifiers
	/// like <c>{FirstName}</c> are treated as expressions when not matching a known markup extension.
	/// </summary>
	static bool IsExpressionForUC(string markupString)
	{
		// Explicit expressions: {= expr}
		if (CSharpExpressionHelpers.IsExplicitExpression(markupString))
			return true;

		var trimmed = markupString.Trim();
		if (trimmed.Length < 3 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
			return false;

		// Escape sequence {}{...}
		if (trimmed.StartsWith("{}", StringComparison.Ordinal))
			return false;

		// If it starts with a known markup extension name, it's not an expression
		// (DynamicResource/StaticResource already handled above, but check others too)
		if (StartsWithKnownMarkupExtension(trimmed))
			return false;

		// Everything else: treat as expression (bare identifier, method call, operator, etc.)
		return true;
	}

	static bool StartsWithKnownMarkupExtension(string trimmed)
	{
		int start = 1;
		while (start < trimmed.Length && char.IsWhiteSpace(trimmed[start]))
			start++;
		int end = start;
		while (end < trimmed.Length && (char.IsLetterOrDigit(trimmed[end]) || trimmed[end] == ':'))
			end++;
		if (end <= start)
			return false;
		var identifier = trimmed.Substring(start, end - start);
		// Prefixed identifiers like x:Type are always markup extensions
		if (identifier.IndexOf(':') >= 0)
			return true;
		return CSharpExpressionHelpers.IsKnownMarkupExtensionName(identifier);
	}

	/// <summary>
	/// Emits a <c>SetBinding</c> call with a <c>TypedBinding</c> for a C# expression.
	/// </summary>
	static bool TryEmitExpressionBinding(
		IndentedTextWriter codeWriter,
		string markupString,
		string propName,
		INamedTypeSymbol ownerType,
		string targetAccessor,
		bool isRoot,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		MarkupNode markupNode)
	{
		// Find the BindableProperty field
		var bpFieldName = $"{propName}Property";
		var bpField = FindStaticField(ownerType, bpFieldName);
		if (bpField == null)
			return false;

		// Find x:DataType by walking up the parent chain
		if (!TryResolveXDataType(markupNode, compilation, xmlnsCache, typeCache, out var dataTypeSymbol) || dataTypeSymbol == null)
			return false;

		var expressionCode = CSharpExpressionHelpers.GetExpressionCode(markupString);

		// Transform quotes
		var transformedExpression = CSharpExpressionHelpers.TransformQuotesWithSemantics(
			expressionCode, compilation, dataTypeSymbol, rootType);

		// Analyze expression
		var analysis = ExpressionAnalyzer.Analyze(transformedExpression, "__source", dataTypeSymbol, rootType);
		var handlers = analysis.Handlers;

		// Resolve expression result type
		var expressionType = ResolveExpressionTypeForUC(expressionCode, dataTypeSymbol, compilation);
		var sourceTypeName = dataTypeSymbol.ToFQDisplayString();
		var propertyTypeName = expressionType?.ToFQDisplayString() ?? "object";
		var bpName = bpField.ToFQDisplayString();

		// Wrap in scoped block if we have captures
		bool hasCaptures = analysis.Captures.Count > 0;
		if (hasCaptures)
		{
			codeWriter.WriteLine("{");
			codeWriter.Indent++;
			foreach (var capture in analysis.Captures)
			{
				codeWriter.WriteLine($"var {capture.CaptureVariable} = this.{capture.InvocationExpression};");
			}
		}

		var setBindingTarget = isRoot ? "this" : $"(({ownerType.ToFQDisplayString()}){targetAccessor}!)";
		codeWriter.WriteLine($"{setBindingTarget}.SetBinding({bpName},");
		codeWriter.Indent++;
		codeWriter.WriteLine($"new global::Microsoft.Maui.Controls.Internals.TypedBinding<{sourceTypeName}, {propertyTypeName}>(");
		codeWriter.Indent++;

		// Getter
		var getterExpression = analysis.TransformedExpression;
		if (getterExpression.Contains("?."))
			getterExpression += "!";
		codeWriter.WriteLine($"__source => ({getterExpression}, true),");

		// Setter
		if (analysis.IsSettable)
			codeWriter.WriteLine($"(__source, __value) => {analysis.TransformedExpression} = __value,");
		else
			codeWriter.WriteLine("null,");

		// Handlers array
		if (handlers.Count == 0)
		{
			codeWriter.WriteLine("null));");
		}
		else
		{
			codeWriter.WriteLine($"new global::System.Tuple<global::System.Func<{sourceTypeName}, object>, string>[] {{");
			codeWriter.Indent++;
			for (int i = 0; i < handlers.Count; i++)
			{
				var handler = handlers[i];
				var comma = i < handlers.Count - 1 ? "," : "";
				codeWriter.WriteLine($"new(static __source => {handler.ParentExpression}, \"{handler.PropertyName}\"){comma}");
			}
			codeWriter.Indent--;
			codeWriter.WriteLine("}));");
		}
		codeWriter.Indent -= 2;

		if (hasCaptures)
		{
			codeWriter.Indent--;
			codeWriter.WriteLine("}");
		}

		return true;
	}

	/// <summary>
	/// Emits a <c>SetDynamicResource</c> call for a <c>{DynamicResource Key}</c> markup.
	/// </summary>
	static bool TryEmitDynamicResource(
		IndentedTextWriter codeWriter,
		string resourceKey,
		string propName,
		INamedTypeSymbol ownerType,
		string targetAccessor,
		bool isRoot)
	{
		var bpFieldName = $"{propName}Property";
		var bpField = FindStaticField(ownerType, bpFieldName);
		if (bpField == null)
			return false;

		var bpName = bpField.ToFQDisplayString();
		var target = isRoot ? "this" : $"(({ownerType.ToFQDisplayString()}){targetAccessor}!)";
		codeWriter.WriteLine($"((global::Microsoft.Maui.Controls.Internals.IDynamicResourceHandler){target}).SetDynamicResource({bpName}, \"{EscapeString(resourceKey)}\");");
		return true;
	}

	/// <summary>
	/// Emits code to apply a <c>{StaticResource Key}</c> at runtime.
	/// Uses the element's resource lookup chain.
	/// </summary>
	static bool TryEmitStaticResource(
		IndentedTextWriter codeWriter,
		string resourceKey,
		string propName,
		INamedTypeSymbol ownerType,
		string targetAccessor,
		bool isRoot)
	{
		var bpFieldName = $"{propName}Property";
		var bpField = FindStaticField(ownerType, bpFieldName);
		if (bpField == null)
			return false;

		var bpName = bpField.ToFQDisplayString();
		var target = isRoot ? "this" : $"(({ownerType.ToFQDisplayString()}){targetAccessor}!)";
		// Use SetValue with inline resource lookup walking the element tree
		codeWriter.WriteLine($"{{");
		codeWriter.Indent++;
		codeWriter.WriteLine($"object __sr_val = null;");
		codeWriter.WriteLine($"var __sr_element = {target} as global::Microsoft.Maui.Controls.Element;");
		codeWriter.WriteLine($"while (__sr_element != null)");
		codeWriter.WriteLine($"{{");
		codeWriter.Indent++;
		codeWriter.WriteLine($"if (__sr_element is global::Microsoft.Maui.Controls.VisualElement __sr_ve && __sr_ve.Resources != null && __sr_ve.Resources.TryGetValue(\"{EscapeString(resourceKey)}\", out __sr_val))");
		codeWriter.Indent++;
		codeWriter.WriteLine($"break;");
		codeWriter.Indent--;
		codeWriter.WriteLine($"__sr_element = __sr_element.Parent as global::Microsoft.Maui.Controls.Element;");
		codeWriter.Indent--;
		codeWriter.WriteLine($"}}");
		codeWriter.WriteLine($"if (__sr_val == null && global::Microsoft.Maui.Controls.Application.Current?.Resources?.TryGetValue(\"{EscapeString(resourceKey)}\", out __sr_val) != true)");
		codeWriter.Indent++;
		codeWriter.WriteLine($"__sr_val = null;");
		codeWriter.Indent--;
		codeWriter.WriteLine($"if (__sr_val != null)");
		codeWriter.Indent++;
		codeWriter.WriteLine($"{target}.SetValue({bpName}, __sr_val);");
		codeWriter.Indent--;
		codeWriter.Indent--;
		codeWriter.WriteLine($"}}");
		return true;
	}

	// -----------------------------------------------------------------------
	// Helpers for MarkupNode parsing
	// -----------------------------------------------------------------------

	static bool TryParseDynamicResource(string markupString, out string key)
	{
		key = "";
		var trimmed = markupString.Trim();
		if (!trimmed.StartsWith("{DynamicResource ", StringComparison.Ordinal))
			return false;
		// {DynamicResource Key} → extract "Key"
		key = trimmed.Substring("{DynamicResource ".Length, trimmed.Length - "{DynamicResource ".Length - 1).Trim();
		return key.Length > 0;
	}

	static bool TryParseStaticResource(string markupString, out string key)
	{
		key = "";
		var trimmed = markupString.Trim();
		if (!trimmed.StartsWith("{StaticResource ", StringComparison.Ordinal))
			return false;
		key = trimmed.Substring("{StaticResource ".Length, trimmed.Length - "{StaticResource ".Length - 1).Trim();
		return key.Length > 0;
	}

	/// <summary>
	/// Walks up the XAML node tree from a MarkupNode to find the nearest x:DataType declaration.
	/// Simplified version of <c>XDataTypeResolver.TryGetXDataType</c> that doesn't require <c>SourceGenContext</c>.
	/// </summary>
	static bool TryResolveXDataType(
		MarkupNode markupNode,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		out ITypeSymbol? dataTypeSymbol)
	{
		dataTypeSymbol = null;

		// Walk up: MarkupNode → parent ElementNode → ancestors
		var current = markupNode.Parent as ElementNode;
		while (current != null)
		{
			if (current.Properties.TryGetValue(XmlName.xDataType, out var dtNode))
			{
				// x:DataType="local:TypeName" stored as ValueNode
				if (dtNode is ValueNode vn && vn.Value is string dtString)
				{
					XmlType? xmlType;
					try
					{
						xmlType = TypeArgumentsParser.ParseSingle(dtString, current.NamespaceResolver, null);
					}
					catch (XamlParseException)
					{
						return false;
					}
					if (xmlType != null && xmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var resolved))
					{
						dataTypeSymbol = resolved;
						return true;
					}
				}
				return false;
			}

			// Walk up to parent ElementNode
			var parent = current.Parent;
			if (parent is ListNode listNode)
				current = listNode.Parent as ElementNode;
			else
				current = parent as ElementNode;
		}

		return false;
	}

	/// <summary>
	/// Resolves the result type of a C# expression for TypedBinding TProperty.
	/// Simplified version that tries member lookup on the data type.
	/// </summary>
	static ITypeSymbol? ResolveExpressionTypeForUC(string expression, ITypeSymbol dataType, Compilation compilation)
	{
		// Simple property access: "PropertyName"
		var trimmed = expression.Trim();
		var members = dataType.GetMembers(trimmed);
		foreach (var member in members)
		{
			if (member is IPropertySymbol prop)
				return prop.Type;
			if (member is IFieldSymbol field)
				return field.Type;
		}

		// Method call: "MethodName()" — strip parens and look up return type
		if (trimmed.EndsWith("()", StringComparison.Ordinal))
		{
			var methodName = trimmed.Substring(0, trimmed.Length - 2);
			foreach (var member in dataType.GetMembers(methodName))
			{
				if (member is IMethodSymbol method)
					return method.ReturnType;
			}
		}

		// Complex expression — fall back to object
		return compilation.GetSpecialType(SpecialType.System_Object);
	}

	/// <summary>
	/// Returns a C# expression string for <paramref name="rawXamlValue"/>.
	/// Returns <see langword="null"/> when no inline encoding is possible.
	/// </summary>
	static string? BuildValueExpression(string rawXamlValue, INamedTypeSymbol? nodeType, string propertyName)
	{
		// Look up the C# property type
		ITypeSymbol? propType = null;
		if (nodeType != null)
		{
			var member = FindProperty(nodeType, propertyName);
			propType = member?.Type;
		}

		if (propType == null)
		{
			// Type is unresolvable — cannot generate safe code; fall through to goto fallback
			return null;
		}

		var fqName = propType.ToFQDisplayString();

		// Direct string
		if (fqName == "string" || fqName == "System.String" || fqName == "global::System.String")
			return $"\"{EscapeString(rawXamlValue)}\"";

		// bool
		if (fqName is "bool" or "global::System.Boolean")
		{
			if (rawXamlValue.Equals("true", StringComparison.OrdinalIgnoreCase)) return "true";
			if (rawXamlValue.Equals("false", StringComparison.OrdinalIgnoreCase)) return "false";
			return null;
		}

		// Integer types
		if (fqName is "int" or "global::System.Int32" or "long" or "global::System.Int64"
			or "short" or "global::System.Int16" or "byte" or "global::System.Byte")
		{
			var trimmed = rawXamlValue.Trim();
			if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
				return trimmed;
			return null; // invalid numeric — fallback
		}

		// Float/double — reject NaN/Infinity which parse successfully but produce invalid C# literals
		if (fqName is "float" or "global::System.Single")
		{
			var trimmed = rawXamlValue.Trim();
			if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var fVal)
				&& !double.IsNaN(fVal) && !double.IsInfinity(fVal))
				return $"{trimmed}f";
			return null;
		}
		if (fqName is "double" or "global::System.Double")
		{
			var trimmed = rawXamlValue.Trim();
			if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var dVal)
				&& !double.IsNaN(dVal) && !double.IsInfinity(dVal))
				return trimmed;
			return null;
		}
		if (fqName is "decimal" or "global::System.Decimal")
		{
			var trimmed = rawXamlValue.Trim();
			if (decimal.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
				return $"{trimmed}m";
			return null;
		}

		// All other types: use TypeDescriptor at runtime
		return BuildTypeDescriptorExpression(rawXamlValue, fqName);
	}

	static string BuildTypeDescriptorExpression(string rawValue, string? fqTypeName)
	{
		if (fqTypeName == null)
			return $"global::System.ComponentModel.TypeDescriptor.GetConverter(null)?.ConvertFromInvariantString(\"{EscapeString(rawValue)}\")";

		return $"({fqTypeName})global::System.ComponentModel.TypeDescriptor.GetConverter(typeof({fqTypeName})).ConvertFromInvariantString(\"{EscapeString(rawValue)}\")!";
	}

	static IPropertySymbol? FindProperty(INamedTypeSymbol type, string name)
	{
		var current = (ITypeSymbol)type;
		while (current != null)
		{
			foreach (var member in current.GetMembers(name))
			{
				if (member is IPropertySymbol prop)
					return prop;
			}
			current = current.BaseType!;
		}
		return null;
	}

	static IFieldSymbol? FindStaticField(INamedTypeSymbol type, string name)
	{
		var current = (ITypeSymbol)type;
		while (current != null)
		{
			foreach (var member in current.GetMembers(name))
			{
				if (member is IFieldSymbol field && field.IsStatic)
					return field;
			}
			current = current.BaseType!;
		}
		return null;
	}

	/// <summary>
	/// Checks whether <paramref name="type"/> inherits from a type with the given fully-qualified
	/// metadata name (e.g. <c>"Microsoft.Maui.Controls.Layout"</c>).
	/// </summary>
	static bool InheritsFrom(INamedTypeSymbol type, string baseTypeFqn)
	{
		var current = type.BaseType;
		while (current != null)
		{
			if (string.Equals(current.ToFQDisplayString(), baseTypeFqn, StringComparison.Ordinal))
				return true;
			current = current.BaseType;
		}
		return false;
	}

	static string EscapeString(string value) =>
		value.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("\0", "\\0")
			.Replace("\a", "\\a")
			.Replace("\b", "\\b")
			.Replace("\f", "\\f")
			.Replace("\n", "\\n")
			.Replace("\r", "\\r")
			.Replace("\t", "\\t")
			.Replace("\v", "\\v");

	/// <summary>
	/// Emits the diff summary as a C# comment block at the top of the method body.
	/// This makes the diff visible when inspecting generated source files in the IDE.
	/// </summary>
	static void EmitDiffSummary(IndentedTextWriter codeWriter, XamlTreeDiff diff)
	{
		var summary = diff.ToDebugString();
		var lines = summary.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		foreach (var line in lines)
		{
			codeWriter.WriteLine($"// {line}");
		}
		codeWriter.WriteLine();
	}
}
