using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public class HybridWebViewInvokeJavaScriptRequest(string methodName, JsonTypeInfo? returnTypeJsonTypeInfo, object?[]? paramValues, JsonTypeInfo?[]? paramJsonTypeInfos)
		: TaskCompletionSource<object?>
	{
		public string MethodName { get; } = methodName;
		public JsonTypeInfo? ReturnTypeJsonTypeInfo { get; } = returnTypeJsonTypeInfo;
		public object?[]? ParamValues { get; } = paramValues;
		public JsonTypeInfo?[]? ParamJsonTypeInfos { get; } = paramJsonTypeInfos;
	}
}
