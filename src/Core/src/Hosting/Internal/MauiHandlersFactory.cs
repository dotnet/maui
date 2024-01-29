#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Hosting.Internal
{
	sealed class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
	{
		public MauiHandlersFactory(IMauiHandlersCollection collection)
			: base(collection)
		{
		}

		public IElementHandler? GetHandler(Type type)
			=> GetService(type) as IElementHandler;

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(Type iview)
		{
			if (!TryGetServiceDescriptors(ref iview, out var single, out var enumerable))
				return default;

			if (single != null)
				return single.ImplementationType;

			if (enumerable != null)
			{
				foreach (var descriptor in enumerable)
				{
					return descriptor.ImplementationType;
				}
			}

			return default;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;
	}
}