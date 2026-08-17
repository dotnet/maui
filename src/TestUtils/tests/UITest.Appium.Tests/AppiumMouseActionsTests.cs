using System.Drawing;
using OpenQA.Selenium;
using UITest.Core;
using Xunit;

namespace UITest.Appium.Tests;

public class AppiumMouseActionsTests
{
	[Fact]
	public void ClickWithRefresh_SuccessDoesNotReadTagNameOrRefresh()
	{
		var element = new TestElement();
		var tagNameReads = 0;
		var refreshes = 0;

		var response = ExecuteClick(
			element,
			getTagName: _ =>
			{
				tagNameReads++;
				return "tag";
			},
			clickElement: _ => { },
			refreshElement: () =>
			{
				refreshes++;
				return new TestElement();
			});

		Assert.Equal(CommandResponseResult.Success, response.Result);
		Assert.Equal(0, tagNameReads);
		Assert.Equal(0, refreshes);
	}

	[Fact]
	public void ClickWithRefresh_FailureRefreshesOnceAndUsesRefreshedCoordinates()
	{
		var original = new TestElement();
		var refreshed = new TestElement();
		var refreshes = 0;
		TestElement? coordinateElement = null;
		PointF? clickedPoint = null;

		var response = ExecuteClick(
			original,
			clickElement: _ => throw new WebDriverException("click failed"),
			refreshElement: () =>
			{
				refreshes++;
				return refreshed;
			},
			elementToClickablePoint: element =>
			{
				coordinateElement = element;
				return new PointF(10, 20);
			},
			clickCoordinates: (x, y) => clickedPoint = new PointF(x, y));

		Assert.Equal(CommandResponseResult.Success, response.Result);
		Assert.Equal(1, refreshes);
		Assert.Same(refreshed, coordinateElement);
		Assert.Equal(new PointF(10, 20), clickedPoint);
	}

	[Fact]
	public void ClickWithRefresh_NullRefreshReturnsFailedResponse()
	{
		var response = ExecuteClick(
			new TestElement(),
			clickElement: _ => throw new WebDriverException("click failed"),
			refreshElement: () => null);

		Assert.Equal(CommandResponseResult.Failed, response.Result);
	}

	[Fact]
	public void ClickWithRefresh_ThrowingRefreshReturnsFailedResponse()
	{
		var refreshes = 0;

		var response = ExecuteClick(
			new TestElement(),
			clickElement: _ => throw new WebDriverException("click failed"),
			refreshElement: () =>
			{
				refreshes++;
				throw new WebDriverException("refresh failed");
			});

		Assert.Equal(CommandResponseResult.Failed, response.Result);
		Assert.Equal(1, refreshes);
	}

	static CommandResponse ExecuteClick(
		TestElement element,
		Func<TestElement, string>? getTagName = null,
		Action<TestElement>? clickElement = null,
		Func<TestElement?>? refreshElement = null,
		Func<TestElement, PointF>? elementToClickablePoint = null,
		Action<float, float>? clickCoordinates = null)
	{
		return AppiumMouseActions.ClickElement(
			element,
			isMacDriver: true,
			getTagName ?? (_ => "tag"),
			clickElement ?? (_ => { }),
			refreshElement,
			_ => new TestElement(),
			elementToClickablePoint ?? (_ => new PointF(1, 2)),
			clickCoordinates ?? ((_, _) => { }));
	}

	sealed class TestElement
	{
	}
}
