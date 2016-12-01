using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Xamarin.Forms.Build.Tasks
{
	static class TypeReferenceExtensions
	{
		public static PropertyDefinition GetProperty(this TypeReference typeRef, Func<PropertyDefinition, bool> predicate,
			out TypeReference declaringTypeRef)
		{
			declaringTypeRef = typeRef;
			var typeDef = typeRef.Resolve();
			var properties = typeDef.Properties.Where(predicate);
			if (properties.Any())
				return properties.Single();
			if (typeDef.BaseType == null || typeDef.BaseType.FullName == "System.Object")
				return null;
			return typeDef.BaseType.GetProperty(predicate, out declaringTypeRef);
		}

		public static EventDefinition GetEvent(this TypeReference typeRef, Func<EventDefinition, bool> predicate)
		{
			var typeDef = typeRef.Resolve();
			var events = typeDef.Events.Where(predicate);
			if (events.Any())
				return events.Single();
			if (typeDef.BaseType == null || typeDef.BaseType.FullName == "System.Object")
				return null;
			return typeDef.BaseType.GetEvent(predicate);
		}

		public static FieldDefinition GetField(this TypeReference typeRef, Func<FieldDefinition, bool> predicate,
			out TypeReference declaringTypeRef)
		{
			declaringTypeRef = typeRef;
			var typeDef = typeRef.Resolve();
			var bp = typeDef.Fields.Where
				(predicate);
			if (bp.Any())
				return bp.Single();
			if (typeDef.BaseType == null || typeDef.BaseType.FullName == "System.Object")
				return null;
			var basetype = typeDef.BaseType.ResolveGenericParameters(typeRef);
			return basetype.GetField(predicate, out declaringTypeRef);
		}

		public static bool ImplementsInterface(this TypeReference typeRef, TypeReference @interface)
		{
			var typeDef = typeRef.Resolve();
			if (typeDef.Interfaces.Any(tr => tr.FullName == @interface.FullName))
				return true;
			var baseTypeRef = typeDef.BaseType;
			if (baseTypeRef != null && baseTypeRef.FullName != "System.Object")
				return baseTypeRef.ImplementsInterface(@interface);
			return false;
		}

		public static bool ImplementsGenericInterface(this TypeReference typeRef, string @interface,
			out GenericInstanceType interfaceReference, out IList<TypeReference> genericArguments)
		{
			interfaceReference = null;
			genericArguments = null;
			var typeDef = typeRef.Resolve();
			TypeReference iface;
			if (
				(iface =
					typeDef.Interfaces.FirstOrDefault(
						tr =>
							tr.FullName.StartsWith(@interface) && tr.IsGenericInstance && (tr as GenericInstanceType).HasGenericArguments)) !=
				null)
			{
				interfaceReference = iface as GenericInstanceType;
				genericArguments = (iface as GenericInstanceType).GenericArguments;
				return true;
			}
			var baseTypeRef = typeDef.BaseType;
			if (baseTypeRef != null && baseTypeRef.FullName != "System.Object")
				return baseTypeRef.ImplementsGenericInterface(@interface, out interfaceReference, out genericArguments);
			return false;
		}

		public static bool InheritsFromOrImplements(this TypeReference typeRef, TypeReference baseClass)
		{
			if (typeRef.FullName == baseClass.FullName)
				return true;

			var arrayInterfaces = new[]
			{
				"System.IEnumerable",
				"System.Collections.IList",
				"System.Collections.Collection"
			};

			var arrayGenericInterfaces = new[]
			{
				"System.IEnumerable`1",
				"System.Collections.Generic.IList`1",
				"System.Collections.Generic.IReadOnlyCollection<T>",
				"System.Collections.Generic.IReadOnlyList<T>",
				"System.Collections.Generic.Collection<T>"
			};

			if (typeRef.IsArray)
			{
				var arrayType = typeRef.Resolve();
				if (arrayInterfaces.Contains(baseClass.FullName))
					return true;
				if (arrayGenericInterfaces.Contains(baseClass.Resolve().FullName) &&
				    baseClass.IsGenericInstance &&
				    (baseClass as GenericInstanceType).GenericArguments[0].FullName == arrayType.FullName)
					return true;
			}
			var typeDef = typeRef.Resolve();
			if (typeDef.FullName == baseClass.FullName)
				return true;
			if (typeDef.Interfaces.Any(ir => ir.FullName == baseClass.FullName))
				return true;
			if (typeDef.FullName == "System.Object")
				return false;
			if (typeDef.BaseType == null)
				return false;
			return typeDef.BaseType.InheritsFromOrImplements(baseClass);
		}

		public static CustomAttribute GetCustomAttribute(this TypeReference typeRef, TypeReference attribute)
		{
			var typeDef = typeRef.Resolve();
			//FIXME: avoid string comparison. make sure the attribute TypeRef is the same one
			var attr = typeDef.CustomAttributes.SingleOrDefault(ca => ca.AttributeType.FullName == attribute.FullName);
			if (attr != null)
				return attr;
			var baseTypeRef = typeDef.BaseType;
			if (baseTypeRef != null && baseTypeRef.FullName != "System.Object")
				return baseTypeRef.GetCustomAttribute(attribute);
			return null;
		}

		[Obsolete]
		public static MethodDefinition GetMethod(this TypeReference typeRef, Func<MethodDefinition, bool> predicate)
		{
			TypeReference declaringTypeReference;
			return typeRef.GetMethod(predicate, out declaringTypeReference);
		}

		[Obsolete]
		public static MethodDefinition GetMethod(this TypeReference typeRef, Func<MethodDefinition, bool> predicate,
			out TypeReference declaringTypeRef)
		{
			declaringTypeRef = typeRef;
			var typeDef = typeRef.Resolve();
			var methods = typeDef.Methods.Where(predicate);
			if (methods.Any())
				return methods.Single();
			if (typeDef.BaseType != null && typeDef.BaseType.FullName == "System.Object")
				return null;
			if (typeDef.IsInterface)
			{
				foreach (var face in typeDef.Interfaces)
				{
					var m = face.GetMethod(predicate);
					if (m != null)
						return m;
				}
				return null;
			}
			return typeDef.BaseType.GetMethod(predicate, out declaringTypeRef);
		}

		public static IEnumerable<Tuple<MethodDefinition, TypeReference>> GetMethods(this TypeReference typeRef,
			Func<MethodDefinition, bool> predicate, ModuleDefinition module)
		{
			return typeRef.GetMethods((md, tr) => predicate(md), module);
		}

		public static IEnumerable<Tuple<MethodDefinition, TypeReference>> GetMethods(this TypeReference typeRef,
			Func<MethodDefinition, TypeReference, bool> predicate, ModuleDefinition module)
		{
			var typeDef = typeRef.Resolve();
			foreach (var method in typeDef.Methods.Where(md => predicate(md, typeRef)))
				yield return new Tuple<MethodDefinition, TypeReference>(method, typeRef);
			if (typeDef.IsInterface)
			{
				foreach (var face in typeDef.Interfaces)
				{
					if (face.IsGenericInstance && typeRef is GenericInstanceType)
					{
						int i = 0;
						foreach (var arg in ((GenericInstanceType)typeRef).GenericArguments)
							((GenericInstanceType)face).GenericArguments[i++] = module.Import(arg);
					}
					foreach (var tuple in face.GetMethods(predicate, module))
						yield return tuple;
				}
				yield break;
			}
			if (typeDef.BaseType == null || typeDef.BaseType.FullName == "System.Object")
				yield break;
			var baseType = typeDef.BaseType.ResolveGenericParameters(typeRef);
			foreach (var tuple in baseType.GetMethods(predicate, module))
				yield return tuple;
		}

		public static MethodReference GetImplicitOperatorTo(this TypeReference fromType, TypeReference toType, ModuleDefinition module)
		{
			var implicitOperatorsOnFromType = fromType.GetMethods(md => md.IsPublic && md.IsStatic && md.IsSpecialName && md.Name == "op_Implicit", module);
			var implicitOperatorsOnToType = toType.GetMethods(md => md.IsPublic && md.IsStatic && md.IsSpecialName && md.Name == "op_Implicit", module);
			var implicitOperators = implicitOperatorsOnFromType.Concat(implicitOperatorsOnToType).ToList();
			if (implicitOperators.Any()) {
				foreach (var op in implicitOperators) {
					var cast = op.Item1;
					var opDeclTypeRef = op.Item2;
					var castDef = module.Import(cast).ResolveGenericParameters(opDeclTypeRef, module);
					var returnType = castDef.ReturnType;
					if (returnType.IsGenericParameter)
						returnType = ((GenericInstanceType)opDeclTypeRef).GenericArguments [((GenericParameter)returnType).Position];
					if (returnType.FullName == toType.FullName &&
					    cast.Parameters [0].ParameterType.Name == fromType.Name) {
						return castDef;
					}
				}
			}
			return null;
		}

		public static TypeReference ResolveGenericParameters(this TypeReference self, TypeReference declaringTypeReference)
		{
			var genericself = self as GenericInstanceType;
			if (genericself == null)
				return self;

			var genericdeclType = declaringTypeReference as GenericInstanceType;
			if (genericdeclType == null)
				return self;

			if (!genericself.GenericArguments.Any(arg => arg.IsGenericParameter))
				return self;

			List<TypeReference> args = new List<TypeReference>();
			for (var i = 0; i < genericself.GenericArguments.Count; i++)
				args.Add(genericdeclType.GenericArguments[(genericself.GenericArguments[i] as GenericParameter).Position]);
			return self.GetElementType().MakeGenericInstanceType(args.ToArray());
		}
	}
}