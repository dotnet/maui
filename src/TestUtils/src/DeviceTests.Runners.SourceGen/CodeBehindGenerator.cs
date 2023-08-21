// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen
{
	[Generator]
	public class CodeBehindGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			//#if DEBUG
			//if (!System.Diagnostics.Debugger.IsAttached)
			//	System.Diagnostics.Debugger.Launch();
			//#endif
		}

		public void Execute(GeneratorExecutionContext context)
		{
			if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFramework))
				return;

			context.Log($"TargetFramework: {targetFramework}");

			var generator = new RunnerGenerator(context, targetFramework);

			generator?.Generate();
		}
	}
}