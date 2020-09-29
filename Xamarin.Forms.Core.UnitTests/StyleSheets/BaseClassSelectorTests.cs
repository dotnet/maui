using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.StyleSheets;
using Xamarin.Forms.StyleSheets.UnitTests;

namespace Xamarin.Forms.Core.UnitTests.StyleSheets
{
	[TestFixture]
	public class BaseClassSelectorTests
	{
		IStyleSelectable Page;
		IStyleSelectable StackLayout => Page.Children.First();
		IStyleSelectable Label0 => StackLayout.Children.First();
		IStyleSelectable Label1 => AbsoluteLayout0.Children.First();
		IStyleSelectable CustomLabel0 => StackLayout.Children.Skip(1).First();
		IStyleSelectable CustomLabel1 => AbsoluteLayout0.Children.Skip(1).First();
		IStyleSelectable AbsoluteLayout0 => StackLayout.Children.Skip(2).First();

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
							new MockStylable {NameAndBases = new[] { "CustomLabel", "Label", "VisualElement" }},										//CustomLabel0
							new MockStylable {														//AbsoluteLayout0
								NameAndBases = new[] { "AbsoluteLayout", "Layout", "VisualElement" },
								Classes = new[]{"test"},
								Children = new List<IStyleSelectable> {
									new MockStylable {NameAndBases = new[] { "Label", "VisualElement" }, Classes = new[]{"test"}},		//Label1
									new MockStylable {NameAndBases = new[] { "CustomLabel", "Label", "VisualElement" }, Classes = new[]{"test"}},		//CustomLabel1
								}
							}
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

		[TestCase("stacklayout label", true, true, false, false, false)]
		[TestCase("stacklayout>label", true, false, false, false, false)]
		[TestCase("stacklayout ^label", true, true, true, true, false)]
		[TestCase("stacklayout>^label", true, false, true, false, false)]
		public void TestCase(string selectorString, bool label0match, bool label1match, bool customLabel0match, bool customLabel1match, bool absoluteLayout0match)
		{
			var selector = Selector.Parse(new CssReader(new StringReader(selectorString)));
			Assert.AreEqual(label0match, selector.Matches(Label0));
			Assert.AreEqual(label1match, selector.Matches(Label1));
			Assert.AreEqual(customLabel0match, selector.Matches(CustomLabel0));
			Assert.AreEqual(customLabel1match, selector.Matches(CustomLabel1));
			Assert.AreEqual(absoluteLayout0match, selector.Matches(AbsoluteLayout0));
		}
	}
}