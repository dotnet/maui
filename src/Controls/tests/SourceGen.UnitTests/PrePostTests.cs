using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Xml;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.SourceGen;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;
/// <summary>
/// Unit tests for the PrePost class.
/// </summary>
[TestFixture]
public partial class PrePostTests
{
	/// <summary>
	/// Tests that NewDisableWarning can be used with using statement for automatic disposal.
	/// </summary>
	[Test]
	public void NewDisableWarning_UsedInUsingStatement_DisposesAutomatically()
	{
		// Arrange
		var mockTextWriter = new Mock<TextWriter>();
		var codeWriter = new IndentedTextWriter(mockTextWriter.Object);
		const string warning = "CS1234";
		// Act
		using (var prePost = PrePost.NewDisableWarning(codeWriter, warning))
		{
			// Assert - Disable should be called immediately
			mockTextWriter.Verify(tw => tw.WriteLine($"#pragma warning disable {warning}"), Times.Once);
			mockTextWriter.Verify(tw => tw.WriteLine($"#pragma warning restore {warning}"), Times.Never);
		}

		// Assert - Restore should be called after using block
		mockTextWriter.Verify(tw => tw.WriteLine($"#pragma warning restore {warning}"), Times.Once);
	}

	/// <summary>
	/// Tests that NoBlock returns a non-null PrePost instance.
	/// Verifies the basic functionality of creating a no-operation PrePost.
	/// Expected result: A valid PrePost instance is returned.
	/// </summary>
	[Test]
	public void NoBlock_WhenCalled_ReturnsNonNullPrePostInstance()
	{
		// Act
		var result = PrePost.NoBlock();
		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.InstanceOf<PrePost>());
	}

	/// <summary>
	/// Tests that NoBlock returns a PrePost instance that implements IDisposable.
	/// Verifies that the returned object can be used in using statements.
	/// Expected result: The instance implements IDisposable interface.
	/// </summary>
	[Test]
	public void NoBlock_WhenCalled_ReturnsDisposableInstance()
	{
		// Act
		var result = PrePost.NoBlock();
		// Assert
		Assert.That(result, Is.InstanceOf<IDisposable>());
	}

	/// <summary>
	/// Tests that NoBlock returns a PrePost instance that can be disposed without exceptions.
	/// Verifies that the empty post action executes safely.
	/// Expected result: No exceptions are thrown during disposal.
	/// </summary>
	[Test]
	public void NoBlock_WhenDisposed_DoesNotThrowException()
	{
		// Arrange
		var prePost = PrePost.NoBlock();
		// Act & Assert
		Assert.DoesNotThrow(() => ((IDisposable)prePost).Dispose());
	}

	/// <summary>
	/// Tests that NoBlock can be used safely in a using statement.
	/// Verifies the complete lifecycle of the no-operation PrePost.
	/// Expected result: The using block completes without exceptions.
	/// </summary>
	[Test]
	public void NoBlock_InUsingStatement_CompletesWithoutException()
	{
		// Act & Assert
		Assert.DoesNotThrow(() =>
		{
			using var prePost = PrePost.NoBlock();
			// The using statement will automatically dispose the PrePost
		});
	}

	/// <summary>
	/// Tests that multiple calls to NoBlock return different instances.
	/// Verifies that each call creates a new PrePost object.
	/// Expected result: Different object references are returned.
	/// </summary>
	[Test]
	public void NoBlock_MultipleCalls_ReturnsDifferentInstances()
	{
		// Act
		var first = PrePost.NoBlock();
		var second = PrePost.NoBlock();
		// Assert
		Assert.That(first, Is.Not.SameAs(second));
	}

	/// <summary>
	/// Tests that NoBlock can be called multiple times and each instance disposed safely.
	/// Verifies that the empty actions don't interfere with each other.
	/// Expected result: All instances can be disposed without exceptions.
	/// </summary>
	[Test]
	public void NoBlock_MultipleInstancesDisposed_DoesNotThrowException()
	{
		// Arrange
		var instances = new PrePost[5];
		for (int i = 0; i < instances.Length; i++)
		{
			instances[i] = PrePost.NoBlock();
		}

		// Act & Assert
		Assert.DoesNotThrow(() =>
		{
			foreach (var instance in instances)
			{
				((IDisposable)instance).Dispose();
			}
		});
	}
}