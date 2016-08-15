using System.Collections.Generic;
using Mono.Cecil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	static class MethodDefinitionExtensions
	{
		public static bool MatchXArguments(this MethodDefinition methodDefinition, ElementNode enode, ModuleDefinition module, ILContext context)
		{
			if (!enode.Properties.ContainsKey(XmlName.xArguments))
				return !methodDefinition.HasParameters;

			var arguments = new List<INode>();
			var node = enode.Properties[XmlName.xArguments] as ElementNode;
			if (node != null)
				arguments.Add(node);
			var list = enode.Properties[XmlName.xArguments] as ListNode;
			if (list != null)
			{
				foreach (var n in list.CollectionItems)
					arguments.Add(n);
			}

			if (methodDefinition.Parameters.Count != arguments.Count)
				return false;

			for (var i = 0; i < methodDefinition.Parameters.Count; i++)
			{
				var paramType = methodDefinition.Parameters[i].ParameterType;
				var argType = context.Variables [arguments [i] as IElementNode].VariableType;
				if (!argType.InheritsFromOrImplements(paramType))
					return false;
			}
			return true;
		}
	}
}