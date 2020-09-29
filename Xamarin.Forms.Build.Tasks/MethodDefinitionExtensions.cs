using System.Collections.Generic;
using Mono.Cecil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	static class MethodDefinitionExtensions
	{
		public static bool MatchXArguments(this MethodDefinition methodDef, ElementNode enode, TypeReference declaringTypeRef, ModuleDefinition module, ILContext context)
		{
			if (!enode.Properties.ContainsKey(XmlName.xArguments))
				return !methodDef.HasParameters;

			var arguments = new List<INode>();
			var node = enode.Properties[XmlName.xArguments] as ElementNode;
			if (node != null)
				arguments.Add(node);

			var list = enode.Properties[XmlName.xArguments] as ListNode;
			if (list != null)
				foreach (var n in list.CollectionItems)
					arguments.Add(n);

			if (methodDef.Parameters.Count != arguments.Count)
				return false;

			for (var i = 0; i < methodDef.Parameters.Count; i++)
			{
				var paramType = methodDef.Parameters[i].ParameterType;
				var genParam = paramType as GenericParameter;
				if (genParam != null)
				{
					var index = genParam.DeclaringType.GenericParameters.IndexOf(genParam);
					paramType = (declaringTypeRef as GenericInstanceType).GenericArguments[index];
				}
				var argType = context.Variables[arguments[i] as IElementNode].VariableType;
				if (!argType.InheritsFromOrImplements(paramType))
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