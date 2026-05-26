using System.Linq;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class SetFieldVisitor(ILContext context) : IXamlNodeVisitor
	{
		public ILContext Context { get; } = context;

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
			if (!IsXNameProperty(node, parentNode))
				return;
			var field = Context.Body.Method.DeclaringType.Fields.SingleOrDefault(fd => fd.Name == ((string)node.Value).Trim());
			if (field == null)
				return;
			Context.IL.Emit(OpCodes.Ldarg_0);
			Context.IL.Append(Context.Variables[(ElementNode)parentNode].LoadAs(Context.Cache, field.FieldType, Context.Module));
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
			if (   parentNode is ElementNode parentElement
				&& parentElement.Properties.TryGetValue(XmlName.xName, out INode xNameNode)
				&& xNameNode == node)
				return true;
			return false;
		}
	}
}