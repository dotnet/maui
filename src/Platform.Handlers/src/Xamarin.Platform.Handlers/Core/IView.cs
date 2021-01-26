using System;

namespace Xamarin.Platform
{
	public interface IView : IFrameworkElement
	{
#if NETSTANDARD2_1
		//Alignment GetVerticalAlignment(ILayout layout) => Alignment.Fill;
		//Alignment GetHorizontalAlignment(ILayout layout) => Alignment.Fill;
#else
		//Alignment GetVerticalAlignment(ILayout layout);
		//Alignment GetHorizontalAlignment(ILayout layout);
#endif
	}
}