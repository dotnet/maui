using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms
{
	public class FontFile
	{
		public string FileName { get; set; }
		public string Extension { get; set; }
		public string FileNameWithExtension(string extension) => $"{FileName}{extension}";
		public string FileNameWithExtension() => FileNameWithExtension(Extension);
		public string PostScriptName { get; set; }

		public string GetPostScriptNameWithSpaces() =>
			string.Join(" ", GetFontName(PostScriptName));

		public static readonly string[] Extensions = {
				".ttf",
				".otf",
			};

		public static FontFile FromString(string input)
		{
			var hashIndex = input.IndexOf("#", System.StringComparison.Ordinal);
			//UWP names require Spaces. Sometimes people may use those, "CuteFont-Regular#Cute Font" should be "CuteFont-Regular#CuteFont"
			var postScriptName = hashIndex > 0 ? input.Substring(hashIndex + 1).Replace(" ", "") : input;
			//Get the fontFamily name;
			var fontFamilyName = hashIndex > 0 ? input.Substring(0, hashIndex) : input;

			var foundExtension = Extensions.
				FirstOrDefault(x => fontFamilyName.EndsWith(x, StringComparison.OrdinalIgnoreCase));

			if (!string.IsNullOrWhiteSpace(foundExtension))
				fontFamilyName = fontFamilyName.Substring(0, fontFamilyName.Length - foundExtension.Length);

			return new FontFile
			{
				FileName = fontFamilyName,
				Extension = foundExtension,
				PostScriptName = postScriptName,
			};
		}


		static IEnumerable<string> GetFontName(string fontFamily)
		{
			if (fontFamily.Contains(" "))
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