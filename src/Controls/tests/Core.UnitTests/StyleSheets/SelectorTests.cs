#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.StyleSheets;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.StyleSheets.UnitTests
{

    public class SelectorTests
    {
        IStyleSelectable Page;

        IStyleSelectable StackLayout => Page.Children.First();

        IStyleSelectable Label0 => StackLayout.Children.Skip(0).First();

        IStyleSelectable Label1 => StackLayout.Children.Skip(1).First();

        IStyleSelectable Label2 => ContentView0.Children.First();

        IStyleSelectable Label3 => StackLayout.Children.Skip(3).First();

        IStyleSelectable Label4 => StackLayout.Children.Skip(4).First();

        IStyleSelectable ContentView0 => StackLayout.Children.Skip(2).First();


        public SelectorTests()
        {
            Page = new MockStylable
            {
                NameAndBases = new[] { "Page", "Layout", "VisualElement" },
                Children = new List<IStyleSelectable> {
                    new MockStylable {
                        NameAndBases = new[] { "StackLayout", "Layout", "VisualElement" },
                        Children = new List<IStyleSelectable> {
                            new MockStylable {NameAndBases = new[] { "Label", "VisualElement" }, Classes = new[]{"test"}},				//Label0
							new MockStylable {NameAndBases = new[] { "Label", "VisualElement" }},										//Label1
							new MockStylable {														//ContentView0
								NameAndBases = new[] { "ContentView", "Layout", "VisualElement" },
                                Classes = new[]{"test"},
                                Children = new List<IStyleSelectable> {
                                    new MockStylable {NameAndBases = new[] { "Label", "VisualElement" }, Classes = new[]{"test"}},		//Label2
								}
                            },
                            new MockStylable {NameAndBases = new[] { "Label", "VisualElement" }, Id="foo"},							//Label3
							new MockStylable {NameAndBases = new[] { "Label", "VisualElement" }},										//Label4
						}
                    }
                }
            };
            SetParents(Page);
        }

        void SetParents(IStyleSelectable stylable, IStyleSelectable parent = null)
        {
            ((MockStylable)stylable).Parent = parent;
            if (stylable.Children == null)
                return;
            foreach (var s in stylable.Children)
                SetParents(s, stylable);
        }

        [Theory]
        [InlineData("label", true, true, true, true, true, false)]
        [InlineData(" label", true, true, true, true, true, false)]
        [InlineData("label ", true, true, true, true, true, false)]
        [InlineData(".test", true, false, true, false, false, true)]
        [InlineData("label.test", true, false, true, false, false, false)]
        [InlineData("stacklayout>label.test", true, false, false, false, false, false)]
        [InlineData("stacklayout >label.test", true, false, false, false, false, false)]
        [InlineData("stacklayout> label.test", true, false, false, false, false, false)]
        [InlineData("stacklayout label.test", true, false, true, false, false, false)]
        [InlineData("stacklayout  label.test", true, false, true, false, false, false)]
        [InlineData("stacklayout .test", true, false, true, false, false, true)]
        [InlineData("stacklayout.test", false, false, false, false, false, false)]
        [InlineData("*", true, true, true, true, true, true)]
        [InlineData("#foo", false, false, false, true, false, false)]
        [InlineData("label#foo", false, false, false, true, false, false)]
        [InlineData("div#foo", false, false, false, false, false, false)]
        [InlineData(".test,#foo", true, false, true, true, false, true)]
        [InlineData(".test ,#foo", true, false, true, true, false, true)]
        [InlineData(".test, #foo", true, false, true, true, false, true)]
        [InlineData("#foo,.test", true, false, true, true, false, true)]
        [InlineData("#foo ,.test", true, false, true, true, false, true)]
        [InlineData("#foo, .test", true, false, true, true, false, true)]
        [InlineData("contentview+label", false, false, false, true, false, false)]
        [InlineData("contentview +label", false, false, false, true, false, false)]
        [InlineData("contentview+ label", false, false, false, true, false, false)]
        [InlineData("contentview~label", false, false, false, true, true, false)]
        [InlineData("contentview ~label", false, false, false, true, true, false)]
        [InlineData("contentview\r\n~label", false, false, false, true, true, false)]
        [InlineData("contentview~ label", false, false, false, true, true, false)]
        [InlineData("label~*", false, true, false, true, true, true)]
        [InlineData("label~.test", false, false, false, false, false, true)]
        [InlineData("label~#foo", false, false, false, true, false, false)]
        [InlineData("page contentview stacklayout label", false, false, false, false, false, false)]
        [InlineData("page stacklayout contentview label", false, false, true, false, false, false)]
        [InlineData("page contentview label", false, false, true, false, false, false)]
        [InlineData("page contentview>label", false, false, true, false, false, false)]
        [InlineData("page>stacklayout contentview label", false, false, true, false, false, false)]
        [InlineData("page stacklayout>contentview label", false, false, true, false, false, false)]
        [InlineData("page stacklayout contentview>label", false, false, true, false, false, false)]
        [InlineData("page>stacklayout>contentview label", false, false, true, false, false, false)]
        [InlineData("page>stack/* comment * */layout>contentview label", false, false, true, false, false, false)]
        [InlineData("page>stacklayout contentview>label", false, false, true, false, false, false)]
        [InlineData("page stacklayout>contentview>label", false, false, true, false, false, false)]
        [InlineData("page>stacklayout>contentview>label", false, false, true, false, false, false)]
        [InlineData("visualelement", false, false, false, false, false, false)]
        [InlineData("^visualelement", true, true, true, true, true, true)]
        [InlineData("^layout", false, false, false, false, false, true)]
        [InlineData("stacklayout visualelement", false, false, false, false, false, false)]
        [InlineData("stacklayout>visualelement", false, false, false, false, false, false)]
        [InlineData("stacklayout ^visualelement", true, true, true, true, true, true)]
        [InlineData("stacklayout>^visualelement", true, true, false, true, true, true)]
        public void TestCase(string selectorString, bool label0match, bool label1match, bool label2match, bool label3match, bool label4match, bool content0match)
        {
            var selector = Selector.Parse(new CssReader(new StringReader(selectorString)));
            Assert.Equal(label0match, selector.Matches(Label0));
            Assert.Equal(label1match, selector.Matches(Label1));
            Assert.Equal(label2match, selector.Matches(Label2));
            Assert.Equal(label3match, selector.Matches(Label3));
            Assert.Equal(label4match, selector.Matches(Label4));
            Assert.Equal(content0match, selector.Matches(ContentView0));
        }
    }

    /// <summary>
    /// Unit tests for the Descendent selector's Matches method.
    /// </summary>
    public partial class DescendentSelectorTests
    {
        /// <summary>
        /// Tests that Matches returns false when Right selector does not match the styleable element.
        /// This tests the uncovered line where Right.Matches returns false.
        /// </summary>
        [Fact]
        public void Matches_WhenRightSelectorDoesNotMatch_ReturnsFalse()
        {
            // Arrange
            var styleable = Substitute.For<IStyleSelectable>();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();

            rightSelector.Matches(styleable).Returns(false);

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = rightSelector
            };

            // Act
            var result = descendentSelector.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when Right selector matches but there are no parents.
        /// Tests the case where styleable.Parent is null.
        /// </summary>
        [Fact]
        public void Matches_WhenRightMatchesButNoParents_ReturnsFalse()
        {
            // Arrange
            var styleable = Substitute.For<IStyleSelectable>();
            styleable.Parent.Returns((IStyleSelectable)null);

            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();

            rightSelector.Matches(styleable).Returns(true);

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = rightSelector
            };

            // Act
            var result = descendentSelector.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when Right selector matches but Left selector never matches any parent.
        /// Tests the scenario where we traverse the entire parent hierarchy without finding a match.
        /// </summary>
        [Fact]
        public void Matches_WhenRightMatchesButLeftNeverMatchesParents_ReturnsFalse()
        {
            // Arrange
            var grandparent = Substitute.For<IStyleSelectable>();
            grandparent.Parent.Returns((IStyleSelectable)null);

            var parent = Substitute.For<IStyleSelectable>();
            parent.Parent.Returns(grandparent);

            var styleable = Substitute.For<IStyleSelectable>();
            styleable.Parent.Returns(parent);

            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();

            rightSelector.Matches(styleable).Returns(true);
            leftSelector.Matches(parent).Returns(false);
            leftSelector.Matches(grandparent).Returns(false);

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = rightSelector
            };

            // Act
            var result = descendentSelector.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(parent);
            leftSelector.Received(1).Matches(grandparent);
        }

        /// <summary>
        /// Tests that Matches returns true when Right selector matches and Left selector matches immediate parent.
        /// Tests the successful case where the first parent matches.
        /// </summary>
        [Fact]
        public void Matches_WhenRightMatchesAndLeftMatchesImmediateParent_ReturnsTrue()
        {
            // Arrange
            var parent = Substitute.For<IStyleSelectable>();
            parent.Parent.Returns((IStyleSelectable)null);

            var styleable = Substitute.For<IStyleSelectable>();
            styleable.Parent.Returns(parent);

            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();

            rightSelector.Matches(styleable).Returns(true);
            leftSelector.Matches(parent).Returns(true);

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = rightSelector
            };

            // Act
            var result = descendentSelector.Matches(styleable);

            // Assert
            Assert.True(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(parent);
        }

        /// <summary>
        /// Tests that Matches returns true when Right selector matches and Left selector matches ancestor (not immediate parent).
        /// Tests the case where we need to traverse multiple levels to find a matching ancestor.
        /// </summary>
        [Fact]
        public void Matches_WhenRightMatchesAndLeftMatchesDistantAncestor_ReturnsTrue()
        {
            // Arrange
            var greatGrandparent = Substitute.For<IStyleSelectable>();
            greatGrandparent.Parent.Returns((IStyleSelectable)null);

            var grandparent = Substitute.For<IStyleSelectable>();
            grandparent.Parent.Returns(greatGrandparent);

            var parent = Substitute.For<IStyleSelectable>();
            parent.Parent.Returns(grandparent);

            var styleable = Substitute.For<IStyleSelectable>();
            styleable.Parent.Returns(parent);

            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();

            rightSelector.Matches(styleable).Returns(true);
            leftSelector.Matches(parent).Returns(false);
            leftSelector.Matches(grandparent).Returns(false);
            leftSelector.Matches(greatGrandparent).Returns(true);

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = rightSelector
            };

            // Act
            var result = descendentSelector.Matches(styleable);

            // Assert
            Assert.True(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(parent);
            leftSelector.Received(1).Matches(grandparent);
            leftSelector.Received(1).Matches(greatGrandparent);
        }

        /// <summary>
        /// Tests that Matches stops traversing when Left selector matches and returns true early.
        /// Verifies that traversal doesn't continue once a match is found.
        /// </summary>
        [Fact]
        public void Matches_WhenLeftMatchesSecondParent_StopsTraversalAndReturnsTrue()
        {
            // Arrange
            var greatGrandparent = Substitute.For<IStyleSelectable>();
            greatGrandparent.Parent.Returns((IStyleSelectable)null);

            var grandparent = Substitute.For<IStyleSelectable>();
            grandparent.Parent.Returns(greatGrandparent);

            var parent = Substitute.For<IStyleSelectable>();
            parent.Parent.Returns(grandparent);

            var styleable = Substitute.For<IStyleSelectable>();
            styleable.Parent.Returns(parent);

            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();

            rightSelector.Matches(styleable).Returns(true);
            leftSelector.Matches(parent).Returns(false);
            leftSelector.Matches(grandparent).Returns(true);
            // greatGrandparent should never be checked since we return early

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = rightSelector
            };

            // Act
            var result = descendentSelector.Matches(styleable);

            // Assert
            Assert.True(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(parent);
            leftSelector.Received(1).Matches(grandparent);
            leftSelector.DidNotReceive().Matches(greatGrandparent);
        }

        /// <summary>
        /// Tests edge case with null styleable parameter.
        /// </summary>
        [Fact]
        public void Matches_WithNullStyleable_ThrowsNullReferenceException()
        {
            // Arrange
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = rightSelector
            };

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => descendentSelector.Matches(null));
        }

        /// <summary>
        /// Tests behavior when Right selector is null.
        /// </summary>
        [Fact]
        public void Matches_WithNullRightSelector_ThrowsNullReferenceException()
        {
            // Arrange
            var styleable = Substitute.For<IStyleSelectable>();
            var leftSelector = Substitute.For<Selector>();

            var descendentSelector = new TestableDescendent
            {
                Left = leftSelector,
                Right = null
            };

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => descendentSelector.Matches(styleable));
        }

        /// <summary>
        /// Tests behavior when Left selector is null but Right doesn't match (should not reach Left).
        /// </summary>
        [Fact]
        public void Matches_WithNullLeftSelectorButRightDoesNotMatch_ReturnsFalse()
        {
            // Arrange
            var styleable = Substitute.For<IStyleSelectable>();
            var rightSelector = Substitute.For<Selector>();

            rightSelector.Matches(styleable).Returns(false);

            var descendentSelector = new TestableDescendent
            {
                Left = null,
                Right = rightSelector
            };

            // Act
            var result = descendentSelector.Matches(styleable);

            // Assert
            Assert.False(result);
        }

    }


    /// <summary>
    /// Tests for the Sibling selector's Matches method functionality.
    /// </summary>
    public partial class SiblingTests
    {
        /// <summary>
        /// Tests that Matches returns false when the Right selector does not match the styleable.
        /// Input: Right selector that returns false for the target styleable.
        /// Expected: false (Right selector mismatch should cause immediate false return).
        /// </summary>
        [Fact]
        public void Matches_RightSelectorDoesNotMatch_ReturnsFalse()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            styleable.Parent.Returns(parent);

            rightSelector.Matches(styleable).Returns(false);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when the styleable has no parent.
        /// Input: Styleable with null Parent property.
        /// Expected: false (sibling relationships require a parent context).
        /// </summary>
        [Fact]
        public void Matches_StyleableHasNoParent_ReturnsFalse()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            styleable.Parent.Returns((IStyleSelectable)null);

            rightSelector.Matches(styleable).Returns(true);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when the styleable is not found in its parent's children.
        /// Input: Styleable that is not present in its parent's Children collection.
        /// Expected: false (inconsistent parent-child relationship should cause failure).
        /// </summary>
        [Fact]
        public void Matches_StyleableNotFoundInParentChildren_ReturnsFalse()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var otherChild = Substitute.For<IStyleSelectable>();

            styleable.Parent.Returns(parent);
            parent.Children.Returns(new List<IStyleSelectable> { otherChild });

            rightSelector.Matches(styleable).Returns(true);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when no previous sibling matches the Left selector.
        /// Input: Styleable with previous siblings, but none match the Left selector.
        /// Expected: false (no matching sibling found).
        /// </summary>
        [Fact]
        public void Matches_NoPreviousSiblingMatches_ReturnsFalse()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var sibling1 = Substitute.For<IStyleSelectable>();
            var sibling2 = Substitute.For<IStyleSelectable>();

            styleable.Parent.Returns(parent);
            parent.Children.Returns(new List<IStyleSelectable> { sibling1, sibling2, styleable });

            rightSelector.Matches(styleable).Returns(true);
            leftSelector.Matches(sibling1).Returns(false);
            leftSelector.Matches(sibling2).Returns(false);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(sibling1);
            leftSelector.Received(1).Matches(sibling2);
        }

        /// <summary>
        /// Tests that Matches returns true when a previous sibling matches the Left selector.
        /// Input: Styleable with a previous sibling that matches the Left selector.
        /// Expected: true (matching sibling found).
        /// </summary>
        [Fact]
        public void Matches_PreviousSiblingMatches_ReturnsTrue()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var matchingSibling = Substitute.For<IStyleSelectable>();
            var otherSibling = Substitute.For<IStyleSelectable>();

            styleable.Parent.Returns(parent);
            parent.Children.Returns(new List<IStyleSelectable> { matchingSibling, otherSibling, styleable });

            rightSelector.Matches(styleable).Returns(true);
            leftSelector.Matches(matchingSibling).Returns(true);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.True(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(matchingSibling);
            leftSelector.DidNotReceive().Matches(otherSibling);
        }

        /// <summary>
        /// Tests that Matches returns false when the styleable is the first child (no previous siblings).
        /// Input: Styleable that is the first child in its parent's children collection.
        /// Expected: false (no previous siblings to check).
        /// </summary>
        [Fact]
        public void Matches_StyleableIsFirstChild_ReturnsFalse()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();

            styleable.Parent.Returns(parent);
            parent.Children.Returns(new List<IStyleSelectable> { styleable });

            rightSelector.Matches(styleable).Returns(true);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns true when multiple previous siblings exist and one matches.
        /// Input: Styleable with multiple previous siblings where the second one matches Left selector.
        /// Expected: true (should find the matching sibling and return true immediately).
        /// </summary>
        [Fact]
        public void Matches_MultiplePrefiousSiblingsOneMatches_ReturnsTrue()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var sibling1 = Substitute.For<IStyleSelectable>();
            var sibling2 = Substitute.For<IStyleSelectable>();
            var sibling3 = Substitute.For<IStyleSelectable>();

            styleable.Parent.Returns(parent);
            parent.Children.Returns(new List<IStyleSelectable> { sibling1, sibling2, sibling3, styleable });

            rightSelector.Matches(styleable).Returns(true);
            leftSelector.Matches(sibling1).Returns(false);
            leftSelector.Matches(sibling2).Returns(true);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.True(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(sibling1);
            leftSelector.Received(1).Matches(sibling2);
            leftSelector.DidNotReceive().Matches(sibling3);
        }

        /// <summary>
        /// Tests that Matches handles empty parent children collection correctly.
        /// Input: Parent with empty Children collection.
        /// Expected: false (styleable cannot be found in empty collection).
        /// </summary>
        [Fact]
        public void Matches_EmptyParentChildren_ReturnsFalse()
        {
            // Arrange
            var sibling = new TestSibling();
            var rightSelector = Substitute.For<Selector>();
            var leftSelector = Substitute.For<Selector>();
            sibling.Right = rightSelector;
            sibling.Left = leftSelector;

            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();

            styleable.Parent.Returns(parent);
            parent.Children.Returns(new List<IStyleSelectable>());

            rightSelector.Matches(styleable).Returns(true);

            // Act
            var result = sibling.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the Adjacent selector's Matches method.
    /// Tests the CSS adjacent sibling selector (+) functionality.
    /// </summary>
    public partial class AdjacentSelectorTests
    {
        /// <summary>
        /// Tests that Matches returns false when the Right selector does not match the styleable element.
        /// Input: styleable element where Right.Matches returns false.
        /// Expected: false without checking parent or siblings.
        /// </summary>
        [Fact]
        public void Matches_RightSelectorDoesNotMatch_ReturnsFalse()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(false);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when Right selector matches but styleable has no parent.
        /// Input: styleable element with null Parent property.
        /// Expected: false since adjacent siblings require a parent element.
        /// </summary>
        [Fact]
        public void Matches_StyleableParentIsNull_ReturnsFalse()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(true);
            styleable.Parent.Returns((IStyleSelectable)null);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when styleable is the first child (no previous sibling).
        /// Input: styleable element that is the first child in parent's children collection.
        /// Expected: false since there is no previous adjacent sibling.
        /// </summary>
        [Fact]
        public void Matches_StyleableIsFirstChild_ReturnsFalse()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var children = new List<IStyleSelectable> { styleable };

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(true);
            styleable.Parent.Returns(parent);
            parent.Children.Returns(children);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches returns false when Left selector does not match the previous sibling.
        /// Input: styleable element with a previous sibling, but Left selector returns false for that sibling.
        /// Expected: false since the adjacent sibling does not match the Left selector.
        /// </summary>
        [Fact]
        public void Matches_LeftSelectorDoesNotMatchPreviousSibling_ReturnsFalse()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();
            var previousSibling = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var children = new List<IStyleSelectable> { previousSibling, styleable };

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(true);
            styleable.Parent.Returns(parent);
            parent.Children.Returns(children);
            leftSelector.Matches(previousSibling).Returns(false);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(previousSibling);
        }

        /// <summary>
        /// Tests that Matches returns true when both Right and Left selectors match their respective elements.
        /// Input: styleable element with a previous sibling where both selectors match.
        /// Expected: true since the adjacent sibling selector pattern is satisfied.
        /// </summary>
        [Fact]
        public void Matches_BothSelectorsMatch_ReturnsTrue()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();
            var previousSibling = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var children = new List<IStyleSelectable> { previousSibling, styleable };

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(true);
            styleable.Parent.Returns(parent);
            parent.Children.Returns(children);
            leftSelector.Matches(previousSibling).Returns(true);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.True(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(previousSibling);
        }

        /// <summary>
        /// Tests that Matches works correctly with multiple siblings where styleable is in the middle.
        /// Input: styleable element as the third child with multiple siblings.
        /// Expected: true when Left selector matches the immediate previous sibling.
        /// </summary>
        [Fact]
        public void Matches_StyleableInMiddleOfMultipleSiblings_ReturnsTrue()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();
            var firstSibling = Substitute.For<IStyleSelectable>();
            var secondSibling = Substitute.For<IStyleSelectable>();
            var fourthSibling = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var children = new List<IStyleSelectable> { firstSibling, secondSibling, styleable, fourthSibling };

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(true);
            styleable.Parent.Returns(parent);
            parent.Children.Returns(children);
            leftSelector.Matches(secondSibling).Returns(true);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.True(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.Received(1).Matches(secondSibling);
        }

        /// <summary>
        /// Tests that Matches returns false when styleable is not found in parent's children collection.
        /// Input: styleable element whose parent's children collection does not contain the styleable.
        /// Expected: false since the element cannot be located among siblings.
        /// </summary>
        [Fact]
        public void Matches_StyleableNotInParentChildren_ReturnsFalse()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();
            var otherChild = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var children = new List<IStyleSelectable> { otherChild };

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(true);
            styleable.Parent.Returns(parent);
            parent.Children.Returns(children);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }

        /// <summary>
        /// Tests that Matches handles empty children collection correctly.
        /// Input: styleable element whose parent has an empty children collection.
        /// Expected: false since the styleable cannot be found among siblings.
        /// </summary>
        [Fact]
        public void Matches_EmptyChildrenCollection_ReturnsFalse()
        {
            // Arrange
            var adjacent = new Selector.Adjacent();
            var leftSelector = Substitute.For<Selector>();
            var rightSelector = Substitute.For<Selector>();
            var styleable = Substitute.For<IStyleSelectable>();
            var parent = Substitute.For<IStyleSelectable>();
            var children = new List<IStyleSelectable>();

            adjacent.Left = leftSelector;
            adjacent.Right = rightSelector;

            rightSelector.Matches(styleable).Returns(true);
            styleable.Parent.Returns(parent);
            parent.Children.Returns(children);

            // Act
            var result = adjacent.Matches(styleable);

            // Assert
            Assert.False(result);
            rightSelector.Received(1).Matches(styleable);
            leftSelector.DidNotReceive().Matches(Arg.Any<IStyleSelectable>());
        }
    }


    /// <summary>
    /// Unit tests for the SelectorSpecificity struct's Add method.
    /// </summary>
    public partial class SelectorSpecificityTests
    {
        /// <summary>
        /// Tests that Add method returns a new instance with summed values for basic positive cases.
        /// </summary>
        /// <param name="leftId">Left instance id value</param>
        /// <param name="leftClass">Left instance class value</param>
        /// <param name="leftType">Left instance type value</param>
        /// <param name="rightId">Right instance id value</param>
        /// <param name="rightClass">Right instance class value</param>
        /// <param name="rightType">Right instance type value</param>
        /// <param name="expectedId">Expected result id value</param>
        /// <param name="expectedClass">Expected result class value</param>
        /// <param name="expectedType">Expected result type value</param>
        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(1, 0, 0, 0, 0, 0, 1, 0, 0)]
        [InlineData(0, 1, 0, 0, 0, 0, 0, 1, 0)]
        [InlineData(0, 0, 1, 0, 0, 0, 0, 0, 1)]
        [InlineData(1, 1, 1, 1, 1, 1, 2, 2, 2)]
        [InlineData(5, 3, 7, 2, 4, 1, 7, 7, 8)]
        [InlineData(10, 20, 30, 5, 10, 15, 15, 30, 45)]
        public void Add_BasicCases_ReturnsCorrectSum(
            int leftId, int leftClass, int leftType,
            int rightId, int rightClass, int rightType,
            int expectedId, int expectedClass, int expectedType)
        {
            // Arrange
            var left = CreateSelectorSpecificity(leftId, leftClass, leftType);
            var right = CreateSelectorSpecificity(rightId, rightClass, rightType);

            // Act
            var result = left.Add(right);

            // Assert
            Assert.Equal(expectedId, result.Id);
            Assert.Equal(expectedClass, result.Class);
            Assert.Equal(expectedType, result.Type);
        }

        /// <summary>
        /// Tests that Add method handles negative values correctly.
        /// </summary>
        [Theory]
        [InlineData(-1, -2, -3, 1, 2, 3, 0, 0, 0)]
        [InlineData(-5, 10, -7, 3, -4, 2, -2, 6, -5)]
        [InlineData(0, 0, 0, -1, -1, -1, -1, -1, -1)]
        public void Add_NegativeValues_ReturnsCorrectSum(
            int leftId, int leftClass, int leftType,
            int rightId, int rightClass, int rightType,
            int expectedId, int expectedClass, int expectedType)
        {
            // Arrange
            var left = CreateSelectorSpecificity(leftId, leftClass, leftType);
            var right = CreateSelectorSpecificity(rightId, rightClass, rightType);

            // Act
            var result = left.Add(right);

            // Assert
            Assert.Equal(expectedId, result.Id);
            Assert.Equal(expectedClass, result.Class);
            Assert.Equal(expectedType, result.Type);
        }

        /// <summary>
        /// Tests that Add method handles integer overflow scenarios correctly.
        /// </summary>
        [Fact]
        public void Add_IntegerOverflow_WrapsAroundCorrectly()
        {
            // Arrange
            var left = CreateSelectorSpecificity(int.MaxValue, int.MaxValue, int.MaxValue);
            var right = CreateSelectorSpecificity(1, 1, 1);

            // Act
            var result = left.Add(right);

            // Assert
            Assert.Equal(int.MinValue, result.Id);
            Assert.Equal(int.MinValue, result.Class);
            Assert.Equal(int.MinValue, result.Type);
        }

        /// <summary>
        /// Tests that Add method handles integer underflow scenarios correctly.
        /// </summary>
        [Fact]
        public void Add_IntegerUnderflow_WrapsAroundCorrectly()
        {
            // Arrange
            var left = CreateSelectorSpecificity(int.MinValue, int.MinValue, int.MinValue);
            var right = CreateSelectorSpecificity(-1, -1, -1);

            // Act
            var result = left.Add(right);

            // Assert
            Assert.Equal(int.MaxValue, result.Id);
            Assert.Equal(int.MaxValue, result.Class);
            Assert.Equal(int.MaxValue, result.Type);
        }

        /// <summary>
        /// Tests that Add method returns a new instance and doesn't modify the original instances.
        /// </summary>
        [Fact]
        public void Add_DoesNotModifyOriginalInstances_ReturnsNewInstance()
        {
            // Arrange
            var left = CreateSelectorSpecificity(5, 3, 7);
            var right = CreateSelectorSpecificity(2, 4, 1);
            var originalLeftId = left.Id;
            var originalLeftClass = left.Class;
            var originalLeftType = left.Type;
            var originalRightId = right.Id;
            var originalRightClass = right.Class;
            var originalRightType = right.Type;

            // Act
            var result = left.Add(right);

            // Assert - Original instances remain unchanged
            Assert.Equal(originalLeftId, left.Id);
            Assert.Equal(originalLeftClass, left.Class);
            Assert.Equal(originalLeftType, left.Type);
            Assert.Equal(originalRightId, right.Id);
            Assert.Equal(originalRightClass, right.Class);
            Assert.Equal(originalRightType, right.Type);

            // Assert - Result is a new instance with correct values
            Assert.Equal(7, result.Id);
            Assert.Equal(7, result.Class);
            Assert.Equal(8, result.Type);
        }

        /// <summary>
        /// Tests that Add method produces the same result as the + operator.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(1, 2, 3, 4, 5, 6)]
        [InlineData(-1, -2, -3, 1, 2, 3)]
        [InlineData(int.MaxValue, 0, int.MinValue, 0, int.MaxValue, 0)]
        public void Add_ProducesSameResultAsOperatorPlus(
            int leftId, int leftClass, int leftType,
            int rightId, int rightClass, int rightType)
        {
            // Arrange
            var left = CreateSelectorSpecificity(leftId, leftClass, leftType);
            var right = CreateSelectorSpecificity(rightId, rightClass, rightType);

            // Act
            var addResult = left.Add(right);
            var operatorResult = left + right;

            // Assert
            Assert.Equal(operatorResult.Id, addResult.Id);
            Assert.Equal(operatorResult.Class, addResult.Class);
            Assert.Equal(operatorResult.Type, addResult.Type);
        }

        /// <summary>
        /// Tests that Add method works correctly when adding an instance to itself.
        /// </summary>
        [Fact]
        public void Add_SelfAddition_DoublesAllValues()
        {
            // Arrange
            var instance = CreateSelectorSpecificity(5, 10, 15);

            // Act
            var result = instance.Add(instance);

            // Assert
            Assert.Equal(10, result.Id);
            Assert.Equal(20, result.Class);
            Assert.Equal(30, result.Type);
        }

        /// <summary>
        /// Helper method to create SelectorSpecificity instances with specific values for testing.
        /// Uses combinations of selector types and operators to achieve desired values.
        /// </summary>
        /// <param name="idValue">Desired id value</param>
        /// <param name="classValue">Desired class value</param>
        /// <param name="typeValue">Desired type value</param>
        /// <returns>SelectorSpecificity instance with specified values</returns>
        private static Selector.SelectorSpecificity CreateSelectorSpecificity(int idValue, int classValue, int typeValue)
        {
            var result = new Selector.SelectorSpecificity();

            // Build up id value
            for (int i = 0; i < Math.Abs(idValue); i++)
            {
                var idSelector = new Selector.Id("test");
                var increment = Selector.SelectorSpecificity.FromSelector(idSelector);
                if (idValue >= 0)
                    result = result + increment;
                else
                    result = result + (-increment);
            }

            // Build up class value
            for (int i = 0; i < Math.Abs(classValue); i++)
            {
                var classSelector = new Selector.Class("test");
                var increment = Selector.SelectorSpecificity.FromSelector(classSelector);
                if (classValue >= 0)
                    result = result + increment;
                else
                    result = result + (-increment);
            }

            // Build up type value
            for (int i = 0; i < Math.Abs(typeValue); i++)
            {
                var elementSelector = new Selector.Element("test");
                var increment = Selector.SelectorSpecificity.FromSelector(elementSelector);
                if (typeValue >= 0)
                    result = result + increment;
                else
                    result = result + (-increment);
            }

            return result;
        }

    }
}