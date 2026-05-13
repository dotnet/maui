using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Reflection-based invoker for the legacy SetInvokeJavaScriptTarget overload.
	/// </summary>
	[RequiresUnreferencedCode("Uses reflection and dynamic JSON serialization.")]
#if !NETSTANDARD
	[RequiresDynamicCode("Uses reflection and dynamic JSON serialization.")]
#endif
	internal sealed class ReflectionHybridWebViewInvoker : IHybridWebViewInvoker
	{
		private readonly object _target;
		private readonly Type _targetType;

		public ReflectionHybridWebViewInvoker(object target, Type targetType)
		{
			_target = target;
			_targetType = targetType;
		}

		public async Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues)
		{
			var method = _targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
				?? throw new InvalidOperationException($"The method '{methodName}' was not found on type '{_targetType.FullName}'.");

			var parameters = method.GetParameters();
			if (paramJsonValues is not null && parameters.Length != paramJsonValues.Length)
			{
				throw new InvalidOperationException($"Method '{methodName}' expects {parameters.Length} parameter(s) but {paramJsonValues.Length} were provided.");
			}

			// Deserialize parameters
			object?[]? args = null;
			if (paramJsonValues is not null)
			{
				args = new object?[paramJsonValues.Length];
				for (var i = 0; i < paramJsonValues.Length; i++)
				{
					args[i] = JsonSerializer.Deserialize(paramJsonValues[i], parameters[i].ParameterType);
				}
			}

			// Invoke
			object? returnValue;
			try
			{
				returnValue = method.Invoke(_target, args);
			}
			catch (TargetInvocationException tie) when (tie.InnerException is not null)
			{
				ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
				throw; // unreachable
			}

			if (returnValue is null)
			{
				return null;
			}

			// Handle Task / Task<T>
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

			// Synchronous return
			return JsonSerializer.Serialize(returnValue);
		}
	}
}
