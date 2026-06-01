using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Marks a method as callable from JavaScript in a HybridWebView.
	/// When any method in a class has this attribute, the source generator switches to
	/// explicit mode and only exposes attributed methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class HybridWebViewCallableAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the JavaScript-facing method name. If not set, defaults to the C# method name.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Gets or sets a per-method <c>JsonSerializerContext</c> type override. If set, this context
		/// is used for serializing/deserializing this method's parameters and return value instead of
		/// the class-level context specified in <see cref="HybridWebViewDotNetMethodProviderAttribute"/>.
		/// </summary>
		public Type? JsonSerializerContext { get; set; }
	}
}
