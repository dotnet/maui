#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;


using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class VisualStateManagerTests : IDisposable
    {
        [Fact]
        public void InitialStateIsNormalIfAvailable()
        {
            var label1 = new Label();

            VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());

            var groups1 = VisualStateManager.GetVisualStateGroups(label1);

            Assert.Equal(NormalStateName, groups1[0].CurrentState.Name);
        }

        [Fact]
        public void InitialStateIsNullIfNormalNotAvailable()
        {
            var label1 = new Label();

            VisualStateManager.SetVisualStateGroups(label1, CreateStateGroupsWithoutNormalState());

            var groups1 = VisualStateManager.GetVisualStateGroups(label1);

            Assert.Null(groups1[0].CurrentState);
        }

        [Fact]
        public void VisualElementsStateGroupsAreDistinct()
        {
            var label1 = new Label();
            var label2 = new Label();

            VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());
            VisualStateManager.SetVisualStateGroups(label2, CreateTestStateGroups());

            var groups1 = VisualStateManager.GetVisualStateGroups(label1);
            var groups2 = VisualStateManager.GetVisualStateGroups(label2);

            Assert.NotSame(groups1, groups2);

            Assert.Equal(NormalStateName, groups1[0].CurrentState.Name);
            Assert.Equal(NormalStateName, groups2[0].CurrentState.Name);

            VisualStateManager.GoToState(label1, InvalidStateName);

            Assert.Equal(InvalidStateName, groups1[0].CurrentState.Name);
            Assert.Equal(NormalStateName, groups2[0].CurrentState.Name);
        }

        [Fact]
        public void VisualStateGroupsFromSettersAreDistinct()
        {
            var x = new Setter();
            x.Property = VisualStateManager.VisualStateGroupsProperty;
            x.Value = CreateTestStateGroups();

            var label1 = new Label();
            var label2 = new Label();

            x.Apply(label1, new SetterSpecificity());
            x.Apply(label2, new SetterSpecificity());

            var groups1 = VisualStateManager.GetVisualStateGroups(label1);
            var groups2 = VisualStateManager.GetVisualStateGroups(label2);

            Assert.NotNull(groups1);
            Assert.NotNull(groups2);

            Assert.NotSame(groups1, groups2);

            Assert.Equal(NormalStateName, groups1[0].CurrentState.Name);
            Assert.Equal(NormalStateName, groups2[0].CurrentState.Name);

            VisualStateManager.GoToState(label1, InvalidStateName);

            Assert.Equal(InvalidStateName, groups1[0].CurrentState.Name);
            Assert.Equal(NormalStateName, groups2[0].CurrentState.Name);
        }

        [Fact]
        public void ElementsDoNotHaveVisualStateGroupsCollectionByDefault()
        {
            var label1 = new Label();
            Assert.False(label1.HasVisualStateGroups());
            var vsg = VisualStateManager.GetVisualStateGroups(label1);
            Assert.False(label1.HasVisualStateGroups());
            vsg.Add(new VisualStateGroup());
            Assert.True(label1.HasVisualStateGroups());
        }

        [Fact]
        public void StateNamesMustBeUniqueWithinGroup()
        {
            IList<VisualStateGroup> vsgs = CreateTestStateGroups();

            var duplicate = new VisualState { Name = NormalStateName };

            Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(duplicate));
        }

        [Fact]
        public void StateNamesMustBeUniqueWithinGroupList()
        {
            IList<VisualStateGroup> vsgs = CreateTestStateGroups();

            // Create and add a second VisualStateGroup
            var secondGroup = new VisualStateGroup { Name = "Foo" };
            vsgs.Add(secondGroup);

            // Create a VisualState with the same name as one in another group in this list
            var duplicate = new VisualState { Name = NormalStateName };

            Assert.Throws<InvalidOperationException>(() => secondGroup.States.Add(duplicate));
        }

        [Fact]
        public void StateNamesMustBeUniqueWithinGroupListWhenAddingGroup()
        {
            IList<VisualStateGroup> vsgs = CreateTestStateGroups();

            // Create and add a second VisualStateGroup
            var secondGroup = new VisualStateGroup { Name = "Foo" };

            // Create a VisualState with the same name as one in another group in the list
            var duplicate = new VisualState { Name = NormalStateName };
            secondGroup.States.Add(duplicate);

            Assert.Throws<InvalidOperationException>(() => vsgs.Add(secondGroup));
        }

        [Fact]
        public void GroupNamesMustBeUniqueWithinGroupList()
        {
            IList<VisualStateGroup> vsgs = CreateTestStateGroups();
            var secondGroup = new VisualStateGroup { Name = CommonStatesGroupName };

            Assert.Throws<InvalidOperationException>(() => vsgs.Add(secondGroup));
        }

        [Fact]
        public void StateNamesInGroupMayNotBeNull()
        {
            IList<VisualStateGroup> vsgs = CreateTestStateGroups();

            var nullStateName = new VisualState();

            Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(nullStateName));
        }

        [Fact]
        public void StateNamesInGroupMayNotBeEmpty()
        {
            IList<VisualStateGroup> vsgs = CreateTestStateGroups();

            var emptyStateName = new VisualState { Name = "" };

            Assert.Throws<InvalidOperationException>(() => vsgs[0].States.Add(emptyStateName));
        }

        [Fact]
        public void VerifyVisualStateChanges()
        {
            var label1 = new Label();
            VisualStateManager.SetVisualStateGroups(label1, CreateTestStateGroups());

            var groups1 = VisualStateManager.GetVisualStateGroups(label1);
            Assert.Equal(NormalStateName, groups1[0].CurrentState.Name);

            label1.IsEnabled = false;

            groups1 = VisualStateManager.GetVisualStateGroups(label1);
            Assert.Equal(DisabledStateName, groups1[0].CurrentState.Name);


            label1.SetValue(VisualElement.IsFocusedPropertyKey, true);
            groups1 = VisualStateManager.GetVisualStateGroups(label1);
            Assert.Equal(DisabledStateName, groups1[0].CurrentState.Name);

            label1.IsEnabled = true;
            groups1 = VisualStateManager.GetVisualStateGroups(label1);
            Assert.Equal(FocusedStateName, groups1[0].CurrentState.Name);


            label1.SetValue(VisualElement.IsFocusedPropertyKey, false);
            groups1 = VisualStateManager.GetVisualStateGroups(label1);
            Assert.Equal(NormalStateName, groups1[0].CurrentState.Name);
        }

        [Fact]
        public void VisualElementGoesToCorrectStateWhenAvailable()
        {
            var label = new Label();
            double targetBottomMargin = 1.5;

            var group = new VisualStateGroup();
            var list = new VisualStateGroupList();

            var normalState = new VisualState { Name = NormalStateName };
            normalState.Setters.Add(new Setter { Property = View.MarginBottomProperty, Value = targetBottomMargin });

            list.Add(group);
            group.States.Add(normalState);

            VisualStateManager.SetVisualStateGroups(label, list);

            Assert.Equal(label.Margin.Bottom, targetBottomMargin);
        }

        [Fact]
        public void VisualElementGoesToCorrectStateWhenAvailableFromSetter()
        {
            double targetBottomMargin = 1.5;

            var group = new VisualStateGroup();
            var list = new VisualStateGroupList();

            var normalState = new VisualState { Name = NormalStateName };
            normalState.Setters.Add(new Setter { Property = View.MarginBottomProperty, Value = targetBottomMargin });

            var x = new Setter
            {
                Property = VisualStateManager.VisualStateGroupsProperty,
                Value = list
            };

            list.Add(group);
            group.States.Add(normalState);

            var label1 = new Label();
            var label2 = new Label();

            x.Apply(label1, new SetterSpecificity());
            x.Apply(label2, new SetterSpecificity());

            Assert.Equal(label1.Margin.Bottom, targetBottomMargin);
            Assert.Equal(label2.Margin.Bottom, targetBottomMargin);
        }

        [Fact]
        public void VisualElementGoesToCorrectStateWhenSetterHasTarget()
        {
            double defaultMargin = default(double);
            double targetMargin = 1.5;

            var label1 = new Label();
            var label2 = new Label();
            INameScope nameScope = new NameScope();
            NameScope.SetNameScope(label1, nameScope);
            nameScope.RegisterName("Label1", label1);
            NameScope.SetNameScope(label2, nameScope);
            nameScope.RegisterName("Label2", label2);

            var list = new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    States =
                    {
                        new VisualState
                        {
                            Name = NormalStateName,
                            Setters =
                            {
                                new Setter { Property = View.MarginBottomProperty, Value = targetMargin },
                                new Setter { TargetName = "Label2", Property = View.MarginTopProperty, Value = targetMargin }
                            }
                        }
                    }
                }
            };

            VisualStateManager.SetVisualStateGroups(label1, list);

            Assert.Equal(label1.Margin.Top, defaultMargin);
            Assert.Equal(label1.Margin.Bottom, targetMargin);
            Assert.Equal(label1.Margin.Left, defaultMargin);

            Assert.Equal(label2.Margin.Top, targetMargin);
            Assert.Equal(label2.Margin.Bottom, defaultMargin);
        }

        [Fact]
        public void CanRemoveAStateAndAddANewStateWithTheSameName()
        {
            var stateGroups = new VisualStateGroupList();
            var visualStateGroup = new VisualStateGroup { Name = CommonStatesGroupName };
            var normalState = new VisualState { Name = NormalStateName };
            var invalidState = new VisualState { Name = InvalidStateName };

            stateGroups.Add(visualStateGroup);
            visualStateGroup.States.Add(normalState);
            visualStateGroup.States.Add(invalidState);

            var name = visualStateGroup.States[0].Name;

            visualStateGroup.States.Remove(visualStateGroup.States[0]);

            visualStateGroup.States.Add(new VisualState { Name = name });
        }

        [Fact]
        public void CanRemoveAGroupAndAddANewGroupWithTheSameName()
        {
            var stateGroups = new VisualStateGroupList();
            var visualStateGroup = new VisualStateGroup { Name = CommonStatesGroupName };
            var secondVisualStateGroup = new VisualStateGroup { Name = "Whatevs" };
            var normalState = new VisualState { Name = NormalStateName };
            var invalidState = new VisualState { Name = InvalidStateName };

            stateGroups.Add(visualStateGroup);
            visualStateGroup.States.Add(normalState);
            visualStateGroup.States.Add(invalidState);

            stateGroups.Add(secondVisualStateGroup);

            var name = stateGroups[0].Name;

            stateGroups.Remove(stateGroups[0]);

            stateGroups.Add(new VisualStateGroup { Name = name });
        }


        public VisualStateManagerTests()
        {
            AppInfo.SetCurrent(new MockAppInfo() { RequestedTheme = AppTheme.Light });
            Application.Current = new Application();
        }


        public void Dispose()
        {
            Application.Current = null;
        }

        [Fact]
        //https://github.com/dotnet/maui/issues/6251
        public void AppThemeBindingInVSM()
        {

            var label = new Label() { BackgroundColor = Colors.Red };
            var list = new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    States =
                    {
                        new VisualState { Name = NormalStateName},
                        new VisualState
                        {
                            Name = DisabledStateName,
                            Setters =
                            {
                                new Setter { Property = View.BackgroundColorProperty, Value = new AppThemeBinding{ Light=Colors.Purple, Dark=Colors.Purple, Default=Colors.Purple } },
                            }
                        }
                    }
                }
            };

            VisualStateManager.SetVisualStateGroups(label, list);

            Assert.Equal(label.BackgroundColor, Colors.Red);
            VisualStateManager.GoToState(label, DisabledStateName);
            Assert.Equal(label.BackgroundColor, Colors.Purple);
            VisualStateManager.GoToState(label, NormalStateName);
            Assert.Equal(label.BackgroundColor, Colors.Red);
        }

        [Fact]
        //https://github.com/dotnet/maui/issues/4139
        public void ChangingStyleContainingVSMShouldResetStateValue()
        {
            var label = new Label();
            var SelectedStateName = "Selected";

            var style0 = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Black},
                    new Setter {
                        Property = VisualStateManager.VisualStateGroupsProperty,
                        Value = new VisualStateGroupList {
                            new VisualStateGroup {
                                States = {
                                    new VisualState { Name = NormalStateName },
                                    new VisualState {
                                        Name = SelectedStateName,
                                        Setters = { new Setter { Property = Label.TextColorProperty, Value=Colors.Red } }
                                    }
                                }
                            }
                        }
                    },
                }
            };

            var style1 = new Style(typeof(Label))
            {
                Setters = {
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Black},
                    new Setter {
                        Property = VisualStateManager.VisualStateGroupsProperty,
                        Value = new VisualStateGroupList {
                            new VisualStateGroup {
                                States = {
                                    new VisualState { Name = NormalStateName },
                                    new VisualState {
                                        Name = SelectedStateName,
                                        Setters = { new Setter { Property = Label.TextColorProperty, Value=Colors.Cyan } }
                                    }
                                }
                            }
                        }
                    },
                }
            };

            label.Style = style0;
            Assert.Equal(label.TextColor, Colors.Black);
            VisualStateManager.GoToState(label, SelectedStateName);
            Assert.Equal(label.TextColor, Colors.Red);
            label.Style = style1;
            Assert.Equal(label.TextColor, Colors.Black);
        }

        [Fact]
        //https://github.com/dotnet/maui/issues/6857
        public void VSMFromStyleAreUnApplied()
        {
            var label = new Label
            {
                Style = new Style(typeof(Label))
                {
                    Setters = {
                        new Setter {
                            Property = VisualStateManager.VisualStateGroupsProperty,
                            Value = new VisualStateGroupList {
                                new VisualStateGroup {
                                    States = {
                                        new VisualState { Name = NormalStateName },
                                    }
                                }
                            }
                        },
                    }
                }
            };

            Assert.NotNull(VisualStateManager.GetVisualStateGroups(label));
            Assert.Single(VisualStateManager.GetVisualStateGroups(label)); //the list applied by style has one group			
            label.ClearValue(Label.StyleProperty); //clear the style
            Assert.Empty(VisualStateManager.GetVisualStateGroups(label)); //default style (created by defaultValueCreator) has no groups
        }

        [Fact]
        //https://github.com/dotnet/maui/issues/6885
        public void UnapplyingVSMShouldUnapplySetters()
        {
            var label = new Label();
            var SelectedStateName = "Selected";

            VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList {
                new VisualStateGroup {
                    States = {
                        new VisualState {
                            Name = SelectedStateName,
                            Setters = { new Setter { Property=Label.TextColorProperty, Value=Colors.HotPink} }
                        }
                    }
                }
            });

            VisualStateManager.GoToState(label, SelectedStateName);
            Assert.Equal(label.TextColor, Colors.HotPink);
            label.ClearValue(VisualStateManager.VisualStateGroupsProperty);
            Assert.Empty(VisualStateManager.GetVisualStateGroups(label)); //default style (created by defaultValueCreator) has no groups
            Assert.False(label.TextColor == Colors.HotPink); //setter should be unapplied
        }

        [Theory(Skip = "This test was created to check performance characteristics; leaving it in because it may be useful again.")]
        [InlineData(1, 10)]
        [InlineData(1, 10000)]
        [InlineData(10, 100)]
        [InlineData(10, 10000)]
        public void ValidatePerformance(int groups, int states)
        {
            IList<VisualStateGroup> vsgs = new VisualStateGroupList();

            var groupList = new List<VisualStateGroup>();

            for (int n = 0; n < groups; n++)
            {
                groupList.Add(new VisualStateGroup { Name = n.ToString() });
            }

            var watch = new Stopwatch();

            watch.Start();

            foreach (var group in groupList)
            {
                vsgs.Add(group);
            }

            watch.Stop();

            double iterations = states;
            var random = new Random();

            for (int n = 0; n < iterations; n++)
            {
                var state = new VisualState { Name = n.ToString() };
                var group = groupList[random.Next(0, groups - 1)];
                watch.Start();
                group.States.Add(state);
                watch.Stop();
            }

            var average = watch.ElapsedMilliseconds / iterations;

            Debug.WriteLine($">>>>> VisualStateManagerTests ValidatePerformance: {watch.ElapsedMilliseconds}ms over {iterations} iterations; average of {average}ms");

        }
    }

    public partial class VisualStateGroupListTests
    {
        /// <summary>
        /// Tests that the VisualStateGroupList constructor correctly initializes with isDefault parameter set to true.
        /// Verifies that the IsDefault property is set to true and the internal list is properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_WithIsDefaultTrue_SetsIsDefaultPropertyCorrectly()
        {
            // Arrange & Act
            var visualStateGroupList = new VisualStateGroupList(isDefault: true);

            // Assert
            Assert.True(visualStateGroupList.IsDefault);
            Assert.Equal(0, visualStateGroupList.Count);
            Assert.False(visualStateGroupList.IsReadOnly);
        }

        /// <summary>
        /// Tests that the VisualStateGroupList constructor correctly initializes with isDefault parameter set to false.
        /// Verifies that the IsDefault property is set to false and the internal list is properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_WithIsDefaultFalse_SetsIsDefaultPropertyCorrectly()
        {
            // Arrange & Act
            var visualStateGroupList = new VisualStateGroupList(isDefault: false);

            // Assert
            Assert.False(visualStateGroupList.IsDefault);
            Assert.Equal(0, visualStateGroupList.Count);
            Assert.False(visualStateGroupList.IsReadOnly);
        }

        /// <summary>
        /// Tests that the VisualStateGroupList constructor with boolean parameter creates a functional IList implementation.
        /// Verifies that the created object can be used as a generic collection and supports basic list operations.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_WithBooleanParameter_CreatesFunctionalList(bool isDefault)
        {
            // Arrange & Act
            var visualStateGroupList = new VisualStateGroupList(isDefault);

            // Assert
            Assert.NotNull(visualStateGroupList);
            Assert.Equal(isDefault, visualStateGroupList.IsDefault);
            Assert.Equal(0, visualStateGroupList.Count);
            Assert.False(visualStateGroupList.IsReadOnly);

            // Verify it implements IList<VisualStateGroup>
            Assert.IsAssignableFrom<IList<VisualStateGroup>>(visualStateGroupList);

            // Verify basic enumeration works (empty collection)
            Assert.Empty(visualStateGroupList);
        }

        /// <summary>
        /// Tests that the VisualStateGroupList constructor creates an object that supports enumeration.
        /// Verifies that both generic and non-generic enumerators work correctly for an empty list.
        /// </summary>
        [Fact]
        public void Constructor_CreatesEnumerableObject()
        {
            // Arrange & Act
            var visualStateGroupList = new VisualStateGroupList(false);

            // Assert
            var genericEnumerator = visualStateGroupList.GetEnumerator();
            Assert.NotNull(genericEnumerator);

            var nonGenericEnumerator = ((IEnumerable)visualStateGroupList).GetEnumerator();
            Assert.NotNull(nonGenericEnumerator);

            // Verify empty enumeration
            Assert.False(genericEnumerator.MoveNext());
            Assert.False(nonGenericEnumerator.MoveNext());
        }

        /// <summary>
        /// Tests that the IsReadOnly property always returns false for VisualStateGroupList instances,
        /// regardless of how the instance was constructed (default constructor or with isDefault parameter).
        /// This verifies that VisualStateGroupList collections are always modifiable.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsReadOnly_WithAnyConstructorParameter_ReturnsFalse(bool isDefault)
        {
            // Arrange
            var visualStateGroupList = new VisualStateGroupList(isDefault);

            // Act
            var isReadOnly = visualStateGroupList.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns false when using the default constructor.
        /// This verifies that VisualStateGroupList collections created with the parameterless constructor are modifiable.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithDefaultConstructor_ReturnsFalse()
        {
            // Arrange
            var visualStateGroupList = new VisualStateGroupList();

            // Act
            var isReadOnly = visualStateGroupList.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests the IndexOf method with a null item parameter.
        /// Should return -1 when searching for null.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var visualStateGroupList = new VisualStateGroupList();
            var group = new VisualStateGroup { Name = "TestGroup" };
            visualStateGroupList.Add(group);

            // Act
            int result = visualStateGroupList.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests the IndexOf method with an item that doesn't exist in the list.
        /// Should return -1 when the item is not found.
        /// </summary>
        [Fact]
        public void IndexOf_ItemNotInList_ReturnsMinusOne()
        {
            // Arrange
            var visualStateGroupList = new VisualStateGroupList();
            var existingGroup = new VisualStateGroup { Name = "ExistingGroup" };
            var nonExistentGroup = new VisualStateGroup { Name = "NonExistentGroup" };
            visualStateGroupList.Add(existingGroup);

            // Act
            int result = visualStateGroupList.IndexOf(nonExistentGroup);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests the IndexOf method with an empty list.
        /// Should return -1 when searching in an empty list.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyList_ReturnsMinusOne()
        {
            // Arrange
            var visualStateGroupList = new VisualStateGroupList();
            var group = new VisualStateGroup { Name = "TestGroup" };

            // Act
            int result = visualStateGroupList.IndexOf(group);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests the IndexOf method with items at various positions in the list.
        /// Should return the correct zero-based index for each item's position.
        /// </summary>
        [Theory]
        [InlineData(0, "FirstGroup")]
        [InlineData(1, "SecondGroup")]
        [InlineData(2, "ThirdGroup")]
        public void IndexOf_ItemAtVariousPositions_ReturnsCorrectIndex(int expectedIndex, string groupName)
        {
            // Arrange
            var visualStateGroupList = new VisualStateGroupList();
            var firstGroup = new VisualStateGroup { Name = "FirstGroup" };
            var secondGroup = new VisualStateGroup { Name = "SecondGroup" };
            var thirdGroup = new VisualStateGroup { Name = "ThirdGroup" };

            visualStateGroupList.Add(firstGroup);
            visualStateGroupList.Add(secondGroup);
            visualStateGroupList.Add(thirdGroup);

            var targetGroup = groupName switch
            {
                "FirstGroup" => firstGroup,
                "SecondGroup" => secondGroup,
                "ThirdGroup" => thirdGroup,
                _ => throw new ArgumentException("Invalid group name")
            };

            // Act
            int result = visualStateGroupList.IndexOf(targetGroup);

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests the IndexOf method with the same item added multiple times.
        /// Should return the index of the first occurrence.
        /// </summary>
        [Fact]
        public void IndexOf_SameItemMultipleTimes_ReturnsFirstOccurrenceIndex()
        {
            // Arrange
            var visualStateGroupList = new VisualStateGroupList();
            var group = new VisualStateGroup { Name = "TestGroup" };
            var otherGroup = new VisualStateGroup { Name = "OtherGroup" };

            visualStateGroupList.Add(group);
            visualStateGroupList.Add(otherGroup);
            visualStateGroupList.Add(group); // Add same group again

            // Act
            int result = visualStateGroupList.IndexOf(group);

            // Assert
            Assert.Equal(0, result); // Should return index of first occurrence
        }
    }


    /// <summary>
    /// Tests for WatchAddList constructor functionality.
    /// </summary>
    public partial class WatchAddListTests
    {
        /// <summary>
        /// Tests that the WatchAddList constructor properly initializes with a valid Action parameter.
        /// Verifies that the object is created successfully and the internal list is properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_ValidAction_InitializesSuccessfully()
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();

            // Act
            var watchAddList = new WatchAddList<string>(mockAction);

            // Assert
            Assert.NotNull(watchAddList);
            Assert.Equal(0, watchAddList.Count);
            Assert.False(watchAddList.IsReadOnly);
        }

        /// <summary>
        /// Tests that the WatchAddList constructor properly handles a null Action parameter.
        /// Verifies that the object is created successfully even with null Action.
        /// </summary>
        [Fact]
        public void Constructor_NullAction_InitializesSuccessfully()
        {
            // Arrange
            Action<List<string>> nullAction = null;

            // Act
            var watchAddList = new WatchAddList<string>(nullAction);

            // Assert
            Assert.NotNull(watchAddList);
            Assert.Equal(0, watchAddList.Count);
            Assert.False(watchAddList.IsReadOnly);
        }

        /// <summary>
        /// Tests that the WatchAddList constructor works with different generic type parameters.
        /// Verifies that the constructor is properly generic and works with various types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(VisualElement))]
        public void Constructor_DifferentGenericTypes_InitializesSuccessfully(Type genericType)
        {
            // Arrange & Act & Assert
            if (genericType == typeof(int))
            {
                var mockAction = Substitute.For<Action<List<int>>>();
                var watchAddList = new WatchAddList<int>(mockAction);

                Assert.NotNull(watchAddList);
                Assert.Equal(0, watchAddList.Count);
            }
            else if (genericType == typeof(object))
            {
                var mockAction = Substitute.For<Action<List<object>>>();
                var watchAddList = new WatchAddList<object>(mockAction);

                Assert.NotNull(watchAddList);
                Assert.Equal(0, watchAddList.Count);
            }
            else if (genericType == typeof(VisualElement))
            {
                var mockAction = Substitute.For<Action<List<VisualElement>>>();
                var watchAddList = new WatchAddList<VisualElement>(mockAction);

                Assert.NotNull(watchAddList);
                Assert.Equal(0, watchAddList.Count);
            }
        }

        /// <summary>
        /// Tests that the WatchAddList constructor properly stores the Action parameter
        /// by verifying the Action is called when items are added to the list.
        /// </summary>
        [Fact]
        public void Constructor_ValidAction_ActionIsCalledOnAdd()
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(mockAction);

            // Act
            watchAddList.Add("test item");

            // Assert
            mockAction.Received(1).Invoke(Arg.Any<List<string>>());
        }

        /// <summary>
        /// Tests that the WatchAddList constructor with null Action parameter
        /// does not throw exceptions when operations that would call the Action are performed.
        /// </summary>
        [Fact]
        public void Constructor_NullAction_NoExceptionOnAdd()
        {
            // Arrange
            Action<List<string>> nullAction = null;
            var watchAddList = new WatchAddList<string>(nullAction);

            // Act & Assert - Should not throw NullReferenceException
            var exception = Record.Exception(() => watchAddList.Add("test item"));
            Assert.Null(exception);
            Assert.Equal(1, watchAddList.Count);
        }

        /// <summary>
        /// Tests that the WatchAddList constructor properly initializes the internal collection
        /// by verifying enumeration works correctly on a newly created instance.
        /// </summary>
        [Fact]
        public void Constructor_ValidAction_EnumerationWorksCorrectly()
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();

            // Act
            var watchAddList = new WatchAddList<string>(mockAction);

            // Assert
            var enumerator = watchAddList.GetEnumerator();
            Assert.NotNull(enumerator);

            var itemCount = 0;
            foreach (var item in watchAddList)
            {
                itemCount++;
            }
            Assert.Equal(0, itemCount);
        }

        /// <summary>
        /// Tests that the IsReadOnly property always returns false for WatchAddList with string generic type.
        /// Verifies the list is always modifiable regardless of its state.
        /// Expected result: IsReadOnly should return false.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithStringType_ReturnsFalse()
        {
            // Arrange
            var watchAddList = new WatchAddList<string>(list => { });

            // Act
            var result = watchAddList.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsReadOnly property always returns false for WatchAddList with integer generic type.
        /// Verifies the property behavior is consistent across different generic types.
        /// Expected result: IsReadOnly should return false.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithIntType_ReturnsFalse()
        {
            // Arrange
            var watchAddList = new WatchAddList<int>(list => { });

            // Act
            var result = watchAddList.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns false for WatchAddList with complex object generic type.
        /// Verifies the property behavior is consistent with reference types.
        /// Expected result: IsReadOnly should return false.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithComplexObjectType_ReturnsFalse()
        {
            // Arrange
            var watchAddList = new WatchAddList<VisualState>(list => { });

            // Act
            var result = watchAddList.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsReadOnly property remains false even after adding items to the list.
        /// Verifies the property is not affected by the list's content state.
        /// Expected result: IsReadOnly should return false both before and after adding items.
        /// </summary>
        [Fact]
        public void IsReadOnly_AfterAddingItems_StillReturnsFalse()
        {
            // Arrange
            var watchAddList = new WatchAddList<string>(list => { });
            var initialReadOnlyState = watchAddList.IsReadOnly;

            // Act
            watchAddList.Add("test1");
            watchAddList.Add("test2");
            var finalReadOnlyState = watchAddList.IsReadOnly;

            // Assert
            Assert.False(initialReadOnlyState);
            Assert.False(finalReadOnlyState);
        }

        /// <summary>
        /// Tests that the IsReadOnly property remains false even after clearing the list.
        /// Verifies the property is not affected by list operations.
        /// Expected result: IsReadOnly should return false before, during, and after list modifications.
        /// </summary>
        [Fact]
        public void IsReadOnly_AfterClearingList_StillReturnsFalse()
        {
            // Arrange
            var watchAddList = new WatchAddList<string>(list => { });
            watchAddList.Add("test");
            var beforeClearState = watchAddList.IsReadOnly;

            // Act
            watchAddList.Clear();
            var afterClearState = watchAddList.IsReadOnly;

            // Assert
            Assert.False(beforeClearState);
            Assert.False(afterClearState);
        }

        /// <summary>
        /// Tests that multiple WatchAddList instances all return false for IsReadOnly property.
        /// Verifies consistency across different instances with different callback actions.
        /// Expected result: All instances should return false for IsReadOnly.
        /// </summary>
        [Fact]
        public void IsReadOnly_MultipleInstances_AllReturnFalse()
        {
            // Arrange
            var list1 = new WatchAddList<string>(list => { });
            var list2 = new WatchAddList<int>(list => { });
            var list3 = new WatchAddList<object>(list => throw new InvalidOperationException("Should not be called"));

            // Act & Assert
            Assert.False(list1.IsReadOnly);
            Assert.False(list2.IsReadOnly);
            Assert.False(list3.IsReadOnly);
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns false when cast to IList interface.
        /// Verifies the property behavior through interface polymorphism.
        /// Expected result: IsReadOnly should return false when accessed through IList interface.
        /// </summary>
        [Fact]
        public void IsReadOnly_ThroughIListInterface_ReturnsFalse()
        {
            // Arrange
            IList<string> watchAddList = new WatchAddList<string>(list => { });

            // Act
            var result = watchAddList.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns false when cast to non-generic IList interface.
        /// Verifies the property behavior through non-generic interface polymorphism.
        /// Expected result: IsReadOnly should return false when accessed through non-generic IList interface.
        /// </summary>
        [Fact]
        public void IsReadOnly_ThroughNonGenericIListInterface_ReturnsFalse()
        {
            // Arrange
            IList watchAddList = new WatchAddList<string>(list => { });

            // Act
            var result = watchAddList.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that RemoveAt removes the item at the specified valid index.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_RemovesItem()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("item1");
            watchList.Add("item2");
            watchList.Add("item3");

            // Act
            watchList.RemoveAt(1);

            // Assert
            Assert.Equal(2, watchList.Count);
            Assert.Equal("item1", watchList[0]);
            Assert.Equal("item3", watchList[1]);
            Assert.False(watchList.Contains("item2"));
        }

        /// <summary>
        /// Tests that RemoveAt removes the first item when index is 0.
        /// </summary>
        [Fact]
        public void RemoveAt_FirstIndex_RemovesFirstItem()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("first");
            watchList.Add("second");

            // Act
            watchList.RemoveAt(0);

            // Assert
            Assert.Equal(1, watchList.Count);
            Assert.Equal("second", watchList[0]);
            Assert.False(watchList.Contains("first"));
        }

        /// <summary>
        /// Tests that RemoveAt removes the last item when index is Count - 1.
        /// </summary>
        [Fact]
        public void RemoveAt_LastIndex_RemovesLastItem()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("first");
            watchList.Add("last");

            // Act
            watchList.RemoveAt(1);

            // Assert
            Assert.Equal(1, watchList.Count);
            Assert.Equal("first", watchList[0]);
            Assert.False(watchList.Contains("last"));
        }

        /// <summary>
        /// Tests that RemoveAt makes the list empty when removing the only item.
        /// </summary>
        [Fact]
        public void RemoveAt_SingleItem_MakesListEmpty()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("onlyItem");

            // Act
            watchList.RemoveAt(0);

            // Assert
            Assert.Equal(0, watchList.Count);
            Assert.False(watchList.Contains("onlyItem"));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException for negative index.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("item");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => watchList.RemoveAt(negativeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException for index equal to or greater than Count.
        /// </summary>
        [Theory]
        [InlineData(0)] // Empty list
        [InlineData(1)] // Index equals Count for single item
        [InlineData(2)] // Index greater than Count for single item
        [InlineData(int.MaxValue)]
        public void RemoveAt_IndexOutOfBounds_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            if (index > 0)
            {
                watchList.Add("item");
            }

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => watchList.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called on an empty list.
        /// </summary>
        [Fact]
        public void RemoveAt_EmptyList_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => watchList.RemoveAt(0));
        }

        /// <summary>
        /// Tests that RemoveAt correctly updates enumeration after removal.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_UpdatesEnumeration()
        {
            // Arrange
            var watchList = new WatchAddList<int>(_ => { });
            watchList.Add(10);
            watchList.Add(20);
            watchList.Add(30);
            watchList.Add(40);

            // Act
            watchList.RemoveAt(2); // Remove 30

            // Assert
            var items = watchList.ToList();
            Assert.Equal(3, items.Count);
            Assert.Equal(new[] { 10, 20, 40 }, items);
        }

        /// <summary>
        /// Tests that RemoveAt works correctly with multiple removals.
        /// </summary>
        [Fact]
        public void RemoveAt_MultipleRemovals_MaintainsCorrectState()
        {
            // Arrange
            var watchList = new WatchAddList<char>(_ => { });
            watchList.Add('A');
            watchList.Add('B');
            watchList.Add('C');
            watchList.Add('D');
            watchList.Add('E');

            // Act
            watchList.RemoveAt(4); // Remove 'E'
            watchList.RemoveAt(0); // Remove 'A'
            watchList.RemoveAt(1); // Remove 'C' (was at index 2, now at 1)

            // Assert
            Assert.Equal(2, watchList.Count);
            Assert.Equal('B', watchList[0]);
            Assert.Equal('D', watchList[1]);
        }

        /// <summary>
        /// Tests IndexOf method with various items to verify it returns correct index when item exists.
        /// Tests string reference type items at different positions in the list.
        /// Expected result: Returns the zero-based index of the first occurrence of the item.
        /// </summary>
        [Theory]
        [InlineData("first", 0)]
        [InlineData("second", 1)]
        [InlineData("third", 2)]
        public void IndexOf_ItemExists_ReturnsCorrectIndex(string item, int expectedIndex)
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();
            var watchList = new WatchAddList<string>(mockAction);
            watchList.Add("first");
            watchList.Add("second");
            watchList.Add("third");

            // Act
            var result = watchList.IndexOf(item);

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOf method when the item does not exist in the list.
        /// Tests with both empty list and populated list scenarios.
        /// Expected result: Returns -1 when item is not found.
        /// </summary>
        [Theory]
        [InlineData("nonexistent")]
        [InlineData("missing")]
        [InlineData("")]
        public void IndexOf_ItemNotFound_ReturnsMinusOne(string item)
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();
            var watchList = new WatchAddList<string>(mockAction);
            watchList.Add("existing1");
            watchList.Add("existing2");

            // Act
            var result = watchList.IndexOf(item);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method with null item for reference types.
        /// Tests both when null exists in the list and when it doesn't.
        /// Expected result: Returns correct index when null is found, -1 when not found.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsCorrectResult()
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();
            var watchList = new WatchAddList<string>(mockAction);
            watchList.Add("first");
            watchList.Add(null);
            watchList.Add("third");

            // Act
            var result = watchList.IndexOf(null);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests IndexOf method with null item when null does not exist in the list.
        /// Expected result: Returns -1 when null is not found.
        /// </summary>
        [Fact]
        public void IndexOf_NullItemNotFound_ReturnsMinusOne()
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();
            var watchList = new WatchAddList<string>(mockAction);
            watchList.Add("first");
            watchList.Add("second");

            // Act
            var result = watchList.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method on an empty list.
        /// Expected result: Returns -1 for any item when list is empty.
        /// </summary>
        [Theory]
        [InlineData("anyitem")]
        [InlineData("")]
        public void IndexOf_EmptyList_ReturnsMinusOne(string item)
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();
            var watchList = new WatchAddList<string>(mockAction);

            // Act
            var result = watchList.IndexOf(item);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method when there are multiple occurrences of the same item.
        /// Expected result: Returns the index of the first occurrence.
        /// </summary>
        [Fact]
        public void IndexOf_MultipleOccurrences_ReturnsFirstIndex()
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<string>>>();
            var watchList = new WatchAddList<string>(mockAction);
            watchList.Add("duplicate");
            watchList.Add("other");
            watchList.Add("duplicate");
            watchList.Add("duplicate");

            // Act
            var result = watchList.IndexOf("duplicate");

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests IndexOf method with value types (integers).
        /// Tests finding existing values and non-existing values.
        /// Expected result: Returns correct index for existing values, -1 for non-existing.
        /// </summary>
        [Theory]
        [InlineData(10, 0)]
        [InlineData(20, 1)]
        [InlineData(30, 2)]
        [InlineData(999, -1)]
        [InlineData(int.MinValue, -1)]
        [InlineData(int.MaxValue, -1)]
        public void IndexOf_ValueType_ReturnsExpectedIndex(int item, int expectedIndex)
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<int>>>();
            var watchList = new WatchAddList<int>(mockAction);
            watchList.Add(10);
            watchList.Add(20);
            watchList.Add(30);

            // Act
            var result = watchList.IndexOf(item);

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOf method with special integer values including zero and negative numbers.
        /// Expected result: Returns correct index when values exist, -1 when they don't.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, 1)]
        [InlineData(-100, 2)]
        public void IndexOf_SpecialIntegerValues_ReturnsCorrectIndex(int item, int expectedIndex)
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<int>>>();
            var watchList = new WatchAddList<int>(mockAction);
            watchList.Add(0);
            watchList.Add(-1);
            watchList.Add(-100);

            // Act
            var result = watchList.IndexOf(item);

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOf method with custom reference type objects.
        /// Tests object equality and reference equality scenarios.
        /// Expected result: Returns correct index when objects are found, -1 when not found.
        /// </summary>
        [Fact]
        public void IndexOf_CustomReferenceType_ReturnsCorrectIndex()
        {
            // Arrange
            var mockAction = Substitute.For<Action<List<object>>>();
            var watchList = new WatchAddList<object>(mockAction);
            var obj1 = new object();
            var obj2 = new object();
            var obj3 = new object();

            watchList.Add(obj1);
            watchList.Add(obj2);
            watchList.Add(obj3);

            // Act & Assert
            Assert.Equal(0, watchList.IndexOf(obj1));
            Assert.Equal(1, watchList.IndexOf(obj2));
            Assert.Equal(2, watchList.IndexOf(obj3));
            Assert.Equal(-1, watchList.IndexOf(new object()));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies all elements from the internal list to the destination array.
        /// Input: WatchAddList with multiple elements, valid array and index.
        /// Expected: All elements are copied to the destination array starting at the specified index.
        /// </summary>
        [Fact]
        public void CopyTo_WithValidArrayAndIndex_CopiesAllElements()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);
            watchAddList.Add("Item1");
            watchAddList.Add("Item2");
            watchAddList.Add("Item3");

            var destinationArray = new string[5];
            var arrayIndex = 1;

            // Act
            watchAddList.CopyTo(destinationArray, arrayIndex);

            // Assert
            Assert.Null(destinationArray[0]);
            Assert.Equal("Item1", destinationArray[1]);
            Assert.Equal("Item2", destinationArray[2]);
            Assert.Equal("Item3", destinationArray[3]);
            Assert.Null(destinationArray[4]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies elements from an empty list.
        /// Input: Empty WatchAddList, valid array and index.
        /// Expected: No elements are copied, array remains unchanged.
        /// </summary>
        [Fact]
        public void CopyTo_WithEmptyList_DoesNotModifyArray()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);

            var destinationArray = new string[] { "Existing1", "Existing2", "Existing3" };
            var originalArray = (string[])destinationArray.Clone();

            // Act
            watchAddList.CopyTo(destinationArray, 0);

            // Assert
            Assert.Equal(originalArray, destinationArray);
        }

        /// <summary>
        /// Tests that CopyTo copies elements starting at index 0.
        /// Input: WatchAddList with elements, valid array with index 0.
        /// Expected: Elements are copied starting from the beginning of the array.
        /// </summary>
        [Fact]
        public void CopyTo_WithZeroIndex_CopiesFromBeginning()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<int>>>();
            var watchAddList = new WatchAddList<int>(onAddAction);
            watchAddList.Add(10);
            watchAddList.Add(20);

            var destinationArray = new int[3];

            // Act
            watchAddList.CopyTo(destinationArray, 0);

            // Assert
            Assert.Equal(10, destinationArray[0]);
            Assert.Equal(20, destinationArray[1]);
            Assert.Equal(0, destinationArray[2]);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array is null.
        /// Input: WatchAddList with elements, null array.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithNullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);
            watchAddList.Add("Item1");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => watchAddList.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Input: WatchAddList with elements, valid array, negative index.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);
            watchAddList.Add("Item1");

            var destinationArray = new string[3];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => watchAddList.CopyTo(destinationArray, -1));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is insufficient space in destination array.
        /// Input: WatchAddList with multiple elements, small array that cannot fit all elements.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithInsufficientSpace_ThrowsArgumentException()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);
            watchAddList.Add("Item1");
            watchAddList.Add("Item2");
            watchAddList.Add("Item3");

            var destinationArray = new string[2]; // Too small for 3 items

            // Act & Assert
            Assert.Throws<ArgumentException>(() => watchAddList.CopyTo(destinationArray, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is beyond array bounds.
        /// Input: WatchAddList with elements, valid array, index equal to array length.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithIndexEqualToArrayLength_ThrowsArgumentException()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);
            watchAddList.Add("Item1");

            var destinationArray = new string[3];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => watchAddList.CopyTo(destinationArray, 3));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is insufficient space from the specified index.
        /// Input: WatchAddList with multiple elements, array with insufficient space from given index.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithInsufficientSpaceFromIndex_ThrowsArgumentException()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);
            watchAddList.Add("Item1");
            watchAddList.Add("Item2");

            var destinationArray = new string[3];
            var arrayIndex = 2; // Only 1 space available from index 2, but need 2 spaces

            // Act & Assert
            Assert.Throws<ArgumentException>(() => watchAddList.CopyTo(destinationArray, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo works correctly when copying a single element.
        /// Input: WatchAddList with single element, valid array and index.
        /// Expected: Single element is copied to the specified position.
        /// </summary>
        [Fact]
        public void CopyTo_WithSingleElement_CopiesCorrectly()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<string>>>();
            var watchAddList = new WatchAddList<string>(onAddAction);
            watchAddList.Add("SingleItem");

            var destinationArray = new string[3];

            // Act
            watchAddList.CopyTo(destinationArray, 1);

            // Assert
            Assert.Null(destinationArray[0]);
            Assert.Equal("SingleItem", destinationArray[1]);
            Assert.Null(destinationArray[2]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly when filling the entire destination array.
        /// Input: WatchAddList with elements that exactly fit the destination array.
        /// Expected: All array positions are filled with list elements.
        /// </summary>
        [Fact]
        public void CopyTo_FillingEntireArray_CopiesAllElements()
        {
            // Arrange
            var onAddAction = Substitute.For<Action<List<int>>>();
            var watchAddList = new WatchAddList<int>(onAddAction);
            watchAddList.Add(1);
            watchAddList.Add(2);
            watchAddList.Add(3);

            var destinationArray = new int[3];

            // Act
            watchAddList.CopyTo(destinationArray, 0);

            // Assert
            Assert.Equal(1, destinationArray[0]);
            Assert.Equal(2, destinationArray[1]);
            Assert.Equal(3, destinationArray[2]);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the list.
        /// </summary>
        [Fact]
        public void Contains_ItemExists_ReturnsTrue()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            var testItem = "test item";
            watchList.Add(testItem);

            // Act
            var result = watchList.Contains(testItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the list.
        /// </summary>
        [Fact]
        public void Contains_ItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("existing item");

            // Act
            var result = watchList.Contains("non-existing item");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty list.
        /// </summary>
        [Fact]
        public void Contains_EmptyList_ReturnsFalse()
        {
            // Arrange
            var watchList = new WatchAddList<int>(_ => { });

            // Act
            var result = watchList.Contains(42);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains properly handles null items for reference types.
        /// </summary>
        [Fact]
        public void Contains_NullItem_ReturnsCorrectResult()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add(null);

            // Act
            var resultForExistingNull = watchList.Contains(null);
            var resultForNonExistingItem = watchList.Contains("not null");

            // Assert
            Assert.True(resultForExistingNull);
            Assert.False(resultForNonExistingItem);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for null in a list that doesn't contain null.
        /// </summary>
        [Fact]
        public void Contains_NullItemNotInList_ReturnsFalse()
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("test item");

            // Act
            var result = watchList.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists multiple times in the list.
        /// </summary>
        [Fact]
        public void Contains_DuplicateItems_ReturnsTrue()
        {
            // Arrange
            var watchList = new WatchAddList<int>(_ => { });
            var testItem = 123;
            watchList.Add(testItem);
            watchList.Add(testItem);
            watchList.Add(456);

            // Act
            var result = watchList.Contains(testItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests Contains functionality with different value types using parameterized test data.
        /// </summary>
        [Theory]
        [InlineData(42, true)]
        [InlineData(999, false)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void Contains_IntegerValues_ReturnsExpectedResult(int searchItem, bool expectedResult)
        {
            // Arrange
            var watchList = new WatchAddList<int>(_ => { });
            watchList.Add(42);
            watchList.Add(100);

            // Act
            var result = watchList.Contains(searchItem);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests Contains functionality with different string values using parameterized test data.
        /// </summary>
        [Theory]
        [InlineData("apple", true)]
        [InlineData("banana", true)]
        [InlineData("cherry", false)]
        [InlineData("", false)]
        [InlineData("APPLE", false)] // Case sensitive
        public void Contains_StringValues_ReturnsExpectedResult(string searchItem, bool expectedResult)
        {
            // Arrange
            var watchList = new WatchAddList<string>(_ => { });
            watchList.Add("apple");
            watchList.Add("banana");

            // Act
            var result = watchList.Contains(searchItem);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Contains works correctly with complex reference types.
        /// </summary>
        [Fact]
        public void Contains_ComplexReferenceType_ReturnsCorrectResult()
        {
            // Arrange
            var watchList = new WatchAddList<VisualState>(_ => { });
            var visualState1 = new VisualState { Name = "State1" };
            var visualState2 = new VisualState { Name = "State2" };
            var visualState3 = new VisualState { Name = "State3" };

            watchList.Add(visualState1);
            watchList.Add(visualState2);

            // Act
            var resultForExisting = watchList.Contains(visualState1);
            var resultForNonExisting = watchList.Contains(visualState3);

            // Assert
            Assert.True(resultForExisting);
            Assert.False(resultForNonExisting);
        }
    }


    /// <summary>
    /// Unit tests for the WatchAddList Insert method.
    /// </summary>
    public class WatchAddListInsertTests
    {
        /// <summary>
        /// Tests that Insert method adds item at the beginning of the list and calls the callback.
        /// </summary>
        [Fact]
        public void Insert_AtBeginning_InsertsItemAndCallsCallback()
        {
            // Arrange
            var callbackInvoked = false;
            List<string> callbackList = null;
            var watchList = new WatchAddList<string>(list =>
            {
                callbackInvoked = true;
                callbackList = new List<string>(list);
            });
            watchList.Add("existing");

            // Act
            watchList.Insert(0, "inserted");

            // Assert
            Assert.Equal(2, watchList.Count);
            Assert.Equal("inserted", watchList[0]);
            Assert.Equal("existing", watchList[1]);
            Assert.True(callbackInvoked);
            Assert.NotNull(callbackList);
            Assert.Equal(2, callbackList.Count);
            Assert.Equal("inserted", callbackList[0]);
        }

        /// <summary>
        /// Tests that Insert method adds item at the end of the list and calls the callback.
        /// </summary>
        [Fact]
        public void Insert_AtEnd_InsertsItemAndCallsCallback()
        {
            // Arrange
            var callbackInvoked = false;
            List<string> callbackList = null;
            var watchList = new WatchAddList<string>(list =>
            {
                callbackInvoked = true;
                callbackList = new List<string>(list);
            });
            watchList.Add("existing");

            // Act
            watchList.Insert(1, "inserted");

            // Assert
            Assert.Equal(2, watchList.Count);
            Assert.Equal("existing", watchList[0]);
            Assert.Equal("inserted", watchList[1]);
            Assert.True(callbackInvoked);
            Assert.NotNull(callbackList);
            Assert.Equal(2, callbackList.Count);
            Assert.Equal("inserted", callbackList[1]);
        }

        /// <summary>
        /// Tests that Insert method adds item in the middle of the list and calls the callback.
        /// </summary>
        [Fact]
        public void Insert_InMiddle_InsertsItemAndCallsCallback()
        {
            // Arrange
            var callbackInvoked = false;
            List<string> callbackList = null;
            var watchList = new WatchAddList<string>(list =>
            {
                callbackInvoked = true;
                callbackList = new List<string>(list);
            });
            watchList.Add("first");
            watchList.Add("last");

            // Act
            watchList.Insert(1, "middle");

            // Assert
            Assert.Equal(3, watchList.Count);
            Assert.Equal("first", watchList[0]);
            Assert.Equal("middle", watchList[1]);
            Assert.Equal("last", watchList[2]);
            Assert.True(callbackInvoked);
            Assert.NotNull(callbackList);
            Assert.Equal(3, callbackList.Count);
            Assert.Equal("middle", callbackList[1]);
        }

        /// <summary>
        /// Tests that Insert method inserts item in empty list and calls the callback.
        /// </summary>
        [Fact]
        public void Insert_InEmptyList_InsertsItemAndCallsCallback()
        {
            // Arrange
            var callbackInvoked = false;
            List<string> callbackList = null;
            var watchList = new WatchAddList<string>(list =>
            {
                callbackInvoked = true;
                callbackList = new List<string>(list);
            });

            // Act
            watchList.Insert(0, "inserted");

            // Assert
            Assert.Equal(1, watchList.Count);
            Assert.Equal("inserted", watchList[0]);
            Assert.True(callbackInvoked);
            Assert.NotNull(callbackList);
            Assert.Single(callbackList);
            Assert.Equal("inserted", callbackList[0]);
        }

        /// <summary>
        /// Tests that Insert method can insert null items when type allows nulls and calls the callback.
        /// </summary>
        [Fact]
        public void Insert_WithNullItem_InsertsNullAndCallsCallback()
        {
            // Arrange
            var callbackInvoked = false;
            List<string> callbackList = null;
            var watchList = new WatchAddList<string>(list =>
            {
                callbackInvoked = true;
                callbackList = new List<string>(list);
            });

            // Act
            watchList.Insert(0, null);

            // Assert
            Assert.Equal(1, watchList.Count);
            Assert.Null(watchList[0]);
            Assert.True(callbackInvoked);
            Assert.NotNull(callbackList);
            Assert.Single(callbackList);
            Assert.Null(callbackList[0]);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void Insert_WithNegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var watchList = new WatchAddList<string>(list => { });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => watchList.Insert(negativeIndex, "item"));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index is greater than count.
        /// </summary>
        [Theory]
        [InlineData(1, 0)] // index 1 when count is 0
        [InlineData(5, 2)] // index 5 when count is 2
        [InlineData(int.MaxValue, 0)] // max value when count is 0
        public void Insert_WithIndexGreaterThanCount_ThrowsArgumentOutOfRangeException(int index, int initialCount)
        {
            // Arrange
            var watchList = new WatchAddList<string>(list => { });
            for (int i = 0; i < initialCount; i++)
            {
                watchList.Add($"item{i}");
            }

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => watchList.Insert(index, "item"));
        }

        /// <summary>
        /// Tests that Insert method calls callback with the correct list instance.
        /// </summary>
        [Fact]
        public void Insert_CallsCallbackWithCorrectListInstance()
        {
            // Arrange
            List<string> receivedList = null;
            var watchList = new WatchAddList<string>(list => receivedList = list);
            watchList.Add("existing");

            // Act
            watchList.Insert(0, "inserted");

            // Assert
            Assert.NotNull(receivedList);
            Assert.Equal(2, receivedList.Count);
            Assert.Equal("inserted", receivedList[0]);
            Assert.Equal("existing", receivedList[1]);
        }

        /// <summary>
        /// Tests that Insert method works correctly with value types.
        /// </summary>
        [Fact]
        public void Insert_WithValueTypes_InsertsCorrectlyAndCallsCallback()
        {
            // Arrange
            var callbackInvoked = false;
            List<int> callbackList = null;
            var watchList = new WatchAddList<int>(list =>
            {
                callbackInvoked = true;
                callbackList = new List<int>(list);
            });
            watchList.Add(1);
            watchList.Add(3);

            // Act
            watchList.Insert(1, 2);

            // Assert
            Assert.Equal(3, watchList.Count);
            Assert.Equal(1, watchList[0]);
            Assert.Equal(2, watchList[1]);
            Assert.Equal(3, watchList[2]);
            Assert.True(callbackInvoked);
            Assert.NotNull(callbackList);
            Assert.Equal(3, callbackList.Count);
            Assert.Equal(2, callbackList[1]);
        }

        /// <summary>
        /// Tests that Insert method maintains correct order when inserting multiple items.
        /// </summary>
        [Fact]
        public void Insert_MultipleInsertions_MaintainsCorrectOrderAndCallsCallback()
        {
            // Arrange
            var callbackCount = 0;
            var watchList = new WatchAddList<string>(list => callbackCount++);
            watchList.Add("item3");

            // Act
            watchList.Insert(0, "item1");
            watchList.Insert(1, "item2");

            // Assert
            Assert.Equal(3, watchList.Count);
            Assert.Equal("item1", watchList[0]);
            Assert.Equal("item2", watchList[1]);
            Assert.Equal("item3", watchList[2]);
            Assert.Equal(2, callbackCount); // Called twice for the two inserts
        }
    }
}