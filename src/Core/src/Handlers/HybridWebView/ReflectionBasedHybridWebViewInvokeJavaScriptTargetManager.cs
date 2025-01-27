using System.Threading.Tasks;
using System;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.Maui.Handlers;

[RequiresUnreferencedCode(HybridWebViewHandler.DynamicFeatures)]
#if !NETSTANDARD
[RequiresDynamicCode(HybridWebViewHandler.DynamicFeatures)]
#endif
class ReflectionBasedHybridWebViewInvokeJavaScriptTargetManager : HybridWebViewInvokeJavaScriptTargetManager
{
	private Type _invokeTargetType;

	public ReflectionBasedHybridWebViewInvokeJavaScriptTargetManager(object invokeTarget, Type invokeTargetType)
		: base(invokeTarget)
	{
		_invokeTargetType = invokeTargetType;
	}

	/// <inheritdoc/>
	public override async Task<string?> InvokeDotNetMethodAsync(string methodName, string[]? parameterValues)
	{
		// get the method and its parameters from the .NET object instance
		var dotnetMethod = _invokeTargetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
		if (dotnetMethod is null)
		{
			throw new InvalidOperationException($"The method {methodName} couldn't be found on the JavaScript invoke target of type {InvokeTarget.GetType().FullName}.");
		}
		var dotnetParams = dotnetMethod.GetParameters();
		if (parameterValues is not null && dotnetParams.Length != parameterValues.Length)
		{
			throw new InvalidOperationException($"The number of parameters on the JavaScript invoke target's method {methodName} ({dotnetParams.Length}) doesn't match the number of values passed from JavaScript code ({parameterValues.Length}).");
		}

		// deserialize the parameters from JSON to .NET types
		object?[]? invokeParamValues = null;
		if (parameterValues is not null)
		{
			invokeParamValues = new object?[parameterValues.Length];
			for (var i = 0; i < parameterValues.Length; i++)
			{
				var reqValue = parameterValues[i];
				var paramType = dotnetParams[i].ParameterType;
				var deserialized = JsonSerializer.Deserialize(reqValue, paramType);
				invokeParamValues[i] = deserialized;
			}
		}

		// invoke the .NET method
		var dotnetReturnValue = dotnetMethod.Invoke(InvokeTarget, invokeParamValues);

		// void result, so return null
		if (dotnetMethod.ReturnType == typeof(void))
		{
			return null;
		}

		// Task or Task<T> result
		if (dotnetReturnValue is Task task)
		{
			await task;

			if (dotnetMethod.ReturnType.IsGenericType)
			{
				// Task<T>
				var resultProperty = dotnetMethod.ReturnType.GetProperty(nameof(Task<object>.Result));
				dotnetReturnValue = resultProperty?.GetValue(task);
			}
			else
			{
				// Task
				dotnetReturnValue = null;
			}
		}

		return JsonSerializer.Serialize(dotnetReturnValue);
	}
}
