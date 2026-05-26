using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	/// <summary>
	/// Helper class for asserting XAML-related exceptions in xUnit tests.
	/// Replaces NUnit's BuildExceptionConstraint and XamlParseExceptionConstraint.
	/// </summary>
	internal static class XamlExceptionAssert
	{
		/// <summary>
		/// Asserts that the action throws a BuildException.
		/// </summary>
		internal static BuildException ThrowsBuildException(Action action)
		{
			return Assert.Throws<BuildException>(action);
		}

		/// <summary>
		/// Asserts that the action throws a BuildException with the specified line info.
		/// </summary>
		internal static BuildException ThrowsBuildException(int lineNumber, int linePosition, Action action)
		{
			var ex = Assert.Throws<BuildException>(action);
			Assert.NotNull(ex.XmlInfo);
			Assert.True(ex.XmlInfo.HasLineInfo(), "BuildException should have line info");
			Assert.Equal(lineNumber, ex.XmlInfo.LineNumber);
			Assert.Equal(linePosition, ex.XmlInfo.LinePosition);
			return ex;
		}

		/// <summary>
		/// Asserts that the action throws a BuildException with the specified line info and message predicate.
		/// </summary>
		internal static BuildException ThrowsBuildException(int lineNumber, int linePosition, Func<string, bool> messagePredicate, Action action)
		{
			var ex = Assert.Throws<BuildException>(action);
			Assert.NotNull(ex.XmlInfo);
			Assert.True(ex.XmlInfo.HasLineInfo(), "BuildException should have line info");
			Assert.Equal(lineNumber, ex.XmlInfo.LineNumber);
			Assert.Equal(linePosition, ex.XmlInfo.LinePosition);
			if (messagePredicate != null)
			{
				Assert.True(messagePredicate(ex.Message), $"Message predicate failed for message: {ex.Message}");
			}
			return ex;
		}

		/// <summary>
		/// Asserts that the action throws a XamlParseException.
		/// </summary>
		internal static XamlParseException ThrowsXamlParseException(Action action)
		{
			return Assert.Throws<XamlParseException>(action);
		}

		/// <summary>
		/// Asserts that the action throws a XamlParseException with the specified line info.
		/// </summary>
		internal static XamlParseException ThrowsXamlParseException(int lineNumber, int linePosition, Action action)
		{
			var ex = Assert.Throws<XamlParseException>(action);
			Assert.NotNull(ex.XmlInfo);
			Assert.True(ex.XmlInfo.HasLineInfo(), "XamlParseException should have line info");
			Assert.Equal(lineNumber, ex.XmlInfo.LineNumber);
			Assert.Equal(linePosition, ex.XmlInfo.LinePosition);
			return ex;
		}

		/// <summary>
		/// Asserts that the action throws a XamlParseException with the specified line info and message predicate.
		/// </summary>
		internal static XamlParseException ThrowsXamlParseException(int lineNumber, int linePosition, Func<string, bool> messagePredicate, Action action)
		{
			var ex = Assert.Throws<XamlParseException>(action);
			Assert.NotNull(ex.XmlInfo);
			Assert.True(ex.XmlInfo.HasLineInfo(), "XamlParseException should have line info");
			Assert.Equal(lineNumber, ex.XmlInfo.LineNumber);
			Assert.Equal(linePosition, ex.XmlInfo.LinePosition);
			if (messagePredicate != null)
			{
				Assert.True(messagePredicate(ex.UnformattedMessage), $"Message predicate failed for message: {ex.UnformattedMessage}");
			}
			return ex;
		}
	}
}
