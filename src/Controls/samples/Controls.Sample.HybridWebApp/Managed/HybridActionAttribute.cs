namespace Maui.HybridApp;

/// <summary>Exposes an accessible method through an explicitly opted-in hybrid action provider.</summary>
/// <param name="name">The unique, bounded native wire-command name.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class HybridActionAttribute(string name) : Attribute
{
	/// <summary>Gets the native wire-command name.</summary>
	public string Name { get; } = name;
	/// <summary>Gets or sets whether the generated handler dispatches once to the host's native thread.</summary>
	public bool MainThread { get; set; }
	/// <summary>Gets or sets whether the action may await OS interaction for the existing five-minute budget.</summary>
	public bool Interactive { get; set; }
}

/// <summary>Resolves a method parameter from the current owning service scope instead of JSON arguments.</summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class FromServicesAttribute : Attribute;
