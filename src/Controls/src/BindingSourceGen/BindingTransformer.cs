
namespace Microsoft.Maui.Controls.BindingSourceGen;

public interface IBindingInvocationTransformer
{
    SetBindingInvocationDescription Transform(SetBindingInvocationDescription setBindingInvocationDescription);
}

public class ReferenceTypesConditionalAccessTransformer : IBindingInvocationTransformer
{
    public SetBindingInvocationDescription Transform(SetBindingInvocationDescription setBindingInvocationDescription)
    {
        var path = TransformPath(setBindingInvocationDescription);
        return setBindingInvocationDescription with { Path = path };
    }

    private static EquatableArray<IPathPart> TransformPath(SetBindingInvocationDescription setBindingInvocationDescription)
    {
        var newPath = new List<IPathPart>();
        foreach (var pathPart in setBindingInvocationDescription.Path)
        {
            var sourceIsReferenceType = newPath.Count == 0 && !setBindingInvocationDescription.SourceType.IsValueType;
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