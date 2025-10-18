#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class TemplateUtilitiesTests
    {
        /// <summary>
        /// Tests that GetRealParentAsync throws NullReferenceException when element parameter is null.
        /// Input: null element parameter.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetRealParentAsync_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            Element element = null;

            // Act & Assert
            Assert.ThrowsAsync<NullReferenceException>(() => TemplateUtilities.GetRealParentAsync(element));
        }

        /// <summary>
        /// Tests that GetRealParentAsync returns Task.FromResult(null) when RealParent is an Application.
        /// Input: Element with RealParent set to an Application instance.
        /// Expected: Task.FromResult(null) is returned.
        /// </summary>
        [Fact]
        public async Task GetRealParentAsync_RealParentIsApplication_ReturnsTaskWithNull()
        {
            // Arrange
            var element = Substitute.For<Element>();
            var application = Substitute.For<Application>();
            element.RealParent.Returns(application);

            // Act
            var result = await TemplateUtilities.GetRealParentAsync(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetRealParentAsync returns Task.FromResult(parent) when RealParent is a non-null, non-Application Element.
        /// Input: Element with RealParent set to another Element instance (not Application).
        /// Expected: Task.FromResult(parent) is returned.
        /// </summary>
        [Fact]
        public async Task GetRealParentAsync_RealParentIsNonNullNonApplication_ReturnsTaskWithParent()
        {
            // Arrange
            var element = Substitute.For<Element>();
            var parent = Substitute.For<Element>();
            element.RealParent.Returns(parent);

            // Act
            var result = await TemplateUtilities.GetRealParentAsync(element);

            // Assert
            Assert.Same(parent, result);
        }

        /// <summary>
        /// Tests that GetRealParentAsync sets up ParentSet event handler and returns TaskCompletionSource.Task when RealParent is null.
        /// Input: Element with RealParent set to null.
        /// Expected: ParentSet event handler is attached and task waits for event.
        /// </summary>
        [Fact]
        public async Task GetRealParentAsync_RealParentIsNull_SetsUpEventHandlerAndReturnsTask()
        {
            // Arrange
            var element = Substitute.For<Element>();
            var futureParent = Substitute.For<Element>();
            element.RealParent.Returns(null, futureParent);

            // Act
            var task = TemplateUtilities.GetRealParentAsync(element);

            // Simulate the ParentSet event being raised
            element.ParentSet += Raise.Event<EventHandler>(element, EventArgs.Empty);

            var result = await task;

            // Assert
            Assert.Same(futureParent, result);
        }

        /// <summary>
        /// Tests that GetRealParentAsync works correctly when ParentSet event is raised multiple times.
        /// Input: Element with RealParent initially null, then set to a parent after ParentSet event.
        /// Expected: Task completes with the parent and event handler is properly removed.
        /// </summary>
        [Fact]
        public async Task GetRealParentAsync_ParentSetEventRaisedMultipleTimes_TaskCompletesOnFirstEvent()
        {
            // Arrange
            var element = Substitute.For<Element>();
            var parent1 = Substitute.For<Element>();
            var parent2 = Substitute.For<Element>();
            element.RealParent.Returns(null, parent1, parent2);

            // Act
            var task = TemplateUtilities.GetRealParentAsync(element);

            // Simulate ParentSet event being raised multiple times
            element.ParentSet += Raise.Event<EventHandler>(element, EventArgs.Empty);
            element.ParentSet += Raise.Event<EventHandler>(element, EventArgs.Empty);

            var result = await task;

            // Assert
            Assert.Same(parent1, result);
        }

        /// <summary>
        /// Tests that GetRealParentAsync correctly handles Application subclasses as RealParent.
        /// Input: Element with RealParent set to a derived Application class.
        /// Expected: Task.FromResult(null) is returned since it's still an Application type.
        /// </summary>
        [Fact]
        public async Task GetRealParentAsync_RealParentIsApplicationSubclass_ReturnsTaskWithNull()
        {
            // Arrange
            var element = Substitute.For<Element>();
            var applicationSubclass = Substitute.For<Application>();
            element.RealParent.Returns(applicationSubclass);

            // Act
            var result = await TemplateUtilities.GetRealParentAsync(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when the new template is null.
        /// Should clear old template content and not create new content.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_NewTemplateIsNull_DoesNotCreateContent()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var oldTemplate = Substitute.For<ControlTemplate>();
            var children = new ReadOnlyCollection<Element>(new List<Element>());

            controlTemplated.InternalChildren.Returns(children);
            controlTemplated.ControlTemplate.Returns((ControlTemplate)null);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);

            // Act
            TemplateUtilities.OnControlTemplateChanged(bindable, oldTemplate, null);

            // Assert
            controlTemplated.Received().RemoveAt(Arg.Any<int>());
            controlTemplated.DidNotReceive().AddLogicalChild(Arg.Any<Element>());
            controlTemplated.DidNotReceive().OnControlTemplateChanged(Arg.Any<ControlTemplate>(), Arg.Any<ControlTemplate>());
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when template.CreateContent() returns null.
        /// Should throw NotSupportedException.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_CreateContentReturnsNull_ThrowsNotSupportedException()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var newTemplate = Substitute.For<ControlTemplate>();
            var children = new ReadOnlyCollection<Element>(new List<Element>());

            controlTemplated.InternalChildren.Returns(children);
            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns((object)null);

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                TemplateUtilities.OnControlTemplateChanged(bindable, null, newTemplate));

            Assert.Equal("ControlTemplate must return a type derived from View.", exception.Message);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when template.CreateContent() returns a non-View object.
        /// Should throw NotSupportedException.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_CreateContentReturnsNonView_ThrowsNotSupportedException()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var newTemplate = Substitute.For<ControlTemplate>();
            var children = new ReadOnlyCollection<Element>(new List<Element>());
            var nonViewContent = new object();

            controlTemplated.InternalChildren.Returns(children);
            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns(nonViewContent);

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                TemplateUtilities.OnControlTemplateChanged(bindable, null, newTemplate));

            Assert.Equal("ControlTemplate must return a type derived from View.", exception.Message);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged with valid template that creates View content.
        /// Should successfully create and apply the template.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_ValidTemplate_CreatesAndAppliesContent()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var newTemplate = Substitute.For<ControlTemplate>();
            var oldTemplate = Substitute.For<ControlTemplate>();
            var view = Substitute.For<View>();
            var children = new ReadOnlyCollection<Element>(new List<Element>());

            controlTemplated.InternalChildren.Returns(children);
            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns(view);

            // Act
            TemplateUtilities.OnControlTemplateChanged(bindable, oldTemplate, newTemplate);

            // Assert
            controlTemplated.Received().AddLogicalChild(view);
            controlTemplated.Received().OnControlTemplateChanged(oldTemplate, newTemplate);
            controlTemplated.Received().TemplateRoot = view;
            controlTemplated.Received().OnApplyTemplate();
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when oldValue is null.
        /// Should skip cleanup phase and proceed with new template application.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_OldValueIsNull_SkipsCleanupPhase()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var newTemplate = Substitute.For<ControlTemplate>();
            var view = Substitute.For<View>();
            var children = new ReadOnlyCollection<Element>(new List<Element>());

            controlTemplated.InternalChildren.Returns(children);
            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns(view);

            // Act
            TemplateUtilities.OnControlTemplateChanged(bindable, null, newTemplate);

            // Assert
            controlTemplated.Received().AddLogicalChild(view);
            controlTemplated.Received().OnControlTemplateChanged(null, newTemplate);
            controlTemplated.Received().TemplateRoot = view;
            controlTemplated.Received().OnApplyTemplate();
        }

        /// <summary>
        /// Tests OnControlTemplateChanged with ContentPresenter children during cleanup.
        /// Should call Clear() on ContentPresenter children.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_WithContentPresenterChildren_ClearsContentPresenters()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var oldTemplate = Substitute.For<ControlTemplate>();
            var newTemplate = Substitute.For<ControlTemplate>();
            var view = Substitute.For<View>();
            var contentPresenter = Substitute.For<ContentPresenter>();
            var regularElement = Substitute.For<Element>();

            var childrenList = new List<Element> { contentPresenter, regularElement };
            var children = new ReadOnlyCollection<Element>(childrenList);
            var emptyChildren = new ReadOnlyCollection<Element>(new List<Element>());

            controlTemplated.InternalChildren.Returns(children);
            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns(view);

            // Setup LogicalChildrenInternal for the main element
            bindable.When(x => ((Element)x).LogicalChildrenInternal).DoNotCallBase();
            ((Element)bindable).LogicalChildrenInternal.Returns(children);

            // Setup LogicalChildrenInternal for child elements
            contentPresenter.LogicalChildrenInternal.Returns(emptyChildren);
            regularElement.LogicalChildrenInternal.Returns(emptyChildren);

            // Act
            TemplateUtilities.OnControlTemplateChanged(bindable, oldTemplate, newTemplate);

            // Assert
            contentPresenter.Received().Clear();
        }

        /// <summary>
        /// Tests OnControlTemplateChanged with nested IControlTemplated children.
        /// Should skip enqueuing children that have their own ControlTemplate.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_WithNestedControlTemplatedChildren_SkipsEnqueuing()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var oldTemplate = Substitute.For<ControlTemplate>();
            var newTemplate = Substitute.For<ControlTemplate>();
            var view = Substitute.For<View>();
            var nestedControlTemplated = Substitute.For<Element, IControlTemplated>();
            var nestedTemplate = Substitute.For<ControlTemplate>();

            var childrenList = new List<Element> { (Element)nestedControlTemplated };
            var children = new ReadOnlyCollection<Element>(childrenList);
            var emptyChildren = new ReadOnlyCollection<Element>(new List<Element>());

            controlTemplated.InternalChildren.Returns(children);
            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns(view);

            // Setup the nested control templated element
            ((IControlTemplated)nestedControlTemplated).ControlTemplate.Returns(nestedTemplate);
            ((Element)nestedControlTemplated).LogicalChildrenInternal.Returns(emptyChildren);

            // Setup LogicalChildrenInternal for the main element
            bindable.When(x => ((Element)x).LogicalChildrenInternal).DoNotCallBase();
            ((Element)bindable).LogicalChildrenInternal.Returns(children);

            // Act
            TemplateUtilities.OnControlTemplateChanged(bindable, oldTemplate, newTemplate);

            // Assert - The nested element should not have its children processed further
            // since it has its own ControlTemplate
            controlTemplated.Received().AddLogicalChild(view);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged when InternalChildren has multiple items to remove.
        /// Should remove all internal children before applying new template.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_WithMultipleInternalChildren_RemovesAllChildren()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var newTemplate = Substitute.For<ControlTemplate>();
            var view = Substitute.For<View>();
            var child1 = Substitute.For<Element>();
            var child2 = Substitute.For<Element>();

            var childrenList = new List<Element> { child1, child2 };
            var children = new ReadOnlyCollection<Element>(childrenList);

            // Simulate removing children one by one
            var callCount = 0;
            controlTemplated.InternalChildren.Returns(_ =>
            {
                if (callCount == 0)
                {
                    callCount++;
                    return new ReadOnlyCollection<Element>(new List<Element> { child1, child2 });
                }
                else if (callCount == 1)
                {
                    callCount++;
                    return new ReadOnlyCollection<Element>(new List<Element> { child1 });
                }
                else
                {
                    return new ReadOnlyCollection<Element>(new List<Element>());
                }
            });

            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns(view);

            // Act
            TemplateUtilities.OnControlTemplateChanged(bindable, null, newTemplate);

            // Assert
            controlTemplated.Received(2).RemoveAt(Arg.Any<int>());
            controlTemplated.Received().AddLogicalChild(view);
        }

        /// <summary>
        /// Tests OnControlTemplateChanged with both old and new template values.
        /// Should perform cleanup and then apply new template.
        /// </summary>
        [Fact]
        public void OnControlTemplateChanged_WithOldAndNewTemplate_PerformsCleanupAndAppliesNew()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject, IControlTemplated>();
            var controlTemplated = (IControlTemplated)bindable;
            var oldTemplate = Substitute.For<ControlTemplate>();
            var newTemplate = Substitute.For<ControlTemplate>();
            var view = Substitute.For<View>();
            var emptyChildren = new ReadOnlyCollection<Element>(new List<Element>());

            controlTemplated.InternalChildren.Returns(emptyChildren);
            controlTemplated.ControlTemplate.Returns(newTemplate);
            controlTemplated.RemoveAt(Arg.Any<int>()).Returns(true);
            newTemplate.CreateContent().Returns(view);

            // Setup LogicalChildrenInternal for the main element
            bindable.When(x => ((Element)x).LogicalChildrenInternal).DoNotCallBase();
            ((Element)bindable).LogicalChildrenInternal.Returns(emptyChildren);

            // Act
            TemplateUtilities.OnControlTemplateChanged(bindable, oldTemplate, newTemplate);

            // Assert
            controlTemplated.Received().AddLogicalChild(view);
            controlTemplated.Received().OnControlTemplateChanged(oldTemplate, newTemplate);
            controlTemplated.Received().TemplateRoot = view;
            controlTemplated.Received().OnApplyTemplate();
        }

        /// <summary>
        /// Tests FindTemplatedParentAsync when the element's RealParent is an Application.
        /// Should return null immediately without traversing the hierarchy.
        /// </summary>
        [Fact]
        public async Task FindTemplatedParentAsync_ElementRealParentIsApplication_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<Element>();
            var application = Substitute.For<Application>();
            element.RealParent.Returns(application);

            // Act
            var result = await TemplateUtilities.FindTemplatedParentAsync(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests FindTemplatedParentAsync when element is null.
        /// Should handle null input gracefully.
        /// </summary>
        [Fact]
        public async Task FindTemplatedParentAsync_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            Element element = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => TemplateUtilities.FindTemplatedParentAsync(element));
        }

        /// <summary>
        /// Tests FindTemplatedParentAsync when it finds an immediate templated parent with skipCount = 0.
        /// Should return the first templated element found.
        /// </summary>
        [Fact]
        public async Task FindTemplatedParentAsync_ImmediateTemplatedParent_ReturnsTemplatedElement()
        {
            // Arrange
            var childElement = Substitute.For<Element>();
            var templatedParent = Substitute.For<Element, IControlTemplated>();
            var controlTemplate = Substitute.For<ControlTemplate>();

            // Set up the hierarchy: childElement -> templatedParent
            childElement.RealParent.Returns((Element)null); // Not Application
            ((IControlTemplated)templatedParent).ControlTemplate.Returns(controlTemplate);

            // Mock the static method behavior by setting up a simple parent chain
            // Since GetRealParentAsync is static, we need to set up RealParent to simulate the async behavior
            SetupElementHierarchy(childElement, templatedParent, null);

            // Act
            var result = await TemplateUtilities.FindTemplatedParentAsync(childElement);

            // Assert
            Assert.Same(templatedParent, result);
        }

        /// <summary>
        /// Tests FindTemplatedParentAsync when element is ContentPresenter in hierarchy.
        /// Should increment skipCount when encountering ContentPresenter.
        /// </summary>
        [Fact]
        public async Task FindTemplatedParentAsync_ContentPresenterInHierarchy_SkipsTemplatedParent()
        {
            // Arrange
            var childElement = Substitute.For<Element>();
            var contentPresenter = Substitute.For<ContentPresenter>();
            var templatedParent1 = Substitute.For<Element, IControlTemplated>();
            var templatedParent2 = Substitute.For<Element, IControlTemplated>();
            var controlTemplate1 = Substitute.For<ControlTemplate>();
            var controlTemplate2 = Substitute.For<ControlTemplate>();

            // Set up the hierarchy: childElement -> contentPresenter -> templatedParent1 -> templatedParent2
            ((IControlTemplated)templatedParent1).ControlTemplate.Returns(controlTemplate1);
            ((IControlTemplated)templatedParent2).ControlTemplate.Returns(controlTemplate2);

            SetupElementHierarchy(childElement, contentPresenter, templatedParent1, templatedParent2, null);

            // Act
            var result = await TemplateUtilities.FindTemplatedParentAsync(childElement);

            // Assert
            Assert.Same(templatedParent2, result);
        }

        /// <summary>
        /// Tests FindTemplatedParentAsync when no templated parent is found in hierarchy.
        /// Should return null after traversing to Application/null.
        /// </summary>
        [Fact]
        public async Task FindTemplatedParentAsync_NoTemplatedParent_ReturnsNull()
        {
            // Arrange
            var childElement = Substitute.For<Element>();
            var regularParent = Substitute.For<Element>();

            // Set up hierarchy without any templated parents: childElement -> regularParent -> null
            SetupElementHierarchy(childElement, regularParent, null);

            // Act
            var result = await TemplateUtilities.FindTemplatedParentAsync(childElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests FindTemplatedParentAsync with multiple ContentPresenters in hierarchy.
        /// Should handle multiple skip counts correctly.
        /// </summary>
        [Fact]
        public async Task FindTemplatedParentAsync_MultipleContentPresenters_HandlesSkipCountCorrectly()
        {
            // Arrange
            var childElement = Substitute.For<Element>();
            var contentPresenter1 = Substitute.For<ContentPresenter>();
            var contentPresenter2 = Substitute.For<ContentPresenter>();
            var templatedParent1 = Substitute.For<Element, IControlTemplated>();
            var templatedParent2 = Substitute.For<Element, IControlTemplated>();
            var finalTemplatedParent = Substitute.For<Element, IControlTemplated>();

            var controlTemplate1 = Substitute.For<ControlTemplate>();
            var controlTemplate2 = Substitute.For<ControlTemplate>();
            var controlTemplate3 = Substitute.For<ControlTemplate>();

            ((IControlTemplated)templatedParent1).ControlTemplate.Returns(controlTemplate1);
            ((IControlTemplated)templatedParent2).ControlTemplate.Returns(controlTemplate2);
            ((IControlTemplated)finalTemplatedParent).ControlTemplate.Returns(controlTemplate3);

            // Hierarchy: child -> cp1 -> cp2 -> templated1 -> templated2 -> finalTemplated
            // skipCount: 0 -> 1 -> 2 -> 1 -> 0 -> return finalTemplated
            SetupElementHierarchy(childElement, contentPresenter1, contentPresenter2,
                templatedParent1, templatedParent2, finalTemplatedParent, null);

            // Act
            var result = await TemplateUtilities.FindTemplatedParentAsync(childElement);

            // Assert
            Assert.Same(finalTemplatedParent, result);
        }

        /// <summary>
        /// Tests FindTemplatedParentAsync when templated element has null ControlTemplate.
        /// Should skip elements with null ControlTemplate.
        /// </summary>
        [Fact]
        public async Task FindTemplatedParentAsync_TemplatedElementWithNullControlTemplate_SkipsElement()
        {
            // Arrange
            var childElement = Substitute.For<Element>();
            var templatedWithNullTemplate = Substitute.For<Element, IControlTemplated>();
            var templatedWithTemplate = Substitute.For<Element, IControlTemplated>();
            var controlTemplate = Substitute.For<ControlTemplate>();

            ((IControlTemplated)templatedWithNullTemplate).ControlTemplate.Returns((ControlTemplate)null);
            ((IControlTemplated)templatedWithTemplate).ControlTemplate.Returns(controlTemplate);

            SetupElementHierarchy(childElement, templatedWithNullTemplate, templatedWithTemplate, null);

            // Act
            var result = await TemplateUtilities.FindTemplatedParentAsync(childElement);

            // Assert
            Assert.Same(templatedWithTemplate, result);
        }

        /// <summary>
        /// Helper method to set up element hierarchy for testing.
        /// Simulates the behavior of GetRealParentAsync by setting up RealParent properties.
        /// </summary>
        private void SetupElementHierarchy(params Element[] elements)
        {
            for (int i = 0; i < elements.Length - 1; i++)
            {
                if (elements[i] != null && elements[i + 1] != null)
                {
                    elements[i].RealParent.Returns(elements[i + 1]);
                }
                else if (elements[i] != null)
                {
                    elements[i].RealParent.Returns((Element)null);
                }
            }

            // The last element (if not null) should have null parent or we handle it separately
            if (elements.Length > 0 && elements[elements.Length - 1] != null)
            {
                elements[elements.Length - 1].RealParent.Returns((Element)null);
            }
        }
    }
}