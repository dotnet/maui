using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Internals
{
	interface INameScopeProvider
	{
		INameScope NameScope { get; }
	}
}