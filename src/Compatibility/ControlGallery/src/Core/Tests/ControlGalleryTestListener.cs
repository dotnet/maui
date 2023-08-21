//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.Diagnostics;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.Controls.ControlGallery.Tests
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