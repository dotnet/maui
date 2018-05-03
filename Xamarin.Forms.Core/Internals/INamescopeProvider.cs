using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Internals
{
	[Obsolete]
	interface INameScopeProvider
	{
		INameScope NameScope { get; }
	}
}