using System;
using System.ComponentModel;
using System.Maui.Internals;

namespace System.Maui.Xaml.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface INameScopeProvider
	{
		INameScope NameScope { get; }
	}
}