using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Marks a <c>partial</c> class for HybridWebView source generation. The source generator
	/// implements <see cref="IHybridWebViewDotNetMethodProvider"/> on the class, creating an
	/// AOT-friendly dispatch method that routes JavaScript calls to the appropriate C# methods
	/// using <c>JsonTypeInfo&lt;T&gt;</c> for serialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class HybridWebViewDotNetMethodProviderAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HybridWebViewDotNetMethodProviderAttribute"/> class.
		/// </summary>
		/// <param name="jsonSerializerContextType">
		/// The type of the <c>JsonSerializerContext</c> to use for serialization and deserialization
		/// of method parameters and return values. The context must have <c>[JsonSerializable]</c>
		/// entries for all types used in callable method signatures.
		/// </param>
		public HybridWebViewDotNetMethodProviderAttribute(Type jsonSerializerContextType)
		{
			JsonSerializerContextType = jsonSerializerContextType;
		}

		/// <summary>
		/// Gets the type of the <c>JsonSerializerContext</c> used for serialization.
		/// </summary>
		public Type JsonSerializerContextType { get; }

		/// <summary>
		/// When <c>true</c>, all public instance methods are exposed to JavaScript
		/// (matches the legacy <c>SetInvokeJavaScriptTarget&lt;T&gt;</c> behavior, convenient for migration).
		/// When <c>false</c> (default), only methods decorated with <see cref="HybridWebViewCallableAttribute"/>
		/// are exposed.
		/// </summary>
		public bool ExposeAllPublicMethods { get; set; }
	}
}
