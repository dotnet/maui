namespace Microsoft.Maui.ManualTests.Tests;

public class TestPageInfo : IComparable<TestPageInfo>
{
	public TestPageInfo(Type type, TestAttribute test)
	{
		Type = type;
		Test = test;
	}

	public Type Type { get; }

	public TestAttribute Test { get; }

	public int CompareTo(TestPageInfo other)
	{
		if (Test.Id.Length > other.Test.Id.Length)
		{
			return 1;
		}
		else if (Test.Id.Length < other.Test.Id.Length)
		{
			return -1;
		}
		else
		{
			return string.Compare(Test.Id, other.Test.Id, StringComparison.Ordinal);
		}
	}
}
