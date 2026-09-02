using Xunit;

namespace DotNet.Release.Tests;

public class AzurePipelineCommandTests
{
    [Fact]
    public void BarId_is_emitted_as_an_output_variable()
    {
        Assert.Equal("##vso[task.setvariable variable=BarId;isOutput=true]4242", AzurePipelineCommand.SetBarId(4242));
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Workload_classification_is_emitted_as_an_output_variable(bool value, string expected)
    {
        Assert.Equal($"##vso[task.setvariable variable=IsWorkload;isOutput=true]{expected}", AzurePipelineCommand.SetIsWorkload(value));
    }

    [Fact]
    public void PackagesToPublish_is_a_plain_variable()
    {
        Assert.Equal("##vso[task.setvariable variable=NuGetPackagesToPublish]true", AzurePipelineCommand.SetPackagesToPublish(true));

        Assert.EndsWith("]false", AzurePipelineCommand.SetPackagesToPublish(false), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("100%", "100%AZP25")]
    [InlineData("a;b", "a%3Bb")]
    [InlineData("a]b", "a%5Db")]
    [InlineData("a\nb", "a%0Ab")]
    public void Values_that_would_break_the_command_syntax_are_escaped(string value, string expected)
    {
        Assert.Equal($"##vso[task.setvariable variable=X]{expected}", AzurePipelineCommand.SetVariable("X", value));
    }

    [Fact]
    public void A_missing_variable_name_is_a_programming_error()
    {
        Assert.Throws<ArgumentException>(
            () => AzurePipelineCommand.SetVariable("  ", "x"));
    }
}
