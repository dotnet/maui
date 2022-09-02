using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class FlowDirectionTests : BaseTestFixture
	{
		[Fact]
		public void ListViewFlowDirectionIsInheritedByViewCells()
		{
			var lv = new ListView { FlowDirection = FlowDirection.RightToLeft, ItemTemplate = new DataTemplate(() => new ViewCell { View = new View() }) };

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			IViewController target = cell.View;
			Assert.True(target.EffectiveFlowDirection.IsRightToLeft(), "ViewCell View is not RightToLeft");
		}

		[Fact]
		public void ListViewFlowDirectionIsInheritedByImageInViewCells()
		{
			var lv = new ListView { FlowDirection = FlowDirection.RightToLeft, ItemTemplate = new DataTemplate(() => new ViewCell { View = new Label() }) };

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			IViewController target = cell.View;
			Assert.True(target.EffectiveFlowDirection.IsRightToLeft(), "ViewCell View is not RightToLeft");
		}

		[Fact]
		public void ScrollViewSetsFlowDirectionAndGrandchildMaintainsParentExplicitValue()
		{
			var layout = ImplicitLeftToRightScrollView();
			var layout2 = ExplicitRightToLeftScrollView();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitRTLToScrollView(layout, layout2);
			AddImplicitToRTLScrollView(layout2, (View)view);

			layout.FlowDirection = FlowDirection.LeftToRight;

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");

			Assert.Equal(FlowDirection.MatchParent, ((View)view).FlowDirection);
			Assert.Equal(FlowDirection.RightToLeft, layout2.FlowDirection);
		}

		[Fact]
		public void GrandparentSetsFlowDirectionAndGrandchildMaintainsParentExplicitValue()
		{
			var layout = ImplicitLeftToRightLayout();
			var layout2 = ExplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitLTRToLayout(layout, layout2);
			AddImplicitToLTR(layout2, (View)view);

			layout.FlowDirection = FlowDirection.RightToLeft;

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");

			Assert.Equal(FlowDirection.MatchParent, ((View)view).FlowDirection);
			Assert.Equal(FlowDirection.LeftToRight, layout2.FlowDirection);
		}

		[Fact]
		public void GrandparentSetsFlowDirectionAndImplicitDescendentsInheritValue()
		{
			var layout = ImplicitLeftToRightLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToLTR(layout, layout2);

			AddImplicitToLTR(layout2, (View)view);

			layout.FlowDirection = FlowDirection.RightToLeft;

			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsImplicit());
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsRightToLeft());

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");

			Assert.Equal(FlowDirection.MatchParent, ((View)view).FlowDirection);
			Assert.Equal(FlowDirection.MatchParent, layout2.FlowDirection);
		}

		[Fact]
		public void GrandparentSetsOppositeFlowDirectionAndGrandchildInheritsParentExplicitValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ExplicitRightToLeftLayout();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitRTLToLayout(layout, layout2);

			AddImplicitToRTL(layout2, (View)view);

			layout.FlowDirection = FlowDirection.LeftToRight;

			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsLeftToRight());

			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsRightToLeft());

			Assert.True(view.EffectiveFlowDirection.IsImplicit());
			Assert.True(view.EffectiveFlowDirection.IsRightToLeft());

			var target = ((View)view).FlowDirection;

			Assert.Equal(FlowDirection.MatchParent, target);
		}

		[Fact]
		public void NotifyFlowDirectionChangedDoesNotTriggerFlowDirectionPropertyChangedUnnecessarily()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = new PropertyWatchingView();

			AddImplicitToRTL(layout, layout2);

			AddImplicitToRTL(layout2, (View)view);

			layout2.FlowDirection = FlowDirection.RightToLeft;
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsExplicit(), "Explicit EffectiveFlowDirection not set on inner layout");
			Assert.True(view.EffectiveFlowDirection.IsRightToLeft(), "Implicit FlowDirection not set on view");

			layout.FlowDirection = FlowDirection.LeftToRight;
			Assert.True(layout2.FlowDirection == FlowDirection.RightToLeft, "Explicit FlowDirection not respected on inner layout");
			Assert.True(view.EffectiveFlowDirection.IsRightToLeft(), "Implicit FlowDirection not set on view");

			var target = ((PropertyWatchingView)view).FlowDirectionPropertyChangedCount;

			Assert.Equal(1, target);
		}

		[Fact]
		public void ReParentAndInheritNewParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			IViewController view = ImplicitLeftToRightView();
			var layout2 = ExplicitLeftToRightLayout();

			AddImplicitToRTL(layout, (View)view);

			((View)view).Parent = layout2;

			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");

			Assert.Equal(FlowDirection.MatchParent, ((View)view).FlowDirection);
		}

		[Fact]
		public void ReParentParentAndInheritNewGrandParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();
			var layout3 = ExplicitLeftToRightLayout();

			AddImplicitToRTL(layout, layout2);
			AddImplicitToRTL(layout2, (View)view);

			layout2.Parent = layout3;

			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsImplicit());
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assert.True(view.EffectiveFlowDirection.IsImplicit());
			Assert.True(view.EffectiveFlowDirection.IsLeftToRight());

			var target = ((View)view).FlowDirection;

			Assert.Equal(FlowDirection.MatchParent, target);
		}

		[Fact]
		public void SetFlowDirectionToMatchParentAndInheritParentValue()
		{
			var layout = ImplicitLeftToRightLayout();
			var layout2 = ExplicitRightToLeftLayout();
			IViewController view = ExplicitLeftToRightView();

			AddExplicitRTLToLayout(layout, layout2);

			AddExplicitLTRToLayout(layout2, (View)view);

			((View)view).FlowDirection = FlowDirection.MatchParent;

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}

		[Fact]
		public void SetGrandparentAndInheritExplicitParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ExplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitLTRToLayout(layout, layout2);
			AddImplicitToLTR(layout2, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
		}

		[Fact]
		public void SetGrandparentUsingAnonCtorAndMaintainExplicitParentValue()
		{
			var layout = new StackLayout
			{
				FlowDirection = FlowDirection.RightToLeft,
				Children = {
					new StackLayout {
						FlowDirection = FlowDirection.LeftToRight,
						Children = { ImplicitLeftToRightView() }
					}
				}
			};

			var layout2 = layout.Children[0] as StackLayout;
			IViewController view = layout2.Children[0] as View;

			var target = view.EffectiveFlowDirection;

			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
		}

		[Fact]
		public void SetGrandparentUsingCtorAndMaintainExplicitParentValue()
		{
			IViewController view = ImplicitLeftToRightView();
			var layout2 = new StackLayout { FlowDirection = FlowDirection.LeftToRight, Children = { (View)view } };
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft, Children = { layout2 } };

			var target = view.EffectiveFlowDirection;

			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
		}

		[Fact]
		public void SetParentAndGrandchildrenInheritValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToRTL(layout, layout2);

			AddImplicitToRTL(layout2, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}

		[Fact]
		public void SetParentAndContentAndGrandchildrenInheritValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightScrollView();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToRTL(layout, layout2);

			AddImplicitToRTLScrollView(layout2, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}


		[Fact]
		public void SetParentAndInheritExplicitParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToRTL(layout, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}

		[Fact]
		public void SetParentAndMaintainExplicitValue()
		{
			var layout = ExplicitRightToLeftLayout();
			IViewController view = ExplicitLeftToRightView();

			AddExplicitLTRToLayout(layout, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.True(!target.IsImplicit(), "EffectiveFlowDirection should be Explicit");
			Assert.True(target.IsExplicit(), "EffectiveFlowDirection should be Explicit");
			Assert.True(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
			Assert.Equal(FlowDirection.LeftToRight, ((View)view).FlowDirection);
		}

		[Fact]
		public void SetParentUsingCtorAndInheritParentValue()
		{
			IViewController view = ImplicitLeftToRightView();
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft, Children = { (View)view } };

			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assert.True(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assert.True(((View)view).FlowDirection == FlowDirection.MatchParent);

			var target = view.EffectiveFlowDirection;

			Assert.True(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.Equal(FlowDirection.MatchParent, ((View)view).FlowDirection);
		}

		[Fact]
		public void ShellPropagatesDownRightToLeftChange()
		{
			Button button = new Button();
			StackLayout flyout = new StackLayout();
			Shell shell = new Shell()
			{
				FlyoutHeader = flyout,
				Items =
				{
					new ShellItem()
					{
						Items =
						{
							new ShellSection()
							{
								Items =
								{
									new ShellContent()
									{
										Content = new ContentPage()
										{
											Content = button
										}
									}
								}
							}
						}
					}
				}
			};

			var buttonVisualController = (button as IViewController).EffectiveFlowDirection;
			var stacklayoutVisualController = (flyout as IViewController).EffectiveFlowDirection;

			Assert.True(buttonVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!buttonVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(buttonVisualController.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(!buttonVisualController.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");

			Assert.True(stacklayoutVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!stacklayoutVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(stacklayoutVisualController.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
			Assert.True(!stacklayoutVisualController.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");

			shell.FlowDirection = FlowDirection.RightToLeft;
			buttonVisualController = (button as IViewController).EffectiveFlowDirection;
			stacklayoutVisualController = (flyout as IViewController).EffectiveFlowDirection;

			Assert.True(buttonVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!buttonVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!buttonVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(buttonVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");

			Assert.True(stacklayoutVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!stacklayoutVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!stacklayoutVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(stacklayoutVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
		}


		[Fact]
		public void ShellPropagatesRightToLeftChangetoNewElements()
		{
			Button button = new Button();
			StackLayout flyout = new StackLayout();
			Shell shell = new Shell()
			{
				Visual = Maui.Controls.VisualMarker.Default,
				Items =
				{
					new ShellItem()
					{
						Items =
						{
							new ShellSection()
							{
								Items =
								{
									new ShellContent()
									{
										Content = new ContentPage()
										{
											Content = new Label()
										}
									}
								}
							}
						}
					}
				}
			};

			shell.FlowDirection = FlowDirection.RightToLeft;
			shell.FlyoutHeader = flyout;
			shell.Items.Add(
				new ShellItem()
				{
					Items =
						{
							new ShellSection()
							{
								Items =
								{
									new ShellContent()
									{
										Content = new ContentPage()
										{
											Content = button
										}
									}
								}
							}
						}
				});

			var buttonVisualController = (button as IViewController).EffectiveFlowDirection;
			var stacklayoutVisualController = (flyout as IViewController).EffectiveFlowDirection;

			Assert.True(buttonVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!buttonVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!buttonVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(buttonVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");

			Assert.True(stacklayoutVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!stacklayoutVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.True(!stacklayoutVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.True(stacklayoutVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
		}

		static void AddExplicitLTRToScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "child view FlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "child view FlowDirection should be LeftToRight");
			Assert.True(child.FlowDirection == FlowDirection.LeftToRight, "child view FlowDirection should be LeftToRight");
		}

		static void AddExplicitLTRToLayout(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "child view FlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "child view FlowDirection should be LeftToRight");
			Assert.True(child.FlowDirection == FlowDirection.LeftToRight, "child view FlowDirection should be LeftToRight");
		}

		static void AddExplicitRTLToScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assert.True(child.FlowDirection == FlowDirection.RightToLeft, "child view FlowDirection should be RightToLeft");
		}

		static void AddExplicitRTLToLayout(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assert.True(child.FlowDirection == FlowDirection.RightToLeft, "child view FlowDirection should be RightToLeft");
		}

		static void AddImplicitToLTR(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "child view EffectiveFlowDirection should be LeftToRight");
			Assert.True(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static void AddImplicitToLTRScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "child view EffectiveFlowDirection should be LeftToRight");
			Assert.True(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static void AddImplicitToRTL(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assert.True(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static void AddImplicitToRTLScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assert.True(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assert.True(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static ScrollView ExplicitLeftToRightScrollView()
		{
			var layout = new ScrollView { FlowDirection = FlowDirection.LeftToRight };

			IViewController controller = layout;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "Explicit LTR view EffectiveFlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "Explicit LTR view EffectiveFlowDirection should be LeftToRight");
			Assert.True(layout.FlowDirection == FlowDirection.LeftToRight, "Explicit LTR view FlowDirection should be LeftToRight");
			return layout;
		}

		static StackLayout ExplicitLeftToRightLayout()
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.LeftToRight };

			IViewController controller = layout;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "Explicit LTR view EffectiveFlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "Explicit LTR view EffectiveFlowDirection should be LeftToRight");
			Assert.True(layout.FlowDirection == FlowDirection.LeftToRight, "Explicit LTR view FlowDirection should be LeftToRight");
			return layout;
		}

		static View ExplicitLeftToRightView()
		{
			var view = new View { FlowDirection = FlowDirection.LeftToRight };

			IViewController controller = view;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "Explicit LTR view EffectiveFlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "Explicit LTR view EffectiveFlowDirection should be LeftToRight");
			Assert.True(((View)view).FlowDirection == FlowDirection.LeftToRight, "Explicit LTR view FlowDirection should be LeftToRight");

			return view;
		}

		static ScrollView ExplicitRightToLeftScrollView()
		{
			var layout = new ScrollView { FlowDirection = FlowDirection.RightToLeft };

			IViewController controller = layout;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "Explicit RTL view EffectiveFlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsRightToLeft(), "Explicit RTL view EffectiveFlowDirection should be RightToLeft");
			Assert.True(layout.FlowDirection == FlowDirection.RightToLeft, "Explicit RTL view FlowDirection should be RightToLeft");

			return layout;
		}

		static StackLayout ExplicitRightToLeftLayout()
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };

			IViewController controller = layout;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "Explicit RTL view EffectiveFlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsRightToLeft(), "Explicit RTL view EffectiveFlowDirection should be RightToLeft");
			Assert.True(layout.FlowDirection == FlowDirection.RightToLeft, "Explicit RTL view FlowDirection should be RightToLeft");

			return layout;
		}

		static View ExplicitRightToLeftView()
		{
			var view = new View { FlowDirection = FlowDirection.RightToLeft };

			IViewController controller = view;

			Assert.True(controller.EffectiveFlowDirection.IsExplicit(), "Explicit RTL view EffectiveFlowDirection should be Explicit");
			Assert.True(controller.EffectiveFlowDirection.IsRightToLeft(), "Explicit RTL view EffectiveFlowDirection should be RightToLeft");
			Assert.True(((View)view).FlowDirection == FlowDirection.RightToLeft, "Explicit RTL view FlowDirection should be RightToLeft");

			return view;
		}

		static ScrollView ImplicitLeftToRightScrollView()
		{
			var layout = new ScrollView();

			IViewController controller = layout;

			Assert.True(controller.EffectiveFlowDirection.IsImplicit(), "New view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "New view EffectiveFlowDirection should be LeftToRight");
			Assert.True(layout.FlowDirection == FlowDirection.MatchParent, "New view FlowDirection should be MatchParent");

			return layout;
		}

		static StackLayout ImplicitLeftToRightLayout()
		{
			var layout = new StackLayout();

			IViewController controller = layout;

			Assert.True(controller.EffectiveFlowDirection.IsImplicit(), "New view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "New view EffectiveFlowDirection should be LeftToRight");
			Assert.True(layout.FlowDirection == FlowDirection.MatchParent, "New view FlowDirection should be MatchParent");

			return layout;
		}

		static View ImplicitLeftToRightView()
		{
			var view = new View();

			IViewController controller = view;

			Assert.True(controller.EffectiveFlowDirection.IsImplicit(), "New view EffectiveFlowDirection should be Implicit");
			Assert.True(controller.EffectiveFlowDirection.IsLeftToRight(), "New view EffectiveFlowDirection should be LeftToRight");
			Assert.True(((View)view).FlowDirection == FlowDirection.MatchParent, "New view FlowDirection should be MatchParent");

			return view;
		}

		class PropertyWatchingView : View
		{
			public int FlowDirectionPropertyChangedCount { get; private set; }

			protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				base.OnPropertyChanged(propertyName);

				if (propertyName == View.FlowDirectionProperty.PropertyName)
					FlowDirectionPropertyChangedCount++;
			}
		}
	}
}