using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public class HybridWebViewInvokeJavaScriptRequest(string methodName, object?[]? paramValues, JsonTypeInfo?[]? paramJsonTypeInfos)
		: TaskCompletionSource<string>
	{
		public string MethodName { get; } = methodName;
		public object?[]? ParamValues { get; } = paramValues;
		public JsonTypeInfo?[]? ParamJsonTypeInfos { get; } = paramJsonTypeInfos;
	}
}
