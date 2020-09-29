using System;

namespace Xamarin.Forms.StyleSheets
{
	abstract class Selector
	{
		Selector()
		{
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
					case '[':
						throw new NotImplementedException("Attributes not implemented");
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

		static void SetCurrentSelector(ref Selector root, ref Selector workingRoot, ref Operator workingRootParent, Operator op, Selector sel)
		{
			var updateRoot = root == workingRoot;

			op.Left = workingRoot;
			op.Right = sel;
			workingRoot = op;
			if (workingRootParent != null)
				workingRootParent.Right = workingRoot;

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
	}
}