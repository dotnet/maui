using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
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

    /// <summary>
    /// Tests EnableDiagnostics property when build_metadata.additionalfiles.EnableDiagnostics is explicitly set to true.
    /// Should return true when the EnableDiagnostics metadata is set to "true".
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void EnableDiagnostics_WhenMetadataExplicitlyEnabled_ReturnsTrue()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup IsTrue to return true for EnableDiagnostics metadata
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = "true";
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        var result = projectItem.EnableDiagnostics;

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests EnableDiagnostics property when build_metadata.additionalfiles.EnableDiagnostics is explicitly set to false.
    /// Should return false when the EnableDiagnostics metadata is set to "false".
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void EnableDiagnostics_WhenMetadataExplicitlyDisabled_ReturnsFalse()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup to return false for IsTrue but true for IsFalse
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = "false";
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        var result = projectItem.EnableDiagnostics;

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests EnableDiagnostics property with case-insensitive "true" values.
    /// Should return true for various case combinations of "true" in metadata.
    /// </summary>
    [TestCase("true")]
    [TestCase("TRUE")]
    [TestCase("True")]
    [TestCase("tRuE")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void EnableDiagnostics_WhenMetadataEnabledWithDifferentCasing_ReturnsTrue(string enableValue)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = enableValue;
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        var result = projectItem.EnableDiagnostics;

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests EnableDiagnostics property with case-insensitive "false" values.
    /// Should return false for various case combinations of "false" in metadata.
    /// </summary>
    [TestCase("false")]
    [TestCase("FALSE")]
    [TestCase("False")]
    [TestCase("fAlSe")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void EnableDiagnostics_WhenMetadataDisabledWithDifferentCasing_ReturnsFalse(string disableValue)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = disableValue;
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        var result = projectItem.EnableDiagnostics;

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests EnableDiagnostics property fallback behavior when metadata is not set and MauiXamlDiagnostics is enabled.
    /// Should return true when EnableDiagnostics metadata is not present and MauiXamlDiagnostics property is enabled.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void EnableDiagnostics_WhenMetadataNotSetAndMauiXamlDiagnosticsEnabled_ReturnsTrue()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup metadata to not exist
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out It.Ref<string?>.IsAny))
            .Returns(false);

        // Setup fallback property to return true
        mockOptions.Setup(o => o.TryGetValue("build_property.EnableMauiXamlDiagnostics", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = "true";
                return true;
            });

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        var result = projectItem.EnableDiagnostics;

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests EnableDiagnostics property fallback behavior when metadata is not set and MauiXamlDiagnostics is disabled.
    /// Should return false when EnableDiagnostics metadata is not present and MauiXamlDiagnostics property is disabled.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void EnableDiagnostics_WhenMetadataNotSetAndMauiXamlDiagnosticsDisabled_ReturnsFalse()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup metadata to not exist
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out It.Ref<string?>.IsAny))
            .Returns(false);

        // Setup fallback property to return false
        mockOptions.Setup(o => o.TryGetValue("build_property.EnableMauiXamlDiagnostics", out It.Ref<string?>.IsAny))
            .Returns(false);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        var result = projectItem.EnableDiagnostics;

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests EnableDiagnostics property when metadata has unrecognized value.
    /// Should fall back to MauiXamlDiagnostics setting when metadata value is neither "true" nor "false".
    /// </summary>
    [TestCase("invalid", true)]
    [TestCase("yes", false)]
    [TestCase("1", true)]
    [TestCase("0", false)]
    [TestCase("", true)]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void EnableDiagnostics_WhenMetadataHasUnrecognizedValue_FallsBackToMauiXamlDiagnostics(string metadataValue, bool mauiXamlDiagnosticsEnabled)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup metadata to have unrecognized value
        mockOptions.Setup(o => o.TryGetValue("build_metadata.additionalfiles.EnableDiagnostics", out It.Ref<string?>.IsAny))
            .Returns((string key, out string? value) =>
            {
                value = metadataValue;
                return true;
            });

        // Setup fallback property
        if (mauiXamlDiagnosticsEnabled)
        {
            mockOptions.Setup(o => o.TryGetValue("build_property.EnableMauiXamlDiagnostics", out It.Ref<string?>.IsAny))
                .Returns((string key, out string? value) =>
                {
                    value = "true";
                    return true;
                });
        }
        else
        {
            mockOptions.Setup(o => o.TryGetValue("build_property.EnableMauiXamlDiagnostics", out It.Ref<string?>.IsAny))
                .Returns(false);
        }

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

        // Act
        var result = projectItem.EnableDiagnostics;

        // Assert
        Assert.That(result, Is.EqualTo(mauiXamlDiagnosticsEnabled));
    }
}
