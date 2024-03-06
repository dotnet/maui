#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Hosting.Internal
{
	sealed class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
	{
		public MauiHandlersFactory(IEnumerable<HandlerMauiAppBuilderExtensions.HandlerRegistration> registrationActions) :
			base(CreateHandlerCollection(registrationActions))
		{
		}

		static MauiHandlersCollection CreateHandlerCollection(IEnumerable<HandlerMauiAppBuilderExtensions.HandlerRegistration> registrationActions)
		{
			var collection = new MauiHandlersCollection();
			if (registrationActions != null)
			{
				foreach (var registrationAction in registrationActions)
				{
					registrationAction.AddRegistration(collection);
				}
			}
			HotReload.MauiHotReloadHelper.RegisterHandlers(collection);
			return collection;
		}

		public IElementHandler? GetHandler(Type type)
			=> GetService(type) as IElementHandler;

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(Type iview)
		{
			if (!TryGetServiceDescriptors(ref iview, out var single, out var enumerable))

/* Unmerged change from project 'Core(net8.0)'
Before:
				return default;

			if (single != null)
				return single.ImplementationType;
After:
			{
				return default;
			}
*/

/* Unmerged change from project 'Core(net8.0-maccatalyst)'
Before:
				return default;

			if (single != null)
				return single.ImplementationType;
After:
			{
				return default;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.19041.0)'
Before:
				return default;

			if (single != null)
				return single.ImplementationType;
After:
			{
				return default;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.20348.0)'
Before:
				return default;

			if (single != null)
				return single.ImplementationType;
After:
			{
				return default;
			}
*/
			{
				return default;
			}

			if (single != null)
			{
				return single.ImplementationType;
			}

			if (single != null)
			{
				return single.ImplementationType;
			}

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