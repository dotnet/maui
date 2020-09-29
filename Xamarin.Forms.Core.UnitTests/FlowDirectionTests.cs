using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FlowDirectionTests : BaseTestFixture
	{
		[Test]
		public void ListViewFlowDirectionIsInheritedByViewCells()
		{
			var lv = new ListView { FlowDirection = FlowDirection.RightToLeft, ItemTemplate = new DataTemplate(() => new ViewCell { View = new View() }) };

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			IViewController target = cell.View;
			Assert.IsTrue(target.EffectiveFlowDirection.IsRightToLeft(), "ViewCell View is not RightToLeft");
		}

		[Test]
		public void ListViewFlowDirectionIsInheritedByImageInViewCells()
		{
			var lv = new ListView { FlowDirection = FlowDirection.RightToLeft, ItemTemplate = new DataTemplate(() => new ViewCell { View = new Label() }) };

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			IViewController target = cell.View;
			Assert.IsTrue(target.EffectiveFlowDirection.IsRightToLeft(), "ViewCell View is not RightToLeft");
		}

		[Test]
		public void ScrollViewSetsFlowDirectionAndGrandchildMaintainsParentExplicitValue()
		{
			var layout = ImplicitLeftToRightScrollView();
			var layout2 = ExplicitRightToLeftScrollView();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitRTLToScrollView(layout, layout2);
			AddImplicitToRTLScrollView(layout2, (View)view);

			layout.FlowDirection = FlowDirection.LeftToRight;

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");

			Assert.AreEqual(FlowDirection.MatchParent, ((View)view).FlowDirection);
			Assert.AreEqual(FlowDirection.RightToLeft, layout2.FlowDirection);
		}

		[Test]
		public void GrandparentSetsFlowDirectionAndGrandchildMaintainsParentExplicitValue()
		{
			var layout = ImplicitLeftToRightLayout();
			var layout2 = ExplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitLTRToLayout(layout, layout2);
			AddImplicitToLTR(layout2, (View)view);

			layout.FlowDirection = FlowDirection.RightToLeft;

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");

			Assert.AreEqual(FlowDirection.MatchParent, ((View)view).FlowDirection);
			Assert.AreEqual(FlowDirection.LeftToRight, layout2.FlowDirection);
		}

		[Test]
		public void GrandparentSetsFlowDirectionAndImplicitDescendentsInheritValue()
		{
			var layout = ImplicitLeftToRightLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToLTR(layout, layout2);

			AddImplicitToLTR(layout2, (View)view);

			layout.FlowDirection = FlowDirection.RightToLeft;

			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsImplicit());
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsRightToLeft());

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");

			Assert.AreEqual(FlowDirection.MatchParent, ((View)view).FlowDirection);
			Assert.AreEqual(FlowDirection.MatchParent, layout2.FlowDirection);
		}

		[Test]
		public void GrandparentSetsOppositeFlowDirectionAndGrandchildInheritsParentExplicitValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ExplicitRightToLeftLayout();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitRTLToLayout(layout, layout2);

			AddImplicitToRTL(layout2, (View)view);

			layout.FlowDirection = FlowDirection.LeftToRight;

			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsLeftToRight());

			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsRightToLeft());

			Assume.That(view.EffectiveFlowDirection.IsImplicit());
			Assume.That(view.EffectiveFlowDirection.IsRightToLeft());

			var target = ((View)view).FlowDirection;

			Assert.AreEqual(FlowDirection.MatchParent, target);
		}

		[Test]
		public void NotifyFlowDirectionChangedDoesNotTriggerFlowDirectionPropertyChangedUnnecessarily()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = new PropertyWatchingView();

			AddImplicitToRTL(layout, layout2);

			AddImplicitToRTL(layout2, (View)view);

			layout2.FlowDirection = FlowDirection.RightToLeft;
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsExplicit(), "Explicit EffectiveFlowDirection not set on inner layout");
			Assume.That(view.EffectiveFlowDirection.IsRightToLeft(), "Implicit FlowDirection not set on view");

			layout.FlowDirection = FlowDirection.LeftToRight;
			Assume.That(layout2.FlowDirection == FlowDirection.RightToLeft, "Explicit FlowDirection not respected on inner layout");
			Assume.That(view.EffectiveFlowDirection.IsRightToLeft(), "Implicit FlowDirection not set on view");

			var target = ((PropertyWatchingView)view).FlowDirectionPropertyChangedCount;

			Assert.AreEqual(1, target);
		}

		[Test]
		public void ReParentAndInheritNewParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			IViewController view = ImplicitLeftToRightView();
			var layout2 = ExplicitLeftToRightLayout();

			AddImplicitToRTL(layout, (View)view);

			((View)view).Parent = layout2;

			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");

			Assert.AreEqual(FlowDirection.MatchParent, ((View)view).FlowDirection);
		}

		[Test]
		public void ReParentParentAndInheritNewGrandParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();
			var layout3 = ExplicitLeftToRightLayout();

			AddImplicitToRTL(layout, layout2);
			AddImplicitToRTL(layout2, (View)view);

			layout2.Parent = layout3;

			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsImplicit());
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assume.That(view.EffectiveFlowDirection.IsImplicit());
			Assume.That(view.EffectiveFlowDirection.IsLeftToRight());

			var target = ((View)view).FlowDirection;

			Assert.AreEqual(FlowDirection.MatchParent, target);
		}

		[Test]
		public void SetFlowDirectionToMatchParentAndInheritParentValue()
		{
			var layout = ImplicitLeftToRightLayout();
			var layout2 = ExplicitRightToLeftLayout();
			IViewController view = ExplicitLeftToRightView();

			AddExplicitRTLToLayout(layout, layout2);

			AddExplicitLTRToLayout(layout2, (View)view);

			((View)view).FlowDirection = FlowDirection.MatchParent;

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}

		[Test]
		public void SetGrandparentAndInheritExplicitParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ExplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddExplicitLTRToLayout(layout, layout2);
			AddImplicitToLTR(layout2, (View)view);

			var target = view.EffectiveFlowDirection;

			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
		}

		[Test]
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

			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
		}

		[Test]
		public void SetGrandparentUsingCtorAndMaintainExplicitParentValue()
		{
			IViewController view = ImplicitLeftToRightView();
			var layout2 = new StackLayout { FlowDirection = FlowDirection.LeftToRight, Children = { (View)view } };
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft, Children = { layout2 } };

			var target = view.EffectiveFlowDirection;

			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout2).EffectiveFlowDirection.IsLeftToRight());

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
		}

		[Test]
		public void SetParentAndGrandchildrenInheritValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightLayout();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToRTL(layout, layout2);

			AddImplicitToRTL(layout2, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}

		[Test]
		public void SetParentAndContentAndGrandchildrenInheritValue()
		{
			var layout = ExplicitRightToLeftLayout();
			var layout2 = ImplicitLeftToRightScrollView();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToRTL(layout, layout2);

			AddImplicitToRTLScrollView(layout2, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}


		[Test]
		public void SetParentAndInheritExplicitParentValue()
		{
			var layout = ExplicitRightToLeftLayout();
			IViewController view = ImplicitLeftToRightView();

			AddImplicitToRTL(layout, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
		}

		[Test]
		public void SetParentAndMaintainExplicitValue()
		{
			var layout = ExplicitRightToLeftLayout();
			IViewController view = ExplicitLeftToRightView();

			AddExplicitLTRToLayout(layout, (View)view);

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(!target.IsImplicit(), "EffectiveFlowDirection should be Explicit");
			Assert.IsTrue(target.IsExplicit(), "EffectiveFlowDirection should be Explicit");
			Assert.IsTrue(!target.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(target.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
			Assert.AreEqual(FlowDirection.LeftToRight, ((View)view).FlowDirection);
		}

		[Test]
		public void SetParentUsingCtorAndInheritParentValue()
		{
			IViewController view = ImplicitLeftToRightView();
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft, Children = { (View)view } };

			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsExplicit());
			Assume.That(((IViewController)layout).EffectiveFlowDirection.IsRightToLeft());

			Assume.That(((View)view).FlowDirection == FlowDirection.MatchParent);

			var target = view.EffectiveFlowDirection;

			Assert.IsTrue(target.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!target.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(target.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(!target.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.AreEqual(FlowDirection.MatchParent, ((View)view).FlowDirection);
		}

		[Test]
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

			Assert.IsTrue(buttonVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!buttonVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(buttonVisualController.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(!buttonVisualController.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");

			Assert.IsTrue(stacklayoutVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!stacklayoutVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(stacklayoutVisualController.IsLeftToRight(), "EffectiveFlowDirection should be LeftToRight");
			Assert.IsTrue(!stacklayoutVisualController.IsRightToLeft(), "EffectiveFlowDirection should be LeftToRight");

			shell.FlowDirection = FlowDirection.RightToLeft;
			buttonVisualController = (button as IViewController).EffectiveFlowDirection;
			stacklayoutVisualController = (flyout as IViewController).EffectiveFlowDirection;

			Assert.IsTrue(buttonVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!buttonVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!buttonVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(buttonVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");

			Assert.IsTrue(stacklayoutVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!stacklayoutVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!stacklayoutVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(stacklayoutVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
		}


		[Test]
		public void ShellPropagatesRightToLeftChangetoNewElements()
		{
			Button button = new Button();
			StackLayout flyout = new StackLayout();
			Shell shell = new Shell()
			{
				Visual = Forms.VisualMarker.Default,
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

			Assert.IsTrue(buttonVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!buttonVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!buttonVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(buttonVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");

			Assert.IsTrue(stacklayoutVisualController.IsImplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!stacklayoutVisualController.IsExplicit(), "EffectiveFlowDirection should be Implicit");
			Assert.IsTrue(!stacklayoutVisualController.IsLeftToRight(), "EffectiveFlowDirection should be RightToLeft");
			Assert.IsTrue(stacklayoutVisualController.IsRightToLeft(), "EffectiveFlowDirection should be RightToLeft");
		}


		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
			Device.FlowDirection = FlowDirection.LeftToRight;
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		static void AddExplicitLTRToScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "child view FlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "child view FlowDirection should be LeftToRight");
			Assume.That(child.FlowDirection == FlowDirection.LeftToRight, "child view FlowDirection should be LeftToRight");
		}

		static void AddExplicitLTRToLayout(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "child view FlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "child view FlowDirection should be LeftToRight");
			Assume.That(child.FlowDirection == FlowDirection.LeftToRight, "child view FlowDirection should be LeftToRight");
		}

		static void AddExplicitRTLToScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assume.That(child.FlowDirection == FlowDirection.RightToLeft, "child view FlowDirection should be RightToLeft");
		}

		static void AddExplicitRTLToLayout(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assume.That(child.FlowDirection == FlowDirection.RightToLeft, "child view FlowDirection should be RightToLeft");
		}

		static void AddImplicitToLTR(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "child view EffectiveFlowDirection should be LeftToRight");
			Assume.That(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static void AddImplicitToLTRScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "child view EffectiveFlowDirection should be LeftToRight");
			Assume.That(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static void AddImplicitToRTL(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assume.That(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static void AddImplicitToRTLScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IViewController controller = child;

			Assume.That(controller.EffectiveFlowDirection.IsImplicit(), "child view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsRightToLeft(), "child view EffectiveFlowDirection should be RightToLeft");
			Assume.That(child.FlowDirection == FlowDirection.MatchParent, "child view FlowDirection should be MatchParent");
		}

		static ScrollView ExplicitLeftToRightScrollView()
		{
			var layout = new ScrollView { FlowDirection = FlowDirection.LeftToRight };

			IViewController controller = layout;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "Explicit LTR view EffectiveFlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "Explicit LTR view EffectiveFlowDirection should be LeftToRight");
			Assume.That(layout.FlowDirection == FlowDirection.LeftToRight, "Explicit LTR view FlowDirection should be LeftToRight");
			return layout;
		}

		static StackLayout ExplicitLeftToRightLayout()
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.LeftToRight };

			IViewController controller = layout;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "Explicit LTR view EffectiveFlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "Explicit LTR view EffectiveFlowDirection should be LeftToRight");
			Assume.That(layout.FlowDirection == FlowDirection.LeftToRight, "Explicit LTR view FlowDirection should be LeftToRight");
			return layout;
		}

		static View ExplicitLeftToRightView()
		{
			var view = new View { FlowDirection = FlowDirection.LeftToRight };

			IViewController controller = view;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "Explicit LTR view EffectiveFlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "Explicit LTR view EffectiveFlowDirection should be LeftToRight");
			Assume.That(((View)view).FlowDirection == FlowDirection.LeftToRight, "Explicit LTR view FlowDirection should be LeftToRight");

			return view;
		}

		static ScrollView ExplicitRightToLeftScrollView()
		{
			var layout = new ScrollView { FlowDirection = FlowDirection.RightToLeft };

			IViewController controller = layout;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "Explicit RTL view EffectiveFlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsRightToLeft(), "Explicit RTL view EffectiveFlowDirection should be RightToLeft");
			Assume.That(layout.FlowDirection == FlowDirection.RightToLeft, "Explicit RTL view FlowDirection should be RightToLeft");

			return layout;
		}

		static StackLayout ExplicitRightToLeftLayout()
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };

			IViewController controller = layout;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "Explicit RTL view EffectiveFlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsRightToLeft(), "Explicit RTL view EffectiveFlowDirection should be RightToLeft");
			Assume.That(layout.FlowDirection == FlowDirection.RightToLeft, "Explicit RTL view FlowDirection should be RightToLeft");

			return layout;
		}

		static View ExplicitRightToLeftView()
		{
			var view = new View { FlowDirection = FlowDirection.RightToLeft };

			IViewController controller = view;

			Assume.That(controller.EffectiveFlowDirection.IsExplicit(), "Explicit RTL view EffectiveFlowDirection should be Explicit");
			Assume.That(controller.EffectiveFlowDirection.IsRightToLeft(), "Explicit RTL view EffectiveFlowDirection should be RightToLeft");
			Assume.That(((View)view).FlowDirection == FlowDirection.RightToLeft, "Explicit RTL view FlowDirection should be RightToLeft");

			return view;
		}

		static ScrollView ImplicitLeftToRightScrollView()
		{
			var layout = new ScrollView();

			IViewController controller = layout;

			Assume.That(controller.EffectiveFlowDirection.IsImplicit(), "New view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "New view EffectiveFlowDirection should be LeftToRight");
			Assume.That(layout.FlowDirection == FlowDirection.MatchParent, "New view FlowDirection should be MatchParent");

			return layout;
		}

		static StackLayout ImplicitLeftToRightLayout()
		{
			var layout = new StackLayout();

			IViewController controller = layout;

			Assume.That(controller.EffectiveFlowDirection.IsImplicit(), "New view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "New view EffectiveFlowDirection should be LeftToRight");
			Assume.That(layout.FlowDirection == FlowDirection.MatchParent, "New view FlowDirection should be MatchParent");

			return layout;
		}

		static View ImplicitLeftToRightView()
		{
			var view = new View();

			IViewController controller = view;

			Assume.That(controller.EffectiveFlowDirection.IsImplicit(), "New view EffectiveFlowDirection should be Implicit");
			Assume.That(controller.EffectiveFlowDirection.IsLeftToRight(), "New view EffectiveFlowDirection should be LeftToRight");
			Assume.That(((View)view).FlowDirection == FlowDirection.MatchParent, "New view FlowDirection should be MatchParent");

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