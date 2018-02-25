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
			var parentType = typeof (object);

			return AddDefaultConstructor(targetType, parentType);
		}

		public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType, Type parentType)
		{
			var module = targetType.Module;
			var voidType = module.ImportReferenceCached(typeof (void));
			var methodAttributes = MethodAttributes.Public |
			                       MethodAttributes.HideBySig |
			                       MethodAttributes.SpecialName |
			                       MethodAttributes.RTSpecialName;

			var flags = BindingFlags.Public |
			            BindingFlags.NonPublic |
			            BindingFlags.Instance;

			var objectConstructor = parentType.GetConstructor(flags, null, new Type[0], null);

			if (objectConstructor == null)
				objectConstructor = typeof (object).GetConstructor(new Type[0]);

			var baseConstructor = module.ImportReferenceCached(objectConstructor);

			var ctor = new MethodDefinition(".ctor", methodAttributes, voidType)
			{
				CallingConvention = MethodCallingConvention.Default,
				ImplAttributes = (MethodImplAttributes.IL | MethodImplAttributes.Managed)
			};
			ctor.Body.InitLocals = true;

			var IL = ctor.Body.GetILProcessor();

			IL.Emit(OpCodes.Ldarg_0);
			IL.Emit(OpCodes.Call, baseConstructor);
			IL.Emit(OpCodes.Ret);

			targetType.Methods.Add(ctor);
			return ctor;
		}

		public static IEnumerable<MethodDefinition> AllMethods(this TypeDefinition self)
		{
			while (self != null)
			{
				foreach (var md in self.Methods)
					yield return md;
				self = self.BaseType == null ? null : self.BaseType.ResolveCached();
			}
		}
	}
}