#nullable disable
using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Maui.Controls.StyleSheets
{
	abstract class Selector
	{
		Selector()
		{
		}

		/// <summary>Parses a selector from a pre-cleaned text string (no comments). Used by compiled path.</summary>
		internal static Selector Parse(string selectorText)
		{
			using var reader = new StringReader(selectorText);
			using var cssReader = new CssReader(reader);
			return Parse(cssReader);
		}

		public static Selector Parse(CssReader reader, char stopChar = '\0')
		{
			Selector root = All, workingRoot = All;
			Operator workingRootParent = null;
			Action<Operator, Selector> setCurrentSelector = (op, sel) => SetCurrentSelector(ref root, ref workingRoot, ref workingRootParent, op, sel);

			int p;
			reader.SkipWhiteSpaces();
			while ((p = reader.Peek()) > 0)
			{
				switch (unchecked((char)p))
				{
					case '*':
						setCurrentSelector(new And(), All);
						reader.Read();
						break;
					case '.':
						reader.Read();
						var className = reader.ReadIdent();
						if (className == null)
							return Invalid;
						setCurrentSelector(new And(), new Class(className));
						break;
					case '#':
						reader.Read();
						var id = reader.ReadName();
						if (id == null)
							return Invalid;
						setCurrentSelector(new And(), new Id(id));
						break;
					case ':':
						reader.Read();
						// Check for :: (pseudo-elements — not supported)
						if (reader.Peek() == ':')
							return Invalid;
						var pseudoClass = reader.ReadIdent();
						if (string.Equals(pseudoClass, "root", StringComparison.OrdinalIgnoreCase))
							setCurrentSelector(new And(), new Root());
						else if (string.Equals(pseudoClass, "first-child", StringComparison.OrdinalIgnoreCase))
							setCurrentSelector(new And(), new FirstChild());
						else if (string.Equals(pseudoClass, "last-child", StringComparison.OrdinalIgnoreCase))
							setCurrentSelector(new And(), new LastChild());
						else if (string.Equals(pseudoClass, "hover", StringComparison.OrdinalIgnoreCase))
							setCurrentSelector(new And(), new Hover());
						else if (string.Equals(pseudoClass, "focus", StringComparison.OrdinalIgnoreCase))
							setCurrentSelector(new And(), new Focused());
						else if (string.Equals(pseudoClass, "disabled", StringComparison.OrdinalIgnoreCase))
							setCurrentSelector(new And(), new Disabled());
						else if (string.Equals(pseudoClass, "not", StringComparison.OrdinalIgnoreCase))
						{
							// Parse :not(inner-selector)
							if (reader.Peek() != '(')
								return Invalid;
							reader.Read(); // consume '('
							var inner = Parse(reader, ')');
							if (reader.Peek() == ')')
								reader.Read(); // consume ')'
							setCurrentSelector(new And(), new Not(inner));
						}
						else
							return Invalid; // Unsupported pseudo-class
						break;
					case '[':
						reader.Read(); // consume '['
						reader.SkipWhiteSpaces();
						var attrName = reader.ReadIdent();
						if (string.IsNullOrEmpty(attrName))
							return Invalid;
						reader.SkipWhiteSpaces();
						var next = reader.Peek();
						if (next == ']')
						{
							reader.Read();
							// [attr] — presence check, not supported meaningfully in MAUI
							setCurrentSelector(new And(), Invalid);
						}
						else if (next == '=')
						{
							reader.Read(); // consume '='
							reader.SkipWhiteSpaces();
							var attrValue = ReadAttributeValue(reader);
							reader.SkipWhiteSpaces();
							if (reader.Peek() == ']')
								reader.Read();
							setCurrentSelector(new And(), new AttributeEquals(attrName, attrValue));
						}
						else if (next == '~')
						{
							reader.Read(); // consume '~'
							if (reader.Peek() == '=')
								reader.Read(); // consume '='
							reader.SkipWhiteSpaces();
							var attrValue = ReadAttributeValue(reader);
							reader.SkipWhiteSpaces();
							if (reader.Peek() == ']')
								reader.Read();
							// [attr~=value] on class → class contains
							if (string.Equals(attrName, "class", StringComparison.OrdinalIgnoreCase))
								setCurrentSelector(new And(), new Class(attrValue));
							else
								setCurrentSelector(new And(), new AttributeEquals(attrName, attrValue));
						}
						else
						{
							// Skip to closing ]
							reader.ReadUntil(']');
							if (reader.Peek() == ']')
								reader.Read();
							setCurrentSelector(new And(), Invalid);
						}
						break;
					case ',':
						reader.Read();
						setCurrentSelector(new Or(), All);
						reader.SkipWhiteSpaces();
						break;
					case '+':
						reader.Read();
						setCurrentSelector(new Adjacent(), All);
						reader.SkipWhiteSpaces();
						break;
					case '~':
						reader.Read();
						setCurrentSelector(new Sibling(), All);
						reader.SkipWhiteSpaces();
						break;
					case '>':
						reader.Read();
						setCurrentSelector(new Child(), All);
						reader.SkipWhiteSpaces();
						break;
					case '^':               //not in CSS spec
						reader.Read();
						var element = reader.ReadIdent();
						if (element == null)
							return Invalid;
						setCurrentSelector(new And(), new Base(element));
						break;
					case ' ':
					case '\t':
					case '\n':
					case '\r':
					case '\f':
						reader.Read();
						bool processWs = false;
						while ((p = reader.Peek()) > 0)
						{
							var c = unchecked((char)p);
							if (char.IsWhiteSpace(c))
							{
								reader.Read();
								continue;
							}
							processWs = (c != '+'
										&& c != '>'
										&& c != ','
										&& c != '~'
										&& c != stopChar);
							break;
						}
						if (!processWs)
							break;
						setCurrentSelector(new Descendent(), All);
						reader.SkipWhiteSpaces();
						break;
					default:
						if (unchecked((char)p) == stopChar)
							return root;

						var elementName = reader.ReadIdent();
						if (elementName == null)
							return Invalid;
						setCurrentSelector(new And(), new Element(elementName));
						break;
				}
			}
			return root;
		}

		static string ReadAttributeValue(TextReader reader)
		{
			reader.SkipWhiteSpaces();
			int p = reader.Peek();
			if (p == '"' || p == '\'')
			{
				var quote = (char)reader.Read();
				var val = reader.ReadUntil(quote);
				if (reader.Peek() == quote)
					reader.Read();
				return val;
			}
			return reader.ReadUntil(']', ' ', '\t');
		}

		static void SetCurrentSelector(ref Selector root, ref Selector workingRoot, ref Operator workingRootParent, Operator op, Selector sel)
		{
			var updateRoot = root == workingRoot;

			op.Left = workingRoot;
			op.Right = sel;
			workingRoot = op;
			workingRootParent?.Right = workingRoot;

			if (updateRoot)
				root = workingRoot;

			if (workingRoot is Or)
			{
				workingRootParent = (Operator)workingRoot;
				workingRoot = sel;
			}
		}

		public abstract bool Matches(IStyleSelectable styleable);

		internal static Selector Invalid = new Generic(s => false);
		internal static Selector All = new Generic(s => true);

		abstract class UnarySelector : Selector
		{
		}

		abstract class Operator : Selector
		{
			public Selector Left { get; set; } = Invalid;
			public Selector Right { get; set; } = Invalid;
		}

		sealed class Generic : UnarySelector
		{
			readonly Func<IStyleSelectable, bool> func;
			public Generic(Func<IStyleSelectable, bool> func)
			{
				this.func = func;
			}

			public override bool Matches(IStyleSelectable styleable) => func(styleable);
		}

		sealed class Class : UnarySelector
		{
			public Class(string className)
			{
				ClassName = className;
			}

			public string ClassName { get; }
			public override bool Matches(IStyleSelectable styleable)
				=> styleable.Classes != null && styleable.Classes.Contains(ClassName);
		}

		sealed class Id : UnarySelector
		{
			public Id(string id)
			{
				IdName = id;
			}

			public string IdName { get; }
			public override bool Matches(IStyleSelectable styleable) => styleable.Id == IdName;
		}

		sealed class Or : Operator
		{
			public override bool Matches(IStyleSelectable styleable) => Right.Matches(styleable) || Left.Matches(styleable);
		}

		sealed class And : Operator
		{
			public override bool Matches(IStyleSelectable styleable) => Right.Matches(styleable) && Left.Matches(styleable);
		}

		sealed class Element : UnarySelector
		{
			public Element(string elementName)
			{
				ElementName = elementName;
			}

			public string ElementName { get; }
			public override bool Matches(IStyleSelectable styleable) =>
				string.Equals(styleable.NameAndBases[0], ElementName, StringComparison.OrdinalIgnoreCase);
		}

		sealed class Base : UnarySelector
		{
			public Base(string elementName)
			{
				ElementName = elementName;
			}

			public string ElementName { get; }
			public override bool Matches(IStyleSelectable styleable)
			{
				for (var i = 0; i < styleable.NameAndBases.Length; i++)
					if (string.Equals(styleable.NameAndBases[i], ElementName, StringComparison.OrdinalIgnoreCase))
						return true;
				return false;
			}
		}

		sealed class Root : UnarySelector
		{
			public override bool Matches(IStyleSelectable styleable) => styleable.Parent == null;
		}

		sealed class FirstChild : UnarySelector
		{
			public override bool Matches(IStyleSelectable styleable)
			{
				if (styleable.Parent == null)
					return false;
				foreach (var child in styleable.Parent.Children)
					return child == styleable;
				return false;
			}
		}

		sealed class LastChild : UnarySelector
		{
			public override bool Matches(IStyleSelectable styleable)
			{
				if (styleable.Parent == null)
					return false;
				IStyleSelectable last = null;
				foreach (var child in styleable.Parent.Children)
					last = child;
				return last == styleable;
			}
		}

		sealed class Not : UnarySelector
		{
			readonly Selector _inner;
			public Not(Selector inner) => _inner = inner;
			internal Selector Inner => _inner;
			public override bool Matches(IStyleSelectable styleable) => !_inner.Matches(styleable);
		}

		sealed class Hover : UnarySelector
		{
			public override bool Matches(IStyleSelectable styleable) => false;
		}

		sealed class Focused : UnarySelector
		{
			public override bool Matches(IStyleSelectable styleable) => false;
		}

		sealed class Disabled : UnarySelector
		{
			public override bool Matches(IStyleSelectable styleable) => false;
		}

		sealed class AttributeEquals : UnarySelector
		{
			readonly string _attrName;
			readonly string _attrValue;

			public AttributeEquals(string attrName, string attrValue)
			{
				_attrName = attrName;
				_attrValue = attrValue;
			}

			public override bool Matches(IStyleSelectable styleable)
			{
				// Map CSS attribute names to IStyleSelectable properties
				if (string.Equals(_attrName, "id", StringComparison.OrdinalIgnoreCase))
					return string.Equals(styleable.Id, _attrValue, StringComparison.OrdinalIgnoreCase);
				if (string.Equals(_attrName, "class", StringComparison.OrdinalIgnoreCase))
					return styleable.Classes != null && styleable.Classes.Contains(_attrValue);
				return false;
			}
		}

		sealed class Child : Operator
		{
			public override bool Matches(IStyleSelectable styleable) =>
				Right.Matches(styleable) && styleable.Parent != null && Left.Matches(styleable.Parent);
		}

		sealed class Descendent : Operator
		{
			public override bool Matches(IStyleSelectable styleable)
			{
				if (!Right.Matches(styleable))
					return false;
				var parent = styleable.Parent;
				while (parent != null)
				{
					if (Left.Matches(parent))
						return true;
					parent = parent.Parent;
				}
				return false;
			}
		}

		sealed class Adjacent : Operator
		{
			public override bool Matches(IStyleSelectable styleable)
			{
				if (!Right.Matches(styleable))
					return false;
				if (styleable.Parent == null)
					return false;

				IStyleSelectable prev = null;
				foreach (var elem in styleable.Parent.Children)
				{
					if (elem == styleable && prev != null)
						return Left.Matches(prev);
					prev = elem;
				}
				return false;
				//var index = styleable.Parent.Children.IndexOf(styleable);
				//if (index == 0)
				//	return false;
				//var adjacent = styleable.Parent.Children[index - 1];
				//return Left.Matches(adjacent);
			}
		}

		sealed class Sibling : Operator
		{
			public override bool Matches(IStyleSelectable styleable)
			{
				if (!Right.Matches(styleable))
					return false;
				if (styleable.Parent == null)
					return false;

				int selfIndex = 0;
				bool foundSelfInParent = false;
				foreach (var elem in styleable.Parent.Children)
				{
					if (elem == styleable)
					{
						foundSelfInParent = true;
						break;
					}
					++selfIndex;
				}

				if (!foundSelfInParent)
					return false;

				int index = 0;
				foreach (var elem in styleable.Parent.Children)
				{
					if (index >= selfIndex)
						return false;
					if (Left.Matches(elem))
						return true;
					++index;
				}

				return false;

				//var index = styleable.Parent.Children.IndexOf(styleable);
				//if (index == 0)
				//	return false;
				//int siblingIndex = -1;
				//for (var i = 0; i < index; i++)
				//	if (Left.Matches(styleable.Parent.Children[i])) {
				//		siblingIndex = i;
				//		break;
				//	}
				//return siblingIndex != -1;
			}
		}

		[DebuggerDisplay("{id} - {class} = {type}")]
		public struct SelectorSpecificity
		{
			int id;
			int @class;
			int type;

			public int Id { get => id; }
			public int Class { get => @class; }
			public int Type { get => type; }

			public static SelectorSpecificity FromSelector(Selector selector)
			{
				switch (selector)
				{
					case Selector.Class c:
						return new SelectorSpecificity { @class = 1 };
					case Selector.Root r:
						return new SelectorSpecificity { @class = 1 };
					case Selector.FirstChild fc:
						return new SelectorSpecificity { @class = 1 };
					case Selector.LastChild lc:
						return new SelectorSpecificity { @class = 1 };
					case Selector.Not n:
						return FromSelector(n.Inner);
					case Selector.AttributeEquals ae:
						return new SelectorSpecificity { @class = 1 };
					case Selector.Id i:
						return new SelectorSpecificity { id = 1 };
					case Selector.Element e:
						return new SelectorSpecificity { type = 1 };
					case Selector.Base b:
						return new SelectorSpecificity { type = 1 };
					case Selector.And a:
						return FromSelector(a.Left) + FromSelector(a.Right);
					case Selector.Or o:
						return FromSelector(o.Left) + FromSelector(o.Right);
					case Selector.Child c:
						return FromSelector(c.Left) + FromSelector(c.Right);
					case Selector.Descendent d:
						return FromSelector(d.Left) + FromSelector(d.Right);
					case Selector.Adjacent a:
						return FromSelector(a.Left) + FromSelector(a.Right);
					case Selector.Sibling s:
						return FromSelector(s.Left) + FromSelector(s.Right);
					default:
						return default;
				}
			}

			public static SelectorSpecificity operator +(SelectorSpecificity left, SelectorSpecificity right)
			{
				return new SelectorSpecificity
				{
					id = left.id + right.id,
					@class = left.@class + right.@class,
					type = left.type + right.type
				};
			}
			public SelectorSpecificity Add(SelectorSpecificity other)
			{
				return new SelectorSpecificity
				{
					id = id + other.id,
					@class = @class + other.@class,
					type = type + other.type
				};
			}
	}
}

