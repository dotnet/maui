using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
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

		[Fact]
		//https://github.com/dotnet/maui/issues/6856
		public void VSMInStyleShouldHaveStylePriority()
		{
			var label = new Label { TextColor = Colors.HotPink };//Setting the color manually should prevents style override
			var SelectedStateName = "Selected";

			Assert.Equal(label.TextColor, Colors.HotPink);

			label.Style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.AliceBlue },
					new Setter {
						Property = VisualStateManager.VisualStateGroupsProperty,
						Value = new VisualStateGroupList {
							new VisualStateGroup {
								States = {
									new VisualState {
										Name = SelectedStateName,
										Setters = { new Setter { Property = Label.TextColorProperty, Value=Colors.OrangeRed} }
									},
								}
							}
						}
					},
				}
			};

			Assert.Equal(label.TextColor, Colors.HotPink); //textcolor from Style isn't applied
			VisualStateManager.GoToState(label, SelectedStateName);
			Assert.Equal(label.TextColor, Colors.HotPink); //textcolor Style's VSM isn't applied
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
}