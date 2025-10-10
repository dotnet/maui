using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maui.Controls.SourceGen;
using Moq;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for the ProjectItem class.
/// </summary>
[TestFixture]
public class ProjectItemTests
{
    /// <summary>
    /// Tests EnableLineInfo property when LineInfo is explicitly enabled.
    /// Should return true when build_metadata.additionalfiles.LineInfo is set to "enable".
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void EnableLineInfo_WhenLineInfoExplicitlyEnabled_ReturnsTrue()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.LineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = "enable";
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        bool result = projectItem.EnableLineInfo;

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests EnableLineInfo property when LineInfo is explicitly disabled.
    /// Should return false when build_metadata.additionalfiles.LineInfo is set to "disable".
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void EnableLineInfo_WhenLineInfoExplicitlyDisabled_ReturnsFalse()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // LineInfo key returns "disable"
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.LineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = "disable";
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        bool result = projectItem.EnableLineInfo;

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests EnableLineInfo property fallback behavior when LineInfo is not set and MauiXamlLineInfo is not disabled.
    /// Should return true when both LineInfo settings are not present and MauiXamlLineInfo is not disabled.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void EnableLineInfo_WhenLineInfoNotSetAndMauiXamlLineInfoNotDisabled_ReturnsTrue()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // LineInfo key doesn't exist
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.LineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = null;
                return false;
            });

        // MauiXamlLineInfo key doesn't exist (not disabled)
        mockOptions.Setup(o => o.TryGetValue("build_property.MauiXamlLineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = null;
                return false;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        bool result = projectItem.EnableLineInfo;

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests EnableLineInfo property fallback behavior when LineInfo is not set and MauiXamlLineInfo is disabled.
    /// Should return false when both LineInfo settings are not present and MauiXamlLineInfo is disabled.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void EnableLineInfo_WhenLineInfoNotSetAndMauiXamlLineInfoDisabled_ReturnsFalse()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // LineInfo key doesn't exist
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.LineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = null;
                return false;
            });

        // MauiXamlLineInfo key is disabled
        mockOptions.Setup(o => o.TryGetValue("build_property.MauiXamlLineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = "disable";
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        bool result = projectItem.EnableLineInfo;

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests EnableLineInfo property when LineInfo has an unrecognized value.
    /// Should fall back to MauiXamlLineInfo setting when LineInfo value is neither "enable" nor "disable".
    /// </summary>
    [TestCase("true", true)]
    [TestCase("false", true)]
    [TestCase("invalid", true)]
    [TestCase("", true)]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void EnableLineInfo_WhenLineInfoHasUnrecognizedValue_FallsBackToMauiXamlLineInfo(string lineInfoValue, bool expected)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // LineInfo key exists but has unrecognized value
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.LineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = lineInfoValue;
                return true;
            });

        // MauiXamlLineInfo key doesn't exist (not disabled)
        mockOptions.Setup(o => o.TryGetValue("build_property.MauiXamlLineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = null;
                return false;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        bool result = projectItem.EnableLineInfo;

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Tests EnableLineInfo property with case-insensitive "enable" values.
    /// Should return true for various case combinations of "enable".
    /// </summary>
    [TestCase("enable")]
    [TestCase("ENABLE")]
    [TestCase("Enable")]
    [TestCase("eNaBlE")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void EnableLineInfo_WhenLineInfoEnabledWithDifferentCasing_ReturnsTrue(string enableValue)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.LineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = enableValue;
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        bool result = projectItem.EnableLineInfo;

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests EnableLineInfo property with case-insensitive "disable" values.
    /// Should return false for various case combinations of "disable".
    /// </summary>
    [TestCase("disable")]
    [TestCase("DISABLE")]
    [TestCase("Disable")]
    [TestCase("dIsAbLe")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void EnableLineInfo_WhenLineInfoDisabledWithDifferentCasing_ReturnsFalse(string disableValue)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.LineInfo", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = disableValue;
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        bool result = projectItem.EnableLineInfo;

        // Assert
        Assert.That(result, Is.False);
    }
}
