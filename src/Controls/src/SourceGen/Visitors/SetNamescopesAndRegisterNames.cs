using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

class SetNamescopesAndRegisterNamesVisitor(SourceGenContext context) : IXamlNodeVisitor
{
	SourceGenContext Context => context;
	IndentedTextWriter Writer => Context.Writer;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => false;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		var namescope = Context.Scopes[node] = Context.Scopes[parentNode];
		if (!IsXNameProperty(node, parentNode))
			return;
		var name = (string)node.Value;
		if (namescope.namesInScope.ContainsKey(name))
			//TODO send diagnostic instead
			throw new Exception("dup x:Name");
		namescope.namesInScope.Add(name, Context.Variables[(ElementNode)parentNode]);
		using (PrePost.NewConditional(Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
		{
			Writer.WriteLine($"{namescope.namescope.Name}.RegisterName(\"{name}\", {Context.Variables[(ElementNode)parentNode].Name});");
		}
		SetStyleId((string)node.Value, Context.Variables[(ElementNode)parentNode]);
	}

	public void Visit(MarkupNode node, INode parentNode)
		=> Context.Scopes[node] = Context.Scopes[parentNode];

	public void Visit(ElementNode node, INode parentNode)
	{
		LocalVariable namescope;
		IDictionary<string, LocalVariable> namesInNamescope;
		var setNameScope = false;

		if (parentNode == null || IsDataTemplate(node, parentNode) || IsStyle(node, parentNode) || IsVisualStateGroupList(node))
		{
			namescope = CreateNamescope();
			namesInNamescope = new Dictionary<string, LocalVariable>();
			setNameScope = true;
		}
		else
		{
			namescope = Context.Scopes[parentNode].namescope;
			namesInNamescope = Context.Scopes[parentNode].namesInScope;
		}

		if (setNameScope && Context.Variables[node].Type.InheritsFrom(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableObject")!, Context))
			using (PrePost.NewConditional(Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				Writer.WriteLine($"global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope({Context.Variables[node].Name}, {namescope.Name});");
			}
		//workaround when VSM tries to apply state before parenting (https://github.com/dotnet/maui/issues/16208)
		else if (Context.Variables.TryGetValue(node, out var variable) && variable.Type.InheritsFrom(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Element")!, Context))
			using (PrePost.NewConditional(Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				Writer.WriteLine($"{Context.Variables[node].Name}.transientNamescope = {namescope.Name};");
			}

		Context.Scopes[node] = (namescope, namesInNamescope);
	}

	public void Visit(RootNode node, INode parentNode)
	{
		var namescope = GetOrCreateNameScope(node);
		if (Context.RootType.InheritsFrom(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableObject")!, Context))
			using (PrePost.NewConditional(Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				Writer.WriteLine($"global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope({Context.Variables[node].Name}, {namescope.Name});");
			}
		Context.Scopes[node] = new(namescope, new Dictionary<string, LocalVariable>());
	}

	public void Visit(ListNode node, INode parentNode)
		=> Context.Scopes[node] = Context.Scopes[parentNode];

	static bool IsDataTemplate(INode node, INode parentNode)
		=> parentNode is ElementNode parentElement && parentElement.Properties.TryGetValue(XmlName._CreateContent, out INode createContent) && createContent == node;

	static bool IsStyle(INode node, INode parentNode)
		=> parentNode is ElementNode pnode && pnode.XmlType.Name == "Style";

	static bool IsVisualStateGroupList(ElementNode node)
		=> node != null && node.XmlType.Name == "VisualStateGroup" && node.Parent is IListNode;

	static bool IsXNameProperty(ValueNode node, INode parentNode)
		=> parentNode is ElementNode parentElement && parentElement.Properties.TryGetValue(XmlName.xName, out INode xNameNode) && xNameNode == node;

	LocalVariable GetOrCreateNameScope(ElementNode node)
	{
		var namescope = NamingHelpers.CreateUniqueVariableName(Context, Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Internals.INameScope")!);
		using (PrePost.NewConditional(Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
		{
			Writer.Write($"global::Microsoft.Maui.Controls.Internals.INameScope {namescope} = ");
			if (Context.Variables[node].Type.InheritsFrom(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableObject")!, Context))
				Writer.Write($"global::Microsoft.Maui.Controls.Internals.NameScope.GetNameScope({Context.Variables[node].Name}) ?? ");
			Writer.WriteLine($"new global::Microsoft.Maui.Controls.Internals.NameScope();");
		}
		return new LocalVariable(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Internals.NameScope")!, namescope);
	}

	LocalVariable CreateNamescope()
	{
		var namescope = NamingHelpers.CreateUniqueVariableName(Context, Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Internals.INameScope")!);
		using (PrePost.NewConditional(Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
		{
			Writer.WriteLine($"global::Microsoft.Maui.Controls.Internals.INameScope {namescope} = new global::Microsoft.Maui.Controls.Internals.NameScope();");
		}
		return new LocalVariable(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Internals.NameScope")!, namescope);
	}

	void SetStyleId(string str, LocalVariable element)
	{
		if (!element.Type.InheritsFrom(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Element")!, Context))
			return;

		Writer.WriteLine($"{element.Name}.StyleId ??= \"{str}\";");
	}
}
