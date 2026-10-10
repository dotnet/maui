using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;

class SetFieldsForXNamesVisitor : IXamlNodeVisitor
{
	public SetFieldsForXNamesVisitor(SourceGenContext context) => Context = context;

	SourceGenContext Context { get; }
	IndentedTextWriter Writer => Context.Writer;
	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => false;
	public bool SkipChildren(INode node, INode parentNode) => node is ElementNode en && en.IsLazyResource(parentNode, Context);
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		if (!IsXNameProperty(node, parentNode))
			return;

		if (parentNode is not ElementNode parentElement)
			return;

		if (IsVisualStateGroup(parentElement))
			return;

		if (IsVisualState(parentElement))
			return;

		var fieldName = (string)(node.Value);

		// XAML Incremental Hot Reload: field declarations are additive-only (see
		// CodeBehindCodeWriter.ReconcileNamedFieldsForHotReload / issue #38993), so a renamed/reused
		// x:Name can resolve to a field whose declared (ghost) type no longer matches the element
		// actually produced here. Assigning to it would be a straight type-mismatch compile error, so
		// skip the assignment; the field keeps its original, unrelated value (default!).
		if (Context.ProjectItem.EnableIncrementalHotReload)
		{
			var assemblyName = Context.Compilation.AssemblyName ?? string.Empty;
			var targetFramework = Context.ProjectItem.TargetFramework ?? string.Empty;
			var stateKey = Context.ProjectItem.HotReloadStateKey;
			var accumulatedFields = XamlHotReloadState.GetAccumulatedCodeBehindFields(assemblyName, targetFramework, stateKey);
			if (accumulatedFields != null && accumulatedFields.TryGetValue(fieldName, out var accumulatedField))
			{
				var currentType = parentElement.XmlType.GetTypeSymbol(Context.ReportDiagnostic, Context.Compilation, Context.XmlnsCache, Context.TypeCache)?.ToFQDisplayString();
				if (currentType != accumulatedField.Type)
					return;
			}
		}

		Writer.WriteLine($"this.{EscapeIdentifier(fieldName)} = {Context.Variables[(ElementNode)parentNode].ValueAccessor};");
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
	}

	public void Visit(ElementNode node, INode parentNode)
	{
	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}

	static bool IsXNameProperty(ValueNode node, INode parentNode)
		=> parentNode is ElementNode parentElement && parentElement.Properties.TryGetValue(XmlName.xName, out INode xNameNode) && xNameNode == node;

	static bool IsVisualStateGroup(ElementNode node) => node?.XmlType.Name == "VisualStateGroup" && node?.Parent is IListNode;
	static bool IsVisualState(ElementNode node) => node?.XmlType.Name == "VisualState" && node?.Parent is IListNode;
}