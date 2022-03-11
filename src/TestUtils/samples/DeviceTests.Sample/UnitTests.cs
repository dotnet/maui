using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Sample
{
	public class UnitTests
	{
		readonly ITestOutputHelper _output;

		public UnitTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void SuccessfulTest()
		{
			Assert.True(true);
		}

		[Fact(Skip = "This test is skipped.")]
		public void SkippedTest()
		{
		}

		[Fact]
		public void FailingTest()
		{
			throw new Exception("This is meant to fail.");
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public void ParameterizedTest(int number)
		{
			Assert.NotEqual(0, number);
		}

		[Fact]
		public void OutputTest()
		{
			_output.WriteLine("This is test output.");
		}

		[Fact]
		public void FailingOutputTest()
		{
			_output.WriteLine("This is test output.");
			throw new Exception("This is meant to fail.");
		}
	}
}