using System;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Core.UITests
{
	internal class UITestQueryNoResultException : Exception
	{
		readonly string message;

		public UITestQueryNoResultException (string query)
		{
			message = string.Format ("Found no elements for query with target: {0}", query);
		}

		public override string Message
		{
			get { return message; }
		}
	}

	internal class UITestQuerySingleResultException : Exception
	{
		readonly string message;

		public UITestQuerySingleResultException (string query)
		{
			message = string.Format ("Found single element for query with target: {0}", query);
		}

		public override string Message
		{
			get { return message; }
		}
	}

	internal class UITestQueryMultipleResultsException : Exception
	{
		readonly string message;

		public UITestQueryMultipleResultsException (string query)
		{
			message = string.Format ("Found muliple elements for query with target: {0}", query);
		}

		public override string Message
		{
			get { return message; }
		}
	}

	internal class UITestRemoteException : Exception
	{
		readonly string message;

		public UITestRemoteException (string message)
		{
			this.message = message;
		}

		public override string Message
		{
			get { return message; }
		}
	}

	internal class UITestRemoteQueryException : Exception
	{
		readonly string message;

		public UITestRemoteQueryException (string query)
		{
			message = string.Format ("Error for query with target: {0}", query);
		}

		public override string Message
		{
			get { return message; }
		}
	}

	internal class UITestErrorException : Exception
	{
		readonly string message;

		public UITestErrorException (string message, [CallerMemberName] string caller = null)
		{
			message = string.Format ("Test error: {0}, {1}", caller, message);
		}

		public override string Message
		{
			get { return message; }
		}
	}
}
