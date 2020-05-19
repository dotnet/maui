using System.Linq;
using Mono.Cecil.Cil;
using System.Maui.Xaml;

namespace System.Maui.Build.Tasks
{
	class SetFieldVisitor : IXamlNodeVisitor
	{
		public SetFieldVisitor(ILContext context)
		{
			Context = context;
		}

		public ILContext Context { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;
		public bool SkipChildren(INode node, INode parentNode) => false;

		public bool IsResourceDictionary(ElementNode node)
		{
			var parentVar = Context.Variables[(IElementNode)node];
			return parentVar.VariableType.FullName == "System.Maui.ResourceDictionary"
				|| parentVar.VariableType.Resolve().BaseType?.FullName == "System.Maui.ResourceDictionary";
		}

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!IsXNameProperty(node, parentNode))
				return;
			var field = Context.Body.Method.DeclaringType.Fields.SingleOrDefault(fd => fd.Name == (string)node.Value);
			if (field == null)
				return;
			Context.IL.Emit(OpCodes.Ldarg_0);
			Context.IL.Append(Context.Variables[(IElementNode)parentNode].LoadAs(field.FieldType, Context.Module));
			Context.IL.Emit(OpCodes.Stfld, field);
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
		{
			var parentElement = parentNode as IElementNode;
			INode xNameNode;
			if (parentElement != null && parentElement.Properties.TryGetValue(XmlName.xName, out xNameNode) && xNameNode == node)
				return true;
			return false;
		}
	}
}