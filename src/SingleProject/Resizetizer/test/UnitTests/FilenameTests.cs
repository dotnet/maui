using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public sealed class WindowsTheory : TheoryAttribute
	{
		public WindowsTheory()
			: base()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				Skip = $"Ignored unless on Windows";
		}
	}

	public sealed class UnixTheory : TheoryAttribute
	{
		public UnixTheory()
			: base()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				Skip = $"Ignored on Windows";
		}
	}

	public class FilenameTests
	{
		[Theory]
		[InlineData("one.png")]
		[InlineData("o1.png")]
		[InlineData("two2.png")]
		[InlineData("t3hree.png")]
		[InlineData("f4our5.png")]
		[InlineData("o_ne.png")]
		[InlineData("o_1.png")]
		[InlineData("t_w_o2.png")]
		[InlineData("t_3_hree.png")]
		[InlineData("f4_our5.png")]
		[InlineData("a.png")]
		public void ValidFilenames(string filename)
			=> Assert.True(IsValidFilename(filename));

		[WindowsTheory]
		[InlineData("C:\\some\\path\\one.png")]
		[InlineData("C:\\some\\path\\o1.png")]
		[InlineData("C:\\some\\path\\two2.png")]
		[InlineData("C:\\some\\path\\t3hree.png")]
		[InlineData("C:\\some\\path\\f4our5.png")]
		public void ValidFilenames_Windows(string filename)
			=> Assert.True(IsValidFilename(filename));

		[UnixTheory]
		[InlineData("/some/one.png")]
		[InlineData("/some/o1.png")]
		[InlineData("/some/two2.png")]
		[InlineData("/some/t3hree.png")]
		[InlineData("/some/f4our5.png")]
		public void ValidFilenames_Unix(string filename)
			=> Assert.True(IsValidFilename(filename));

		[Theory]
		[InlineData("1one.png")]
		[InlineData("O1.png")]
		[InlineData("t-wo2.png")]
		[InlineData("_t3_hree.png")]
		[InlineData("f4our 5.png")]
		[InlineData("_1one.png")]
		[InlineData("o1_.png")]
		[InlineData("o=.png")]
		[InlineData("1a.png")]
		public void InvalidFilenames(string filename)
			=> Assert.False(IsValidFilename(filename));

		[WindowsTheory]
		[InlineData("C:\\some\\path\\on-e.png")]
		[InlineData("C:\\some\\path\\2o1.png")]
		[InlineData("C:\\some\\path\\tWo2.png")]
		[InlineData("C:\\some\\path\\t3+hree.png")]
		[InlineData("C:\\some\\path\\f4o ur5.png")]
		public void InvalidFilenames_Windows(string filename)
			=> Assert.False(IsValidFilename(filename));

		[UnixTheory]
		[InlineData("/some/path_.png")]
		[InlineData("/some/3two2.png")]
		[InlineData("/some/t-3hree.png")]
		[InlineData("/some/f4oUr5.png")]
		public void InvalidFilenames_Unix(string filename)
			=> Assert.False(IsValidFilename(filename));

		bool IsValidFilename(string filename)
			=> Utils.IsValidResourceFilename(filename);
	}
}
