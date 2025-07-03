using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class DispatcherExtensionsPublicAPITests : BaseTestFixture
    {
        [Fact]
        public void ControlsDispatcherExtensions_IsPublic()
        {
            // Verify that the Controls.DispatcherExtensions class is public
            var type = typeof(Microsoft.Maui.Controls.DispatcherExtensions);
            Assert.True(type.IsPublic, "Controls DispatcherExtensions should be a public class");
        }

        [Fact]
        public void CoreDispatcherExtensions_IsPublic()
        {
            // Verify that the Core.Dispatching.DispatcherExtensions class is public
            var type = typeof(Microsoft.Maui.Dispatching.DispatcherExtensions);
            Assert.True(type.IsPublic, "Core Dispatching DispatcherExtensions should be a public class");
        }

        [Fact]
        public void FindDispatcher_IsPublic()
        {
            // Verify that FindDispatcher method is public (from Controls namespace)
            var method = typeof(Microsoft.Maui.Controls.DispatcherExtensions).GetMethod("FindDispatcher");
            Assert.NotNull(method);
            Assert.True(method.IsPublic, "FindDispatcher should be a public method");
            Assert.True(method.IsStatic, "FindDispatcher should be a static method");
        }

        [Fact]
        public void DispatchIfRequired_IsPublic()
        {
            // Verify that DispatchIfRequired method is public (from Controls namespace)
            var method = typeof(Microsoft.Maui.Controls.DispatcherExtensions).GetMethod("DispatchIfRequired", new[] { typeof(IDispatcher), typeof(Action) });
            Assert.NotNull(method);
            Assert.True(method.IsPublic, "DispatchIfRequired should be a public method");
            Assert.True(method.IsStatic, "DispatchIfRequired should be a static method");
        }

        [Fact]
        public void DispatchIfRequiredAsync_Action_IsPublic()
        {
            // Verify that DispatchIfRequiredAsync(Action) method is public
            var method = typeof(Microsoft.Maui.Controls.DispatcherExtensions).GetMethod("DispatchIfRequiredAsync", new[] { typeof(IDispatcher), typeof(Action) });
            Assert.NotNull(method);
            Assert.True(method.IsPublic, "DispatchIfRequiredAsync(Action) should be a public method");
            Assert.True(method.IsStatic, "DispatchIfRequiredAsync(Action) should be a static method");
        }

        [Fact]
        public void DispatchIfRequiredAsync_FuncTask_IsPublic()
        {
            // Verify that DispatchIfRequiredAsync(Func<Task>) method is public
            var method = typeof(Microsoft.Maui.Controls.DispatcherExtensions).GetMethod("DispatchIfRequiredAsync", new[] { typeof(IDispatcher), typeof(Func<Task>) });
            Assert.NotNull(method);
            Assert.True(method.IsPublic, "DispatchIfRequiredAsync(Func<Task>) should be a public method");
            Assert.True(method.IsStatic, "DispatchIfRequiredAsync(Func<Task>) should be a static method");
        }

        [Fact]
        public void DispatchIfRequiredAsync_FuncT_IsPublic()
        {
            // Verify that DispatchIfRequiredAsync<T>(Func<T>) method is public
            var method = typeof(Microsoft.Maui.Controls.DispatcherExtensions).GetMethod("DispatchIfRequiredAsync", 1, new[] { typeof(IDispatcher), typeof(Func<>).MakeGenericType(typeof(int)) });
            Assert.NotNull(method);
            Assert.True(method.IsPublic, "DispatchIfRequiredAsync<T>(Func<T>) should be a public method");
            Assert.True(method.IsStatic, "DispatchIfRequiredAsync<T>(Func<T>) should be a static method");
        }

        [Fact]
        public void DispatchIfRequiredAsync_FuncTaskT_IsPublic()
        {
            // Verify that DispatchIfRequiredAsync<T>(Func<Task<T>>) method is public
            var method = typeof(Microsoft.Maui.Controls.DispatcherExtensions).GetMethod("DispatchIfRequiredAsync", 1, new[] { typeof(IDispatcher), typeof(Func<>).MakeGenericType(typeof(Task<int>)) });
            Assert.NotNull(method);
            Assert.True(method.IsPublic, "DispatchIfRequiredAsync<T>(Func<Task<T>>) should be a public method");
            Assert.True(method.IsStatic, "DispatchIfRequiredAsync<T>(Func<Task<T>>) should be a static method");
        }

        [Fact]
        public void FindDispatcher_WithoutDispatcher_FindsDefaultDispatcher()
        {
            // Test that FindDispatcher works even when no explicit dispatcher is set
            var grid = new Grid();
            
            // The method should find a dispatcher from the current thread or Application
            // rather than throwing an exception due to the EnsureDispatcher fallback logic
            var dispatcher = grid.FindDispatcher();
            Assert.NotNull(dispatcher);
        }
    }
}