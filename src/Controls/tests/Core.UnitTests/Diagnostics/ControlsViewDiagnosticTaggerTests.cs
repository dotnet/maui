using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Diagnostics;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public class ControlsViewDiagnosticTaggerTests
{
    /// <summary>
    /// Tests AddTags method when source is null.
    /// Should not throw exception and should not modify the TagList.
    /// </summary>
    [Fact]
    public void AddTags_NullSource_DoesNotAddTags()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        var initialCount = tagList.Count;

        // Act
        tagger.AddTags(null, ref tagList);

        // Assert
        Assert.Equal(initialCount, tagList.Count);
    }

    /// <summary>
    /// Tests AddTags method when source is an Element but not a VisualElement.
    /// Should add element-specific tags but not visual element tags.
    /// </summary>
    [Fact]
    public void AddTags_ElementNotVisualElement_AddsElementTags()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        var element = Substitute.For<Element>();
        var testId = Guid.NewGuid();
        element.Id.Returns(testId);
        element.AutomationId.Returns("test-automation-id");
        element.ClassId.Returns("test-class-id");
        element.StyleId.Returns("test-style-id");

        // Act
        tagger.AddTags(element, ref tagList);

        // Assert
        Assert.Contains(new KeyValuePair<string, object>("element.id", testId), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.automation_id", "test-automation-id"), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.class_id", "test-class-id"), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.style_id", "test-style-id"), tagList);
    }

    /// <summary>
    /// Tests AddTags method when source is a VisualElement.
    /// Should add both element and visual element tags since VisualElement inherits from Element.
    /// </summary>
    [Fact]
    public void AddTags_VisualElement_AddsBothElementAndVisualElementTags()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        var visualElement = Substitute.For<VisualElement>();
        var testId = Guid.NewGuid();
        var testFrame = new Rect(10, 20, 100, 200);
        var testClassList = Substitute.For<IList<string>>();

        visualElement.Id.Returns(testId);
        visualElement.AutomationId.Returns("visual-automation-id");
        visualElement.ClassId.Returns("visual-class-id");
        visualElement.StyleId.Returns("visual-style-id");
        visualElement.@class.Returns(testClassList);
        visualElement.Frame.Returns(testFrame);

        // Act
        tagger.AddTags(visualElement, ref tagList);

        // Assert
        // Element tags
        Assert.Contains(new KeyValuePair<string, object>("element.id", testId), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.automation_id", "visual-automation-id"), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.class_id", "visual-class-id"), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.style_id", "visual-style-id"), tagList);
        // VisualElement tags
        Assert.Contains(new KeyValuePair<string, object>("element.class", testClassList), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.frame", testFrame), tagList);
    }

    /// <summary>
    /// Tests AddTags method when source is neither Element nor VisualElement.
    /// Should not add any tags to the TagList.
    /// </summary>
    [Fact]
    public void AddTags_NotElementOrVisualElement_DoesNotAddTags()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        var notAnElement = new object();
        var initialCount = tagList.Count;

        // Act
        tagger.AddTags(notAnElement, ref tagList);

        // Assert
        Assert.Equal(initialCount, tagList.Count);
    }

    /// <summary>
    /// Tests AddTags method with Element having null property values.
    /// Should handle null values gracefully and add them as tags.
    /// </summary>
    [Fact]
    public void AddTags_ElementWithNullProperties_HandlesNullValues()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        var element = Substitute.For<Element>();
        var testId = Guid.NewGuid();
        element.Id.Returns(testId);
        element.AutomationId.Returns((string)null);
        element.ClassId.Returns((string)null);
        element.StyleId.Returns((string)null);

        // Act
        tagger.AddTags(element, ref tagList);

        // Assert
        Assert.Contains(new KeyValuePair<string, object>("element.id", testId), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.automation_id", null), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.class_id", null), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.style_id", null), tagList);
    }

    /// <summary>
    /// Tests AddTags method with VisualElement having null property values.
    /// Should handle null values gracefully in both Element and VisualElement properties.
    /// </summary>
    [Fact]
    public void AddTags_VisualElementWithNullProperties_HandlesNullValues()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        var visualElement = Substitute.For<VisualElement>();
        var testId = Guid.NewGuid();

        visualElement.Id.Returns(testId);
        visualElement.AutomationId.Returns((string)null);
        visualElement.ClassId.Returns((string)null);
        visualElement.StyleId.Returns((string)null);
        visualElement.@class.Returns((IList<string>)null);
        visualElement.Frame.Returns(Rect.Zero);

        // Act
        tagger.AddTags(visualElement, ref tagList);

        // Assert
        Assert.Contains(new KeyValuePair<string, object>("element.id", testId), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.automation_id", null), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.class_id", null), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.style_id", null), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.class", null), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.frame", Rect.Zero), tagList);
    }

    /// <summary>
    /// Tests AddTags method with existing tags in TagList.
    /// Should add new tags without affecting existing ones.
    /// </summary>
    [Fact]
    public void AddTags_ExistingTagsInTagList_PreservesExistingTags()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        tagList.Add("existing.tag", "existing.value");
        var element = Substitute.For<Element>();
        var testId = Guid.NewGuid();
        element.Id.Returns(testId);
        element.AutomationId.Returns("test-automation");
        element.ClassId.Returns("test-class");
        element.StyleId.Returns("test-style");

        // Act
        tagger.AddTags(element, ref tagList);

        // Assert
        Assert.Contains(new KeyValuePair<string, object>("existing.tag", "existing.value"), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.id", testId), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.automation_id", "test-automation"), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.class_id", "test-class"), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.style_id", "test-style"), tagList);
    }

    /// <summary>
    /// Tests AddTags method with Element having empty string property values.
    /// Should handle empty strings and add them as tags.
    /// </summary>
    [Fact]
    public void AddTags_ElementWithEmptyStrings_HandlesEmptyStrings()
    {
        // Arrange
        var tagger = new ControlsViewDiagnosticTagger();
        var tagList = new TagList();
        var element = Substitute.For<Element>();
        var testId = Guid.NewGuid();
        element.Id.Returns(testId);
        element.AutomationId.Returns(string.Empty);
        element.ClassId.Returns(string.Empty);
        element.StyleId.Returns(string.Empty);

        // Act
        tagger.AddTags(element, ref tagList);

        // Assert
        Assert.Contains(new KeyValuePair<string, object>("element.id", testId), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.automation_id", string.Empty), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.class_id", string.Empty), tagList);
        Assert.Contains(new KeyValuePair<string, object>("element.style_id", string.Empty), tagList);
    }
}
