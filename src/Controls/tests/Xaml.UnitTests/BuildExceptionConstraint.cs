using System;
using Microsoft.Maui.Controls.Build.Tasks;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	// Helper class for asserting BuildException in XUnit
	// Usage: BuildExceptionHelper.AssertThrows(() => code, lineNumber, linePosition);
	public static class BuildExceptionHelper
	{
		public static void AssertThrows(Action action, int? lineNumber = null, int? linePosition = null, Func<string, bool> messagePredicate = null)
		{
			var exception = Xunit.Assert.Throws<BuildException>(action);

			if (lineNumber.HasValue || linePosition.HasValue)
			{
				var xmlInfo = exception.XmlInfo;
				Xunit.Assert.NotNull(xmlInfo);
				Xunit.Assert.True(xmlInfo.HasLineInfo(), $"Expected line info but XmlInfo.HasLineInfo() returned false");

				if (lineNumber.HasValue)
					Xunit.Assert.Equal(lineNumber.Value, xmlInfo.LineNumber);

				if (linePosition.HasValue)
					Xunit.Assert.Equal(linePosition.Value, xmlInfo.LinePosition);
			}

			if (messagePredicate != null)
			{
				Xunit.Assert.True(messagePredicate(exception.Message),
					$"Message predicate failed for message: {exception.Message}");
			}
		}
	}

	// Kept for backward compatibility - marked as obsolete
	[Obsolete("Use BuildExceptionHelper.AssertThrows instead")]
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
	}
}