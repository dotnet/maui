using System;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Maui.UITestCategories
{
	public static class CSharpCodeMasker
	{
		public static string Mask(string text)
		{
			// Lex every platform branch without evaluating its preprocessor condition.
			var lexicalInput = Regex.Replace(text, @"(?m)^[ \t]*#", match =>
				match.Value.Substring(0, match.Length - 1) + " ");
			var characters = text.ToCharArray();
			foreach (var token in SyntaxFactory.ParseTokens(
				lexicalInput, options: new CSharpParseOptions(LanguageVersion.Preview)))
			{
				var kind = token.Kind().ToString();
				if (kind.EndsWith("Token", StringComparison.Ordinal) &&
					(kind.Contains("String") || kind == "CharacterLiteralToken"))
				{
					MaskSpan(characters, token.Span.Start, token.Span.Length);
				}
				foreach (var trivia in token.LeadingTrivia)
					MaskComment(characters, trivia);
				foreach (var trivia in token.TrailingTrivia)
					MaskComment(characters, trivia);
			}
			return new string(characters);
		}

		static void MaskComment(char[] characters, SyntaxTrivia trivia)
		{
			if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
				trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
				trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
				trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
			{
				MaskSpan(characters, trivia.Span.Start, trivia.Span.Length);
			}
		}

		static void MaskSpan(char[] characters, int start, int length)
		{
			for (var index = start; index < start + length; index++)
				if (characters[index] != '\r' && characters[index] != '\n')
					characters[index] = ' ';
		}
	}
}
