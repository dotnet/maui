using NUnit.Framework;

namespace UITest.NUnit.Tests;

public class MtpTestArgumentsTests
{
	[TestCase(true, "--report-trx", "--report-trx-filename")]
	[TestCase(false, "--report-xunit-trx", "--report-xunit-trx-filename")]
	public void DesktopReportOptionsMatchTheFramework(bool nunit, string option, string filenameOption)
	{
		Assert.That(MtpTestArguments.DesktopReport(nunit, "results.trx"),
			Is.EqualTo(new[] { option, filenameOption, "results.trx" }));
	}

	[TestCase("FullyQualifiedName~PaintTests", "--filter-method", "*PaintTests*")]
	[TestCase("FullyQualifiedName=Example.PaintTests.Case", "--filter-method", "Example.PaintTests.Case")]
	[TestCase("FullyQualifiedName!~PaintTests", "--filter-not-method", "*PaintTests*")]
	[TestCase("TestCategory=Core", "--filter-trait", "Category=Core")]
	[TestCase("Category!=Slow", "--filter-not-trait", "Category=Slow")]
	[TestCase("PaintTests", "--filter-method", "*PaintTests*")]
	[TestCase("/[Category=Core]", "--filter-query", "/[Category=Core]")]
	public void XunitFiltersUseSupportedMtpOptions(string filter, string option, string value)
	{
		Assert.That(MtpTestArguments.DesktopFilter(filter, false), Is.EqualTo(new[] { option, value }));
	}

	[Test]
	public void NUnitFilterRemainsUnchanged()
	{
		const string filter = "(TestCategory=Issues|TestCategory=Core)&FullyQualifiedName~Case";
		Assert.That(MtpTestArguments.DesktopFilter(filter, true), Is.EqualTo(new[] { "--filter", filter }));
	}

	[Test]
	public void GroupedMethodAlternativesCanBeConstrainedByCategory()
	{
		Assert.That(MtpTestArguments.DesktopFilter(
			"(FullyQualifiedName~First|FullyQualifiedName~Second)&Category=Core", false),
			Is.EqualTo(new[] { "--filter-method", "*First*", "--filter-method", "*Second*", "--filter-trait", "Category=Core" }));
	}

	[Test]
	public void ExclusionsAreAndedWithPositiveSelection()
	{
		Assert.That(MtpTestArguments.DesktopFilter("FullyQualifiedName~First&FullyQualifiedName!~Slow&Category!=Other", false),
			Is.EqualTo(new[] { "--filter-method", "*First*", "--filter-not-method", "*Slow*", "--filter-not-trait", "Category=Other" }));
	}

	[TestCase("FullyQualifiedName~First|FullyQualifiedName~Second&Category=Core")]
	[TestCase("FullyQualifiedName~First|Category=Core")]
	[TestCase("Category=First&Category=Second")]
	[TestCase("Name=Parameterized(1)")]
	[TestCase("(FullyQualifiedName~First")]
	[TestCase("FullyQualifiedName~First)")]
	public void UnsupportedLegacyBooleanExpressionsCannotSilentlyBroadenCoverage(string filter)
	{
		Assert.Throws<ArgumentException>(() => MtpTestArguments.DesktopFilter(filter, false));
	}

	[TestCase("ios")]
	[TestCase("android")]
	[TestCase("catalyst")]
	public void NativeCommandsPreserveBuildFiltersAndRealResultContracts(string platform)
	{
		var args = MtpTestArguments.Native("Native Tests.csproj", "Release", "net11.0-" + platform,
			"runtime-id", platform, "SkipCategories=CollectionView,Shell,HybridWebView", "selected-device", "/host/results", true);
		Assert.That(args.Take(7), Is.EqualTo(new[] { "test", "--project", "Native Tests.csproj",
			"--configuration", "Release", "--framework", "net11.0-" + platform }));
		Assert.That(args, Does.Contain("-p:RuntimeIdentifier=runtime-id"));
		Assert.That(args, Does.Contain("-p:UseMonoRuntime=false"));
		Assert.That(args, Does.Contain("-p:MauiTestFilter=SkipCategories=CollectionView%2CShell%2CHybridWebView"));
		Assert.That(args, Does.Contain("--device"));
		Assert.That(args, Does.Contain("selected-device"));
		Assert.That(args, Does.Not.Contain("--filter"));
		Assert.That(args, Does.Not.Contain("--no-build"));
		if (platform == "android")
		{
			Assert.That(args, Does.Contain("--report-trx"));
			Assert.That(args, Does.Contain("--results-directory"));
		}
		else
		{
			Assert.That(args, Does.Not.Contain("--report-trx"));
			Assert.That(args, Does.Not.Contain("--results-directory"));
			Assert.That(args.Any(value => value.StartsWith("-p:MauiTestResultsDirectory=", StringComparison.Ordinal)),
				Is.EqualTo(platform == "catalyst"));
		}
	}

	[Test]
	public void DeviceSelectionAndNativeFilterAreOptional()
	{
		var args = MtpTestArguments.Native("Tests.csproj", "Debug", "net11.0-ios", null, "ios", null, null, "/host/results", false);
		Assert.That(args, Does.Not.Contain("--device"));
		Assert.That(args.Any(value => value.StartsWith("-p:MauiTestFilter=", StringComparison.Ordinal)), Is.False);
	}

	[TestCase("FullyQualifiedName~Case")]
	[TestCase("Category=")]
	[TestCase("SkipCategories=Core,,Slow")]
	public void InvalidNativeFiltersCannotRunUnfilteredTests(string filter)
	{
		Assert.Throws<ArgumentException>(() =>
			MtpTestArguments.Native("Tests.csproj", "Debug", "net11.0-ios", null, "ios", filter, null, "/host/results", false));
	}
}
