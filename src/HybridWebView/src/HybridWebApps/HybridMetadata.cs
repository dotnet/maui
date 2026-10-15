namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Compiler-emitted wire-name manifest for explicitly registered assemblies.</summary>
/// <param name="name">An action wire name.</param>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class HybridActionAssemblyAttribute(string name) : Attribute
{
	/// <summary>Gets the action name.</summary>
	public string Name { get; } = name;
}

/// <summary>Uses an explicitly source-declared strict STJ context for a complex action contract.</summary>
/// <param name="contract">The JSON contract.</param>
/// <param name="context">A source-declared JsonSerializerContext with this contract rooted.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class HybridJsonContextAttribute(Type contract, Type context) : Attribute
{
	/// <summary>Gets the JSON contract.</summary>
	public Type Contract { get; } = contract;
	/// <summary>Gets its source-declared serializer context.</summary>
	public Type Context { get; } = context;
}
