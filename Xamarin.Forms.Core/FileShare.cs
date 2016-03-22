using System;

namespace Xamarin.Forms
{
	[Flags]
	internal enum FileShare
	{
		None = 0,
		Read = 1,
		Write = 2,
		ReadWrite = 3,
		Delete = 4,
		Inheritable = 16
	}
}