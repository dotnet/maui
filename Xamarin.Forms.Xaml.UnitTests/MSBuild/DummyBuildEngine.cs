using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class DummyBuildEngine : IBuildEngine
	{
		public List<BuildErrorEventArgs> Errors { get; } = new List<BuildErrorEventArgs> ();

		public List<BuildWarningEventArgs> Warnings { get; } = new List<BuildWarningEventArgs> ();

		public List<BuildMessageEventArgs> Messages { get; } = new List<BuildMessageEventArgs> ();

		public void LogErrorEvent (BuildErrorEventArgs e)
		{
			Errors.Add (e);
		}

		public void LogWarningEvent (BuildWarningEventArgs e)
		{
			Warnings.Add (e);
		}

		public void LogMessageEvent (BuildMessageEventArgs e)
		{
			Messages.Add (e);
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

