#nullable enable
using System;

namespace Microsoft.Maui.Interop;

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
	/// <summary>Gets or sets explicit host action availability, independent of OS permissions.</summary>
	public bool Enabled { get; set; } = true;
}

/// <summary>Resolves a method parameter from the current owning service scope instead of JSON arguments.</summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class FromServicesAttribute : Attribute;

/// <summary>Declares an operation-published topic without an OS subscription lifecycle.</summary>
/// <param name="name">The unique event topic.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class HybridTopicAttribute(string name) : Attribute
{
	/// <summary>Gets the event topic.</summary>
	public string Name { get; } = name;
}

/// <summary>Identifies the first or last subscription hook.</summary>
public enum HybridEventLifecycle
{
	/// <summary>Attaches the scoped source when the document first subscribes.</summary>
	Start,
	/// <summary>Invalidates delivery and detaches the source when the document unsubscribes or retires.</summary>
	Stop
}

/// <summary>Exposes an accessible event, or binds a manual first/last subscription hook to a topic.</summary>
[AttributeUsage(AttributeTargets.Event | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class HybridEventAttribute : Attribute
{
	/// <summary>Exposes an event whose subscription lifetime is owned by the current document.</summary>
	/// <param name="name">The unique event topic.</param>
	public HybridEventAttribute(string name) => Name = name;

	/// <summary>Binds a parameterless Task start or void stop method to a manual event source.</summary>
	/// <param name="name">The unique event topic, paired within the same provider.</param>
	/// <param name="lifecycle">The first/last subscription hook.</param>
	public HybridEventAttribute(string name, HybridEventLifecycle lifecycle)
	{
		Name = name;
		Lifecycle = lifecycle;
	}

	/// <summary>Gets the event topic.</summary>
	public string Name { get; }
	/// <summary>Gets the subscription lifecycle.</summary>
	public HybridEventLifecycle Lifecycle { get; }
}

/// <summary>Opts one annotated referenced contract into this assembly's generated action registration.</summary>
/// <param name="contract">The interface or class containing explicitly annotated members.</param>
/// <remarks>Annotations alone expose nothing. A consuming host must use a generator and explicitly register its output.</remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class HybridExportAttribute(Type contract) : Attribute
{
	/// <summary>Gets the selected referenced contract.</summary>
	public Type Contract { get; } = contract;
}
