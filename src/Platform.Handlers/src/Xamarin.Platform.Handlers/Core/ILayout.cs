using System.Collections.Generic;

namespace Xamarin.Platform
{
	public interface ILayout : IView
	{
		IReadOnlyList<IView> Children { get; }
	}
}