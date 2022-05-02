#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="Type[@FullName='Microsoft.Maui.FontFile']/Docs" />
	public class FontFile
	{
		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='Extensions']/Docs" />
		public static readonly string[] Extensions = { ".ttf", ".otf", };

		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='FileName']/Docs" />
		public string? FileName { get; set; }

		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='Extension']/Docs" />
		public string? Extension { get; set; }

		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='PostScriptName']/Docs" />
		public string? PostScriptName { get; set; }

		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='FileNameWithExtension'][2]/Docs" />
		public string FileNameWithExtension(string? extension) => $"{FileName}{extension}";

		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='FileNameWithExtension'][1]/Docs" />
		public string FileNameWithExtension() => FileNameWithExtension(Extension);

		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='GetPostScriptNameWithSpaces']/Docs" />
		public string GetPostScriptNameWithSpaces() => string.Join(" ", GetFontName(PostScriptName!));

		/// <include file="../../docs/Microsoft.Maui/FontFile.xml" path="//Member[@MemberName='FromString']/Docs" />
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