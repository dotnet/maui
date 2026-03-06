using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		[Fact]
		public void RootSelectorMatchesElementWithNoParent()
		{
			var rootNode = new MockStylable
			{
				NameAndBases = new[] { "Page", "VisualElement" },
				Children = new List<IStyleSelectable>()
			};
			// No parent set → :root should match
			var selector = Selector.Parse(new CssReader(new StringReader(":root")));
			Assert.True(selector.Matches(rootNode));
		}

		[Fact]
		public void RootSelectorDoesNotMatchChildElement()
		{
			// Page has a parent set by SetParents → :root should NOT match
			var selector = Selector.Parse(new CssReader(new StringReader(":root")));
			Assert.False(selector.Matches(Label0));
			Assert.False(selector.Matches(StackLayout));
		}

		// --- :first-child / :last-child tests ---

		[Fact]
		public void FirstChildMatchesFirstElement()
		{
			var selector = Selector.Parse(new CssReader(new StringReader("label:first-child")));
			Assert.True(selector.Matches(Label0));   // first label in stacklayout
			Assert.False(selector.Matches(Label1));   // second
			Assert.False(selector.Matches(Label3));
			Assert.False(selector.Matches(Label4));   // last
		}

		[Fact]
		public void LastChildMatchesLastElement()
		{
			var selector = Selector.Parse(new CssReader(new StringReader("label:last-child")));
			Assert.False(selector.Matches(Label0));
			Assert.False(selector.Matches(Label1));
			Assert.True(selector.Matches(Label4));    // last in stacklayout
		}

		[Fact]
		public void FirstChildInNestedContainer()
		{
			// Label2 is the only (thus first and last) child of ContentView0
			var selector = Selector.Parse(new CssReader(new StringReader(":first-child")));
			Assert.True(selector.Matches(Label2));
		}

		// --- :not() tests ---

		[Fact]
		public void NotSelectorNegatesMatch()
		{
			// :not(.test) should match elements WITHOUT class "test"
			var selector = Selector.Parse(new CssReader(new StringReader(":not(.test)")));
			Assert.False(selector.Matches(Label0));  // has class "test"
			Assert.True(selector.Matches(Label1));   // no class "test"
			Assert.False(selector.Matches(Label2));  // has class "test"
			Assert.True(selector.Matches(Label3));
			Assert.True(selector.Matches(Label4));
		}

		[Fact]
		public void NotSelectorWithElementType()
		{
			// label:not(#foo) should match labels that don't have id "foo"
			var selector = Selector.Parse(new CssReader(new StringReader("label:not(#foo)")));
			Assert.True(selector.Matches(Label0));
			Assert.True(selector.Matches(Label1));
			Assert.False(selector.Matches(Label3));  // has id "foo"
			Assert.True(selector.Matches(Label4));
		}

		// --- [attr=value] tests ---

		[Fact]
		public void AttributeSelectorMatchesId()
		{
			var selector = Selector.Parse(new CssReader(new StringReader("[id=foo]")));
			Assert.False(selector.Matches(Label0));
			Assert.True(selector.Matches(Label3));    // has Id="foo"
			Assert.False(selector.Matches(Label4));
		}

		[Fact]
		public void AttributeSelectorMatchesQuotedValue()
		{
			var selector = Selector.Parse(new CssReader(new StringReader("[id=\"foo\"]")));
			Assert.True(selector.Matches(Label3));
			Assert.False(selector.Matches(Label0));
		}

		[Fact]
		public void AttributeSelectorClassContains()
		{
			// [class~=test] — class attribute contains "test" token
			var selector = Selector.Parse(new CssReader(new StringReader("[class~=test]")));
			Assert.True(selector.Matches(Label0));    // has class "test"
			Assert.False(selector.Matches(Label1));   // no class
			Assert.True(selector.Matches(Label2));    // has class "test"
		}
	}
}
