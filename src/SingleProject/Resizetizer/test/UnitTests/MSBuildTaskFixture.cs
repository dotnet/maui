#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public abstract class MSBuildTaskTestFixture<TTask> : BaseTest, IBuildEngine
		where TTask : Microsoft.Build.Framework.ITask
	{
		protected readonly TestLogger? Logger;

		protected List<BuildErrorEventArgs> LogErrorEvents = new List<BuildErrorEventArgs>();
		protected List<BuildMessageEventArgs> LogMessageEvents = new List<BuildMessageEventArgs>();
		protected List<CustomBuildEventArgs> LogCustomEvents = new List<CustomBuildEventArgs>();
		protected List<BuildWarningEventArgs> LogWarningEvents = new List<BuildWarningEventArgs>();

		protected MSBuildTaskTestFixture()
			: this(null)
		{
		}

		protected MSBuildTaskTestFixture(ITestOutputHelper? output)
		{
			Output = output;
		}

		public ITestOutputHelper? Output { get; }

		// IBuildEngine

		bool IBuildEngine.ContinueOnError => false;

		int IBuildEngine.LineNumberOfTaskNode => 0;

		int IBuildEngine.ColumnNumberOfTaskNode => 0;

		string IBuildEngine.ProjectFileOfTaskNode => $"Fake{GetType().Name}Project.proj";

		bool IBuildEngine.BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => throw new NotImplementedException();

		void IBuildEngine.LogCustomEvent(CustomBuildEventArgs e)
		{
			LogCustomEvents.Add(e);
			Output?.WriteLine($"CUSTOM : {e.Message}");
		}

		void IBuildEngine.LogErrorEvent(BuildErrorEventArgs e)
		{
			LogErrorEvents.Add(e);
			Output?.WriteLine($"ERROR  : {e.Message}");
		}

		void IBuildEngine.LogMessageEvent(BuildMessageEventArgs e)
		{
			LogMessageEvents.Add(e);
			Output?.WriteLine($"MESSAGE: {e.Message}");
		}

		void IBuildEngine.LogWarningEvent(BuildWarningEventArgs e)
		{
			LogWarningEvents.Add(e);
			Output?.WriteLine($"WARNING: {e.Message}");
		}
	}
}