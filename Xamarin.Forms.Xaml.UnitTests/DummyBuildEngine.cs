using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class DummyBuildEngine : IBuildEngine
	{
		public void LogErrorEvent (BuildErrorEventArgs e)
		{
		}

		public void LogWarningEvent (BuildWarningEventArgs e)
		{
		}

		public void LogMessageEvent (BuildMessageEventArgs e)
		{
		}

		public void LogCustomEvent (CustomBuildEventArgs e)
		{
		}

		public bool BuildProjectFile (string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
		{
			return false;
		}

		public bool ContinueOnError {
			get { return false; }
		}

		public int LineNumberOfTaskNode {
			get { return 1; }
		}

		public int ColumnNumberOfTaskNode {
			get { return 1; }
		}

		public string ProjectFileOfTaskNode {
			get { return String.Empty; }
		}
	}
}

