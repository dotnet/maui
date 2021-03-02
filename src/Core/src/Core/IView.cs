using System;

namespace Microsoft.Maui
{
	public interface IView : IFrameworkElement
	{
		Thickness Margin { get; }
	}
}