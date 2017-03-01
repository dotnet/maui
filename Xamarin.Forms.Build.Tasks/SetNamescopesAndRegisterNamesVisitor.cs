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

		public bool VisitChildrenFirst
		{
			get { return false; }
		}

		public bool StopOnDataTemplate
		{
			get { return true; }
		}

		public bool StopOnResourceDictionary
		{
			get { return false; }
		}

		public void Visit(ValueNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
			if (IsXNameProperty(node, parentNode))
				RegisterName((string)node.Value, Context.Scopes[node].Item1, Context.Scopes[node].Item2, Context.Variables[(IElementNode)parentNode], node);
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			VariableDefinition namescopeVarDef;
			IList<string> namesInNamescope;
			if (parentNode == null || IsDataTemplate(node, parentNode) || IsStyle(node, parentNode)) {
				namescopeVarDef = CreateNamescope();
				namesInNamescope = new List<string>();
			} else {
				namescopeVarDef = Context.Scopes[parentNode].Item1;
				namesInNamescope = Context.Scopes[parentNode].Item2;
			}
			if (Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Body.Method.Module.ImportReference(typeof (BindableObject))))
				SetNameScope(node, namescopeVarDef);
			Context.Scopes[node] = new System.Tuple<VariableDefinition, IList<string>>(namescopeVarDef, namesInNamescope);
		}

		public void Visit(RootNode node, INode parentNode)
		{
			var namescopeVarDef = CreateNamescope();
			IList<string> namesInNamescope = new List<string>();
			if (Context.Variables[node].VariableType.InheritsFromOrImplements(Context.Body.Method.Module.ImportReference(typeof (BindableObject))))
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
			var nsRef = module.ImportReference(typeof (NameScope));
			var vardef = new VariableDefinition(nsRef);
			Context.Body.Variables.Add(vardef);
			var nsDef = nsRef.Resolve();
			var ctorinfo = nsDef.Methods.First(md => md.IsConstructor && !md.HasParameters);
			var ctor = module.ImportReference(ctorinfo);
			Context.IL.Emit(OpCodes.Newobj, ctor);
			Context.IL.Emit(OpCodes.Stloc, vardef);
			return vardef;
		}

		void SetNameScope(ElementNode node, VariableDefinition ns)
		{
			var module = Context.Body.Method.Module;
			var nsRef = module.ImportReference(typeof (NameScope));
			var nsDef = nsRef.Resolve();
			var setNSInfo = nsDef.Methods.First(md => md.Name == "SetNameScope" && md.IsStatic);
			var setNS = module.ImportReference(setNSInfo);
			Context.IL.Emit(OpCodes.Ldloc, Context.Variables[node]);
			Context.IL.Emit(OpCodes.Ldloc, ns);
			Context.IL.Emit(OpCodes.Call, setNS);
		}

		void RegisterName(string str, VariableDefinition namescopeVarDef, IList<string> namesInNamescope, VariableDefinition element, INode node)
		{
			if (namesInNamescope.Contains(str))
				throw new XamlParseException($"An element with the name \"{str}\" already exists in this NameScope", node as IXmlLineInfo);
			namesInNamescope.Add(str);

			var module = Context.Body.Method.Module;
			var nsRef = module.ImportReference(typeof (INameScope));
			var nsDef = nsRef.Resolve();
			var registerInfo = nsDef.Methods.First(md => md.Name == "RegisterName" && md.Parameters.Count == 2);
			var register = module.ImportReference(registerInfo);

			Context.IL.Emit(OpCodes.Ldloc, namescopeVarDef);
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Emit(OpCodes.Ldloc, element);
			Context.IL.Emit(OpCodes.Callvirt, register);
		}
	}
}