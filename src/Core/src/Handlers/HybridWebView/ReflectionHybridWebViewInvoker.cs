using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Reflection-based invoker for the legacy SetInvokeJavaScriptTarget overload. Use the JsonSerializerContext overload for NativeAOT-safe dispatch.
	/// </summary>
	[RequiresUnreferencedCode("Use SetInvokeJavaScriptTarget<T>(T target, JsonSerializerContext jsonSerializerContext) for trimming and NativeAOT compatibility.")]
#if !NETSTANDARD
	[RequiresDynamicCode("Use SetInvokeJavaScriptTarget<T>(T target, JsonSerializerContext jsonSerializerContext) for trimming and NativeAOT compatibility.")]
#endif
	internal sealed class ReflectionHybridWebViewInvoker : IHybridWebViewInvoker
	{
		public ReflectionHybridWebViewInvoker(object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
			: base(target, targetType)
		{
		}

		[UnconditionalSuppressMessage("Trimming", "IL2075:DynamicallyAccessedMembers", Justification = "The legacy overload preserves the target type and its public members with DynamicallyAccessedMembers.")]
		public override async Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues)
		{
			var target = InvokeJavaScriptTarget
				?? throw new InvalidOperationException($"The {nameof(IHybridWebView)}.{nameof(IHybridWebView.InvokeJavaScriptTarget)} property must have a value in order to invoke a .NET method from JavaScript.");
			var targetType = InvokeJavaScriptType
				?? throw new InvalidOperationException($"The {nameof(IHybridWebView)}.{nameof(IHybridWebView.InvokeJavaScriptType)} property must have a value in order to invoke a .NET method from JavaScript.");
			var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
				?? throw new InvalidOperationException($"The method '{methodName}' was not found on type '{targetType.FullName}'.");

			var parameters = method.GetParameters();
			if (paramJsonValues is not null && parameters.Length != paramJsonValues.Length)
			{
				throw new InvalidOperationException($"Method '{methodName}' expects {parameters.Length} parameter(s) but {paramJsonValues.Length} were provided.");
			}

			object?[]? args = null;
			if (paramJsonValues is not null)
			{
				args = new object?[paramJsonValues.Length];
				for (var i = 0; i < paramJsonValues.Length; i++)
				{
					args[i] = JsonSerializer.Deserialize(paramJsonValues[i], parameters[i].ParameterType);
				}
			}

			object? returnValue;
			try
			{
				returnValue = method.Invoke(target, args);
			}
			catch (TargetInvocationException tie) when (tie.InnerException is not null)
			{
				ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
				throw;
			}

			if (returnValue is null)
			{
				return null;
			}

			if (returnValue is Task task)
			{
				await task;

				var returnType = method.ReturnType;
				if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				{
					var resultProperty = returnType.GetProperty(nameof(Task<object>.Result));
					var result = resultProperty?.GetValue(task);
					return result is null ? null : JsonSerializer.Serialize(result);
				}

				return null;
			}

			return JsonSerializer.Serialize(returnValue);
		}
	}
}
