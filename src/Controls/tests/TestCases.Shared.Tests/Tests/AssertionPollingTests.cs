using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[TestFixture]
[Category(UITestCategories.ViewBaseTests)]
public class AssertionPollingTests
{
	[Test]
	public void TransientAssertionDoesNotFailTheTest()
	{
		var attempts = 0;
		((IApp)null!).RetryAssert(
			() => Assert.That(++attempts, Is.EqualTo(2)),
			retryFrequency: TimeSpan.FromMilliseconds(1));
		Assert.That(attempts, Is.EqualTo(2));
	}

	[Test]
	public void TransientMultipleAssertionsDoNotFailTheTest()
	{
		var enclosingResult = TestExecutionContext.CurrentContext.CurrentResult;
		var attempts = 0;
		((IApp)null!).RetryAssert(() =>
		{
			attempts++;
			Assert.Multiple(() =>
			{
				Assert.That(attempts, Is.EqualTo(2));
				Assert.That(attempts * 2, Is.EqualTo(4));
			});
		}, retryFrequency: TimeSpan.FromMilliseconds(1));

		Assert.That(attempts, Is.EqualTo(2));
		Assert.That(enclosingResult.AssertionResults
			.Where(assertion => assertion.Status == AssertionStatus.Failed), Is.Empty);
	}

	[Test]
	public void PersistentAssertionIsNotSwallowed()
	{
		Assert.Throws<AssertionException>(() => ((IApp)null!).RetryAssert(
			() => Assert.That(false, Is.True, "Persistent failure"),
			timeout: TimeSpan.Zero));
	}

	[Test]
	public void MultipleAssertionFailuresAreNotSwallowed()
	{
		Assert.Throws<MultipleAssertException>(() => ((IApp)null!).RetryAssert(
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
		Assert.Throws<InvalidOperationException>(() => ((IApp)null!).RetryAssert(() =>
		{
			attempts++;
			throw new InvalidOperationException("Invalid fixture state");
		}));
		Assert.That(attempts, Is.EqualTo(1));
	}

	[Test]
	public void WarningsAreNotDiscarded()
	{
		using var context = new TestExecutionContext.IsolatedContext();
		((IApp)null!).RetryAssert(() => Assert.Warn("Visible warning"));
		Assert.That(TestExecutionContext.CurrentContext.CurrentResult.AssertionResults
			.Count(assertion => assertion.Status == AssertionStatus.Warning), Is.EqualTo(1));
	}
}
