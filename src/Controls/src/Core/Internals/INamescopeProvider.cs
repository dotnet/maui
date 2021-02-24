using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface INameScopeProvider
	{
		INameScope NameScope { get; }
	}
}