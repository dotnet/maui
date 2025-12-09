using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class SetNamescopesAndRegisterNamesVisitor(ILContext context) : IXamlNodeVisitor
	{
		ILContext Context { get; } = context;

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;
		public bool SkipChildren(INode node, INode parentNode) => false;

		public bool IsResourceDictionary(ElementNode node)
		{
			var parentVar = Context.Variables[node];
			return parentVar.VariableType.FullName == "Microsoft.Maui.Controls.ResourceDictionary"
				|| parentVar.VariableType.Resolve().BaseType?.FullName == "Microsoft.Maui.Controls.ResourceDictionary";
		}

		public void Visit(ValueNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
			if (!IsXNameProperty(node, parentNode))
				return;
			RegisterName((string)node.Value, Context.Scopes[node].Item1, Context.Scopes[node].Item2, Context.Variables[(ElementNode)parentNode], node);
			SetStyleId((string)node.Value, Context.Variables[(ElementNode)parentNode]);
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
				namesInNamescope = [];
				setNameScope = true;
			}
			else
			{
				namescopeVarDef = Context.Scopes[parentNode].Item1;
				namesInNamescope = Context.Scopes[parentNode].Item2;
			}
			if (setNameScope && Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Cache, Context.Body.Method.Module.ImportReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "BindableObject"))))
				SetNameScope(node, namescopeVarDef);
			//workaround when VSM tries to apply state before parenting
			else if (Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Cache, Context.Body.Method.Module.ImportReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Element"))))
			{
				var module = Context.Body.Method.Module;
				var parameterTypes = new[] {
					("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Element"),
					("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "INameScope"),
				};
				Context.IL.Append(Context.Variables[node].LoadAs(Context.Cache, module.GetTypeDefinition(Context.Cache, parameterTypes[0]), module));
				Context.IL.Append(namescopeVarDef.LoadAs(Context.Cache, module.GetTypeDefinition(Context.Cache, parameterTypes[1]), module));
				Context.IL.Emit(OpCodes.Stfld, module.ImportFieldReference(Context.Cache, parameterTypes[0], nameof(Element.transientNamescope)));
			}
			Context.Scopes[node] = new Tuple<VariableDefinition, IList<string>>(namescopeVarDef, namesInNamescope);
		}

		public void Visit(RootNode node, INode parentNode)
		{
			var namescopeVarDef = GetOrCreateNameScope(node);
			IList<string> namesInNamescope = new List<string>();
			if (Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Cache, Context.Body.Method.Module.ImportReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "BindableObject"))))
				SetNameScope(node, namescopeVarDef);
			Context.Scopes[node] = new System.Tuple<VariableDefinition, IList<string>>(namescopeVarDef, namesInNamescope);
		}

		public void Visit(ListNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
		}

		static bool IsDataTemplate(INode node, INode parentNode)
			=> parentNode is ElementNode parentElement && parentElement.Properties.TryGetValue(XmlName._CreateContent, out INode createContent) && createContent == node;

		static bool IsStyle(INode node, INode parentNode) => parentNode is ElementNode pnode && pnode.XmlType.Name == "Style";

		static bool IsVisualStateGroupList(ElementNode node) => node != null && node.XmlType.Name == "VisualStateGroup" && node.Parent is IListNode;

		static bool IsXNameProperty(ValueNode node, INode parentNode)
			=> parentNode is ElementNode parentElement && parentElement.Properties.TryGetValue(XmlName.xName, out INode xNameNode) && xNameNode == node;

		VariableDefinition GetOrCreateNameScope(ElementNode node)
		{
			var module = Context.Body.Method.Module;
			var vardef = new VariableDefinition(module.ImportReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "NameScope")));
			Context.Body.Variables.Add(vardef);
			var stloc = Instruction.Create(OpCodes.Stloc, vardef);

			if (Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Cache, Context.Body.Method.Module.ImportReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "BindableObject"))))
			{
				var namescoperef = ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "BindableObject");
				Context.IL.Append(Context.Variables[node].LoadAs(Context.Cache, module.GetTypeDefinition(Context.Cache, namescoperef), module));
				Context.IL.Emit(OpCodes.Call, module.ImportMethodReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "NameScope"),
																		   methodName: "GetNameScope",
																		   parameterTypes: [namescoperef],
																		   isStatic: true));
				Context.IL.Emit(OpCodes.Dup);
				Context.IL.Emit(OpCodes.Brtrue, stloc);

				Context.IL.Emit(OpCodes.Pop);
			}
			Context.IL.Emit(OpCodes.Newobj, module.ImportCtorReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "NameScope"), parameterTypes: null));

			Context.IL.Append(stloc);
			return vardef;
		}

		VariableDefinition CreateNamescope()
		{
			var module = Context.Body.Method.Module;
			var vardef = new VariableDefinition(module.ImportReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "NameScope")));
			Context.Body.Variables.Add(vardef);
			Context.IL.Emit(OpCodes.Newobj, module.ImportCtorReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "NameScope"), parameterTypes: null));
			Context.IL.Emit(OpCodes.Stloc, vardef);
			return vardef;
		}

		void SetNameScope(ElementNode node, VariableDefinition ns)
		{
			var module = Context.Body.Method.Module;
			var parameterTypes = new[] {
				("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "BindableObject"),
				("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "INameScope"),
			};
			Context.IL.Append(Context.Variables[node].LoadAs(Context.Cache, module.GetTypeDefinition(Context.Cache, parameterTypes[0]), module));
			Context.IL.Append(ns.LoadAs(Context.Cache, module.GetTypeDefinition(Context.Cache, parameterTypes[1]), module));
			Context.IL.Emit(OpCodes.Call, module.ImportMethodReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "NameScope"),
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
			var namescopeType = ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Internals", "INameScope");
			Context.IL.Append(namescopeVarDef.LoadAs(Context.Cache, module.GetTypeDefinition(Context.Cache, namescopeType), module));
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Append(element.LoadAs(Context.Cache, module.TypeSystem.Object, module));
			Context.IL.Emit(OpCodes.Callvirt, module.ImportMethodReference(Context.Cache,
																		   namescopeType,
																		   methodName: "RegisterName",
																		   parameterTypes: new[] {
																			   ("mscorlib", "System", "String"),
																			   ("mscorlib", "System", "Object"),
																		   }));
		}

		void SetStyleId(string str, VariableDefinition element)
		{
			if (!element.VariableType.InheritsFromOrImplements(Context.Cache, Context.Body.Method.Module.ImportReference(Context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Element"))))
				return;

			var module = Context.Body.Method.Module;
			var elementType = ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Element");
			var elementTypeRef = module.GetTypeDefinition(Context.Cache, elementType);

			var nop = Instruction.Create(OpCodes.Nop);
			Context.IL.Append(element.LoadAs(Context.Cache, elementTypeRef, module));
			Context.IL.Emit(OpCodes.Callvirt, module.ImportPropertyGetterReference(Context.Cache, elementType, propertyName: "StyleId"));
			Context.IL.Emit(OpCodes.Brtrue, nop);
			Context.IL.Append(element.LoadAs(Context.Cache, elementTypeRef, module));
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Emit(OpCodes.Callvirt, module.ImportPropertySetterReference(Context.Cache, elementType, propertyName: "StyleId"));
			Context.IL.Append(nop);
		}
	}
}