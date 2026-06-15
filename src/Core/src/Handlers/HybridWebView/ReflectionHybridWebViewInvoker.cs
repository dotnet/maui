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
	internal sealed class ReflectionHybridWebViewInvoker : IHybridWebViewInvoker
	{
		private readonly object _target;
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		private readonly Type _targetType;

		public ReflectionHybridWebViewInvoker(object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
		{
			_target = target;
			_targetType = targetType;
		}

		[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "Legacy reflection dispatch preserves the target type with DAM; use the JsonSerializerContext overload for the NativeAOT-safe path.")]
		[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Legacy reflection dispatch may use runtime JSON metadata; use the JsonSerializerContext overload for the NativeAOT-safe path.")]
		[UnconditionalSuppressMessage("Trimming", "IL2075:DynamicallyAccessedMembers", Justification = "The legacy overload preserves the target type and its public members with DynamicallyAccessedMembers.")]
		public async Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues)
		{
			var method = _targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
				?? throw new InvalidOperationException($"The method '{methodName}' was not found on type '{_targetType.FullName}'.");

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
				returnValue = method.Invoke(_target, args);
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
