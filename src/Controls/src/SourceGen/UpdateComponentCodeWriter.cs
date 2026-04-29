using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
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
/// <item>IC compile-time converters (Color, Thickness, Enum, GridLength, etc.) — same pipeline as <c>InitializeComponent</c>.</item>
/// <item>Language primitives (<c>string</c>, <c>bool</c>, <c>int</c>, <c>double</c>, etc.) — inline literal.</item>
/// <item>Fallback — <c>TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString("value")</c>.</item>
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
		Dictionary<ElementNode, string>? newIds = null,
		SourceProductionContext sourceProductionContext = default,
		ProjectItem? projectItem = null)
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
			EmitChildListChange(codeWriter, change, changeIdx++, ref addedCounter, newIds, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
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
					EmitRootPropertyChange(codeWriter, propDiff, rootNodeType ?? rootType, compilation, xmlnsCache, typeCache, sourceProductionContext, projectItem);
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
				EmitPropertyChange(codeWriter, castPrefix, propDiff, nodeType, varName, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
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
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		bool isRoot = string.IsNullOrEmpty(change.ParentNodeId);

		// Resolve the parent variable and type
		string parentVar;
		INamedTypeSymbol? parentType;
		if (isRoot)
		{
			parentVar = "this";
			parentType = rootType;
		}
		else
		{
			parentVar = $"__rp_{changeIdx}";
			codeWriter.WriteLine($"if (!global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.TryGet(this, \"{change.ParentNodeId}\", out var {parentVar}))");
			codeWriter.Indent++;
			codeWriter.WriteLine("return;");
			codeWriter.Indent--;
			parentType = change.ParentXmlType?.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var pts) == true ? pts : null;
		}

		// Determine if parent is a Layout (Children collection) or a Content container
		bool isLayout = parentType != null && InheritsFrom(parentType, "global::Microsoft.Maui.Controls.Layout");
		string? contentPropertyName = null;
		if (!isLayout && parentType != null)
			contentPropertyName = FindContentPropertyName(parentType);

		if (!isLayout && contentPropertyName == null)
		{
			codeWriter.WriteLine($"// Container '{parentType?.Name ?? "unknown"}' is not a Layout and has no content property — skipped");
			return;
		}

		if (isLayout)
		{
			var layoutVar = $"__rl_{changeIdx}";
			if (isRoot)
				codeWriter.WriteLine($"var {layoutVar} = (global::Microsoft.Maui.Controls.Layout){parentVar};");
			else
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
					EmitNewElement(codeWriter, newElement, layoutVar, entry.NewNodeId, newIds, ref addedCounter, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
				}
			}
		}
		else
		{
			// Content property container (ContentPage, ContentView, ScrollView, Border, etc.)
			// These have a single content property — set directly instead of using Children.Add()
			EmitContentPropertyChange(codeWriter, change, changeIdx, parentVar, isRoot, contentPropertyName!, ref addedCounter, newIds, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
		}

		// With stable IDs, retained children keep their old IDs — no re-registration needed.

		// Unregister removed children and their entire subtrees
		foreach (var removedId in change.RemovedNodeIds)
		{
			codeWriter.WriteLine($"global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Unregister(this, \"{removedId}\");");
		}
	}

	/// <summary>
	/// Emits code for a child list change on a content property container (ContentPage, ContentView, etc.).
	/// Sets the content property to the single new child, or null if all children were removed.
	/// </summary>
	static void EmitContentPropertyChange(
		IndentedTextWriter codeWriter,
		ChildListChangeDiff change,
		int changeIdx,
		string parentVar,
		bool isRoot,
		string contentPropertyName,
		ref int addedCounter,
		Dictionary<ElementNode, string>? newIds,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		string target = isRoot ? $"this.{contentPropertyName}" : $"(({parentVar} as global::Microsoft.Maui.Controls.BindableObject)!).GetType().GetProperty(\"{contentPropertyName}\")";

		if (change.NewChildren.Count == 0)
		{
			// All children removed — clear the content property
			if (isRoot)
				codeWriter.WriteLine($"this.{contentPropertyName} = null!;");
			else
				codeWriter.WriteLine($"((dynamic){parentVar}!).{contentPropertyName} = null!;");
			return;
		}

		// Content containers only have one child — use the first one
		var entry = change.NewChildren[0];
		if (entry.Kind == ChildChangeKind.Retained)
		{
			// Child is the same element — nothing to do for content property
			// (the retained child is already set as content)
			return;
		}

		// New child — create it and set as content
		var newElement = entry.NewElement!;
		var childIdx = addedCounter++;
		var childVar = $"__na_{childIdx}";

		// Resolve type
		if (!newElement.XmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var childType)
			|| childType == null)
		{
			codeWriter.WriteLine($"// Cannot resolve type '{newElement.XmlType.Name}' — fallback");
			codeWriter.WriteLine("return;");
			return;
		}

		var fqName = childType.ToFQDisplayString();
		codeWriter.WriteLine($"var {childVar} = new {fqName}();");

		// Set properties on the new child
		EmitNewElementProperties(codeWriter, newElement, childVar, childType, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		// Recursively create grandchildren
		EmitNewElementChildren(codeWriter, newElement, childVar, entry.NewNodeId, newIds, ref addedCounter, childType, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		// Set as content
		if (isRoot)
			codeWriter.WriteLine($"this.{contentPropertyName} = (global::Microsoft.Maui.IView){childVar};");
		else
			codeWriter.WriteLine($"((dynamic){parentVar}!).{contentPropertyName} = (global::Microsoft.Maui.IView){childVar};");

		// Register the new child
		codeWriter.WriteLine($"global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Register(this, \"{entry.NewNodeId}\", {childVar});");
	}

	/// <summary>
	/// Finds the content property name for a type by walking the [ContentProperty] attribute
	/// on the type and its base types.
	/// </summary>
	static string? FindContentPropertyName(INamedTypeSymbol type)
	{
		var current = type;
		while (current != null)
		{
			foreach (var attr in current.GetAttributes())
			{
				if (attr.AttributeClass?.Name is "ContentPropertyAttribute"
					&& attr.ConstructorArguments.Length == 1
					&& attr.ConstructorArguments[0].Value is string name)
				{
					return name;
				}
			}
			current = current.BaseType;
		}
		return null;
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
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
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

		// Set properties
		EmitNewElementProperties(codeWriter, element, varName, typeSymbol, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		// Recursively create children
		EmitNewElementChildren(codeWriter, element, varName, nodeId, newIds, ref addedCounter, typeSymbol, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		// Add to parent layout
		codeWriter.WriteLine($"{parentLayoutVar}.Add((global::Microsoft.Maui.IView){varName});");

		// Register in component registry
		codeWriter.WriteLine($"global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Register(this, \"{nodeId}\", {varName});");
	}

	/// <summary>
	/// Emits property assignments for a newly created element.
	/// </summary>
	static void EmitNewElementProperties(
		IndentedTextWriter codeWriter,
		ElementNode element,
		string varName,
		INamedTypeSymbol typeSymbol,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		foreach (var kvp in element.Properties)
		{
			if (kvp.Key.NamespaceURI == "x")
				continue; // skip x:Name, x:Class, etc.

			if (kvp.Value is ValueNode valueNode)
			{
				var propName = kvp.Key.LocalName;
				var rawValue = valueNode.Value?.ToString() ?? string.Empty;

				if (propName.Contains('.'))
				{
					// Attached property on a new element — use SetValue pattern
					var syntheticDiff = new PropertyDiff(kvp.Key, PropertyDiffKind.Set, rawValue);
					TryEmitAttachedPropertyChange(codeWriter, syntheticDiff, varName, typeSymbol, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
				}
				else
				{
					var valueExpr = BuildValueExpression(rawValue, typeSymbol, propName, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
					if (valueExpr != null)
						codeWriter.WriteLine($"{varName}.{propName} = {valueExpr};");
				}
			}
			else if (kvp.Value is MarkupNode)
			{
				// Markup extension (Binding, StaticResource, DynamicResource, etc.)
				var syntheticDiff = new PropertyDiff(kvp.Key, PropertyDiffKind.Set, null, kvp.Value);
				TryEmitMarkupNodeChange(codeWriter, syntheticDiff, typeSymbol, varName, isRoot: false,
					compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
			}
		}
	}

	/// <summary>
	/// Recursively creates children for a newly created element, handling both
	/// Layout containers (Children.Add) and Content containers (Content property).
	/// </summary>
	static void EmitNewElementChildren(
		IndentedTextWriter codeWriter,
		ElementNode element,
		string varName,
		string nodeId,
		Dictionary<ElementNode, string>? newIds,
		ref int addedCounter,
		INamedTypeSymbol typeSymbol,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		if (element.CollectionItems.Count == 0)
			return;

		bool hasElementChildren = false;
		for (int i = 0; i < element.CollectionItems.Count; i++)
		{
			if (element.CollectionItems[i] is ElementNode)
			{ hasElementChildren = true; break; }
		}

		if (!hasElementChildren)
			return;

		bool isLayout = InheritsFrom(typeSymbol, "global::Microsoft.Maui.Controls.Layout");
		if (isLayout)
		{
			var childLayoutVar = $"__nal_{varName.GetHashCode():X8}";
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
					EmitNewElement(codeWriter, childElement, childLayoutVar, childNodeId, newIds, ref addedCounter, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
				}
			}
		}
		else
		{
			// Content property container — set the first element child as content
			var contentProp = FindContentPropertyName(typeSymbol);
			if (contentProp == null)
			{
				codeWriter.WriteLine($"// Non-layout container '{typeSymbol.Name}' has no [ContentProperty] — fallback");
				codeWriter.WriteLine("return;");
				return;
			}

			for (int i = 0; i < element.CollectionItems.Count; i++)
			{
				if (element.CollectionItems[i] is ElementNode childElement)
				{
					string childNodeId;
					if (newIds != null && newIds.TryGetValue(childElement, out var childId))
						childNodeId = childId;
					else
						childNodeId = $"{nodeId}_{i}";

					// Create the child element inline (not via EmitNewElement which adds to layout)
					if (!childElement.XmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var childType)
						|| childType == null)
					{
						codeWriter.WriteLine($"// Cannot resolve type '{childElement.XmlType.Name}' — fallback");
						codeWriter.WriteLine("return;");
						return;
					}

					var childIdx = addedCounter++;
					var childVar = $"__na_{childIdx}";
					codeWriter.WriteLine($"var {childVar} = new {childType.ToFQDisplayString()}();");
					EmitNewElementProperties(codeWriter, childElement, childVar, childType, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
					EmitNewElementChildren(codeWriter, childElement, childVar, childNodeId, newIds, ref addedCounter, childType, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
					codeWriter.WriteLine($"{varName}.{contentProp} = (global::Microsoft.Maui.IView){childVar};");
					codeWriter.WriteLine($"global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.Register(this, \"{childNodeId}\", {childVar});");
					break; // content property only has one child
				}
			}
		}
	}

	// -----------------------------------------------------------------------
	// Resource dictionary patching
	// -----------------------------------------------------------------------

	/// <summary>
	/// Emits code to patch the page's <c>Resources</c> dictionary from the new XAML node.
	/// Removes old resource keys not present in the new XAML, then adds/updates all new keyed resources.
	/// Tracks resource keys via <see cref="XamlComponentRegistry"/> so subsequent patches know what to remove.
	/// </summary>
	/// <returns><see langword="true"/> if resource emission was handled; <see langword="false"/> to fall through.</returns>
	static bool TryEmitResourceDictionaryChange(
		IndentedTextWriter codeWriter,
		INode newNode,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		// Collect resource elements from the new node
		var resourceElements = new List<ElementNode>();
		if (newNode is ListNode listNode)
		{
			foreach (var item in listNode.CollectionItems)
			{
				if (item is ElementNode en)
					resourceElements.Add(en);
			}
		}
		else if (newNode is ElementNode singleElement)
		{
			resourceElements.Add(singleElement);
		}

		if (resourceElements.Count == 0)
			return false;

		// Verify all resources have x:Key — required for dictionary patching
		var keyedResources = new List<(string key, ElementNode element)>();
		foreach (var elem in resourceElements)
		{
			if (elem.Properties.TryGetValue(XmlName.xKey, out var keyNode) && keyNode is ValueNode keyVal && keyVal.Value is string key)
				keyedResources.Add((key, elem));
			// Resources without x:Key (e.g., implicit styles) are skipped
		}

		if (keyedResources.Count == 0)
			return false;

		// Pre-compute which resources can actually be encoded as C# expressions.
		// Resources we can't encode (custom types, converters, etc.) are left untouched.
		var emittableResources = new List<(string key, string valueExpr)>();
		foreach (var (key, elem) in keyedResources)
		{
			var valueExpr = BuildResourceValueExpression(elem, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
			if (valueExpr != null)
				emittableResources.Add((key, valueExpr));
			else
				codeWriter.WriteLine($"// Cannot encode resource '{key}' \u2014 left untouched");
		}

		if (emittableResources.Count == 0)
			return false;

		// Remove old resource keys that are no longer in the new XAML (only keys we manage)
		codeWriter.WriteLine("foreach (var __rk in global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.GetResourceKeys(this))");
		using (PrePost.NewBlock(codeWriter))
		{
			var newKeysList = string.Join(", ", emittableResources.Select(kr => $"\"{EscapeString(kr.key)}\""));
			codeWriter.WriteLine($"if (global::System.Array.IndexOf(new string[] {{ {newKeysList} }}, __rk) < 0)");
			codeWriter.Indent++;
			codeWriter.WriteLine("this.Resources.Remove(__rk);");
			codeWriter.Indent--;
		}

		// Add/update emittable resources
		foreach (var (key, valueExpr) in emittableResources)
		{
			codeWriter.WriteLine($"this.Resources[\"{EscapeString(key)}\"] = {valueExpr};");
		}

		// Register only the keys we manage for next patch
		var keysArrayExpr = string.Join(", ", emittableResources.Select(kr => $"\"{EscapeString(kr.key)}\""));
		codeWriter.WriteLine($"global::Microsoft.Maui.Controls.Xaml.XamlComponentRegistry.RegisterResourceKeys(this, new string[] {{ {keysArrayExpr} }});");

		return true;
	}

	/// <summary>
	/// Builds a C# expression for a resource element value.
	/// For known types (Color, etc.), uses the IC compile-time converter pipeline.
	/// </summary>
	static string? BuildResourceValueExpression(
		ElementNode element,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		// Resolve the element type (e.g., Color, x:String, etc.)
		if (!element.XmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var typeSymbol)
			|| typeSymbol == null)
			return null;

		var fqName = typeSymbol.ToFQDisplayString();

		// For value-typed resources (Color, x:Double, x:String, etc.),
		// the value is typically in the element's CollectionItems as a ValueNode
		if (element.CollectionItems.Count == 1 && element.CollectionItems[0] is ValueNode valueNode)
		{
			var rawValue = valueNode.Value?.ToString() ?? string.Empty;

			// Use the IC converter pipeline via a synthetic property lookup
			var pi = projectItem ?? new ProjectItem(EmptyAdditionalText.Instance, EmptyConfigOptions.Instance);
			var ctx = new SourceGenContext(
				new IndentedTextWriter(new StringWriter()), compilation, sourceProductionContext,
				xmlnsCache, typeCache, rootType, rootType.BaseType, pi);

			try
			{
				var result = valueNode.ConvertTo(typeSymbol, ctx.Writer, ctx, null);
				if (result != null && result != "default" && result != string.Empty)
					return result;
			}
			catch
			{
				// Converter threw — fall through
			}

			// Fallback: TypeDescriptor
			return BuildTypeDescriptorExpression(rawValue, fqName);
		}

		// For custom types (converters, etc.) with no value content,
		// emit a new instance if the type has a parameterless constructor
		if (element.CollectionItems.Count == 0 || element.CollectionItems.All(c => c is not ValueNode))
		{
			var hasParameterlessCtor = typeSymbol.InstanceConstructors.Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public);
			if (hasParameterlessCtor)
				return $"new {fqName}()";
		}

		return null;
	}

	static void EmitRootPropertyChange(
		IndentedTextWriter codeWriter,
		PropertyDiff propDiff,
		INamedTypeSymbol rootType,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
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
			codeWriter.WriteLine($"// Property '{propName}' cleared on root — skipped (no BP found)");
			return;
		}

		if (propDiff.NewNode != null)
		{
			// Resource dictionary: emit Clear() + re-add all keyed resources
			if (propName == "Resources" && TryEmitResourceDictionaryChange(codeWriter, propDiff.NewNode, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem))
				return;

			if (TryEmitMarkupNodeChange(codeWriter, propDiff, rootType, "this", isRoot: true, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem))
				return;
			codeWriter.WriteLine($"// Complex root property '{propName}' ({propDiff.NewNode.GetType().Name}) — skipped (not yet supported)");
			return;
		}

		var rawValue = propDiff.NewValue ?? string.Empty;
		var valueExpr = BuildValueExpression(rawValue, rootType, propName, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		if (valueExpr == null)
		{
			codeWriter.WriteLine($"// Cannot encode root '{propName}' = \"{EscapeString(rawValue)}\" inline — skipped");
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
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
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
			codeWriter.WriteLine($"// Property '{propName}' cleared — skipped (no BP found)");
			return;
		}

		// Kind == Set
		// Attached property detection: "Grid.Row" → DeclaringType.RowProperty
		// Must be checked before generic MarkupNode handling, because the IC pipeline
		// needs the declaring type (Grid) to resolve the BP field for SetBinding.
		if (propName.Contains('.'))
		{
			TryEmitAttachedPropertyChange(codeWriter, propDiff, varName, nodeType, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
			return;
		}

		if (propDiff.NewNode != null)
		{
			if (nodeType != null && TryEmitMarkupNodeChange(codeWriter, propDiff, nodeType, varName, isRoot: false, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem))
				return;
			codeWriter.WriteLine($"// Complex property '{propName}' ({propDiff.NewNode.GetType().Name}) — skipped (not yet supported)");
			return;
		}

		var rawValue = propDiff.NewValue ?? string.Empty;
		var valueExpr = BuildValueExpression(rawValue, nodeType, propName, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		if (valueExpr == null)
		{
			// Cannot encode value inline — skip this property
			codeWriter.WriteLine($"// Cannot encode '{propName}' = \"{EscapeString(rawValue)}\" inline — skipped");
			return;
		}

		codeWriter.WriteLine($"{castPrefix}{propName} = {valueExpr};");
	}

	// -----------------------------------------------------------------------
	// Attached property handling
	// -----------------------------------------------------------------------

	/// <summary>
	/// Emits code for an attached property change like <c>Grid.Row="1"</c>.
	/// Resolves the declaring type, finds the BindableProperty field, and emits
	/// <c>((BindableObject)element).SetValue(Grid.RowProperty, value)</c>.
	/// </summary>
	static void TryEmitAttachedPropertyChange(
		IndentedTextWriter codeWriter,
		PropertyDiff propDiff,
		string varName,
		INamedTypeSymbol? elementType,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		var propName = propDiff.PropertyName.LocalName;
		var dotIdx = propName.IndexOf('.');
		var declaringTypeName = propName.Substring(0, dotIdx);
		var attachedPropName = propName.Substring(dotIdx + 1);

		// Resolve the declaring type (e.g., "Grid" → Microsoft.Maui.Controls.Grid)
		var declaringXmlType = new XmlType(propDiff.PropertyName.NamespaceURI, declaringTypeName, null);
		if (!declaringXmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var declaringType)
			|| declaringType == null)
		{
			// Try the default MAUI namespace
			declaringXmlType = new XmlType("http://schemas.microsoft.com/dotnet/2021/maui", declaringTypeName, null);
			if (!declaringXmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out declaringType)
				|| declaringType == null)
			{
				codeWriter.WriteLine($"// Cannot resolve attached property declaring type '{declaringTypeName}' — skipped");
				return;
			}
		}

		var bpFieldName = $"{attachedPropName}Property";
		var bpField = FindStaticField(declaringType, bpFieldName);
		if (bpField == null)
		{
			codeWriter.WriteLine($"// Cannot find '{declaringType.Name}.{bpFieldName}' — skipped");
			return;
		}

		// Handle markup extension values (e.g., Grid.Row="{Binding RowIndex}")
		// Use the element type (not declaring type) so IC pipeline resolves the attached BP via dotted XmlName
		if (propDiff.NewNode != null)
		{
			var markupOwner = elementType ?? declaringType;
			if (TryEmitMarkupNodeChange(codeWriter, propDiff, markupOwner, varName, isRoot: false, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem))
				return;
			codeWriter.WriteLine($"// Complex attached property '{propName}' — skipped");
			return;
		}

		// Simple value — try BuildValueExpression first (works if declaring type has a CLR property)
		var rawValue = propDiff.NewValue ?? string.Empty;
		var valueExpr = BuildValueExpression(rawValue, declaringType, attachedPropName, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		if (valueExpr == null)
		{
			// No CLR property — try resolving type from the BP's static getter (e.g., Grid.GetRow)
			valueExpr = BuildAttachedValueExpression(rawValue, declaringType, attachedPropName, bpField, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
		}

		if (valueExpr == null)
		{
			codeWriter.WriteLine($"// Cannot encode attached '{propName}' = \"{EscapeString(rawValue)}\" — skipped");
			return;
		}

		var fqDeclaring = declaringType.ToFQDisplayString();
		codeWriter.WriteLine($"({varName} as global::Microsoft.Maui.Controls.BindableObject)?.SetValue({fqDeclaring}.{bpFieldName}, {valueExpr});");
	}

	/// <summary>
	/// Resolves the value type for an attached BP from its static getter method
	/// (e.g., <c>Grid.GetRow(BindableObject)</c> → returns <c>int</c>).
	/// Then uses IC's ConvertTo pipeline to convert the raw XAML string value.
	/// </summary>
	static string? BuildAttachedValueExpression(
		string rawValue,
		INamedTypeSymbol declaringType,
		string attachedPropName,
		IFieldSymbol bpField,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		// Find the static getter: Grid.GetRow(BindableObject) → int
		var getterName = $"Get{attachedPropName}";
		IMethodSymbol? getter = null;
		var current = (ITypeSymbol)declaringType;
		while (current != null)
		{
			foreach (var member in current.GetMembers(getterName))
			{
				if (member is IMethodSymbol m && m.IsStatic && m.Parameters.Length == 1)
				{
					getter = m;
					break;
				}
			}
			if (getter != null) break;
			current = current.BaseType!;
		}

		if (getter == null)
			return null;

		var targetType = getter.ReturnType;
		var fqType = targetType.ToFQDisplayString();

		// Try IC's ConvertTo with a synthetic property-like context
		var pi = projectItem ?? new ProjectItem(EmptyAdditionalText.Instance, EmptyConfigOptions.Instance);
		var ctx = new SourceGenContext(
			new IndentedTextWriter(new StringWriter()), compilation, sourceProductionContext,
			xmlnsCache, typeCache, rootType, rootType.BaseType, pi);

		var valueNode = new ValueNode(rawValue, null, -1, -1);
		try
		{
			var result = valueNode.ConvertTo(targetType, ctx.Writer, ctx);
			if (result != null && result != "default" && result != string.Empty)
				return result;
		}
		catch
		{
			// fall through
		}

		// Last resort: TypeDescriptor
		return BuildTypeDescriptorExpression(rawValue, fqType);
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
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		if (propDiff.NewNode is not MarkupNode markupNode)
			return false;

		var markupString = markupNode.MarkupString;
		var propName = propDiff.PropertyName.LocalName;

		// Use ExpandMarkupsVisitor to convert the MarkupNode, reusing the same
		// classification and parsing logic as InitializeComponent.
		var expandedNode = ExpandMarkupForUC(markupNode, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);

		if (expandedNode is ValueNode valueNode && valueNode.Value is Expression)
		{
			// C# expression — generate SetBinding with TypedBinding
			return TryEmitExpressionBinding(codeWriter, markupString, propName, ownerType, targetAccessor, isRoot, compilation, xmlnsCache, typeCache, rootType, markupNode);
		}

		if (expandedNode is ElementNode elementNode)
		{
			// Preserve parent chain so the IC pipeline can walk up to find x:DataType, etc.
			elementNode.Parent = markupNode.Parent;

			// Markup extension (Binding, StaticResource, DynamicResource, etc.)
			// Resolve the extension type and call the appropriate KnownMarkups handler
			return TryEmitExpandedMarkupExtension(codeWriter, elementNode, propDiff.PropertyName, ownerType, targetAccessor, isRoot, compilation, xmlnsCache, typeCache, rootType, sourceProductionContext, projectItem);
		}

		// Fallback: bare string value from escape sequence {}{...}
		if (expandedNode is ValueNode plainValue && plainValue.Value is string sv)
		{
			var bpFieldName = $"{propName}Property";
			var bpField = FindStaticField(ownerType, bpFieldName);
			if (bpField != null)
			{
				var target = isRoot ? "this" : $"(({ownerType.ToFQDisplayString()}){targetAccessor}!)";
				codeWriter.WriteLine($"{target}.SetValue({bpField.ToFQDisplayString()}, \"{EscapeString(sv)}\");");
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Expands a <see cref="MarkupNode"/> using the same logic as <see cref="ExpandMarkupsVisitor"/>,
	/// converting it to a <see cref="ValueNode"/> (for C# expressions) or <see cref="ElementNode"/>
	/// (for markup extensions like Binding, StaticResource, etc.).
	/// </summary>
	static INode? ExpandMarkupForUC(
		MarkupNode markupNode,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		var markupString = markupNode.MarkupString;

		// Build a minimal SourceGenContext for ExpandMarkupsVisitor's parser
		var pi = projectItem ?? new ProjectItem(EmptyAdditionalText.Instance, EmptyConfigOptions.Instance);
		var ctx = new SourceGenContext(
			new IndentedTextWriter(new StringWriter()), compilation, sourceProductionContext,
			xmlnsCache, typeCache, rootType, rootType.BaseType, pi);

		// Classification: expression or markup extension?
		bool TryResolveMarkup(string name)
		{
			var ns = markupNode.NamespaceResolver.LookupNamespace("") ?? "";
			var xmlTypeExt = new XmlType(ns, name + "Extension", null);
			if (xmlTypeExt.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out _))
				return true;
			var xmlType = new XmlType(ns, name, null);
			return xmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out _);
		}

		var classification = CSharpExpressionHelpers.ClassifyExpression(
			markupString,
			TryResolveMarkup,
			_ => true); // treat bare identifiers as properties for UC

		if (classification.IsExpression)
		{
			var expressionCode = CSharpExpressionHelpers.GetExpressionCode(markupString);
			return new ValueNode(new Expression(expressionCode), markupNode.NamespaceResolver, markupNode.LineNumber, markupNode.LinePosition);
		}

		// Escape sequence: {}{...} → plain string
		if (markupString.StartsWith("{}", StringComparison.Ordinal))
			return new ValueNode(markupString.Substring(2), null, markupNode.LineNumber, markupNode.LinePosition);

		// Parse as markup extension using the same parser as ExpandMarkupsVisitor
		var remaining = markupString;
		if (!MarkupExpressionParser.MatchMarkup(out var match, remaining, out var len))
			return null;

		remaining = remaining.Substring(len).TrimStart();

		// Build service provider with same services ExpandMarkupsVisitor uses
		var serviceProvider = new XamlServiceProvider(markupNode, ctx);
		serviceProvider.Add(typeof(IXmlNamespaceResolver), markupNode.NamespaceResolver);
		serviceProvider.Add(typeof(ExpandMarkupsVisitor.SGContextProvider), new ExpandMarkupsVisitor.SGContextProvider(ctx));
		serviceProvider.Add(typeof(IXmlLineInfoProvider), new UCXmlLineInfoProvider(markupNode));

		try
		{
			var parser = new ExpandMarkupsVisitor.MarkupExpansionParser();
			return parser.Parse(match!, ref remaining, serviceProvider);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Emits code for an expanded markup extension <see cref="ElementNode"/> by reusing IC's
	/// full pipeline: CreateValuesVisitor → SetPropertiesVisitor → TryProvideValue → SetPropertyValue.
	/// Handles all known markup extensions (DynamicResource, StaticResource, Binding, AppThemeBinding,
	/// x:Reference, x:Static, etc.) and falls back to runtime ProvideValue() for unknown extensions.
	/// </summary>
	static bool TryEmitExpandedMarkupExtension(
		IndentedTextWriter codeWriter,
		ElementNode elementNode,
		XmlName propertyXmlName,
		INamedTypeSymbol ownerType,
		string targetAccessor,
		bool isRoot,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		// Build a SourceGenContext with a capture writer
		var pi = projectItem ?? new ProjectItem(EmptyAdditionalText.Instance, EmptyConfigOptions.Instance);
		var captureStringWriter = new StringWriter(CultureInfo.InvariantCulture);
		var captureWriter = new IndentedTextWriter(captureStringWriter, "\t") { NewLine = NewLine };
		var ctx = new SourceGenContext(
			captureWriter, compilation, sourceProductionContext,
			xmlnsCache, typeCache, rootType, rootType.BaseType, pi);

		// Create a synthetic parent variable so SetPropertyValue can emit "parent.SetValue(...)"
		var parentAccessor = isRoot ? "this" : $"(({ownerType.ToFQDisplayString()}){targetAccessor}!)";
		var syntheticParent = new ElementNode(new XmlType("", "SyntheticParent", null), "", null, -1, -1);
		ctx.Variables[syntheticParent] = new DirectValue(ownerType, parentAccessor);

		try
		{
			// Step 1: CreateValue — resolves type, handles early extensions (DynamicResource, x:Static, etc.)
			CreateValuesVisitor.CreateValue(elementNode, captureWriter, ctx.Variables, compilation, xmlnsCache, ctx);

			// Step 2: SetPropertiesVisitor — sets properties on the extension object
			var setPropsVisitor = new SetPropertiesVisitor(ctx);
			foreach (var kvp in elementNode.Properties)
			{
				if (elementNode.SkipProperties.Contains(kvp.Key))
					continue;
				kvp.Value.Accept(setPropsVisitor, elementNode);
			}
			foreach (var child in elementNode.CollectionItems)
				child.Accept(setPropsVisitor, elementNode);

			// Step 2.5: Patch StaticResource lookups that failed in the UC's synthetic tree.
			// When a Binding has Converter={StaticResource X}, the IC pipeline can't resolve it
			// because the UC tree has no parent chain to walk up for Resources. Emit runtime lookups
			// BEFORE TryProvideValue creates the TypedBinding (which reads extension.Converter).
			if (ctx.Variables.TryGetValue(elementNode, out var extVar))
			{
				foreach (var kvp in elementNode.Properties)
				{
					if (kvp.Value is ElementNode propElement
						&& propElement.XmlType.Name == "StaticResourceExtension"
						&& propElement.CollectionItems.Count == 1
						&& propElement.CollectionItems[0] is ValueNode keyVal
						&& keyVal.Value is string resourceKey)
					{
						var propLocalName = kvp.Key.LocalName;
						var propSymbol = extVar.Type.GetAllProperties(propLocalName, ctx).FirstOrDefault();
						if (propSymbol != null)
						{
							var castType = propSymbol.Type.ToFQDisplayString();
							captureWriter.WriteLine($"{extVar.ValueAccessor}.{propLocalName} = ({castType})this.Resources[\"{EscapeString(resourceKey)}\"];");
						}
					}
				}
			}

			// Step 3: TryProvideValue — handles late extensions (Binding, StaticResource, AppThemeBinding, etc.)
			// and falls back to runtime ProvideValue() for unknown IMarkupExtension implementors
			elementNode.TryProvideValue(captureWriter, ctx);

			// Step 4: SetPropertyValue — emits the assignment to the parent
			SetPropertyHelpers.SetPropertyValue(captureWriter, ctx.Variables[syntheticParent], propertyXmlName, elementNode, ctx);
		}
		catch
		{
			// If the IC pipeline fails (e.g., missing type info), fall through
			codeWriter.WriteLine($"// Markup extension '{elementNode.XmlType.Name}' failed IC pipeline — skipped");
			return false;
		}

		// Flush captured code to the UC codeWriter
		captureWriter.Flush();
		var capturedCode = captureStringWriter.ToString();
		if (string.IsNullOrWhiteSpace(capturedCode))
			return false;

		foreach (var line in capturedCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
		{
			if (!string.IsNullOrEmpty(line))
				codeWriter.WriteLine(line);
		}

		// Flush any local methods (e.g., compiled binding helpers like CreateTypedBindingFrom_*)
		// These are emitted as static local functions inside the UC method body.
		foreach (var localMethod in ctx.LocalMethods)
		{
			codeWriter.WriteLine();
			foreach (var mline in localMethod.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
			{
				if (string.IsNullOrWhiteSpace(mline))
					codeWriter.InnerWriter.WriteLine();
				else
					codeWriter.WriteLine(mline);
			}
		}

		return true;
	}

	/// <summary>Simple IXmlLineInfoProvider for UC's ExpandMarkupForUC.</summary>
	record UCXmlLineInfoProvider(IXmlLineInfo XmlLineInfo) : IXmlLineInfoProvider;

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
	/// Returns a C# expression string for <paramref name="rawXamlValue"/> using the same
	/// type converter pipeline as InitializeComponent (Color, Thickness, Enum, GridLength, etc.).
	/// Falls back to <c>TypeDescriptor.GetConverter</c> at runtime only when no compile-time
	/// converter is registered.
	/// Returns <see langword="null"/> when no encoding is possible at all.
	/// </summary>
	static string? BuildValueExpression(
		string rawXamlValue,
		INamedTypeSymbol? nodeType,
		string propertyName,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		INamedTypeSymbol rootType,
		SourceProductionContext sourceProductionContext,
		ProjectItem? projectItem)
	{
		if (nodeType == null)
			return null;

		var property = FindProperty(nodeType, propertyName);
		if (property == null)
			return null;

		// Build a lightweight SourceGenContext for the converter pipeline
		var pi = projectItem ?? new ProjectItem(EmptyAdditionalText.Instance, EmptyConfigOptions.Instance);
		var ctx = new SourceGenContext(
			new IndentedTextWriter(new StringWriter()), compilation, sourceProductionContext,
			xmlnsCache, typeCache, rootType, rootType.BaseType, pi);

		// Create a synthetic ValueNode to feed into IC's ConvertTo
		var valueNode = new ValueNode(rawXamlValue, null, -1, -1);

		// Use the IC's full conversion: TypeConverterAttribute lookup → known SG converters → ValueForLanguagePrimitive
		try
		{
			var result = valueNode.ConvertTo(property, ctx.Writer, ctx, null);
			if (result != null && result != "default" && result != string.Empty)
				return result;
		}
		catch
		{
			// Converter threw — fall through to runtime fallback
		}

		// Last resort: runtime TypeDescriptor (handles types not covered by SG converters)
		var fqName = property.Type.ToFQDisplayString();
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

	// Minimal Roslyn stubs for creating a ProjectItem when none is available (test scenarios)
	sealed class EmptyAdditionalText : AdditionalText
	{
		public static readonly EmptyAdditionalText Instance = new();
		public override string Path => "";
		public override SourceText? GetText(System.Threading.CancellationToken ct = default) => null;
	}

	sealed class EmptyConfigOptions : AnalyzerConfigOptions
	{
		public static readonly EmptyConfigOptions Instance = new();
#pragma warning disable CS8765
		public override bool TryGetValue(string key, out string? value) { value = null; return false; }
#pragma warning restore CS8765
	}
}
