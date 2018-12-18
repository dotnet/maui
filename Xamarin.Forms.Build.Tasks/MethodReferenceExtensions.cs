using System;
using Mono.Cecil;

namespace Xamarin.Forms.Build.Tasks
{
	static class MethodReferenceExtensions
	{
		public static MethodReference ResolveGenericParameters(this MethodReference self, TypeReference declaringTypeRef,
			ModuleDefinition module)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));
			if (declaringTypeRef == null)
				throw new ArgumentNullException(nameof(declaringTypeRef));

			var reference = new MethodReference(self.Name, ImportUnresolvedType(self.ReturnType, module))
			{
				DeclaringType = module.ImportReference(declaringTypeRef),
				HasThis = self.HasThis,
				ExplicitThis = self.ExplicitThis,
				CallingConvention = self.CallingConvention
			};

			foreach (var parameter in self.Parameters) {
				var p = ImportUnresolvedType(parameter.ParameterType, module);
				reference.Parameters.Add(new ParameterDefinition(p));
			}

			foreach (var generic_parameter in self.GenericParameters)
				reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

			return reference;
		}

		static TypeReference ImportUnresolvedType(TypeReference type, ModuleDefinition module)
		{
			return type.IsGenericParameter ? type : module.ImportReference(type);
		}

		public static void ImportTypes(this MethodReference self, ModuleDefinition module)
		{
			if (!self.HasParameters)
				return;

			for (var i = 0; i < self.Parameters.Count; i++)
				self.Parameters[i].ParameterType = module.ImportReference(self.Parameters[i].ParameterType);
		}

		public static MethodReference MakeGeneric(this MethodReference self, TypeReference declaringType, params TypeReference [] arguments)
		{
			var reference = new MethodReference(self.Name, self.ReturnType) {
				DeclaringType = declaringType,
				HasThis = self.HasThis,
				ExplicitThis = self.ExplicitThis,
				CallingConvention = self.CallingConvention,
			};

			foreach (var parameter in self.Parameters)
				reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

			foreach (var generic_parameter in self.GenericParameters)
				reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

			return reference;
		}
	}
}