using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Handlers;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal abstract class ElementHandlerAttribute : Attribute
{
	public abstract IElementHandler CreateHandler();
	public abstract Type HandlerType { get; }
}

internal sealed class ElementHandlerAttribute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler> : ElementHandlerAttribute
	where THandler : IElementHandler, new()
{
	public override IElementHandler CreateHandler() => new THandler();
	public override Type HandlerType => typeof(THandler);
}
