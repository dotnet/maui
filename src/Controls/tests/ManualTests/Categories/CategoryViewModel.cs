using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Maui.ManualTests.Tests;

namespace Microsoft.Maui.ManualTests.Categories;

public abstract class CategoryViewModel
{
	[RequiresUnreferencedCode()]
	public CategoryViewModel()
	{
		Tests = GetType().Assembly.GetTypes()
			.Where(t => t.IsAssignableTo(typeof(Page)))
			.Select(t => new TestPageInfo(t, t.GetCustomAttribute<TestAttribute>()))
			.Where(t => t.Test is not null)
			.Where(t => t.Test.Category == CategoryName)
			.ToList();
		Tests.Sort();
	}

	public List<TestPageInfo> Tests { get; set; }

	public abstract string CategoryName { get; }
}

[Category(id: "A", title: Category.Layout)]
public class LayoutCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.Layout;
}

[Category(id: "B", title: Category.Scroll)]
public class ScrollCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.Scroll;
}

[Category(id: "C", title: Category.RefreshView)]
public class RefreshViewCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.RefreshView;
}

[Category(id: "D", title: Category.Editor)]
public class EditorCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.Editor;
}

[Category(id: "E", title: Category.Entry)]
public class EntryCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.Entry;
}

[Category(id: "F", title: Category.Animation)]
public class AnimationCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.Animation;
}

[Category(id: "G", title: Category.Commands)]
public class CommandsCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.Commands;
}

[Category(id: "H", title: Category.FlyoutPage)]
public class FlyoutPageCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.FlyoutPage;
}

[Category(id: "I", title: Category.CollectionView)]
public class CollectionViewCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.CollectionView;
}

[Category(id: "J", title: Category.TitleBar)]
public class TitleBarCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.TitleBar;
}

[Category(id: "K", title: Category.Performance)]
public class PerformanceCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.Performance;
}

[Category(id: "L", title: Category.BugFixes)]
public class BugFixesCategoryViewModel : CategoryViewModel
{
	public override string CategoryName => Category.BugFixes;
}
