using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ReturnTypeExtensions
	{
		public static UIReturnKeyType ToPlatform(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return UIReturnKeyType.Go;
				case ReturnType.Next:
					return UIReturnKeyType.Next;
				case ReturnType.Send:
					return UIReturnKeyType.Send;
				case ReturnType.Search:
					return UIReturnKeyType.Search;
				case ReturnType.Done:
					return UIReturnKeyType.Done;
				case ReturnType.Default:
					return UIReturnKeyType.Default;
				default:
					throw new NotImplementedException($"ReturnType {returnType} not supported");
			}
		}
	}
}