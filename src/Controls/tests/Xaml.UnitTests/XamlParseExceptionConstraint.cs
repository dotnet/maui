using System;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	// Helper class for asserting XamlParseException in XUnit
	// Usage: XamlParseExceptionHelper.AssertThrows(() => code, lineNumber, linePosition);
	public static class XamlParseExceptionHelper
	{
		public static void AssertThrows(Action action, int? lineNumber = null, int? linePosition = null, Func<string, bool> messagePredicate = null)
		{
			var exception = Xunit.Assert.Throws<XamlParseException>(action);

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
				Xunit.Assert.True(messagePredicate(exception.UnformattedMessage),
					$"Message predicate failed for message: {exception.UnformattedMessage}");
			}
		}
	}

	// Kept for backward compatibility - marked as obsolete
	[Obsolete("Use XamlParseExceptionHelper.AssertThrows instead")]
	public class XamlParseExceptionConstraint
	{
		bool haslineinfo;
		int linenumber;
		int lineposition;
		Func<string, bool> messagePredicate;

		XamlParseExceptionConstraint(bool haslineinfo)
		{
			this.haslineinfo = haslineinfo;
		}

		public XamlParseExceptionConstraint() : this(false)
		{
		}

		public XamlParseExceptionConstraint(int linenumber, int lineposition, Func<string, bool> messagePredicate = null) : this(true)
		{
			this.linenumber = linenumber;
			this.lineposition = lineposition;
			this.messagePredicate = messagePredicate;
		}
	}
}
