using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil.Cil;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	class SetNamescopesAndRegisterNamesVisitor : IXamlNodeVisitor
	{
		public SetNamescopesAndRegisterNamesVisitor(ILContext context)
		{
			Context = context;
		}

		ILContext Context { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;
		public bool SkipChildren(INode node, INode parentNode) => false;

		public bool IsResourceDictionary(ElementNode node)
		{
			var parentVar = Context.Variables[(IElementNode)node];
			return parentVar.VariableType.FullName == "Xamarin.Forms.ResourceDictionary"
				|| parentVar.VariableType.Resolve().BaseType?.FullName == "Xamarin.Forms.ResourceDictionary";
		}

		public void Visit(ValueNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
			if (!IsXNameProperty(node, parentNode))
				return;
			RegisterName((string)node.Value, Context.Scopes[node].Item1, Context.Scopes[node].Item2, Context.Variables[(IElementNode)parentNode], node);
			SetStyleId((string)node.Value, Context.Variables[(IElementNode)parentNode]);
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			VariableDefinition namescopeVarDef;
			IList<string> namesInNamescope;
			var setNameScope = false;
			if (parentNode == null || IsDataTemplate(node, parentNode) || IsStyle(node, parentNode) || IsVisualStateGroupList(node)) {
				namescopeVarDef = CreateNamescope();
				namesInNamescope = new List<string>();
				setNameScope = true;
			} else {
				namescopeVarDef = Context.Scopes[parentNode].Item1;
				namesInNamescope = Context.Scopes[parentNode].Item2;
			}
			if (setNameScope && Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Body.Method.Module.ImportReference(("Xamarin.Forms.Core","Xamarin.Forms","BindableObject"))))
				SetNameScope(node, namescopeVarDef);
			Context.Scopes[node] = new Tuple<VariableDefinition, IList<string>>(namescopeVarDef, namesInNamescope);
		}
	
		public void Visit(RootNode node, INode parentNode)
		{
			var namescopeVarDef = CreateNamescope();
			IList<string> namesInNamescope = new List<string>();
			if (Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Body.Method.Module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms", "BindableObject"))))
				SetNameScope(node, namescopeVarDef);
			Context.Scopes[node] = new System.Tuple<VariableDefinition, IList<string>>(namescopeVarDef, namesInNamescope);
		}

		public void Visit(ListNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
		}

		static bool IsDataTemplate(INode node, INode parentNode)
		{
			var parentElement = parentNode as IElementNode;
			INode createContent;
			if (parentElement != null && parentElement.Properties.TryGetValue(XmlName._CreateContent, out createContent) &&
			    createContent == node)
				return true;
			return false;
		}

		static bool IsStyle(INode node, INode parentNode)
		{
			var pnode = parentNode as ElementNode;
			return pnode != null && pnode.XmlType.Name == "Style";
		}

		static bool IsVisualStateGroupList(ElementNode node)
		{
			return node != null  && node.XmlType.Name == "VisualStateGroup" && node.Parent is IListNode;
		}

		static bool IsXNameProperty(ValueNode node, INode parentNode)
		{
			var parentElement = parentNode as IElementNode;
			INode xNameNode;
			if (parentElement != null && parentElement.Properties.TryGetValue(XmlName.xName, out xNameNode) && xNameNode == node)
				return true;
			return false;
		}

		VariableDefinition CreateNamescope()
		{
			var module = Context.Body.Method.Module;
			var vardef = new VariableDefinition(module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "NameScope")));
			Context.Body.Variables.Add(vardef);
			Context.IL.Emit(OpCodes.Newobj, module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "NameScope"), parameterTypes: null));
			Context.IL.Emit(OpCodes.Stloc, vardef);
			return vardef;
		}

		void SetNameScope(ElementNode node, VariableDefinition ns)
		{
			var module = Context.Body.Method.Module;
			Context.IL.Emit(OpCodes.Ldloc, Context.Variables[node]);
			Context.IL.Emit(OpCodes.Ldloc, ns);
			Context.IL.Emit(OpCodes.Call, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "NameScope"),
																	   methodName: "SetNameScope",
																	   parameterTypes: new[] {
																		   ("Xamarin.Forms.Core", "Xamarin.Forms", "BindableObject"),
																		   ("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "INameScope"),
																	   },
																	   isStatic: true));
		}

		void RegisterName(string str, VariableDefinition namescopeVarDef, IList<string> namesInNamescope, VariableDefinition element, INode node)
		{
			if (namesInNamescope.Contains(str))
				throw new XamlParseException($"An element with the name \"{str}\" already exists in this NameScope", node as IXmlLineInfo);
			namesInNamescope.Add(str);

			var module = Context.Body.Method.Module;
			Context.IL.Emit(OpCodes.Ldloc, namescopeVarDef);
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Emit(OpCodes.Ldloc, element);
			Context.IL.Emit(OpCodes.Callvirt, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "INameScope"),
																		   methodName: "RegisterName",
																		   parameterTypes: new[] {
																			   ("mscorlib", "System", "String"),
																			   ("mscorlib", "System", "Object"),
																		   }));
		}

		void SetStyleId(string str, VariableDefinition element)
		{
			if (!element.VariableType.InheritsFromOrImplements(Context.Body.Method.Module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Element"))))
				return;

			var module = Context.Body.Method.Module;

			var nop = Instruction.Create(OpCodes.Nop);
			Context.IL.Emit(OpCodes.Ldloc, element);
			Context.IL.Emit(OpCodes.Callvirt, module.ImportPropertyGetterReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Element"), propertyName: "StyleId"));
			Context.IL.Emit(OpCodes.Brtrue, nop);
			Context.IL.Emit(OpCodes.Ldloc, element);
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Emit(OpCodes.Callvirt, module.ImportPropertySetterReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Element"), propertyName: "StyleId"));
			Context.IL.Append(nop);
		}
	}
}