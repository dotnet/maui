using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.Common;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners;


internal class MicrosoftPlatformTestRunner : TestRunner, IDataConsumer
{
	public MicrosoftPlatformTestRunner(LogWriter logger) : base(logger)
	{
	}

	public Type[] DataTypesConsumed => new[] { typeof(TestNodeUpdateMessage) };

	public string Id => nameof(MicrosoftPlatformTestRunner);

	public string SemVer => "1.0.0";

	public string DisplayName => nameof(MicrosoftPlatformTestRunner);

	public string Description => nameof(MicrosoftPlatformTestRunner);

	public string Uid => throw new NotImplementedException();

	public string Version => throw new NotImplementedException();

	protected override string ResultsFileName
	{
		get => throw new NotImplementedException();
		set => throw new NotImplementedException();
	}

	public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public Task<bool> IsEnabledAsync() => Task.FromResult(true);

	public override async Task Run(IEnumerable<TestAssemblyInfo> testAssemblies)
	{
		string cacheDir = "/data/local/tmp/testanywhere_cache";
#if ANDROID
		cacheDir = global::Android.App.Application.Context.CacheDir!.AbsolutePath;
#endif
		var builder = await Microsoft.Testing.Platform.Builder.TestApplication.CreateServerModeBuilderAsync(new[] {
					"--results-directory", cacheDir
				});
		
		// buider.ServerMode.ConnectToTcpClient(clientHostName: "localhost", clientPort: 6000);
		// buider.AddTestAnywhereTestFramework(new TestTemplate.SourceGeneratedTestNodesBuilder());
		builder.TestHost.AddDataConsumer(_ => this);
		 var testApp = await builder.BuildAsync();
		var exitCode = await testApp.RunAsync();
	}

	public override void SkipCategories(IEnumerable<string> categories)
	{

	}

	public override void SkipClass(string className, bool isExcluded)
	{

	}

	public override void SkipMethod(string method, bool isExcluded)
	{

	}

	public override void SkipTests(IEnumerable<string> tests)
	{

	}

	public override string WriteResultsToFile(XmlResultJargon xmlResultJargon)
	{
		throw new NotImplementedException();
	}

	public override void WriteResultsToFile(TextWriter writer, XmlResultJargon jargon)
	{
		throw new NotImplementedException();
	}
}