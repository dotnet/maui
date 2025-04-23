#nullable disable
using System;

namespace Microsoft.Maui.Controls.Xaml
{
	public interface IXamlTypeResolver
	{
#if NETSTANDARD2_0
		Type Resolve(string qualifiedTypeName, IServiceProvider serviceProvider = null);
#else
		Type Resolve(string qualifiedTypeName, IServiceProvider serviceProvider = null) => Resolve(qualifiedTypeName, serviceProvider, true);
#endif
		Type Resolve(string qualifiedTypeName, IServiceProvider serviceProvider = null, bool expandToExtension = true);
		bool TryResolve(string qualifiedTypeName, out Type type);
	}
}