using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.UnitTests;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.CommandMapping)]
	public class CommandMapperTests
	{
		[Fact]
		public void ChainingMappersOverrideBase()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new CommandMapper<IView>
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper1Called = true
			};

			var mapper2 = new CommandMapper<IButton>(mapper1)
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper2Called = true
			};

			mapper2.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Fact]
		public void ChainingMappersStillAllowReplacingChainedRoot()
		{
			bool wasMapper1Called = false;
			bool wasMapper3Called = false;

			var mapper1 = new CommandMapper<IView>
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper1Called = true
			};

			var mapper2 = new CommandMapper<ITextButton>(mapper1);

			mapper1[nameof(IView.Focus)] = (r, v, a) => wasMapper3Called = true;

			mapper2.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.False(wasMapper1Called, "Mapper 1 was called");
			Assert.True(wasMapper3Called, "Mapper 3 was called");
		}

		[Fact]
		public void GenericMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper1Called = true
			};

			var mapper2 = new CommandMapper<IButton, ButtonHandler>(mapper1)
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper2Called = true
			};

			mapper2.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Fact]
		public void AddCommandWithArgs()
		{
			var mapper = new CommandMapper<IView, IViewHandler>();

			// Add a command mapping with a callback function that accepts an argument
			const string key = "test_key";
			var action = Substitute.For<Action<IViewHandler, IView, object>>();
			mapper.Add(key, action);

			// Invoke the command
			const int arg = 42;
			mapper[key].Invoke(null, null, arg);

			// Verify that the callback function was invoked
			action.Received().Invoke(Arg.Any<IViewHandler>(), Arg.Any<IView>(), arg);
		}

		[Fact]
		public void AddCommandWithNoArgs()
		{
			var mapper = new CommandMapper<IView, IViewHandler>();

			// Add a command mapping with a callback function that does NOT take an argument
			const string key = "test_key";
			var action = Substitute.For<Action<IViewHandler, IView>>();
			mapper.Add(key, action);

			// Invoke the command
			mapper.GetCommand(key)?.Invoke(null, null, null);

			// Verify that the callback function was invoked
			action.Received().Invoke(Arg.Any<IViewHandler>(), Arg.Any<IView>());
		}
	}
}
