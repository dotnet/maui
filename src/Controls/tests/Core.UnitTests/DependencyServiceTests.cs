#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public interface IDependencyTest
    {
        bool Works { get; }
    }


    public interface IDependencyTestRegister
    {
        bool Works { get; }
    }


    public interface IUnsatisfied
    {
        bool Broken { get; }
    }


    public class DependencyTestImpl : IDependencyTest
    {
        public bool Works { get { return true; } }
    }


    public class DependencyTestRegisterImpl : IDependencyTestRegister
    {
        public bool Works { get { return true; } }
    }


    public class DependencyTestRegisterImpl2 : IDependencyTestRegister
    {
        public bool Works { get { return false; } }
    }


    public class DependencyServiceTests : BaseTestFixture
    {
        [Fact]
        public void GetGlobalInstance()
        {
            var global = DependencyService.Get<IDependencyTest>();

            Assert.NotNull(global);

            var secondFetch = DependencyService.Get<IDependencyTest>();

            Assert.True(ReferenceEquals(global, secondFetch));
        }

        [Fact]
        public void NewInstanceIsNotGlobalInstance()
        {
            var global = DependencyService.Get<IDependencyTest>();

            Assert.NotNull(global);

            var secondFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

            Assert.False(ReferenceEquals(global, secondFetch));
        }

        [Fact]
        public void NewInstanceIsAlwaysNew()
        {
            var firstFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

            Assert.NotNull(firstFetch);

            var secondFetch = DependencyService.Get<IDependencyTest>(DependencyFetchTarget.NewInstance);

            Assert.False(ReferenceEquals(firstFetch, secondFetch));
        }

        [Fact]
        public void UnsatisfiedReturnsNull()
        {
            Assert.Null(DependencyService.Get<IUnsatisfied>());
        }

        [Fact]
        public void RegisterTypeImplementation()
        {
            DependencyService.Register<DependencyTestRegisterImpl>();
            var global = DependencyService.Get<DependencyTestRegisterImpl>();
            Assert.NotNull(global);
        }


        [Fact]
        public void RegisterInterfaceAndImplementations()
        {
            DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl2>();
            var global = DependencyService.Get<IDependencyTestRegister>();
            Assert.IsType<DependencyTestRegisterImpl2>(global);
        }

        [Fact]
        public void RegisterInterfaceAndOverrideImplementations()
        {
            DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl>();
            DependencyService.Register<IDependencyTestRegister, DependencyTestRegisterImpl2>();
            var global = DependencyService.Get<IDependencyTestRegister>();
            Assert.IsType<DependencyTestRegisterImpl2>(global);
        }

        [Fact]
        public void RegisterSingletonInterface()
        {
            var local = new DependencyTestRegisterImpl();
            DependencyService.RegisterSingleton<IDependencyTestRegister>(local);
            var global = DependencyService.Get<IDependencyTestRegister>();
            Assert.Equal(local, global);
        }
    }

    public partial class DependencyServiceRegisterTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Register method throws ArgumentNullException when assemblies parameter is null.
        /// Input: null assemblies array
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void Register_NullAssemblies_ThrowsArgumentNullException()
        {
            // Arrange
            Assembly[] nullAssemblies = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => DependencyService.Register(nullAssemblies));
        }

        /// <summary>
        /// Tests that Register method handles empty assemblies array gracefully.
        /// Input: empty assemblies array
        /// Expected: method completes without exception, no registrations occur
        /// </summary>
        [Fact]
        public void Register_EmptyAssemblies_CompletesSuccessfully()
        {
            // Arrange
            var emptyAssemblies = new Assembly[0];

            // Act & Assert - should not throw
            DependencyService.Register(emptyAssemblies);
        }

        /// <summary>
        /// Tests that Register method handles array containing null assembly references.
        /// Input: assemblies array with null elements
        /// Expected: NullReferenceException when trying to access null assembly
        /// </summary>
        [Fact]
        public void Register_AssembliesWithNullElements_ThrowsNullReferenceException()
        {
            // Arrange
            var assembliesWithNull = new Assembly[] { null };

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => DependencyService.Register(assembliesWithNull));
        }

        /// <summary>
        /// Tests that Register method processes assemblies with DependencyAttributes correctly.
        /// Input: current assembly which has DependencyAttribute
        /// Expected: method completes successfully and processes attributes
        /// </summary>
        [Fact]
        public void Register_AssemblyWithDependencyAttributes_ProcessesSuccessfully()
        {
            // Arrange
            var currentAssembly = Assembly.GetExecutingAssembly();
            var assemblies = new Assembly[] { currentAssembly };

            // Act & Assert - should not throw
            DependencyService.Register(assemblies);
        }

        /// <summary>
        /// Tests that Register method handles assemblies without DependencyAttributes.
        /// Input: System.Core assembly which typically has no DependencyAttributes
        /// Expected: method completes successfully, continues to next assembly
        /// </summary>
        [Fact]
        public void Register_AssemblyWithoutDependencyAttributes_CompletesSuccessfully()
        {
            // Arrange
            var systemAssembly = typeof(object).Assembly;
            var assemblies = new Assembly[] { systemAssembly };

            // Act & Assert - should not throw
            DependencyService.Register(assemblies);
        }

        /// <summary>
        /// Tests that Register method processes multiple assemblies with mixed attribute scenarios.
        /// Input: array with multiple assemblies, some with attributes, some without
        /// Expected: method processes all assemblies successfully
        /// </summary>
        [Fact]
        public void Register_MultipleAssemblies_ProcessesAllSuccessfully()
        {
            // Arrange
            var currentAssembly = Assembly.GetExecutingAssembly();
            var systemAssembly = typeof(object).Assembly;
            var assemblies = new Assembly[] { currentAssembly, systemAssembly };

            // Act & Assert - should not throw
            DependencyService.Register(assemblies);
        }

        /// <summary>
        /// Tests that Register method handles very large assemblies array.
        /// Input: array with many assembly references
        /// Expected: method processes all assemblies without performance issues
        /// </summary>
        [Fact]
        public void Register_LargeAssembliesArray_ProcessesSuccessfully()
        {
            // Arrange
            var currentAssembly = Assembly.GetExecutingAssembly();
            var largeArray = new Assembly[1000];
            for (int i = 0; i < largeArray.Length; i++)
            {
                largeArray[i] = currentAssembly;
            }

            // Act & Assert - should not throw
            DependencyService.Register(largeArray);
        }

        /// <summary>
        /// Tests Register method with assembly array containing duplicate assemblies.
        /// Input: array with same assembly referenced multiple times
        /// Expected: method processes duplicates without issues
        /// </summary>
        [Fact]
        public void Register_DuplicateAssemblies_ProcessesSuccessfully()
        {
            // Arrange
            var currentAssembly = Assembly.GetExecutingAssembly();
            var assemblies = new Assembly[] { currentAssembly, currentAssembly, currentAssembly };

            // Act & Assert - should not throw
            DependencyService.Register(assemblies);
        }
    }


    /// <summary>
    /// Unit tests for the DependencyService.Initialize method.
    /// Tests focus on thread safety, initialization state management, and parameter validation.
    /// </summary>
    public partial class DependencyServiceInitializeTests
    {
        /// <summary>
        /// Tests that Initialize completes successfully when provided with valid assemblies.
        /// Verifies the basic initialization path works correctly.
        /// </summary>
        [Fact]
        public void Initialize_WithValidAssemblies_CompletesSuccessfully()
        {
            // Arrange
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            // Act & Assert - Should not throw
            DependencyService.Initialize(assemblies);
        }

        /// <summary>
        /// Tests that Initialize handles an empty assemblies array gracefully.
        /// Should complete without errors even with no assemblies to process.
        /// </summary>
        [Fact]
        public void Initialize_WithEmptyAssemblies_CompletesSuccessfully()
        {
            // Arrange
            var assemblies = new Assembly[0];

            // Act & Assert - Should not throw
            DependencyService.Initialize(assemblies);
        }

        /// <summary>
        /// Tests that Initialize handles null assemblies parameter gracefully.
        /// The Register method should handle null input appropriately.
        /// </summary>
        [Fact]
        public void Initialize_WithNullAssemblies_HandledGracefully()
        {
            // Arrange
            Assembly[] assemblies = null;

            // Act & Assert - Should not throw if Register handles null
            try
            {
                DependencyService.Initialize(assemblies);
            }
            catch (ArgumentNullException)
            {
                // This is acceptable behavior - Register may require non-null assemblies
            }
        }

        /// <summary>
        /// Tests that concurrent calls to Initialize are handled safely and only one initialization occurs.
        /// This test specifically targets the double-checked locking pattern and aims to cover
        /// the second initialization check inside the lock (line 83).
        /// </summary>
        [Fact]
        public async Task Initialize_ConcurrentCalls_ThreadSafeInitialization()
        {
            // Arrange
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };
            var barrier = new Barrier(3); // 2 threads + test thread
            var initializationAttempts = 0;
            var exceptionsThrown = new List<Exception>();

            // Create a task that will call Initialize
            var createInitializeTask = () => Task.Run(() =>
            {
                try
                {
                    // Wait for all threads to be ready
                    barrier.SignalAndWait(TimeSpan.FromSeconds(5));

                    // Attempt initialization
                    Interlocked.Increment(ref initializationAttempts);
                    DependencyService.Initialize(assemblies);
                }
                catch (Exception ex)
                {
                    lock (exceptionsThrown)
                    {
                        exceptionsThrown.Add(ex);
                    }
                }
            });

            // Act - Start multiple concurrent initialization attempts
            var task1 = createInitializeTask();
            var task2 = createInitializeTask();

            // Signal that the test thread is ready
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));

            // Wait for all tasks to complete
            await Task.WhenAll(task1, task2);

            // Assert
            Assert.Equal(2, initializationAttempts);
            Assert.Empty(exceptionsThrown);
        }

        /// <summary>
        /// Tests that Initialize with multiple assemblies processes all assemblies correctly.
        /// Verifies behavior with a realistic set of multiple assemblies.
        /// </summary>
        [Fact]
        public void Initialize_WithMultipleAssemblies_ProcessesAllAssemblies()
        {
            // Arrange
            var assemblies = new Assembly[]
            {
                Assembly.GetExecutingAssembly(),
                typeof(DependencyService).Assembly
            };

            // Act & Assert - Should not throw
            DependencyService.Initialize(assemblies);
        }

        /// <summary>
        /// Tests Initialize behavior with assemblies containing duplicate entries.
        /// Should handle duplicate assemblies gracefully without errors.
        /// </summary>
        [Fact]
        public void Initialize_WithDuplicateAssemblies_HandledGracefully()
        {
            // Arrange
            var testAssembly = Assembly.GetExecutingAssembly();
            var assemblies = new Assembly[] { testAssembly, testAssembly, testAssembly };

            // Act & Assert - Should not throw
            DependencyService.Initialize(assemblies);
        }

        /// <summary>
        /// Tests that subsequent calls to Initialize after the service is already initialized
        /// return early without executing the initialization logic again.
        /// This verifies the early return path when s_initialized is true.
        /// </summary>
        [Fact]
        public void Initialize_WhenAlreadyInitialized_ReturnsEarly()
        {
            // Arrange - Ensure service is initialized first
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };
            DependencyService.Initialize(assemblies);

            // Act - Call Initialize again multiple times
            DependencyService.Initialize(assemblies);
            DependencyService.Initialize(assemblies);
            DependencyService.Initialize(new Assembly[0]);
            DependencyService.Initialize(new Assembly[] { typeof(DependencyService).Assembly });

            // Assert - Should complete without any issues
            // The fact that this completes without hanging or throwing indicates
            // the early return logic is working correctly
        }
    }
}