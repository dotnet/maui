using System;

namespace Xamarin.Forms
{
	internal class InvalidNavigationException : Exception
	{
		public InvalidNavigationException(string message) : base(message)
		{
		}
	}
}