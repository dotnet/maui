using System;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Xamarin.Forms.Build.Tasks
{
	static class MethodReferenceExtensions
	{
		[Obsolete]
		public static MethodReference MakeGeneric(this MethodReference self, params TypeReference[] arguments)
		{
			var reference = new MethodReference(self.Name, self.ReturnType)
			{
				DeclaringType = self.DeclaringType.MakeGenericInstanceType(arguments),
				HasThis = self.HasThis,
				ExplicitThis = self.ExplicitThis,
				CallingConvention = self.CallingConvention
			};

			foreach (var parameter in self.Parameters)
				reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

			foreach (var generic_parameter in self.GenericParameters)
				reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

			return reference;
		}

		public static MethodReference ResolveGenericParameters(this MethodReference self, TypeReference declaringTypeRef,
			ModuleDefinition module)
		{
			if (self == null)
				throw new ArgumentNullException("self");
			if (declaringTypeRef == null)
				throw new ArgumentNullException("declaringTypeRef");

			var reference = new MethodReference(self.Name, self.ReturnType)
			{
				DeclaringType = declaringTypeRef,
				HasThis = self.HasThis,
				ExplicitThis = self.ExplicitThis,
				CallingConvention = self.CallingConvention
			};

			foreach (var parameter in self.Parameters)
				reference.Parameters.Add(new ParameterDefinition(module.Import(parameter.ParameterType)));

			foreach (var generic_parameter in self.GenericParameters)
				reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

			return reference;
		}

		public static void ImportTypes(this MethodReference self, ModuleDefinition module)
		{
			if (self.HasParameters)
			{
				for (var i = 0; i < self.Parameters.Count; i++)
					self.Parameters[i].ParameterType = module.Import(self.Parameters[i].ParameterType);
			}
		}
	}
}