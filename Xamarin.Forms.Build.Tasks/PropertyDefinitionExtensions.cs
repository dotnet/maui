using System;
using Mono.Cecil;

namespace Xamarin.Forms.Build.Tasks
{
	static class PropertyDefinitionExtensions
	{
		public static TypeReference ResolveGenericPropertyType(this PropertyDefinition self, TypeReference declaringTypeRef,
			ModuleDefinition module)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));
			if (declaringTypeRef == null)
				throw new ArgumentNullException(nameof(declaringTypeRef));
			if (!self.PropertyType.IsGenericParameter)
				return self.PropertyType;

			return ((GenericInstanceType)declaringTypeRef).GenericArguments [((GenericParameter)self.PropertyType).Position];
		}
	}
}