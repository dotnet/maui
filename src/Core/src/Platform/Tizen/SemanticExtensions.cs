using System;
using ElmSharp.Accessible;

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(this IView element)
		{
			if (element?.Handler?.NativeView == null)
				throw new NullReferenceException("Can't access view from a null handler");

			if (element.Handler.NativeView is not AccessibleObject)
				return;

			//TODO : Need to implement
		}
	}
}
