using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	static class MethodDefinitionExtensions
	{
		public static bool MatchXArguments(this MethodDefinition methodDef, ElementNode enode, TypeReference declaringTypeRef, ModuleDefinition module, ILContext context)
		{
			if (!enode.Properties.ContainsKey(XmlName.xArguments))
				return !methodDef.HasParameters;

			var arguments = new List<INode>();
			if (enode.Properties[XmlName.xArguments] is ElementNode node)
				arguments.Add(node);

			if (enode.Properties[XmlName.xArguments] is ListNode list)
				foreach (var n in list.CollectionItems)
					arguments.Add(n);

			if (methodDef.Parameters.Count != arguments.Count)
				return false;

			for (var i = 0; i < methodDef.Parameters.Count; i++)
			{
				var paramType = methodDef.Parameters[i].ParameterType;
				if (paramType is GenericParameter genParam)
				{
					var index = genParam.DeclaringType.GenericParameters.IndexOf(genParam);
					paramType = (declaringTypeRef as GenericInstanceType).GenericArguments[index];
				}
				var argType = context.Variables[arguments[i] as ElementNode].VariableType;
				if (!argType.InheritsFromOrImplements(context.Cache, paramType))
					return false;
			}
			return true;
		}

		public static TypeReference ResolveGenericReturnType(this MethodDefinition self, TypeReference declaringTypeRef, ModuleDefinition module)
		{
			if (self == null)
				throw new System.ArgumentNullException(nameof(self));
			if (declaringTypeRef == null)
				throw new System.ArgumentNullException(nameof(declaringTypeRef));
			if (!self.ReturnType.IsGenericParameter)
				return self.ReturnType;

			var t = ((GenericInstanceType)declaringTypeRef).GenericArguments[((GenericParameter)self.ReturnType).Position];
			return t;
		}

		public static bool HasCustomAttributes(this MethodDefinition self, TypeReference attribute)
		{
			if (!self.HasCustomAttributes)
				return false;
			foreach (var arg in self.CustomAttributes)
			{
				if (TypeRefComparer.Default.Equals(arg.AttributeType, attribute))
					return true;
			}
			return false;
		}
	}
}
