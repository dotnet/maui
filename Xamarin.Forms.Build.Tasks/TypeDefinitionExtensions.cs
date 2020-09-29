using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;

namespace Xamarin.Forms.Build.Tasks
{
	static class TypeDefinitionExtensions
	{
		public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType)
		{
			var module = targetType.Module;
			var parentType = module.ImportReference(("mscorlib", "System", "Object"));

			return AddDefaultConstructor(targetType, parentType);
		}

		public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType, TypeReference parentType)
		{
			var module = targetType.Module;
			var voidType = module.ImportReference(("mscorlib", "System", "Void"));
			var methodAttributes = MethodAttributes.Public |
								   MethodAttributes.HideBySig |
								   MethodAttributes.SpecialName |
								   MethodAttributes.RTSpecialName;

			var parentctor = module.ImportCtorReference(parentType, paramCount: 0) ?? module.ImportCtorReference(("mscorlib", "System", "Object"), parameterTypes: null);

			var ctor = new MethodDefinition(".ctor", methodAttributes, voidType)
			{
				CallingConvention = MethodCallingConvention.Default,
				ImplAttributes = (MethodImplAttributes.IL | MethodImplAttributes.Managed)
			};
			ctor.Body.InitLocals = true;

			var IL = ctor.Body.GetILProcessor();

			IL.Emit(OpCodes.Ldarg_0);
			IL.Emit(OpCodes.Call, parentctor);
			IL.Emit(OpCodes.Ret);

			targetType.Methods.Add(ctor);
			return ctor;
		}

		public static IEnumerable<(MethodDefinition methodDef, TypeReference declTypeRef)> AllMethods(this TypeDefinition self)
		{
			TypeReference selfTypeRef = self;
			while (self != null)
			{
				foreach (var md in self.Methods)
					yield return (md, selfTypeRef);
				selfTypeRef = self.BaseType;
				self = self.BaseType?.ResolveCached();
			}
		}
	}
}