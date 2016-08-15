using System.Linq;
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
				RegisterName((string)node.Value, Context.Scopes[node], Context.Variables[(IElementNode)parentNode], node);
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
			Context.Scopes[node] = Context.Scopes[parentNode];
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			VariableDefinition ns;
			if (parentNode == null || IsDataTemplate(node, parentNode) || IsStyle(node, parentNode))
				ns = CreateNamescope();
			else
				ns = Context.Scopes[parentNode];
			if (
				Context.Variables[node].VariableType.InheritsFromOrImplements(
					Context.Body.Method.Module.Import(typeof (BindableObject))))
				SetNameScope(node, ns);
			Context.Scopes[node] = ns;
		}

		public void Visit(RootNode node, INode parentNode)
		{
			var ns = CreateNamescope();
			if (
				Context.Variables[node].VariableType.InheritsFromOrImplements(
					Context.Body.Method.Module.Import(typeof (BindableObject))))
				SetNameScope(node, ns);
			Context.Scopes[node] = ns;
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
			var nsRef = module.Import(typeof (NameScope));
			var vardef = new VariableDefinition(nsRef);
			Context.Body.Variables.Add(vardef);
			var nsDef = nsRef.Resolve();
			var ctorinfo = nsDef.Methods.First(md => md.IsConstructor && !md.HasParameters);
			var ctor = module.Import(ctorinfo);
			Context.IL.Emit(OpCodes.Newobj, ctor);
			Context.IL.Emit(OpCodes.Stloc, vardef);
			return vardef;
		}

		void SetNameScope(ElementNode node, VariableDefinition ns)
		{
			var module = Context.Body.Method.Module;
			var nsRef = module.Import(typeof (NameScope));
			var nsDef = nsRef.Resolve();
			var setNSInfo = nsDef.Methods.First(md => md.Name == "SetNameScope" && md.IsStatic);
			var setNS = module.Import(setNSInfo);
			Context.IL.Emit(OpCodes.Ldloc, Context.Variables[node]);
			Context.IL.Emit(OpCodes.Ldloc, ns);
			Context.IL.Emit(OpCodes.Call, setNS);
		}

		void RegisterName(string str, VariableDefinition scope, VariableDefinition element, INode node)
		{
			var module = Context.Body.Method.Module;
			var nsRef = module.Import(typeof (INameScope));
			var nsDef = nsRef.Resolve();
			var registerInfo = nsDef.Methods.First(md => md.Name == "RegisterName" && md.Parameters.Count == 3);
			var register = module.Import(registerInfo);

			Context.IL.Emit(OpCodes.Ldloc, scope);
			Context.IL.Emit(OpCodes.Ldstr, str);
			Context.IL.Emit(OpCodes.Ldloc, element);
			Context.IL.Append(node.PushXmlLineInfo(Context));
			Context.IL.Emit(OpCodes.Callvirt, register);
		}
	}
}