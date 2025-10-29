using System;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	/// <summary>
	/// Helper for asserting XamlParseException with specific line and position information
	/// </summary>
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

		public void Validate(Action action)
		{
			var ex = Xunit.Assert.Throws<XamlParseException>(action);

			if (!haslineinfo)
				return;

			var xmlInfo = ex.XmlInfo;
			Xunit.Assert.NotNull(xmlInfo);
			Xunit.Assert.True(xmlInfo.HasLineInfo(), "Expected exception to have line info");

			if (messagePredicate != null)
			{
				Xunit.Assert.True(messagePredicate(ex.UnformattedMessage),
					$"Exception message did not match predicate. Message: {ex.UnformattedMessage}");
			}

			Xunit.Assert.Equal(linenumber, xmlInfo.LineNumber);
			Xunit.Assert.Equal(lineposition, xmlInfo.LinePosition);
		}
	}
}
