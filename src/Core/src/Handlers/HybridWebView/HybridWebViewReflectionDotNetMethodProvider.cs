using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// A reflection-based implementation of <see cref="IHybridWebViewDotNetMethodProvider"/>
	/// that wraps the legacy <c>SetInvokeJavaScriptTarget&lt;T&gt;</c> behavior.
	/// This class isolates all reflection code for backward compatibility.
	/// </summary>
	[RequiresUnreferencedCode(HybridWebViewHandler.DynamicFeatures)]
#if !NETSTANDARD
	[RequiresDynamicCode(HybridWebViewHandler.DynamicFeatures)]
#endif
	internal class HybridWebViewReflectionDotNetMethodProvider : IHybridWebViewDotNetMethodProvider
	{
		private readonly object _target;

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		private readonly Type _targetType;

		public HybridWebViewReflectionDotNetMethodProvider(
			object target,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
		{
			_target = target ?? throw new ArgumentNullException(nameof(target));
			_targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
		}

		public async Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues)
		{
			var result = await InvokeDotNetMethodAsync(_targetType, _target, methodName, paramJsonValues);
			return SerializeResult(result);
		}

		private static async Task<object?> InvokeDotNetMethodAsync(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType,
			object target,
			string methodName,
			string[]? paramValues)
		{
			var dotnetMethod = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
			if (dotnetMethod is null)
			{
				throw new InvalidOperationException($"The method {methodName} couldn't be found on the target of type {target.GetType().FullName}.");
			}

			var dotnetParams = dotnetMethod.GetParameters();
			if (paramValues is not null && dotnetParams.Length != paramValues.Length)
			{
				throw new InvalidOperationException($"The number of parameters on method {methodName} ({dotnetParams.Length}) doesn't match the number of values passed from JavaScript code ({paramValues.Length}).");
			}

			object?[]? invokeParamValues = null;
			if (paramValues is not null)
			{
				invokeParamValues = new object?[paramValues.Length];
				for (var i = 0; i < paramValues.Length; i++)
				{
					var reqValue = paramValues[i];
					var paramType = dotnetParams[i].ParameterType;
					var deserialized = JsonSerializer.Deserialize(reqValue, paramType);
					invokeParamValues[i] = deserialized;
				}
			}

			var dotnetReturnValue = InvokeMethod(target, dotnetMethod, invokeParamValues);

			if (dotnetReturnValue is null)
			{
				return null;
			}

			if (dotnetReturnValue is Task task)
			{
				await task;

				if (dotnetMethod.ReturnType.IsGenericType)
				{
					var resultProperty = dotnetMethod.ReturnType.GetProperty(nameof(Task<object>.Result));
					return resultProperty?.GetValue(task);
				}

				return null;
			}

			return dotnetReturnValue;
		}

		private static object? InvokeMethod(object target, MethodInfo method, object?[]? paramValues)
		{
			try
			{
				return method.Invoke(target, paramValues);
			}
			catch (TargetInvocationException tie)
			{
				if (tie.InnerException is not null)
				{
					ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
					throw; // unreachable
				}
				throw;
			}
		}

		private static string? SerializeResult(object? result)
		{
			if (result is null)
			{
				return null;
			}

			return JsonSerializer.Serialize(result);
		}
	}
}
