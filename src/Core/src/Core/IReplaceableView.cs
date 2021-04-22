using System;
namespace Microsoft.Maui
{
	public interface IReplaceableView
	{
		IView ReplacedView { get; }
	}
}
