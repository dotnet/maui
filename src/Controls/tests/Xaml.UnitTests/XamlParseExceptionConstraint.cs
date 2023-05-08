using System;
using NUnit.Framework.Constraints;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class XamlParseExceptionConstraint : ExceptionTypeConstraint
	{
		bool haslineinfo;
		int linenumber;
		int lineposition;
		Func<string, bool> messagePredicate;

		XamlParseExceptionConstraint(bool haslineinfo) : base(typeof(XamlParseException))
		{
			this.haslineinfo = haslineinfo;
		}

		public override string DisplayName => "xamlparse";

		public XamlParseExceptionConstraint() : this(false)
		{
		}

		public XamlParseExceptionConstraint(int linenumber, int lineposition, Func<string, bool> messagePredicate = null) : this(true)
		{
			this.linenumber = linenumber;
			this.lineposition = lineposition;
			this.messagePredicate = messagePredicate;
		}

		protected override bool Matches(object actual)
		{
			if (!base.Matches(actual))
				return false;
			var xmlInfo = ((XamlParseException)actual).XmlInfo;
			if (!haslineinfo)
				return true;
			if (xmlInfo == null || !xmlInfo.HasLineInfo())
				return false;
			if (messagePredicate != null)
				if (!messagePredicate(((XamlParseException)actual).UnformattedMessage))
					return false;
			return xmlInfo.LineNumber == linenumber && xmlInfo.LinePosition == lineposition;
		}

		public override string Description
		{
			get
			{
				if (haslineinfo)
				{
					return string.Format($"{base.Description} line {linenumber}, position {lineposition}");
				}
				return base.Description;
			}
		}
	}
}
