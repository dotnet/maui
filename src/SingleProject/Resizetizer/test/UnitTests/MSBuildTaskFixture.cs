using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public abstract class MSBuildTaskTestFixture<TTask> : BaseTest, IBuildEngine
		where TTask : Microsoft.Build.Framework.ITask
	{
		protected readonly TestLogger Logger;

		protected List<BuildErrorEventArgs> LogErrorEvents = new List<BuildErrorEventArgs>();
		protected List<BuildMessageEventArgs> LogMessageEvents = new List<BuildMessageEventArgs>();
		protected List<CustomBuildEventArgs> LogCustomEvents = new List<CustomBuildEventArgs>();
		protected List<BuildWarningEventArgs> LogWarningEvents = new List<BuildWarningEventArgs>();

		// IBuildEngine

		bool IBuildEngine.ContinueOnError => false;

		int IBuildEngine.LineNumberOfTaskNode => 0;

		int IBuildEngine.ColumnNumberOfTaskNode => 0;

		string IBuildEngine.ProjectFileOfTaskNode => $"Fake{GetType().Name}Project.proj";

		bool IBuildEngine.BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => throw new NotImplementedException();

		void IBuildEngine.LogCustomEvent(CustomBuildEventArgs e) => LogCustomEvents.Add(e);

		void IBuildEngine.LogErrorEvent(BuildErrorEventArgs e) => LogErrorEvents.Add(e);

		void IBuildEngine.LogMessageEvent(BuildMessageEventArgs e) => LogMessageEvents.Add(e);

		void IBuildEngine.LogWarningEvent(BuildWarningEventArgs e) => LogWarningEvents.Add(e);
	}
}