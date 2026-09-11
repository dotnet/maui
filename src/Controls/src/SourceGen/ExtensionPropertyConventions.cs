namespace Microsoft.Maui.Controls.SourceGen
{
	/// <summary>
	/// Describes how the MAUI XAML source generator recognizes C# extension properties. The source generator
	/// is the only inflator that supports them: it resolves everything at compile time, against the very
	/// namespace map type names resolve through, and emits a direct call to the setter.
	/// </summary>
	/// <remarks>
	/// <para>The C# compiler lowers</para>
	/// <code>
	/// public static class LabelExtensions
	/// {
	///     extension(Label label) { public string MyTag { get; set; } }
	/// }
	/// </code>
	/// <para>
	/// into a static class that declares <c>public static string get_MyTag(Label)</c> and
	/// <c>public static void set_MyTag(Label, string)</c> implementation methods, plus a nested, unspeakable
	/// "extension declaration" type that declares the extension property itself. The inflators call the
	/// implementation method, and use the nested declaration as the marker that the container really is an
	/// extension container.
	/// </para>
	/// <para>Both forms are supported in XAML:</para>
	/// <code>
	/// &lt;Label MyTag="Hello" /&gt;                          &lt;!-- resolved in the element's default xmlns --&gt;
	/// &lt;Label local:LabelExtensions.MyTag="Hello" /&gt;     &lt;!-- container named explicitly --&gt;
	/// </code>
	/// <para>
	/// An unprefixed attribute carries no xml namespace, so an unqualified name is resolved in the default
	/// xmlns declared for the element, which is what XAML uses as a scope, the way a C# file scopes extension
	/// member lookup with its <c>using</c> directives. That scope is the very map type names are resolved
	/// through: <c>[XmlnsDefinition]</c>, including everything the MAUI global xmlns pulls in for a document
	/// that declares no xmlns at all. Candidates are therefore never searched for outside the namespaces and
	/// assemblies the document already imports, and the map is cached per xmlns.
	/// </para>
	/// <para>
	/// Naming the container explicitly stays available, to reach a container the document does not import and
	/// to disambiguate. It is also the only form for which a name the container cannot resolve is an error
	/// rather than a fallback, since naming a container means asking for one of its extension properties.
	/// </para>
	/// <para>Requirements for a container/accessor to be usable from XAML:</para>
	/// <list type="bullet">
	/// <item><description>the container is a non-generic static class;</description></item>
	/// <item><description>the container declares an extension property with that name (see <see cref="IsSpeakable"/>);</description></item>
	/// <item><description>the setter is a non-generic <c>public static void set_Name(TReceiver, TValue)</c> method on the container;</description></item>
	/// <item><description>the receiver parameter is not by-ref and is assignable from the target element type;</description></item>
	/// <item><description>the container is accessible from the assembly declaring the XAML;</description></item>
	/// <item><description>when several setters apply, whether on one container or across the containers a xmlns
	/// brings in scope, exactly one has a receiver type more derived than all the others.</description></item>
	/// </list>
	/// <para>
	/// Ordinary members win: an unqualified name is only looked up among extension properties once events,
	/// dynamic resources, bindings, bindable properties and the target's own properties have all been tried.
	/// </para>
	/// <para>
	/// Only assignment is supported, so the selected <c>set_Name</c> method alone defines the receiver type and
	/// the value type. Getters are never consulted: an extension property may be declared with its getter and
	/// its setter in different extension blocks, with different receivers and even different value types, and
	/// merging those into a single property shape would produce a shape no accessor actually has.
	/// Reading an extension property (to add items to the collection it returns, for instance) is not supported
	/// either, so that the three inflators keep accepting exactly the same markup. Generic extension blocks
	/// (<c>extension&lt;T&gt;(ICollection&lt;T&gt;)</c>) and static extension properties have no applicable
	/// <c>set_Name(TReceiver, TValue)</c> method and are reported, not silently ignored.
	/// </para>
	/// <para>
	/// Neither XamlC nor the runtime inflator resolve extension properties. Both would have to discover the
	/// containers a xmlns brings in scope by enumerating the types of the mapped assemblies, which the source
	/// generator gets for free from the namespace symbols of the compilation. XAML using an extension property
	/// therefore has to be inflated by the source generator, and fails to resolve the member otherwise.
	/// </para>
	/// </remarks>
	static class ExtensionPropertyConventions
	{
		public static string SetterName(string propertyName) => "set_" + propertyName;

		/// <summary>
		/// Tells whether a type name can be written in C#. The compiler names the nested extension declaration
		/// types it generates with characters no C# identifier may contain, and those types are the marker the
		/// inflators use to accept a container.
		/// </summary>
		/// <remarks>
		/// The exact names are an implementation detail and differ between compiler versions, so they are never
		/// matched; only their unspeakability is. <c>ExtensionAttribute</c> cannot be used instead: Roslyn does
		/// not surface it through <c>ISymbol.GetAttributes()</c>, neither for source nor for metadata symbols,
		/// and <c>INamedTypeSymbol.IsExtension</c> is newer than the Microsoft.CodeAnalysis version the MAUI
		/// source generator is built against.
		/// </remarks>
		public static bool IsSpeakable(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
				return false;

			foreach (var c in typeName)
			{
				//generic arity (`1) and nesting separators are part of a speakable metadata name
				if (!char.IsLetterOrDigit(c) && c != '_' && c != '.' && c != '`')
					return false;
			}

			return true;
		}
	}
}
