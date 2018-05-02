using System.Linq;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
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
			return parentVar.VariableType.FullName == "Xamarin.Forms.ResourceDictionary"
				|| parentVar.VariableType.Resolve().BaseType?.FullName == "Xamarin.Forms.ResourceDictionary";
		}

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!IsXNameProperty(node, parentNode))
				return;
			var field = Context.Body.Method.DeclaringType.Fields.SingleOrDefault(fd => fd.Name == (string)node.Value);
			if (field == null)
				return;
			Context.IL.Emit(OpCodes.Ldarg_0);
			Context.IL.Emit(OpCodes.Ldloc, Context.Variables[(IElementNode)parentNode]);
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