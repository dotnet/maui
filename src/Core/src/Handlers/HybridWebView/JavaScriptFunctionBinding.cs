using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Maui.Handlers;

#pragma warning disable RS0016 // Add public types and members to the declared API

public static class JavaScriptFunctionBinding
{
	static readonly Dictionary<FunctionSignature, ManagedFunctionBinding> _bindings = new();

	public static void BindManagedFunction<T>(Action<T, IInvokeRequestArgs> method, string methodName, JsonTypeInfo? returnInfo, JsonTypeInfo?[]? parameterInfos)
		where T : notnull
	{
		_bindings.Add(
			new(typeof(T), methodName, parameterInfos?.Length ?? 0),
			new((o, a) => method((T)o, a), methodName, returnInfo, parameterInfos));
	}

	public static string? InvokeManagedFunction<T>(T instance, string methodName, string[]? paramValues)
		where T : notnull
	{
		var key = new FunctionSignature(typeof(T), methodName, paramValues?.Length ?? 0);

		if (!_bindings.TryGetValue(key, out var binding))
			throw new MissingMethodException();

		var args = new InvokeRequestArgs(paramValues ?? []);

		var t = instance;

		binding.Method.Invoke(instance, args);

		return args.ReturnValue;
	}

	record class ManagedFunctionBinding(
		Action<object, IInvokeRequestArgs> Method,
		string MethodName,
		JsonTypeInfo? ReturnTypeInfo,
		JsonTypeInfo?[]? ParameterTypeInfos);

	record struct FunctionSignature(
		Type Type,
		string MethodName,
		int ParameterCount);

	class InvokeRequestArgs : IInvokeRequestArgs
	{
		public string? ReturnValue { get; private set; }

		public string[] ParameterValues { get; }

		public InvokeRequestArgs(string[] parameterValues)
		{
			ParameterValues = parameterValues ?? [];
		}

		public T? GetArgument<T>(int index, JsonTypeInfo<T> info)
		{
			return JsonSerializer.Deserialize<T>(ParameterValues[index], info);
		}

		public void SetReturn(object value, JsonTypeInfo info)
		{
			ReturnValue = JsonSerializer.Serialize(value, info);
		}

		public void SetException(Exception exception)
		{
			throw new NotImplementedException();
		}
	}
}
