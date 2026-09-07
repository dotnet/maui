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
	/// into a static class annotated with <c>System.Runtime.CompilerServices.ExtensionAttribute</c> that
	/// declares <c>public static string get_MyTag(Label)</c> and <c>public static void set_MyTag(Label, string)</c>
	/// implementation methods, plus unspeakable nested "grouping" types holding the skeleton members.
	/// The inflators only ever look at the implementation methods on the container itself: the synthesized
	/// <c>ExtensionAttribute</c> is not observable through Roslyn's symbol model, so the accessor shape is the
	/// only signal all three inflators can agree on. The container is always named explicitly in the markup,
	/// so no type is ever matched by accident.
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
	/// <item><description>the accessors are non-generic <c>public static</c> methods declared on the container;</description></item>
	/// <item><description>the getter takes the receiver only, the setter takes the receiver and the value;</description></item>
	/// <item><description>the receiver parameter is not by-ref and is assignable from the target element type;</description></item>
	/// <item><description>the container is accessible from the assembly declaring the XAML;</description></item>
	/// <item><description>when several accessors apply, exactly one has a receiver type more derived than all the others.</description></item>
	/// </list>
	/// <para>
	/// Only assignment is supported. Reading an extension property (to add items to the collection it returns,
	/// for instance) is not, so that the three inflators keep accepting exactly the same markup. Generic
	/// extension blocks (<c>extension&lt;T&gt;(ICollection&lt;T&gt;)</c>) and static extension properties are
	/// rejected with a diagnostic rather than silently ignored.
	/// </para>
	/// <para>
	/// XamlC and SourceGen resolve everything at build time and emit a direct call to the setter; they are the
	/// trimming and AOT safe paths. The runtime inflator resolves the accessors reflectively and therefore
	/// inherits the existing trimming limitations of runtime XAML inflation.
	/// </para>
	/// </remarks>
	static class ExtensionPropertyConventions
	{
		public static string GetterName(string propertyName) => "get_" + propertyName;

		public static string SetterName(string propertyName) => "set_" + propertyName;
	}
}
