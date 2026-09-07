namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Single source of truth describing how the three XAML inflators (runtime, XamlC and SourceGen)
	/// recognize C# extension properties. The file is linked into Controls.Build.Tasks and
	/// Controls.SourceGen so the three implementations cannot drift apart.
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
	/// <para>Supported in XAML, using the qualified (attached-property-like) syntax only:</para>
	/// <code>&lt;Label local:LabelExtensions.MyTag="Hello" /&gt;</code>
	/// <para>
	/// The container type is always named explicitly, so resolution never depends on which assemblies
	/// happen to be loaded, referenced or compiled, and is therefore identical for all three inflators.
	/// Unqualified names (<c>&lt;Label MyTag="Hello" /&gt;</c>) are deliberately not supported: XAML has no
	/// equivalent of a C# <c>using</c> directive to scope extension member lookup, so an unqualified lookup
	/// would have to search every type of every assembly and could not produce a stable answer.
	/// </para>
	/// <para>Requirements for a container/accessor to be usable from XAML:</para>
	/// <list type="bullet">
	/// <item><description>the container is a non-generic static class;</description></item>
	/// <item><description>the container declares an extension property with that name (see <see cref="IsSpeakable"/>);</description></item>
	/// <item><description>the setter is a non-generic <c>public static void set_Name(TReceiver, TValue)</c> method on the container;</description></item>
	/// <item><description>the receiver parameter is not by-ref and is assignable from the target element type;</description></item>
	/// <item><description>the container is accessible from the assembly declaring the XAML;</description></item>
	/// <item><description>when several setters apply, exactly one has a receiver type more derived than all the others.</description></item>
	/// </list>
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
	/// XamlC and SourceGen resolve everything at build time and emit a direct call to the setter; they are the
	/// trimming and AOT safe paths. The runtime inflator resolves the setter reflectively and therefore
	/// inherits the existing trimming limitations of runtime XAML inflation.
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

		public static string ResolutionError(string propertyName, string containerName, string targetTypeName)
			=> $"Cannot resolve the extension property \"{propertyName}\" on \"{containerName}\". The extension property must be a non-generic instance extension property, declared in an accessible non-generic static extension container, with a setter whose receiver type matches \"{targetTypeName}\", and it must be unambiguous.";
	}
}
