namespace Microsoft.Maui.Controls.BindingSourceGen;

public interface IPathPart : IEquatable<IPathPart>
{
	public string? PropertyName { get; }
}

public sealed record InaccessibleMemberAccess(TypeDescription ContainingType, TypeDescription memberType, AccessorKind Kind, string MemberName, bool IsValueType = false, bool IsGetterInaccessible = true, bool IsSetterInaccessible = true) : IPathPart
{
	public string PropertyName => MemberName;

	public bool Equals(IPathPart other)
	{
		return other is InaccessibleMemberAccess memberAccess
			&& ContainingType == memberAccess.ContainingType
			&& Kind == memberAccess.Kind
			&& MemberName == memberAccess.MemberName
			&& IsValueType == memberAccess.IsValueType
			&& IsGetterInaccessible == memberAccess.IsGetterInaccessible
			&& IsSetterInaccessible == memberAccess.IsSetterInaccessible;
	}
}

public record MemberAccess(string MemberName, bool IsValueType = false) : IPathPart
{
	public string PropertyName => MemberName;
	public bool Equals(IPathPart other)
	{
		return other is MemberAccess memberAccess
			&& MemberName == memberAccess.MemberName
			&& IsValueType == memberAccess.IsValueType;
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
