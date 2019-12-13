using System.Diagnostics;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Xamarin.Forms.Controls.Tests
{
	public class ControlGalleryTestListener : ITestListener
	{
		public void SendMessage(TestMessage message)
		{
			Debug.WriteLine(message);
		}

		public void TestFinished(ITestResult result)
		{
			var test = result.Test;
			if (test is TestAssembly testAssembly)
			{
				Debug.WriteLine($"Assembly finished {testAssembly.Assembly.FullName}");
				MessagingCenter.Send(result, "AssemblyFinished");
			}
			else
			{
				Debug.WriteLine($"{result.Name} finished");
				MessagingCenter.Send(result, "TestFinished");
			}
		}

		public void TestOutput(TestOutput output)
		{
			Debug.WriteLine(output);
		}

		public void TestStarted(ITest test)
		{
			Debug.WriteLine($"{test.Name} started");
			MessagingCenter.Send(test, "TestStarted");
		}
	}
}