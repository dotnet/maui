using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface INameScopeProvider
	{
		INameScope NameScope { get; }
	}
}