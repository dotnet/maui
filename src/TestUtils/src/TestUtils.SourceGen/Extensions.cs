using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Maui.TestUtils.SourceGen
{
	public static class Extensions
	{
		public static void AssertGeneratedContent(this Compilation compile, string content)
			=> Assert.Contains(
				compile.SyntaxTrees.Where(s => !string.IsNullOrEmpty(s.FilePath)), 
				s => s.ToString().Contains(content));
		public static void AssertNotGeneratedContent(this Compilation compile, string content)
			=> Assert.DoesNotContain(
				compile.SyntaxTrees.Where(s => !string.IsNullOrEmpty(s.FilePath)),
				s => s.ToString().Contains(content));

		public static void AssertAnyContent(this Compilation compile, string content)
			=> Assert.Contains(compile.SyntaxTrees, s => s.ToString().Contains(content));
		public static void AssertNotAnyContent(this Compilation compile, string content)
			=> Assert.DoesNotContain(compile.SyntaxTrees, s => s.ToString().Contains(content));
	}
}
