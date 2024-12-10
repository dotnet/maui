using System;
using System.Linq;

namespace Microsoft.Maui.Handlers;

internal partial class HeadlessLayoutHandler : IHeadlessLayoutHandler
{
	public void Add(IView view)
	{
		throw new NotImplementedException();
	}

	public void Remove(IView view)
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		throw new NotImplementedException();
	}

	public void Insert(int index, IView view)
	{
		throw new NotImplementedException();
	}

	public void Update(int index, IView view)
	{
		throw new NotImplementedException();
	}

	public void CreateSubviews(ref int targetIndex)
	{
		throw new NotImplementedException();
	}

	public void MoveSubviews(int targetIndex)
	{
		throw new NotImplementedException();
	}
}