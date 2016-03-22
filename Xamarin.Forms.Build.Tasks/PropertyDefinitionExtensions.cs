using Mono.Cecil;

namespace Xamarin.Forms.Build.Tasks
{
	static class PropertyDefinitionExtensions
	{
		//		public static PropertyDefinition MakeGeneric (this PropertyDefinition self, GenericInstanceType declaringTypeReference)
		//		{
		//			if (declaringTypeReference == null)
		//				throw new ArgumentNullException ("declaringTypeReference");
		//			if (self == null)
		//				throw new ArgumentNullException ("self");
		//
		//			var propertyType = declaringTypeReference.GenericArguments[((GenericParameter)self.PropertyType).Position];
		//			self.PropertyType = propertyType;
		//			self.SetMethod = self.SetMethod.MakeGeneric (propertyType).Resolve ();
		//			self.GetMethod.ReturnType = propertyType;
		//
		//			return self;
		//		}

		public static TypeReference ResolveGenericPropertyType(this PropertyDefinition self,
			TypeReference declaringTypeReference)
		{
			if (self.PropertyType.IsGenericParameter)
			{
				return
					((GenericInstanceType)declaringTypeReference).GenericArguments[((GenericParameter)self.PropertyType).Position];
			}
			return self.PropertyType;
		}
	}
}