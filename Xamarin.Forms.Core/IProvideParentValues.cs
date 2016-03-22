using System.Collections.Generic;

namespace Xamarin.Forms.Xaml
{
	internal interface IProvideParentValues : IProvideValueTarget
	{
		IEnumerable<object> ParentObjects { get; }
	}
}