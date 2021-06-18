#nullable disable
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Maui
{
	// https://devblogs.microsoft.com/premier-developer/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/
	public static class FastActivator
	{
		public static T CreateInstance<T>() where T : new()
		{
#if NETSTANDARD2_0
			return Activator.CreateInstance<T>();
#else
			return FastActivatorImpl<T>.Create();
#endif

		}

#if !NETSTANDARD2_0
		static readonly ConcurrentDictionary<Type, Func<object>> FactoryDictionary
			= new ConcurrentDictionary<Type, Func<object>>();

		static readonly MethodInfo GenerateValueTypeMethod = typeof(DynamicModuleLambdaCompiler)
			.GetMethod(nameof(DynamicModuleLambdaCompiler.GenerateValueTypeFactory));

		static readonly MethodInfo GenerateTMethod = typeof(DynamicModuleLambdaCompiler)
			.GetMethod(nameof(DynamicModuleLambdaCompiler.GenerateFactory));
#endif

		public static object CreateInstance(Type type)
		{
#if NETSTANDARD2_0
			return Activator.CreateInstance(type);
#else
			return FactoryDictionary.GetOrAdd(type,
					key => (Func<object>)(
						key.IsValueType
							? GenerateValueTypeMethod.Invoke(null, new object[] { key })
							: GenerateTMethod.MakeGenericMethod(key).Invoke(null, null))
				)();
#endif
		}
#if !NETSTANDARD2_0
		static class FastActivatorImpl<T> where T : new()
		{
			public static readonly Func<T> Create =
				DynamicModuleLambdaCompiler.GenerateFactory<T>();
		}
#endif
	}

#if !NETSTANDARD2_0
	public static class FastActivator<T> where T : new()
	{
		public static readonly Func<T> Create =
			DynamicModuleLambdaCompiler.GenerateFactory<T>();
	}

	public static class DynamicModuleLambdaCompiler
	{
		public static Func<T> GenerateFactory<T>() where T : new()
		{
			Expression<Func<T>> expr = () => new T();
			NewExpression newExpr = (NewExpression)expr.Body;

			var method = new DynamicMethod(
				name: "lambda",
				returnType: newExpr.Type,
				parameterTypes: Array.Empty<Type>(),
				m: typeof(DynamicModuleLambdaCompiler).Module,
				skipVisibility: true);

			ILGenerator ilGen = method.GetILGenerator();
			
			if (newExpr.Constructor != null)
			{
				ilGen.Emit(OpCodes.Newobj, newExpr.Constructor);
			}
			else
			{
				LocalBuilder temp = ilGen.DeclareLocal(newExpr.Type);
				ilGen.Emit(OpCodes.Ldloca, temp);
				ilGen.Emit(OpCodes.Initobj, newExpr.Type);
				ilGen.Emit(OpCodes.Ldloc, temp);
			}

			ilGen.Emit(OpCodes.Ret);

			return (Func<T>)method.CreateDelegate(typeof(Func<T>));
		}

		public static Func<object> GenerateValueTypeFactory(Type valueType)
		{
			var method = new DynamicMethod(
				name: "lambda",
				returnType: typeof(object),
				parameterTypes: Array.Empty<Type>(),
				m: typeof(DynamicModuleLambdaCompiler).Module,
				skipVisibility: true);

			ILGenerator ilGen = method.GetILGenerator();

			LocalBuilder temp = ilGen.DeclareLocal(valueType);
			ilGen.Emit(OpCodes.Ldloca, temp);
			ilGen.Emit(OpCodes.Initobj, valueType);
			ilGen.Emit(OpCodes.Ldloc, temp);
			ilGen.Emit(OpCodes.Box, valueType);

			ilGen.Emit(OpCodes.Ret);

			return (Func<object>)method.CreateDelegate(typeof(Func<object>));
		}
	}
#endif
}