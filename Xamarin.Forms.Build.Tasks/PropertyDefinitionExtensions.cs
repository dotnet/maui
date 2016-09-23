using System;
using Mono.Cecil;

namespace Xamarin.Forms.Build.Tasks
{
	static class PropertyDefinitionExtensions
	{
		public static TypeReference ResolveGenericPropertyType(this PropertyDefinition self, TypeReference declaringTypeReference)
		{
			if (self.PropertyType.IsGenericParameter)
				return ((GenericInstanceType)declaringTypeReference).GenericArguments [((GenericParameter)self.PropertyType).Position];
			return self.PropertyType;
		}
	}
}