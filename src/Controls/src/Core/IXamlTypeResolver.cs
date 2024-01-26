#nullable disable
using System;

namespace Microsoft.Maui.Controls.Xaml
{
	public interface IXamlTypeResolver
	{
		Type Resolve(string qualifiedTypeName, IServiceProvider serviceProvider = null, bool expandToExtension = true);
		bool TryResolve(string qualifiedTypeName, out Type type);
	}
}