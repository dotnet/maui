using System;

namespace Xamarin.Forms
{
	public class UnsolvableConstraintsException : Exception
	{
		public UnsolvableConstraintsException(string message) : base(message)
		{
		}
	}
}