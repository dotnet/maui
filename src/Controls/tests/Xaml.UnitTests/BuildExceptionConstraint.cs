using System;
using Microsoft.Maui.Controls.Build.Tasks;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	/// <summary>
	/// Helper for asserting BuildException with specific line and position information
	/// </summary>
	public class BuildExceptionConstraint
	{
		readonly bool _haslineinfo;
		readonly int _linenumber;
		readonly int _lineposition;
		readonly Func<string, bool> _messagePredicate;

		BuildExceptionConstraint(bool haslineinfo)
		{
			_haslineinfo = haslineinfo;
		}

		public BuildExceptionConstraint() : this(false)
		{
		}

		public BuildExceptionConstraint(int linenumber, int lineposition, Func<string, bool> messagePredicate = null) : this(true)
		{
			_linenumber = linenumber;
			_lineposition = lineposition;
			_messagePredicate = messagePredicate;
		}

		public void Validate(Action action)
		{
			var ex = Xunit.Assert.Throws<BuildException>(action);
			
			if (!_haslineinfo)
				return;
				
			var xmlInfo = ex.XmlInfo;
			Xunit.Assert.NotNull(xmlInfo);
			Xunit.Assert.True(xmlInfo.HasLineInfo(), $"Expected exception to have line info");
			
			if (_messagePredicate != null)
			{
				Xunit.Assert.True(_messagePredicate(ex.Message), 
					$"Exception message did not match predicate. Message: {ex.Message}");
			}
			
			Xunit.Assert.Equal(_linenumber, xmlInfo.LineNumber);
			Xunit.Assert.Equal(_lineposition, xmlInfo.LinePosition);
		}
	}
}