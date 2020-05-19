using System.Collections.Generic;

namespace System.Maui.Xaml
{
	internal interface IProvideParentValues : IProvideValueTarget
	{
		IEnumerable<object> ParentObjects { get; }
	}
}