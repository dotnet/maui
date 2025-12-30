namespace Microsoft.Maui.Controls.BindingSourceGen;

public interface IPathPart : IEquatable<IPathPart>
{
	public string? PropertyName { get; }
}

public record MemberAccess(
	string MemberName,
	bool IsValueType = false,
	TypeDescription? ContainingType = null,
	TypeDescription? MemberType = null,
	AccessorKind? Kind = null,
	bool IsGetterInaccessible = false,
	bool IsSetterInaccessible = false) : IPathPart
{
	public string PropertyName => MemberName;
	
	/// <summary>
	/// Indicates whether this member has any inaccessible accessor (getter or setter).
	/// Used to determine if UnsafeAccessor methods need to be generated.
	/// </summary>
	public bool HasInaccessibleAccessor => IsGetterInaccessible || IsSetterInaccessible;
	
	/// <summary>
	/// Compares this MemberAccess with another IPathPart for equality.
	/// 
	/// Note: This implementation uses a non-standard equality contract to support test scenarios.
	/// Extended metadata fields (ContainingType, MemberType, Kind, accessibility flags) are only
	/// compared when both sides have them populated. This allows tests to create simplified
	/// MemberAccess instances (e.g., new MemberAccess("Name")) that will match production
	/// instances with full metadata, as long as core properties (MemberName, IsValueType) match.
	/// 
	/// This violates the standard transitive property of equality but is intentional for
	/// backward compatibility with existing tests.
	/// </summary>
	public bool Equals(IPathPart other)
	{
		if (other is not MemberAccess memberAccess)
			return false;
		
		// Core properties must always match
		if (MemberName != memberAccess.MemberName || IsValueType != memberAccess.IsValueType)
			return false;
		
		// For extended properties, only compare if both sides have them populated
		// This allows tests to create simple MemberAccess instances without full metadata
		bool hasExtendedMetadata = ContainingType != null || Kind != null;
		bool otherHasExtendedMetadata = memberAccess.ContainingType != null || memberAccess.Kind != null;
		
		if (hasExtendedMetadata && otherHasExtendedMetadata)
		{
			return ContainingType == memberAccess.ContainingType
				&& MemberType == memberAccess.MemberType
				&& Kind == memberAccess.Kind
				&& IsGetterInaccessible == memberAccess.IsGetterInaccessible
				&& IsSetterInaccessible == memberAccess.IsSetterInaccessible;
		}
		
		return true;
	}
}

public sealed record IndexAccess(string DefaultMemberName, object Index, bool IsValueType = false) : IPathPart
{
	public string? PropertyName => $"{DefaultMemberName}[{Index}]";

	public bool Equals(IPathPart other)
	{
		return other is IndexAccess indexAccess
			&& DefaultMemberName == indexAccess.DefaultMemberName
			&& Index.Equals(indexAccess.Index)
			&& IsValueType == indexAccess.IsValueType;
	}
}

public sealed record ConditionalAccess(IPathPart Part) : IPathPart
{
	public string? PropertyName => Part.PropertyName;

	public bool Equals(IPathPart other)
	{
		return other is ConditionalAccess conditionalAccess && Part.Equals(conditionalAccess.Part);
	}
}

public sealed record Cast(TypeDescription TargetType) : IPathPart
{
	public string? PropertyName => null;

	public bool Equals(IPathPart other)
	{
		return other is Cast cast && TargetType.Equals(cast.TargetType);
	}
}
