using System;
using Android.Views.InputMethods;

namespace Microsoft.Maui
{
	public static class ImeActionExtensions
	{
		public static ImeAction ToNative(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return ImeAction.Go;
				case ReturnType.Next:
					return ImeAction.Next;
				case ReturnType.Send:
					return ImeAction.Send;
				case ReturnType.Search:
					return ImeAction.Search;
				case ReturnType.Done:
					return ImeAction.Done;
				case ReturnType.Default:
					return ImeAction.Done;
				default:
					throw new NotImplementedException($"ReturnType {returnType} not supported");
			}
		}
	}
}