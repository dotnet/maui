using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class EnumerableExtensionsTests
    {
        /// <summary>
        /// Tests that GetChildGesturesFor returns empty sequence when elements collection is null.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_NullElements_ReturnsEmpty()
        {
            // Arrange
            IEnumerable<GestureElement> elements = null;

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor returns empty sequence when elements collection is empty.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_EmptyElements_ReturnsEmpty()
        {
            // Arrange
            var elements = new List<GestureElement>();

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor returns empty sequence when elements have no gesture recognizers.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_ElementsWithNoGestureRecognizers_ReturnsEmpty()
        {
            // Arrange
            var elements = new List<GestureElement>
            {
                new GestureElement(),
                new GestureElement()
            };

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor returns empty sequence when gesture recognizers don't match the target type.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_ElementsWithNonMatchingGestureTypes_ReturnsEmpty()
        {
            // Arrange
            var element = new GestureElement();
            element.GestureRecognizers.Add(new OtherGestureRecognizer());
            var elements = new List<GestureElement> { element };

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor returns matching gestures when type matches and predicate is null.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_MatchingGesturesWithNullPredicate_ReturnsAllMatching()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 1 };
            var gesture2 = new TestGestureRecognizer { Value = 2 };
            var otherGesture = new OtherGestureRecognizer();

            var element1 = new GestureElement();
            element1.GestureRecognizers.Add(gesture1);
            element1.GestureRecognizers.Add(otherGesture);

            var element2 = new GestureElement();
            element2.GestureRecognizers.Add(gesture2);

            var elements = new List<GestureElement> { element1, element2 };

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(gesture1, result);
            Assert.Contains(gesture2, result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor filters gestures correctly when predicate is provided.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_MatchingGesturesWithPredicate_ReturnsFilteredResults()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 1 };
            var gesture2 = new TestGestureRecognizer { Value = 2 };
            var gesture3 = new TestGestureRecognizer { Value = 3 };

            var element = new GestureElement();
            element.GestureRecognizers.Add(gesture1);
            element.GestureRecognizers.Add(gesture2);
            element.GestureRecognizers.Add(gesture3);

            var elements = new List<GestureElement> { element };

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>(g => g.Value > 1).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(gesture2, result);
            Assert.Contains(gesture3, result);
            Assert.DoesNotContain(gesture1, result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor returns empty sequence when all matching gestures fail the predicate.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_MatchingGesturesFailingPredicate_ReturnsEmpty()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 1 };
            var gesture2 = new TestGestureRecognizer { Value = 2 };

            var element = new GestureElement();
            element.GestureRecognizers.Add(gesture1);
            element.GestureRecognizers.Add(gesture2);

            var elements = new List<GestureElement> { element };

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>(g => g.Value > 10);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor works with multiple elements containing multiple matching gestures.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_MultipleElementsWithMultipleMatchingGestures_ReturnsAllMatching()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 1 };
            var gesture2 = new TestGestureRecognizer { Value = 2 };
            var gesture3 = new TestGestureRecognizer { Value = 3 };
            var gesture4 = new TestGestureRecognizer { Value = 4 };
            var otherGesture = new OtherGestureRecognizer();

            var element1 = new GestureElement();
            element1.GestureRecognizers.Add(gesture1);
            element1.GestureRecognizers.Add(otherGesture);
            element1.GestureRecognizers.Add(gesture2);

            var element2 = new GestureElement();
            element2.GestureRecognizers.Add(gesture3);

            var element3 = new GestureElement();
            element3.GestureRecognizers.Add(otherGesture);
            element3.GestureRecognizers.Add(gesture4);

            var elements = new List<GestureElement> { element1, element2, element3 };

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>().ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Contains(gesture1, result);
            Assert.Contains(gesture2, result);
            Assert.Contains(gesture3, result);
            Assert.Contains(gesture4, result);
        }

        /// <summary>
        /// Tests that GetChildGesturesFor preserves the order of gestures as they appear in the elements.
        /// </summary>
        [Fact]
        public void GetChildGesturesFor_MultipleGestures_PreservesOrder()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 1 };
            var gesture2 = new TestGestureRecognizer { Value = 2 };
            var gesture3 = new TestGestureRecognizer { Value = 3 };

            var element1 = new GestureElement();
            element1.GestureRecognizers.Add(gesture1);
            element1.GestureRecognizers.Add(gesture2);

            var element2 = new GestureElement();
            element2.GestureRecognizers.Add(gesture3);

            var elements = new List<GestureElement> { element1, element2 };

            // Act
            var result = elements.GetChildGesturesFor<TestGestureRecognizer>().ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(gesture1, result[0]);
            Assert.Equal(gesture2, result[1]);
            Assert.Equal(gesture3, result[2]);
        }

        private class TestGestureRecognizer : GestureRecognizer
        {
            public int Value { get; set; }
        }

        private class OtherGestureRecognizer : GestureRecognizer
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// Tests that GetGesturesFor returns empty enumerable when gestures parameter is null.
        /// This test specifically targets the uncovered yield break statement.
        /// </summary>
        [Fact]
        public void GetGesturesFor_NullGestures_ReturnsEmptyEnumerable()
        {
            // Arrange
            IEnumerable<IGestureRecognizer> gestures = null;

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetGesturesFor returns empty enumerable when gestures collection is empty.
        /// </summary>
        [Fact]
        public void GetGesturesFor_EmptyCollection_ReturnsEmptyEnumerable()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>();

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetGesturesFor returns matching gestures of the specified type.
        /// </summary>
        [Fact]
        public void GetGesturesFor_MatchingGestures_ReturnsFilteredResults()
        {
            // Arrange
            var gestureRecognizer1 = new GestureRecognizer();
            var gestureRecognizer2 = new GestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gestureRecognizer1, gestureRecognizer2 };

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(gestureRecognizer1, result);
            Assert.Contains(gestureRecognizer2, result);
        }

        /// <summary>
        /// Tests that GetGesturesFor returns empty enumerable when no gestures match the specified type.
        /// </summary>
        [Fact]
        public void GetGesturesFor_NonMatchingGestures_ReturnsEmptyEnumerable()
        {
            // Arrange
            var mockGesture = Substitute.For<IGestureRecognizer>();
            var gestures = new List<IGestureRecognizer> { mockGesture };

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetGesturesFor filters correctly when collection contains mixed gesture types.
        /// </summary>
        [Fact]
        public void GetGesturesFor_MixedGestures_ReturnsOnlyMatchingType()
        {
            // Arrange
            var gestureRecognizer = new GestureRecognizer();
            var mockGesture = Substitute.For<IGestureRecognizer>();
            var gestures = new List<IGestureRecognizer> { gestureRecognizer, mockGesture };

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>().ToList();

            // Assert
            Assert.Single(result);
            Assert.Contains(gestureRecognizer, result);
        }

        /// <summary>
        /// Tests that GetGesturesFor returns all matching gestures when predicate is null.
        /// </summary>
        [Fact]
        public void GetGesturesFor_WithNullPredicate_ReturnsAllMatchingType()
        {
            // Arrange
            var gestureRecognizer1 = new GestureRecognizer();
            var gestureRecognizer2 = new GestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gestureRecognizer1, gestureRecognizer2 };

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>(predicate: null).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(gestureRecognizer1, result);
            Assert.Contains(gestureRecognizer2, result);
        }

        /// <summary>
        /// Tests that GetGesturesFor includes gesture when predicate returns true.
        /// </summary>
        [Fact]
        public void GetGesturesFor_WithPredicateReturningTrue_ReturnsGesture()
        {
            // Arrange
            var gestureRecognizer = new GestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gestureRecognizer };
            Func<GestureRecognizer, bool> predicate = g => true;

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>(predicate).ToList();

            // Assert
            Assert.Single(result);
            Assert.Contains(gestureRecognizer, result);
        }

        /// <summary>
        /// Tests that GetGesturesFor excludes gesture when predicate returns false.
        /// </summary>
        [Fact]
        public void GetGesturesFor_WithPredicateReturningFalse_ReturnsEmptyEnumerable()
        {
            // Arrange
            var gestureRecognizer = new GestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gestureRecognizer };
            Func<GestureRecognizer, bool> predicate = g => false;

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>(predicate);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetGesturesFor applies predicate filtering correctly with mixed results.
        /// </summary>
        [Fact]
        public void GetGesturesFor_WithSelectivePredicate_ReturnsFilteredResults()
        {
            // Arrange
            var gestureRecognizer1 = new GestureRecognizer();
            var gestureRecognizer2 = new GestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gestureRecognizer1, gestureRecognizer2 };
            Func<GestureRecognizer, bool> predicate = g => ReferenceEquals(g, gestureRecognizer1);

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>(predicate).ToList();

            // Assert
            Assert.Single(result);
            Assert.Contains(gestureRecognizer1, result);
            Assert.DoesNotContain(gestureRecognizer2, result);
        }

        /// <summary>
        /// Tests that GetGesturesFor creates a defensive copy and doesn't modify original collection.
        /// </summary>
        [Fact]
        public void GetGesturesFor_CreatesDefensiveCopy_DoesNotModifyOriginal()
        {
            // Arrange
            var gestureRecognizer = new GestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gestureRecognizer };
            var originalCount = gestures.Count;

            // Act
            var result = gestures.GetGesturesFor<GestureRecognizer>().ToList();
            gestures.Clear(); // Modify original after calling GetGesturesFor

            // Assert
            Assert.Equal(originalCount, 1);
            Assert.Single(result); // Result should still contain the gesture
            Assert.Empty(gestures); // Original collection is now empty
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns false when the gestures collection is null.
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_NullGestures_ReturnsFalse()
        {
            // Arrange
            IEnumerable<IGestureRecognizer> gestures = null;

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns false when the gestures collection is empty.
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_EmptyGestures_ReturnsFalse()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>();

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns false when no gestures match the specified type T.
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_NoMatchingType_ReturnsFalse()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>
            {
                new AnotherTestGestureRecognizer(),
                Substitute.For<IGestureRecognizer>()
            };

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns true when there are gestures matching the specified type T.
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_HasMatchingType_ReturnsTrue()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>
            {
                Substitute.For<IGestureRecognizer>(),
                new TestGestureRecognizer(),
                new AnotherTestGestureRecognizer()
            };

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns false when there are matching types but the predicate filters them all out.
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_MatchingTypeButPredicateFilteredOut_ReturnsFalse()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>
            {
                new TestGestureRecognizer { IsEnabled = true },
                new TestGestureRecognizer { IsEnabled = true }
            };

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>(g => g.IsEnabled == false);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns true when there are matching types and the predicate allows at least one.
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_MatchingTypeAndPredicateAllows_ReturnsTrue()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>
            {
                new TestGestureRecognizer { IsEnabled = false },
                new TestGestureRecognizer { IsEnabled = true },
                new AnotherTestGestureRecognizer()
            };

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>(g => g.IsEnabled == true);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns true when there are multiple matching types (first match is sufficient).
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_MultipleMatchingTypes_ReturnsTrue()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>
            {
                new TestGestureRecognizer { IsEnabled = true },
                new TestGestureRecognizer { IsEnabled = false },
                new TestGestureRecognizer { IsEnabled = true }
            };

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor works correctly with null predicate (should match any gesture of type T).
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_NullPredicate_MatchesAnyOfType()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>
            {
                Substitute.For<IGestureRecognizer>(),
                new TestGestureRecognizer { IsEnabled = false }
            };

            // Act
            bool result = gestures.HasAnyGesturesFor<TestGestureRecognizer>(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasAnyGesturesFor returns false when predicate throws an exception.
        /// This tests the behavior when predicate evaluation fails.
        /// </summary>
        [Fact]
        public void HasAnyGesturesFor_PredicateThrowsException_PropagatesException()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>
            {
                new TestGestureRecognizer { IsEnabled = true }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                gestures.HasAnyGesturesFor<TestGestureRecognizer>(g => throw new InvalidOperationException("Test exception")));
        }

        private class AnotherTestGestureRecognizer : GestureRecognizer
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault returns null when the gestures collection is null.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_NullGestures_ReturnsNull()
        {
            // Arrange
            IEnumerable<IGestureRecognizer> gestures = null;

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault returns null when the gestures collection is empty.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_EmptyGestures_ReturnsNull()
        {
            // Arrange
            var gestures = new List<IGestureRecognizer>();

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault returns null when no gestures match the target type.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_NoMatchingType_ReturnsNull()
        {
            // Arrange
            var gesture1 = Substitute.For<IGestureRecognizer>();
            var gesture2 = new AnotherTestGestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gesture1, gesture2 };

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault returns the first matching gesture when predicate is null.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_MatchingTypeNullPredicate_ReturnsFirstMatch()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer();
            var gesture2 = Substitute.For<IGestureRecognizer>();
            var gesture3 = new TestGestureRecognizer();
            var gestures = new List<IGestureRecognizer> { gesture1, gesture2, gesture3 };

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>();

            // Assert
            Assert.Same(gesture1, result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault returns the first gesture matching both type and predicate.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_MatchingTypeAndPredicate_ReturnsFirstMatch()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 1 };
            var gesture2 = new TestGestureRecognizer { Value = 2 };
            var gesture3 = new TestGestureRecognizer { Value = 3 };
            var gestures = new List<IGestureRecognizer> { gesture1, gesture2, gesture3 };

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>(g => g.Value >= 2);

            // Assert
            Assert.Same(gesture2, result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault returns null when matching type exists but predicate returns false for all.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_MatchingTypeButPredicateFalse_ReturnsNull()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 1 };
            var gesture2 = new TestGestureRecognizer { Value = 2 };
            var gestures = new List<IGestureRecognizer> { gesture1, gesture2 };

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>(g => g.Value > 10);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault returns the first match when multiple gestures match the predicate.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_MultipleMatches_ReturnsFirst()
        {
            // Arrange
            var gesture1 = new TestGestureRecognizer { Value = 5 };
            var gesture2 = new AnotherTestGestureRecognizer();
            var gesture3 = new TestGestureRecognizer { Value = 10 };
            var gesture4 = new TestGestureRecognizer { Value = 15 };
            var gestures = new List<IGestureRecognizer> { gesture1, gesture2, gesture3, gesture4 };

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>(g => g.Value >= 10);

            // Assert
            Assert.Same(gesture3, result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault handles mixed IGestureRecognizer implementations correctly.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_MixedImplementations_HandlesCorrectly()
        {
            // Arrange
            var mockGesture = Substitute.For<IGestureRecognizer>();
            var concreteGesture = new TestGestureRecognizer { Value = 42 };
            var anotherGesture = new AnotherTestGestureRecognizer();
            var gestures = new List<IGestureRecognizer> { mockGesture, concreteGesture, anotherGesture };

            // Act
            var result = gestures.FirstGestureOrDefault<TestGestureRecognizer>();

            // Assert
            Assert.Same(concreteGesture, result);
        }

        /// <summary>
        /// Tests that FirstGestureOrDefault correctly handles predicate that throws exception.
        /// </summary>
        [Fact]
        public void FirstGestureOrDefault_PredicateThrows_PropagatesException()
        {
            // Arrange
            var gesture = new TestGestureRecognizer { Value = 1 };
            var gestures = new List<IGestureRecognizer> { gesture };
            Func<TestGestureRecognizer, bool> throwingPredicate = g => throw new InvalidOperationException("Test exception");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => gestures.FirstGestureOrDefault(throwingPredicate));
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns false when the elements collection is null.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_NullElements_ReturnsFalse()
        {
            // Arrange
            IEnumerable<GestureElement> elements = null;

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns false when the elements collection is empty.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_EmptyElements_ReturnsFalse()
        {
            // Arrange
            var elements = new List<GestureElement>();

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns false when elements contain no matching gesture recognizers.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_ElementsWithNoMatchingGestures_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var otherGesture = Substitute.For<IGestureRecognizer>();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { otherGesture });
            var elements = new List<GestureElement> { element };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns false when elements have empty gesture recognizer collections.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_ElementsWithEmptyGestureRecognizers_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer>());
            var elements = new List<GestureElement> { element };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns true when elements contain matching gesture recognizer and no predicate is provided.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_ElementsWithMatchingGestureNoPredicate_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var targetGesture = new TestGestureRecognizer();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture });
            var elements = new List<GestureElement> { element };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns true when elements contain matching gesture recognizer and predicate returns true.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_ElementsWithMatchingGesturePredicateTrue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var targetGesture = new TestGestureRecognizer();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture });
            var elements = new List<GestureElement> { element };
            Func<TestGestureRecognizer, bool> predicate = g => true;

            // Act
            bool result = elements.HasChildGesturesFor(predicate);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns false when elements contain matching gesture recognizer but predicate returns false.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_ElementsWithMatchingGesturePredicateFalse_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var targetGesture = new TestGestureRecognizer();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture });
            var elements = new List<GestureElement> { element };
            Func<TestGestureRecognizer, bool> predicate = g => false;

            // Act
            bool result = elements.HasChildGesturesFor(predicate);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns true when multiple elements exist and one contains matching gesture.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_MultipleElementsOneWithMatchingGesture_ReturnsTrue()
        {
            // Arrange
            var element1 = Substitute.For<GestureElement>();
            element1.GestureRecognizers.Returns(new List<IGestureRecognizer>());

            var element2 = Substitute.For<GestureElement>();
            var targetGesture = new TestGestureRecognizer();
            element2.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture });

            var elements = new List<GestureElement> { element1, element2 };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns true for first matching gesture and stops iteration.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_MultipleMatchingGestures_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var targetGesture1 = new TestGestureRecognizer();
            var targetGesture2 = new TestGestureRecognizer();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture1, targetGesture2 });
            var elements = new List<GestureElement> { element };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor works with mixed gesture recognizer types.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_MixedGestureTypes_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var otherGesture = Substitute.For<IGestureRecognizer>();
            var targetGesture = new TestGestureRecognizer();
            var anotherGesture = Substitute.For<IGestureRecognizer>();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { otherGesture, targetGesture, anotherGesture });
            var elements = new List<GestureElement> { element };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns false when predicate is null and no matching gestures exist.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_NullPredicateNoMatchingGestures_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var otherGesture = Substitute.For<IGestureRecognizer>();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { otherGesture });
            var elements = new List<GestureElement> { element };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns true when predicate is null and matching gesture exists.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_NullPredicateWithMatchingGesture_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var targetGesture = new TestGestureRecognizer();
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture });
            var elements = new List<GestureElement> { element };

            // Act
            bool result = elements.HasChildGesturesFor<TestGestureRecognizer>(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor works with predicate that checks specific properties.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_PredicateWithSpecificCondition_ReturnsExpectedResult()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var targetGesture = new TestGestureRecognizer { IsEnabled = true };
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture });
            var elements = new List<GestureElement> { element };
            Func<TestGestureRecognizer, bool> predicate = g => g.IsEnabled;

            // Act
            bool result = elements.HasChildGesturesFor(predicate);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasChildGesturesFor returns false when predicate condition is not met.
        /// </summary>
        [Fact]
        public void HasChildGesturesFor_PredicateConditionNotMet_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<GestureElement>();
            var targetGesture = new TestGestureRecognizer { IsEnabled = false };
            element.GestureRecognizers.Returns(new List<IGestureRecognizer> { targetGesture });
            var elements = new List<GestureElement> { element };
            Func<TestGestureRecognizer, bool> predicate = g => g.IsEnabled;

            // Act
            bool result = elements.HasChildGesturesFor(predicate);

            // Assert
            Assert.False(result);
        }
    }
}