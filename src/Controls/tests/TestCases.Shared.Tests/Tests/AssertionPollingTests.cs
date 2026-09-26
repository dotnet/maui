using NUnit.Framework;

namespace Microsoft.Maui.TestCases.Tests;

[TestFixture]
[Category(UITestCategories.ViewBaseTests)]
public class AssertionPollingTests
{
	[Test]
	public void TransientAssertionDoesNotFailTheTest()
	{
		var attempts = 0;
		UtilExtensions.RetryAssert(null!,
			() => Assert.That(++attempts, Is.EqualTo(2)),
			retryFrequency: TimeSpan.FromMilliseconds(1));
		Assert.That(attempts, Is.EqualTo(2));
	}

	[Test]
	public void PersistentAssertionIsNotSwallowed()
	{
		Assert.Throws<AssertionException>(() => UtilExtensions.RetryAssert(null!,
			() => Assert.That(false, Is.True, "Persistent failure"),
			timeout: TimeSpan.Zero));
	}

	[Test]
	public void MultipleAssertionFailuresAreNotSwallowed()
	{
		Assert.Throws<MultipleAssertException>(() => UtilExtensions.RetryAssert(null!,
			() => Assert.Multiple(() =>
			{
				Assert.That(false, Is.True);
				Assert.That(1, Is.EqualTo(2));
			}),
			timeout: TimeSpan.Zero));
	}

	[Test]
	public void NonTransientExceptionsAreNotRetried()
	{
		var attempts = 0;
		Assert.Throws<InvalidOperationException>(() => UtilExtensions.RetryAssert(null!, () =>
		{
			attempts++;
			throw new InvalidOperationException("Invalid fixture state");
		}));
		Assert.That(attempts, Is.EqualTo(1));
	}
}
