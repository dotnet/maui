using System.Diagnostics;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Xamarin.Forms.Controls.Tests
{
	public class ControlGalleryTestListener : ITestListener
	{
		public void SendMessage(TestMessage message)
		{
		}

		public void TestFinished(ITestResult result)
		{
			var test = result.Test;
			if (test is TestAssembly testAssembly)
			{
				MessagingCenter.Send(result, "AssemblyFinished");
			}
			else
			{
				MessagingCenter.Send(result, "TestFinished");
			}
		}

		public void TestOutput(TestOutput output)
		{
			Debug.WriteLine(output);
		}

		public void TestStarted(ITest test)
		{
			MessagingCenter.Send(test, "TestStarted");
		}
	}
}