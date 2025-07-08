using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
using System;
using System.Globalization;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue30426 : _IssuesUITest
	{
		public Issue30426(TestDevice device) : base(device)
		{
		}

		public override string Issue => "IImage Downsize() throws Exception in 9.0.81 when called from background thread on iOS";


	}
}