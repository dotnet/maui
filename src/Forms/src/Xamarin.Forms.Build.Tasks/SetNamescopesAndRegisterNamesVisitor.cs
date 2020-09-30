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
		public SetNamescopesAndRegisterNamesVisitor(ILContext context) => Context = context;

		ILContext Context { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;
		public bool SkipChildren(INode node, INode parentNode) => false;

		public bool IsResourceDictionary(ElementNode node)
		{
			var parentVar = Context.Variables[node];
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
			if (parentNode == null || IsDataTemplate(node, parentNode) || IsStyle(node, parentNode) || IsVisualStateGroupList(node))
			{
				namescopeVarDef = CreateNamescope();
				namesInNamescope = new List<string>();
				setNameScope = true;
			}
			else
			{
				namescopeVarDef = Context.Scopes[parentNode].Item1;
				namesInNamescope = Context.Scopes[parentNode].Item2;
			}
			if (setNameScope && Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Body.Method.Module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms", "BindableObject"))))
				SetNameScope(node, namescopeVarDef);
			Context.Scopes[node] = new Tuple<VariableDefinition, IList<string>>(namescopeVarDef, namesInNamescope);
		}

		public void Visit(RootNode node, INode parentNode)
		{
			var namescopeVarDef = GetOrCreateNameScope(node);
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
			=> parentNode is IElementNode parentElement && parentElement.Properties.TryGetValue(XmlName._CreateContent, out INode createContent) && createContent == node;

		static bool IsStyle(INode node, INode parentNode) => parentNode is ElementNode pnode && pnode.XmlType.Name == "Style";

		static bool IsVisualStateGroupList(ElementNode node) => node != null && node.XmlType.Name == "VisualStateGroup" && node.Parent is IListNode;

		static bool IsXNameProperty(ValueNode node, INode parentNode)
			=> parentNode is IElementNode parentElement && parentElement.Properties.TryGetValue(XmlName.xName, out INode xNameNode) && xNameNode == node;

		VariableDefinition GetOrCreateNameScope(ElementNode node)
		{
			var module = Context.Body.Method.Module;
			var vardef = new VariableDefinition(module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "NameScope")));
			Context.Body.Variables.Add(vardef);
			var stloc = Instruction.Create(OpCodes.Stloc, vardef);

			if (Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Body.Method.Module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms", "BindableObject"))))
			{
				var namescoperef = ("Xamarin.Forms.Core", "Xamarin.Forms", "BindableObject");
				Context.IL.Append(Context.Variables[node].LoadAs(module.GetTypeDefinition(namescoperef), module));
				Context.IL.Emit(OpCodes.Call, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "NameScope"),
																		   methodName: "GetNameScope",
																		   parameterTypes: new[] { namescoperef },
																		   isStatic: true));
				Context.IL.Emit(OpCodes.Dup);
				Context.IL.Emit(OpCodes.Brtrue, stloc);

				Context.IL.Emit(OpCodes.Pop);
			}
			Context.IL.Emit(OpCodes.Newobj, module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "NameScope"), parameterTypes: null));

			Context.IL.Append(stloc);
			return vardef;
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
			var parameterTypes = new[] {
				("Xamarin.Forms.Core", "Xamarin.Forms", "BindableObject"),
				("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "INameScope"),
			};
			Context.IL.Append(Context.Variables[node].LoadAs(module.GetTypeDefinition(parameterTypes[0]), module));
			Context.IL.Append(ns.LoadAs(module.GetTypeDefinition(parameterTypes[1]), module));
			Context.IL.Emit(OpCodes.Call, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "NameScope"),
																	   methodName: "SetNameScope",
																	   parameterTypes: parameterTypes,
																	   isStatic: true));
		}

		void RegisterName(string str, VariableDefinition namescopeVarDef, IList<string> namesInNamescope, VariableDefinition element, INode node)
		{
			if (namesInNamescope.Contains(str))
				throw new BuildException(BuildExceptionCode.NamescopeDuplicate, node as IXmlLineInfo, null, str);
			namesInNamescope.Add(str);

			var module = Context.Body.Method.Module;
			var namescopeType = ("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "INameScope");
			Context.IL.Append(namescopeVarDef.LoadAs(module.GetTypeDefinition(namescopeType), module));
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Append(element.LoadAs(module.TypeSystem.Object, module));
			Context.IL.Emit(OpCodes.Callvirt, module.ImportMethodReference(namescopeType,
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
			var elementType = ("Xamarin.Forms.Core", "Xamarin.Forms", "Element");
			var elementTypeRef = module.GetTypeDefinition(elementType);

			var nop = Instruction.Create(OpCodes.Nop);
			Context.IL.Append(element.LoadAs(elementTypeRef, module));
			Context.IL.Emit(OpCodes.Callvirt, module.ImportPropertyGetterReference(elementType, propertyName: "StyleId"));
			Context.IL.Emit(OpCodes.Brtrue, nop);
			Context.IL.Append(element.LoadAs(elementTypeRef, module));
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Emit(OpCodes.Callvirt, module.ImportPropertySetterReference(elementType, propertyName: "StyleId"));
			Context.IL.Append(nop);
		}
	}
}