using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class VisualElementInputTransparentTests
	{
		// this is both for color diff and cols
		const bool truee = true;

		static IReadOnlyDictionary<(bool, bool, bool, bool, bool), (bool, bool)> States = new Dictionary<(bool, bool, bool, bool, bool), (bool, bool)>
		{
			[(truee, truee, truee, truee, truee)] = (truee, truee),
			[(truee, truee, truee, truee, false)] = (truee, truee),
			[(truee, truee, truee, false, truee)] = (truee, truee),
			[(truee, truee, truee, false, false)] = (truee, truee),
			[(truee, truee, false, truee, truee)] = (truee, truee),
			[(truee, truee, false, truee, false)] = (truee, truee),
			[(truee, truee, false, false, truee)] = (truee, truee),
			[(truee, truee, false, false, false)] = (truee, truee),
			[(truee, false, truee, truee, truee)] = (truee, truee),
			[(truee, false, truee, truee, false)] = (truee, truee),
			[(truee, false, truee, false, truee)] = (truee, truee),
			[(truee, false, truee, false, false)] = (truee, false),
			[(truee, false, false, truee, truee)] = (false, truee),
			[(truee, false, false, truee, false)] = (false, false),
			[(truee, false, false, false, truee)] = (false, truee),
			[(truee, false, false, false, false)] = (false, false),
			[(false, truee, truee, truee, truee)] = (truee, truee),
			[(false, truee, truee, truee, false)] = (truee, truee),
			[(false, truee, truee, false, truee)] = (truee, truee),
			[(false, truee, truee, false, false)] = (truee, false),
			[(false, truee, false, truee, truee)] = (false, truee),
			[(false, truee, false, truee, false)] = (false, false),
			[(false, truee, false, false, truee)] = (false, truee),
			[(false, truee, false, false, false)] = (false, false),
			[(false, false, truee, truee, truee)] = (truee, truee),
			[(false, false, truee, truee, false)] = (truee, truee),
			[(false, false, truee, false, truee)] = (truee, truee),
			[(false, false, truee, false, false)] = (truee, false),
			[(false, false, false, truee, truee)] = (false, truee),
			[(false, false, false, truee, false)] = (false, false),
			[(false, false, false, false, truee)] = (false, truee),
			[(false, false, false, false, false)] = (false, false),
		};

		public static IEnumerable<object[]> TransparencyStates()
		{
			foreach (var pair in States)
			{
				var (rT, rC, nT, nC, t) = pair.Key;

				yield return new object[] { rT, rC, nT, nC, t };
			}
		}

		static (Layout Root, Layout Nested, VisualElement Child) CreateViews(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			Layout root;
			Layout nested;
			VisualElement child;

			root = new Grid
			{
				InputTransparent = rootTrans,
				CascadeInputTransparent = rootCascade,
				Children =
 				{
 					 (nested = new Grid
 					 {
 						 InputTransparent = nestedTrans,
 						 CascadeInputTransparent = nestedCascade,
 						 Children =
 						 {
 							 (child = new Button
 							 {
 								 InputTransparent = trans
 							 })
 						 }
 					 })
 				}
			};

			return (root, nested, child);
		}

		static void AssertState(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans, Layout nested, VisualElement child)
		{
			var (finalNestedTrans, finalTrans) = States[(rootTrans, rootCascade, nestedTrans, nestedCascade, trans)];

			if (finalNestedTrans)
				Assert.True(nested.InputTransparent, "Nested layout was not input transparent when it should have been.");
			else
				Assert.False(nested.InputTransparent, "Nested layout was input transparent when it should not have been.");

			if (finalTrans)
				Assert.True(child.InputTransparent, "Child element was not input transparent when it should have been.");
			else
				Assert.False(child.InputTransparent, "Child element was input transparent when it should not have been.");
		}

		static void AssertState(bool layoutTrans, bool layoutCascade, bool trans, Layout layout, VisualElement child)
		{
			var (finalLayoutTrans, finalTrans) = States[(false, false, layoutTrans, layoutCascade, trans)];

			if (finalLayoutTrans)
				Assert.True(layout.InputTransparent, "Layout was not input transparent when it should have been.");
			else
				Assert.False(layout.InputTransparent, "Layout was input transparent when it should not have been.");

			if (finalTrans)
				Assert.True(child.InputTransparent, "Child element was not input transparent when it should have been.");
			else
				Assert.False(child.InputTransparent, "Child element was input transparent when it should not have been.");
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void TestMethodsAreValid(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var (rootLayout, nestedLayout, element) = CreateViews(rootTrans, rootCascade, nestedTrans, nestedCascade, trans);

			AssertState(rootTrans, rootCascade, nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void InitialStateIsCorrectWhenOrderIsPropsRootChildNestedChild(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var rootLayout = new Grid
			{
				InputTransparent = rootTrans,
				CascadeInputTransparent = rootCascade
			};
			var nestedLayout = new Grid
			{
				InputTransparent = nestedTrans,
				CascadeInputTransparent = nestedCascade
			};
			var element = new Button
			{
				InputTransparent = trans
			};

			rootLayout.Add(nestedLayout);
			nestedLayout.Add(element);

			AssertState(rootTrans, rootCascade, nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void InitialStateIsCorrectWhenOrderIsPropsNestedChildRootChild(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var rootLayout = new Grid
			{
				InputTransparent = rootTrans,
				CascadeInputTransparent = rootCascade
			};
			var nestedLayout = new Grid
			{
				InputTransparent = nestedTrans,
				CascadeInputTransparent = nestedCascade
			};
			var element = new Button
			{
				InputTransparent = trans
			};

			nestedLayout.Add(element);
			rootLayout.Add(nestedLayout);

			AssertState(rootTrans, rootCascade, nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void StateIsCorrectWhenSettingChildNestedRoot(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var element = new Button();
			var nestedLayout = new Grid { element };
			var rootLayout = new Grid { nestedLayout };

			element.InputTransparent = trans;

			nestedLayout.InputTransparent = nestedTrans;
			nestedLayout.CascadeInputTransparent = nestedCascade;

			rootLayout.InputTransparent = rootTrans;
			rootLayout.CascadeInputTransparent = rootCascade;

			AssertState(rootTrans, rootCascade, nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void StateIsCorrectWhenSettingRootNestedChild(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var element = new Button();
			var nestedLayout = new Grid { element };
			var rootLayout = new Grid { nestedLayout };

			rootLayout.InputTransparent = rootTrans;
			rootLayout.CascadeInputTransparent = rootCascade;

			nestedLayout.InputTransparent = nestedTrans;
			nestedLayout.CascadeInputTransparent = nestedCascade;

			element.InputTransparent = trans;

			AssertState(rootTrans, rootCascade, nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void InvertingRootLayoutInputTransparentIsCorrect(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var (rootLayout, nestedLayout, element) = CreateViews(rootTrans, rootCascade, nestedTrans, nestedCascade, trans);

			rootLayout.InputTransparent = !rootTrans;

			AssertState(!rootTrans, rootCascade, nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void InvertingRootLayoutCascadeInputTransparentIsCorrect(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var (rootLayout, nestedLayout, element) = CreateViews(rootTrans, rootCascade, nestedTrans, nestedCascade, trans);

			rootLayout.CascadeInputTransparent = !rootCascade;

			AssertState(rootTrans, !rootCascade, nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void InvertingNestedLayoutInputTransparentIsCorrect(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var (rootLayout, nestedLayout, element) = CreateViews(rootTrans, rootCascade, nestedTrans, nestedCascade, trans);

			nestedLayout.InputTransparent = !nestedTrans;

			AssertState(rootTrans, rootCascade, !nestedTrans, nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void InvertingNestedLayoutCascadeInputTransparentIsCorrect(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var (rootLayout, nestedLayout, element) = CreateViews(rootTrans, rootCascade, nestedTrans, nestedCascade, trans);

			nestedLayout.CascadeInputTransparent = !nestedCascade;

			AssertState(rootTrans, rootCascade, nestedTrans, !nestedCascade, trans, nestedLayout, element);
		}

		[Theory]
		[MemberData(nameof(TransparencyStates))]
		public void InvertingChildInputTransparentIsCorrect(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
		{
			var (rootLayout, nestedLayout, element) = CreateViews(rootTrans, rootCascade, nestedTrans, nestedCascade, trans);

			element.InputTransparent = !trans;

			AssertState(rootTrans, rootCascade, nestedTrans, nestedCascade, !trans, nestedLayout, element);
		}

		[Theory]
		[InlineData(typeof(Button))]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(Editor))]
		[InlineData(typeof(Entry))]
		public void VisualElementsCanToggleInputTransparency(Type type)
		{
			var element = (VisualElement)Activator.CreateInstance(type);

			Assert.False(element.InputTransparent);

			element.InputTransparent = true;

			Assert.True(element.InputTransparent);

			element.InputTransparent = false;

			Assert.False(element.InputTransparent);
		}

		// TODO: the tests below may be duplicates of the tests above

		[Theory]
		[InlineData(typeof(Button), true)]
		[InlineData(typeof(VerticalStackLayout), true)]
		[InlineData(typeof(Editor), true)]
		[InlineData(typeof(Entry), true)]
		[InlineData(typeof(Button), false)]
		[InlineData(typeof(VerticalStackLayout), false)]
		[InlineData(typeof(Editor), false)]
		[InlineData(typeof(Entry), false)]
		public void InputTransparencyOnLayoutDoesNotAffectChildWhenNotCascadeInputTransparent(Type type, bool initialInputTransparent)
		{
			var element = (View)Activator.CreateInstance(type);
			var layout = new Grid
			{
				InputTransparent = initialInputTransparent,
				CascadeInputTransparent = false,
				Children =
 				{
 					element
 				}
			};

			AssertState(initialInputTransparent, false, false, layout, element);

			layout.InputTransparent = !initialInputTransparent;

			AssertState(!initialInputTransparent, false, false, layout, element);

			layout.InputTransparent = initialInputTransparent;

			AssertState(initialInputTransparent, false, false, layout, element);
		}

		[Theory]
		[InlineData(typeof(Button), true)]
		[InlineData(typeof(VerticalStackLayout), true)]
		[InlineData(typeof(Editor), true)]
		[InlineData(typeof(Entry), true)]
		[InlineData(typeof(Button), false)]
		[InlineData(typeof(VerticalStackLayout), false)]
		[InlineData(typeof(Editor), false)]
		[InlineData(typeof(Entry), false)]
		public void InputTransparencyOnLayoutAffectsChild(Type type, bool initialInputTransparent)
		{
			var element = (View)Activator.CreateInstance(type);
			var layout = new Grid
			{
				InputTransparent = initialInputTransparent,
				CascadeInputTransparent = true, // default
				Children =
 				{
 					element
 				}
			};

			AssertState(initialInputTransparent, true, false, layout, element);

			layout.InputTransparent = !initialInputTransparent;

			AssertState(!initialInputTransparent, true, false, layout, element);

			layout.InputTransparent = initialInputTransparent;

			AssertState(initialInputTransparent, true, false, layout, element);
		}

		[Theory]
		[InlineData(typeof(Button), true)]
		[InlineData(typeof(VerticalStackLayout), true)]
		[InlineData(typeof(Editor), true)]
		[InlineData(typeof(Entry), true)]
		[InlineData(typeof(Button), false)]
		[InlineData(typeof(VerticalStackLayout), false)]
		[InlineData(typeof(Editor), false)]
		[InlineData(typeof(Entry), false)]
		public void InputTransparencyOnLayoutDoesNotAffectDeeplyNestedChildWhenNotCascadeInputTransparent(Type type, bool initialInputTransparent)
		{
			var element = (View)Activator.CreateInstance(type);
			var layout = new Grid
			{
				InputTransparent = initialInputTransparent,
				CascadeInputTransparent = false,
				Children =
 				{
 					new Grid { new Grid { new Grid { element } } }
 				}
			};

			AssertState(initialInputTransparent, false, false, layout, element);

			layout.InputTransparent = !initialInputTransparent;

			AssertState(!initialInputTransparent, false, false, layout, element);

			layout.InputTransparent = initialInputTransparent;

			AssertState(initialInputTransparent, false, false, layout, element);
		}

		[Theory]
		[InlineData(typeof(Button), true)]
		[InlineData(typeof(VerticalStackLayout), true)]
		[InlineData(typeof(Editor), true)]
		[InlineData(typeof(Entry), true)]
		[InlineData(typeof(Button), false)]
		[InlineData(typeof(VerticalStackLayout), false)]
		[InlineData(typeof(Editor), false)]
		[InlineData(typeof(Entry), false)]
		public void InputTransparencyOnLayoutAffectsDeeplyNestedChild(Type type, bool initialInputTransparent)
		{
			var element = (View)Activator.CreateInstance(type);
			var layout = new Grid
			{
				InputTransparent = initialInputTransparent,
				CascadeInputTransparent = true, // default
				Children =
 				{
 					new Grid { new Grid { new Grid { element } } }
 				}
			};

			AssertState(initialInputTransparent, true, false, layout, element);

			layout.InputTransparent = !initialInputTransparent;

			AssertState(!initialInputTransparent, true, false, layout, element);

			layout.InputTransparent = initialInputTransparent;

			AssertState(initialInputTransparent, true, false, layout, element);
		}

		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public void InputTransparencyOnLayoutDoesNotOverrideNestedLayoutWhenNotCascadeInputTransparentButCascadeOnNested(bool parent, bool nested)
		{
			var element = new Button();
			Grid nestedLayout;
			var layout = new Grid
			{
				InputTransparent = parent,
				CascadeInputTransparent = false,
				Children =
 				{
 					new Grid
 					{
 						(nestedLayout = new Grid
 						{
 							InputTransparent = nested,
 							CascadeInputTransparent = true, // default
 							Children =
 							{
 								new Grid { element }
 							}
 						})
 					}
 				}
			};

			AssertState(parent, false, nested, true, false, nestedLayout, element);

			layout.InputTransparent = !parent;

			AssertState(!parent, false, nested, true, false, nestedLayout, element);

			layout.InputTransparent = parent;

			AssertState(parent, false, nested, true, false, nestedLayout, element);
		}

		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public void InputTransparencyOnLayoutDoesNotOverrideNestedLayoutWhenNotCascadeInputTransparent(bool parent, bool nested)
		{
			var element = new Button();
			Grid nestedLayout;
			var layout = new Grid
			{
				InputTransparent = parent,
				CascadeInputTransparent = false,
				Children =
 				{
 					new Grid
 					{
 						(nestedLayout = new Grid
 						{
 							InputTransparent = nested,
 							CascadeInputTransparent = false,
 							Children =
 							{
 								new Grid { element }
 							}
 						})
 					}
 				}
			};

			AssertState(parent, false, nested, false, false, nestedLayout, element);

			layout.InputTransparent = !parent;

			AssertState(!parent, false, nested, false, false, nestedLayout, element);

			layout.InputTransparent = parent;

			AssertState(parent, false, nested, false, false, nestedLayout, element);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, true)]
		[InlineData(false, true, false)]
		[InlineData(false, false, true)]
		[InlineData(false, false, false)]
		public void InputTransparencyOnLayoutOverridesNestedLayout(bool parent, bool nested, bool cascadeNested)
		{
			var element = new Button();
			Grid nestedLayout;
			var layout = new Grid
			{
				InputTransparent = parent,
				CascadeInputTransparent = true, // default
				Children =
 				{
 					new Grid
 					{
 						(nestedLayout = new Grid
 						{
 							InputTransparent = nested,
 							CascadeInputTransparent = cascadeNested,
 							Children =
 							{
 								new Grid { element }
 							}
 						})
 					}
 				}
			};

			AssertState(parent, true, nested, cascadeNested, false, nestedLayout, element);

			layout.InputTransparent = !parent;

			AssertState(!parent, true, nested, cascadeNested, false, nestedLayout, element);

			layout.InputTransparent = parent;

			AssertState(parent, true, nested, cascadeNested, false, nestedLayout, element);
		}
	}
}
