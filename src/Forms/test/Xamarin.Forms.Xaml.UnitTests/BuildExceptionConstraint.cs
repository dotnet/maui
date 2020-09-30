using System;
using NUnit.Framework.Constraints;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class BuildExceptionConstraint : ExceptionTypeConstraint
	{
		readonly bool _haslineinfo;
		readonly int _linenumber;
		readonly int _lineposition;
		readonly Func<string, bool> _messagePredicate;

		BuildExceptionConstraint(bool haslineinfo) : base(typeof(BuildException)) => _haslineinfo = haslineinfo;

		public override string DisplayName => "xamlparse";

		public BuildExceptionConstraint() : this(false)
		{
		}

		public BuildExceptionConstraint(int linenumber, int lineposition, Func<string, bool> messagePredicate = null) : this(true)
		{
			_linenumber = linenumber;
			_lineposition = lineposition;
			_messagePredicate = messagePredicate;
		}

		protected override bool Matches(object actual)
		{
			if (!base.Matches(actual))
				return false;
			var xmlInfo = ((BuildException)actual).XmlInfo;
			if (!_haslineinfo)
				return true;
			if (xmlInfo == null || !xmlInfo.HasLineInfo())
				return false;
			if (_messagePredicate != null && !_messagePredicate(((BuildException)actual).Message))
				return false;
			return xmlInfo.LineNumber == _linenumber && xmlInfo.LinePosition == _lineposition;
		}

		public override string Description
		{
			get
			{
				if (_haslineinfo)
					return string.Format($"{base.Description} line {_linenumber}, position {_lineposition}");

				return base.Description;
			}
		}
	}
}