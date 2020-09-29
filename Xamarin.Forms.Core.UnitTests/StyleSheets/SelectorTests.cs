using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.Forms.StyleSheets.UnitTests
{
	[TestFixture]
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

		[SetUp]
		public void SetUp()
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

		[TestCase("label", true, true, true, true, true, false)]
		[TestCase(" label", true, true, true, true, true, false)]
		[TestCase("label ", true, true, true, true, true, false)]
		[TestCase(".test", true, false, true, false, false, true)]
		[TestCase("label.test", true, false, true, false, false, false)]
		[TestCase("stacklayout>label.test", true, false, false, false, false, false)]
		[TestCase("stacklayout >label.test", true, false, false, false, false, false)]
		[TestCase("stacklayout> label.test", true, false, false, false, false, false)]
		[TestCase("stacklayout label.test", true, false, true, false, false, false)]
		[TestCase("stacklayout  label.test", true, false, true, false, false, false)]
		[TestCase("stacklayout .test", true, false, true, false, false, true)]
		[TestCase("stacklayout.test", false, false, false, false, false, false)]
		[TestCase("*", true, true, true, true, true, true)]
		[TestCase("#foo", false, false, false, true, false, false)]
		[TestCase("label#foo", false, false, false, true, false, false)]
		[TestCase("div#foo", false, false, false, false, false, false)]
		[TestCase(".test,#foo", true, false, true, true, false, true)]
		[TestCase(".test ,#foo", true, false, true, true, false, true)]
		[TestCase(".test, #foo", true, false, true, true, false, true)]
		[TestCase("#foo,.test", true, false, true, true, false, true)]
		[TestCase("#foo ,.test", true, false, true, true, false, true)]
		[TestCase("#foo, .test", true, false, true, true, false, true)]
		[TestCase("contentview+label", false, false, false, true, false, false)]
		[TestCase("contentview +label", false, false, false, true, false, false)]
		[TestCase("contentview+ label", false, false, false, true, false, false)]
		[TestCase("contentview~label", false, false, false, true, true, false)]
		[TestCase("contentview ~label", false, false, false, true, true, false)]
		[TestCase("contentview\r\n~label", false, false, false, true, true, false)]
		[TestCase("contentview~ label", false, false, false, true, true, false)]
		[TestCase("label~*", false, true, false, true, true, true)]
		[TestCase("label~.test", false, false, false, false, false, true)]
		[TestCase("label~#foo", false, false, false, true, false, false)]
		[TestCase("page contentview stacklayout label", false, false, false, false, false, false)]
		[TestCase("page stacklayout contentview label", false, false, true, false, false, false)]
		[TestCase("page contentview label", false, false, true, false, false, false)]
		[TestCase("page contentview>label", false, false, true, false, false, false)]
		[TestCase("page>stacklayout contentview label", false, false, true, false, false, false)]
		[TestCase("page stacklayout>contentview label", false, false, true, false, false, false)]
		[TestCase("page stacklayout contentview>label", false, false, true, false, false, false)]
		[TestCase("page>stacklayout>contentview label", false, false, true, false, false, false)]
		[TestCase("page>stack/* comment * */layout>contentview label", false, false, true, false, false, false)]
		[TestCase("page>stacklayout contentview>label", false, false, true, false, false, false)]
		[TestCase("page stacklayout>contentview>label", false, false, true, false, false, false)]
		[TestCase("page>stacklayout>contentview>label", false, false, true, false, false, false)]
		[TestCase("visualelement", false, false, false, false, false, false)]
		[TestCase("^visualelement", true, true, true, true, true, true)]
		[TestCase("^layout", false, false, false, false, false, true)]
		[TestCase("stacklayout visualelement", false, false, false, false, false, false)]
		[TestCase("stacklayout>visualelement", false, false, false, false, false, false)]
		[TestCase("stacklayout ^visualelement", true, true, true, true, true, true)]
		[TestCase("stacklayout>^visualelement", true, true, false, true, true, true)]
		public void TestCase(string selectorString, bool label0match, bool label1match, bool label2match, bool label3match, bool label4match, bool content0match)
		{
			var selector = Selector.Parse(new CssReader(new StringReader(selectorString)));
			Assert.AreEqual(label0match, selector.Matches(Label0));
			Assert.AreEqual(label1match, selector.Matches(Label1));
			Assert.AreEqual(label2match, selector.Matches(Label2));
			Assert.AreEqual(label3match, selector.Matches(Label3));
			Assert.AreEqual(label4match, selector.Matches(Label4));
			Assert.AreEqual(content0match, selector.Matches(ContentView0));
		}
	}
}