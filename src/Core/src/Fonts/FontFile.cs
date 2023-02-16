#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a font file.
	/// </summary>
	public class FontFile
	{
		/// <summary>
		/// Supported extensions for <see cref="FontFile"/>.
		/// </summary>
		public static readonly string[] Extensions = { ".ttf", ".otf", };

		/// <summary>
		/// The filename for this font.
		/// </summary>
		/// <remarks>This should not include the extension. If you want the filename including the extension use <see cref="FileNameWithExtension()"/>.</remarks>
		public string? FileName { get; set; }

		/// <summary>
		/// The filename extension for this font.
		/// </summary>
		public string? Extension { get; set; }

		/// <summary>
		/// The font PostScript name as read from the font file.
		/// </summary>
		/// <remarks>Some platforms have issues with spaces in the PostScript name. In this property spaces might be stripped.
		/// To get the PostScript name with spaces (if it had any) use <see cref="GetPostScriptNameWithSpaces"/>.</remarks>
		public string? PostScriptName { get; set; }

		/// <summary>
		/// Gets the filename of this font file with the provided extension appended to the end.
		/// </summary>
		/// <param name="extension">The extension to append to the font filename.</param>
		/// <returns>The filename of this font file including the given extension.</returns>
		/// <remarks>The value for <paramref name="extension"/> should include a leading dot (.) character.</remarks>
		public string FileNameWithExtension(string? extension) => $"{FileName}{extension}";

		/// <summary>
		/// Gets the filename of this font file, including the extension.
		/// </summary>
		/// <returns>The filename of this font file, including the extension.</returns>
		/// <remarks>This returns the combination of <see cref="FileName"/> and <see cref="Extension"/>.</remarks>
		public string FileNameWithExtension() => FileNameWithExtension(Extension);

		/// <summary>
		/// Gets the font PostScript name as read from the font file, including any spaces if it had any.
		/// </summary>
		/// <returns>The font PostScript name including spaces.</returns>
		public string GetPostScriptNameWithSpaces() => string.Join(" ", GetFontName(PostScriptName!));

		/// <summary>
		/// Creates a new instance of a <see cref="FontFile"/> object based on the value in <paramref name="input"/>.
		/// </summary>
		/// <param name="input">Can either be a filename or font family name.</param>
		/// <returns>A new <see cref="FontFile"/> object with all the information that could be deduced through <paramref name="input"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <see langword="null"/></exception>
		public static FontFile FromString(string input)
		{
			_ = input ?? throw new ArgumentNullException(nameof(input));

			var hashIndex = input.IndexOf("#", StringComparison.Ordinal);
			//UWP names require Spaces. Sometimes people may use those, "CuteFont-Regular#Cute Font" should be "CuteFont-Regular#CuteFont"
			var postScriptName = hashIndex > 0 ? input.Substring(hashIndex + 1)
#if NETSTANDARD2_0
				.Replace(" ", "")
#else
				.Replace(" ", "", StringComparison.Ordinal)
#endif
				: input;
			//Get the fontFamily name;
			var fontFamilyName = hashIndex > 0 ? input.Substring(0, hashIndex) : input;

			string? foundExtension = null;
			foreach (var extension in Extensions)
			{
				if (fontFamilyName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
				{
					foundExtension = extension;
					fontFamilyName = fontFamilyName.Substring(0, fontFamilyName.Length - foundExtension.Length);
					break;
				}
			}

			return new FontFile
			{
				FileName = fontFamilyName,
				Extension = foundExtension,
				PostScriptName = postScriptName,
			};
		}

		static IEnumerable<string> GetFontName(string fontFamily)
		{
			_ = fontFamily ?? throw new ArgumentNullException(nameof(fontFamily));

			if (fontFamily.IndexOf(" ", StringComparison.Ordinal) != -1)
			{
				yield return fontFamily;
				//We are done, they have spaces, they have it handled.
				yield break;
			}

			string currentString = "";
			char lastCharacter = ' ';
			var index = fontFamily.LastIndexOf("-", StringComparison.Ordinal);
			bool multipleCaps = false;
			var cleansedString = index > 0 ? fontFamily.Substring(0, index) : fontFamily;
			foreach (var c in cleansedString)
			{
				//Always break on these characters
				if (c == '_' || c == '-')
				{
					yield return currentString;
					//Reset everything,
					currentString = "";
					lastCharacter = ' ';
					multipleCaps = false;
				}
				else
				{
					if (char.IsUpper(c))
					{
						//If the last character is lowercase, we are in a new CamelCase font
						if (char.IsLower(lastCharacter))
						{
							yield return currentString;
							currentString = "";
							lastCharacter = ' ';
						}
						else if (char.IsUpper(lastCharacter))
						{
							multipleCaps = true;
						}
					}

					//Detect multiple UpperCase letters so we can separate things like PTSansNarrow into "PT Sans Narrow"
					else if (multipleCaps && currentString.Length > 1)
					{
						var last = currentString[currentString.Length - 1];
						yield return currentString.Substring(0, currentString.Length - 1);
						//Reset everything so it doesnt do a space
						multipleCaps = false;
						lastCharacter = ' ';
						currentString = last.ToString();
					}

					currentString += c;
					lastCharacter = c;
				}
			}

			//Send what is left!
			if (!string.IsNullOrWhiteSpace(currentString))
				yield return currentString.Trim();
		}
	}
}