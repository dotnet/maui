
namespace Microsoft.Maui.Controls.BindingSourceGen;

public interface IBindingInvocationTransformer
{
	BindingInvocationDescription Transform(BindingInvocationDescription BindingInvocationDescription);
}

public class ReferenceTypesConditionalAccessTransformer : IBindingInvocationTransformer
{
	public BindingInvocationDescription Transform(BindingInvocationDescription BindingInvocationDescription)
	{
		var path = TransformPath(BindingInvocationDescription);
		return BindingInvocationDescription with { Path = path };
	}

	private static EquatableArray<IPathPart> TransformPath(BindingInvocationDescription BindingInvocationDescription)
	{
		var newPath = new List<IPathPart>();
		foreach (var pathPart in BindingInvocationDescription.Path)
		{
			var sourceIsReferenceType = newPath.Count == 0 && !BindingInvocationDescription.SourceType.IsValueType;
			var previousPartIsReferenceType = newPath.Count > 0 && PreviousPartIsReferenceType(newPath.Last());

			if (pathPart is not MemberAccess && pathPart is not IndexAccess)
			{
				newPath.Add(pathPart);
			}
			else if (sourceIsReferenceType || previousPartIsReferenceType)
			{
				newPath.Add(new ConditionalAccess(pathPart));
			}
			else
			{
				newPath.Add(pathPart);
			}
		}

		return new EquatableArray<IPathPart>(newPath.ToArray());

		static bool PreviousPartIsReferenceType(IPathPart previousPathPart) =>
			previousPathPart switch
			{
				MemberAccess memberAccess => !memberAccess.IsValueType,
				IndexAccess indexAccess => !indexAccess.IsValueType,
				ConditionalAccess { Part: var inner } => PreviousPartIsReferenceType(inner),
				_ => false,
			};
	}
}
