using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class VisualTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			var mockDeviceInfo = new TestDeviceInfo();
			Device.Info = mockDeviceInfo;
		}


		[Test]
		public void ShellPropagatesChangeToNewElements()
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

			shell.Visual = VisualMarker.Material;
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

			var buttonVisualController = (button as IVisualController);
			var stacklayoutVisualController = (flyout as IVisualController);
			Assert.AreEqual(Forms.VisualMarker.Material, buttonVisualController.EffectiveVisual);
			Assert.AreEqual(Forms.VisualMarker.Material, stacklayoutVisualController.EffectiveVisual);
		}


		[Test]
		public void ShellPropagatesDownVisualChange()
		{
			Button button = new Button();
			StackLayout flyout = new StackLayout();
			Shell shell = new Shell()
			{

				Visual = Forms.VisualMarker.Default,
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

			var buttonVisualController = (button as IVisualController);
			var stacklayoutVisualController = (flyout as IVisualController);

			Assert.AreEqual(Forms.VisualMarker.Default, buttonVisualController.EffectiveVisual);
			Assert.AreEqual(Forms.VisualMarker.Default, stacklayoutVisualController.EffectiveVisual);
			shell.Visual = VisualMarker.Material;
			Assert.AreEqual(Forms.VisualMarker.Material, buttonVisualController.EffectiveVisual);
			Assert.AreEqual(Forms.VisualMarker.Material, stacklayoutVisualController.EffectiveVisual);
		}


		[Test]
		public void ListViewVisualIsInheritedByViewCells()
		{
			var lv = new ListView { Visual = Forms.VisualMarker.Material, ItemTemplate = new DataTemplate(() => new ViewCell { View = new View() }) };

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			IVisualController target = cell.View;
			Assert.AreEqual(Forms.VisualMarker.Material, target.EffectiveVisual, "ViewCell View is not Material");
		}

		[Test]
		public void ListViewVisualIsInheritedByImageInViewCells()
		{
			var lv = new ListView { Visual = Forms.VisualMarker.Material, ItemTemplate = new DataTemplate(() => new ViewCell { View = new Label() }) };

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			IVisualController target = cell.View;
			Assert.AreEqual(Forms.VisualMarker.Material, target.EffectiveVisual, "ViewCell View is not Material");
		}

		[Test]
		public void ScrollViewSetsVisualAndGrandchildMaintainsParentExplicitValue()
		{
			var layout = ImplicitDefaultScrollView();
			var layout2 = ExplicitMaterialScrollView();
			IVisualController view = ImplicitDefaultView();

			AddExplicitMaterialToScrollView(layout, layout2);
			AddImplicitToMaterialScrollView(layout2, (View)view);

			layout.Visual = Forms.VisualMarker.Default;

			var target = view.EffectiveVisual;

			Assert.IsTrue(target == Forms.VisualMarker.Material, "EffectiveVisual should be Material");

			Assert.AreEqual(Forms.VisualMarker.MatchParent, ((View)view).Visual);
			Assert.AreEqual(Forms.VisualMarker.Material, layout2.Visual);
		}

		[Test]
		public void GrandparentSetsVisualAndGrandchildMaintainsParentExplicitValue()
		{
			var layout = ImplicitDefaultLayout();
			var layout2 = ExplicitDefaultLayout();
			IVisualController view = ImplicitDefaultView();

			AddExplicitDefaultToLayout(layout, layout2);
			AddImplicitToDefault(layout2, (View)view);

			layout.Visual = Forms.VisualMarker.Material;

			var target = view.EffectiveVisual;

			Assert.IsTrue(!target.IsMaterial(), "EffectiveVisual should be Default");
			Assert.IsTrue(target.IsDefault(), "EffectiveVisual should be Default");

			Assert.AreEqual(Forms.VisualMarker.MatchParent, ((View)view).Visual);
			Assert.AreEqual(Forms.VisualMarker.Default, layout2.Visual);
		}

		[Test]
		public void GrandparentSetsVisualAndImplicitDescendentsInheritValue()
		{
			var layout = ImplicitDefaultLayout();
			var layout2 = ImplicitDefaultLayout();
			IVisualController view = ImplicitDefaultView();

			AddImplicitToDefault(layout, layout2);

			AddImplicitToDefault(layout2, (View)view);

			layout.Visual = Forms.VisualMarker.Material;

			Assume.That(((IVisualController)layout).EffectiveVisual.IsMaterial());
			Assume.That(((IVisualController)layout2).EffectiveVisual.IsMaterial());

			var target = view.EffectiveVisual;

			Assert.IsTrue(target.IsMaterial(), "EffectiveVisual should be Material");
			Assert.IsTrue(!target.IsDefault(), "EffectiveVisual should be Material");

			Assert.AreEqual(Forms.VisualMarker.MatchParent, ((View)view).Visual);
			Assert.AreEqual(Forms.VisualMarker.MatchParent, layout2.Visual);
		}

		[Test]
		public void GrandparentSetsOppositeVisualAndGrandchildInheritsParentExplicitValue()
		{
			var layout = ExplicitMaterialLayout();
			var layout2 = ExplicitMaterialLayout();
			IVisualController view = ImplicitDefaultView();

			AddExplicitMaterialToLayout(layout, layout2);

			AddImplicitToMaterial(layout2, (View)view);

			layout.Visual = Forms.VisualMarker.Default;

			Assume.That(((IVisualController)layout).EffectiveVisual.IsDefault());

			Assume.That(((IVisualController)layout2).EffectiveVisual.IsMaterial());

			Assume.That(view.EffectiveVisual.IsMaterial());

			var target = ((View)view).Visual;

			Assert.AreEqual(Forms.VisualMarker.MatchParent, target);
		}

		[Test]
		public void NotifyVisualChangedDoesNotTriggerVisualPropertyChangedUnnecessarily()
		{
			var layout = ExplicitMaterialLayout();
			var layout2 = ImplicitDefaultLayout();
			IVisualController view = new PropertyWatchingView();

			AddImplicitToMaterial(layout, layout2);

			AddImplicitToMaterial(layout2, (View)view);

			layout2.Visual = Forms.VisualMarker.Material;
			Assume.That(view.EffectiveVisual.IsMaterial(), "Implicit Visual not set on view");

			layout.Visual = Forms.VisualMarker.Default;
			Assume.That(layout2.Visual == Forms.VisualMarker.Material, "Explicit Visual not respected on inner layout");
			Assume.That(view.EffectiveVisual.IsMaterial(), "Implicit Visual not set on view");

			var target = ((PropertyWatchingView)view).VisualPropertyChangedCount;

			Assert.AreEqual(1, target);
		}

		[Test]
		public void ReParentAndInheritNewParentValue()
		{
			var layout = ExplicitMaterialLayout();
			IVisualController view = ImplicitDefaultView();
			var layout2 = ExplicitDefaultLayout();

			AddImplicitToMaterial(layout, (View)view);

			((View)view).Parent = layout2;

			Assume.That(((IVisualController)layout2).EffectiveVisual.IsDefault());

			var target = view.EffectiveVisual;

			Assert.IsTrue(!target.IsMaterial(), "EffectiveVisual should be Default");
			Assert.IsTrue(target.IsDefault(), "EffectiveVisual should be Default");

			Assert.AreEqual(Forms.VisualMarker.MatchParent, ((View)view).Visual);
		}

		[Test]
		public void ReParentParentAndInheritNewGrandParentValue()
		{
			var layout = ExplicitMaterialLayout();
			var layout2 = ImplicitDefaultLayout();
			IVisualController view = ImplicitDefaultView();
			var layout3 = ExplicitDefaultLayout();

			AddImplicitToMaterial(layout, layout2);
			AddImplicitToMaterial(layout2, (View)view);

			layout2.Parent = layout3;

			Assume.That(((IVisualController)layout2).EffectiveVisual.IsDefault());

			Assume.That(view.EffectiveVisual.IsDefault());

			var target = ((View)view).Visual;

			Assert.AreEqual(Forms.VisualMarker.MatchParent, target);
		}

		[Test]
		public void SetVisualToMatchParentAndInheritParentValue()
		{
			var layout = ImplicitDefaultLayout();
			var layout2 = ExplicitMaterialLayout();
			IVisualController view = ExplicitDefaultView();

			AddExplicitMaterialToLayout(layout, layout2);

			AddExplicitDefaultToLayout(layout2, (View)view);

			((View)view).Visual = Forms.VisualMarker.MatchParent;

			var target = view.EffectiveVisual;

			Assert.IsTrue(target.IsMaterial(), "EffectiveVisual should be Material");
			Assert.IsTrue(!target.IsDefault(), "EffectiveVisual should be Material");
		}

		[Test]
		public void SetGrandparentAndInheritExplicitParentValue()
		{
			var layout = ExplicitMaterialLayout();
			var layout2 = ExplicitDefaultLayout();
			IVisualController view = ImplicitDefaultView();

			AddExplicitDefaultToLayout(layout, layout2);
			AddImplicitToDefault(layout2, (View)view);

			var target = view.EffectiveVisual;

			Assume.That(((IVisualController)layout).EffectiveVisual.IsMaterial());

			Assume.That(((IVisualController)layout2).EffectiveVisual.IsDefault());

			Assert.IsTrue(!target.IsMaterial(), "EffectiveVisual should be Default");
			Assert.IsTrue(target.IsDefault(), "EffectiveVisual should be Default");
		}

		[Test]
		public void SetGrandparentUsingAnonCtorAndMaintainExplicitParentValue()
		{
			var layout = new StackLayout
			{
				Visual = Forms.VisualMarker.Material,
				Children = {
					new StackLayout {
						Visual = Forms.VisualMarker.Default,
						Children = { ImplicitDefaultView() }
					}
				}
			};

			var layout2 = layout.Children[0] as StackLayout;
			IVisualController view = layout2.Children[0] as View;

			var target = view.EffectiveVisual;

			Assume.That(((IVisualController)layout).EffectiveVisual.IsMaterial());

			Assume.That(((IVisualController)layout2).EffectiveVisual.IsDefault());

			Assert.IsTrue(!target.IsMaterial(), "EffectiveVisual should be Default");
			Assert.IsTrue(target.IsDefault(), "EffectiveVisual should be Default");
		}

		[Test]
		public void SetGrandparentUsingCtorAndMaintainExplicitParentValue()
		{
			IVisualController view = ImplicitDefaultView();
			var layout2 = new StackLayout { Visual = Forms.VisualMarker.Default, Children = { (View)view } };
			var layout = new StackLayout { Visual = Forms.VisualMarker.Material, Children = { layout2 } };

			var target = view.EffectiveVisual;

			Assume.That(((IVisualController)layout).EffectiveVisual.IsMaterial());

			Assume.That(((IVisualController)layout2).EffectiveVisual.IsDefault());

			Assert.IsTrue(!target.IsMaterial(), "EffectiveVisual should be Default");
			Assert.IsTrue(target.IsDefault(), "EffectiveVisual should be Default");
		}

		[Test]
		public void SetParentAndGrandchildrenInheritValue()
		{
			var layout = ExplicitMaterialLayout();
			var layout2 = ImplicitDefaultLayout();
			IVisualController view = ImplicitDefaultView();

			AddImplicitToMaterial(layout, layout2);

			AddImplicitToMaterial(layout2, (View)view);

			var target = view.EffectiveVisual;

			Assert.IsTrue(target.IsMaterial(), "EffectiveVisual should be Material");
			Assert.IsTrue(!target.IsDefault(), "EffectiveVisual should be Material");
		}

		[Test]
		public void SetParentAndContentAndGrandchildrenInheritValue()
		{
			var layout = ExplicitMaterialLayout();
			var layout2 = ImplicitDefaultScrollView();
			IVisualController view = ImplicitDefaultView();

			AddImplicitToMaterial(layout, layout2);

			AddImplicitToMaterialScrollView(layout2, (View)view);

			var target = view.EffectiveVisual;

			Assert.IsTrue(target.IsMaterial(), "EffectiveVisual should be Material");
			Assert.IsTrue(!target.IsDefault(), "EffectiveVisual should be Material");
		}


		[Test]
		public void SetParentAndInheritExplicitParentValue()
		{
			var layout = ExplicitMaterialLayout();
			IVisualController view = ImplicitDefaultView();

			AddImplicitToMaterial(layout, (View)view);

			var target = view.EffectiveVisual;

			Assert.IsTrue(target.IsMaterial(), "EffectiveVisual should be Material");
			Assert.IsTrue(!target.IsDefault(), "EffectiveVisual should be Material");
		}

		[Test]
		public void SetParentAndMaintainExplicitValue()
		{
			var layout = ExplicitMaterialLayout();
			IVisualController view = ExplicitDefaultView();

			AddExplicitDefaultToLayout(layout, (View)view);

			var target = view.EffectiveVisual;

			Assert.IsTrue(!target.IsMaterial(), "EffectiveVisual should be Default");
			Assert.IsTrue(target.IsDefault(), "EffectiveVisual should be Default");
			Assert.AreEqual(Forms.VisualMarker.Default, ((View)view).Visual);
		}

		[Test]
		public void SetParentUsingCtorAndInheritParentValue()
		{
			IVisualController view = ImplicitDefaultView();
			var layout = new StackLayout { Visual = Forms.VisualMarker.Material, Children = { (View)view } };

			Assume.That(((IVisualController)layout).EffectiveVisual.IsMaterial());

			Assume.That(((View)view).Visual == Forms.VisualMarker.MatchParent);

			var target = view.EffectiveVisual;

			Assert.IsTrue(target.IsMaterial(), "EffectiveVisual should be Material");
			Assert.IsTrue(!target.IsDefault(), "EffectiveVisual should be Material");
			Assert.AreEqual(Forms.VisualMarker.MatchParent, ((View)view).Visual);
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

			IVisualController controller = child;

			Assume.That(controller.EffectiveVisual.IsDefault(), "child view Visual should be Default");
			Assume.That(child.Visual == Forms.VisualMarker.Default, "child view Visual should be Default");
		}

		static void AddExplicitDefaultToLayout(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IVisualController controller = child;

			Assume.That(controller.EffectiveVisual.IsDefault(), "child view Visual should be Default");
			Assume.That(child.Visual == Forms.VisualMarker.Default, "child view Visual should be Default");
		}

		static void AddExplicitMaterialToScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IVisualController controller = child;

			Assume.That(child.Visual == Forms.VisualMarker.Material, "child view Visual should be Material");
		}

		static void AddExplicitMaterialToLayout(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IVisualController controller = child;

			Assume.That(controller.EffectiveVisual.IsMaterial(), "child view EffectiveVisual should be Material");
			Assume.That(child.Visual == Forms.VisualMarker.Material, "child view Visual should be Material");
		}

		static void AddImplicitToDefault(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IVisualController controller = child;

			Assume.That(child.Visual == Forms.VisualMarker.MatchParent, "child view Visual should be MatchParent");
		}

		static void AddImplicitToMaterial(StackLayout parent, View child)
		{
			parent.Children.Add(child);

			IVisualController controller = child;


			Assume.That(controller.EffectiveVisual.IsMaterial(), "child view EffectiveVisual should be Material");
			Assume.That(child.Visual == Forms.VisualMarker.MatchParent, "child view Visual should be MatchParent");
		}

		static void AddImplicitToMaterialScrollView(ScrollView parent, View child)
		{
			parent.Content = child;

			IVisualController controller = child;

			Assume.That(controller.EffectiveVisual == Forms.VisualMarker.Material, "child view EffectiveVisual should be Material");
			Assume.That(child.Visual == Forms.VisualMarker.MatchParent, "child view Visual should be MatchParent");
		}

		static StackLayout ExplicitDefaultLayout()
		{
			var layout = new StackLayout { Visual = Forms.VisualMarker.Default };

			IVisualController controller = layout;

			Assume.That(controller.EffectiveVisual == Forms.VisualMarker.Default, "Explicit Default view EffectiveVisual should be Default");
			Assume.That(layout.Visual == Forms.VisualMarker.Default, "Explicit Default view Visual should be Default");
			return layout;
		}

		static View ExplicitDefaultView()
		{
			var view = new View { Visual = Forms.VisualMarker.Default };

			IVisualController controller = view;

			Assume.That(controller.EffectiveVisual.IsDefault(), "Explicit Default view EffectiveVisual should be Default");
			Assume.That(((View)view).Visual == Forms.VisualMarker.Default, "Explicit Default view Visual should be Default");

			return view;
		}

		static ScrollView ExplicitMaterialScrollView()
		{
			var layout = new ScrollView { Visual = Forms.VisualMarker.Material };

			IVisualController controller = layout;

			Assume.That(controller.EffectiveVisual == Forms.VisualMarker.Material, "Explicit RTL view EffectiveVisual should be Material");
			Assume.That(layout.Visual == Forms.VisualMarker.Material, "Explicit RTL view Visual should be Material");

			return layout;
		}

		static StackLayout ExplicitMaterialLayout()
		{
			var layout = new StackLayout { Visual = Forms.VisualMarker.Material };

			IVisualController controller = layout;

			Assume.That(controller.EffectiveVisual.IsMaterial(), "Explicit RTL view EffectiveVisual should be Material");
			Assume.That(layout.Visual == Forms.VisualMarker.Material, "Explicit RTL view Visual should be Material");

			return layout;
		}

		static ScrollView ImplicitDefaultScrollView()
		{
			var layout = new ScrollView();

			IVisualController controller = layout;

			Assume.That(controller.EffectiveVisual == Forms.VisualMarker.Default, "New view EffectiveVisual should be Default");
			Assume.That(layout.Visual == Forms.VisualMarker.MatchParent, "New view Visual should be MatchParent");

			return layout;
		}

		static StackLayout ImplicitDefaultLayout()
		{
			var layout = new StackLayout();

			IVisualController controller = layout;


			Assume.That(controller.EffectiveVisual == Forms.VisualMarker.Default, "New view EffectiveVisual should be Default");
			Assume.That(layout.Visual == Forms.VisualMarker.MatchParent, "New view Visual should be MatchParent");

			return layout;
		}

		static View ImplicitDefaultView()
		{
			var view = new View();

			IVisualController controller = view;

			Assume.That(((View)view).Visual == Forms.VisualMarker.MatchParent, "New view Visual should be MatchParent");

			return view;
		}

		class PropertyWatchingView : View
		{
			public int VisualPropertyChangedCount { get; private set; }

			protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				base.OnPropertyChanged(propertyName);

				if (propertyName == View.VisualProperty.PropertyName)
					VisualPropertyChangedCount++;
			}
		}
	}
}