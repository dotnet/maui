using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Represents a text with attributes applied to some parts.
	/// </summary>
	/// <remarks>
	/// Formatted string consists of spans that represent text segments with various attributes applied.
	/// </remarks>
	public class FormattedString
	{
		/// <summary>
		/// A flag indicating whether the instance contains just a plain string without any formatting.
		/// </summary>
		/// <remarks>
		/// <c>true</c> if the instance contains an unformatted string.
		/// </remarks>
		readonly bool _just_string;

		/// <summary>
		/// Holds the unformatted string.
		/// </summary>
		/// <remarks>
		/// The contents of this field are accurate if and only if the _just_string flag is set.
		/// </remarks>
		readonly string _string;

		/// <summary>
		/// Holds the collection of span elements.
		/// </summary>
		/// <remarks>
		/// Span elements are basically chunks of text with uniform formatting.
		/// </remarks>
		readonly ObservableCollection<Span> _spans;

		/// <summary>
		/// Returns the collection of span elements.
		/// </summary>
		public IList<Span> Spans { get { return _spans; } }

		/// <summary>
		/// Creates a new FormattedString instance with an empty string.
		/// </summary>
		public FormattedString()
		{
			_just_string = false;
			_spans = new ObservableCollection<Span>();
		}

		/// <summary>
		/// Creates a new FormattedString instance based on given <c>str</c>.
		/// </summary>
		/// <param name="str">
		/// A string used to make a new FormattedString instance.
		/// </param>
		public FormattedString(string str)
		{
			_just_string = true;
			_string = str;
		}

		/// <summary>
		/// Returns the plain text of the FormattedString as an unformatted string.
		/// </summary>
		/// <returns>
		/// The text content of the FormattedString without any format applied.
		/// </returns>
		public override string ToString()
		{
			if (_just_string)
			{
				return _string;
			}
			else
			{
				return string.Concat(from span in this.Spans select span.Text);
			}
		}

		/// <summary>
		/// Returns the markup text representation of the FormattedString instance.
		/// </summary>
		/// <returns>The string containing a markup text.</returns>
		internal string ToMarkupString()
		{
			if (_just_string)
			{
				return _string;
			}
			else
			{
				return string.Concat(from span in Spans select span.GetMarkupText());
			}
		}

		/// <summary>
		/// Casts the FormattedString to a string.
		/// </summary>
		/// <param name="formatted">The FormattedString instance which will be used for the conversion.</param>
		public static explicit operator string (FormattedString formatted)
		{
			return formatted.ToString();
		}

		/// <summary>
		/// Casts the string to a FormattedString.
		/// </summary>
		/// <param name="text">The text which will be put in a new FormattedString instance.</param>
		public static implicit operator FormattedString(string text)
		{
			return new FormattedString(text);
		}

		/// <summary>
		/// Casts the Span to a FormattedString.
		/// </summary>
		/// <param name="span">The span which will be used for the conversion.</param>
		public static implicit operator FormattedString(Span span)
		{
			return new FormattedString()
			{
				Spans = { span }
			};
		}
	}
}
