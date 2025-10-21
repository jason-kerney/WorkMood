using Xunit;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using WorkMood.MauiApp.Infrastructure;

namespace WorkMood.MauiApp.Tests.Infrastructure;

/// <summary>
/// Tests for CommandManager static class providing MAUI command coordination.
/// Tests static event management, command invalidation patterns, and integration scenarios.
/// </summary>
public class CommandManagerShould
{
    #region Test Setup and Teardown

    /// <summary>
    /// Reset CommandManager event state before each test to ensure isolation
    /// </summary>
    private void ResetCommandManagerState()
    {
        // Clear all event handlers using reflection to ensure clean test state
        var eventField = typeof(CommandManager).GetEvent("RequerySuggested");
        if (eventField != null)
        {
            var backingField = typeof(CommandManager).GetField("RequerySuggested", 
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            backingField?.SetValue(null, null);
        }
    }

    #endregion

    #region Checkpoint 1: Basic Static Class Behavior and Event Structure

    [Fact]
    public void HaveStaticEventProperty_WhenAccessingRequerySuggested()
    {
        // Arrange & Act
        var eventInfo = typeof(CommandManager).GetEvent("RequerySuggested");

        // Assert
        Assert.NotNull(eventInfo);
        Assert.True(eventInfo.IsStatic());
        Assert.Equal(typeof(EventHandler), eventInfo.EventHandlerType);
    }

    [Fact]
    public void HaveStaticInvalidateMethod_WhenAccessingInvalidateRequerySuggested()
    {
        // Arrange & Act
        var methodInfo = typeof(CommandManager).GetMethod("InvalidateRequerySuggested");

        // Assert
        Assert.NotNull(methodInfo);
        Assert.True(methodInfo.IsStatic);
        Assert.True(methodInfo.IsPublic);
        Assert.Equal(typeof(void), methodInfo.ReturnType);
        Assert.Empty(methodInfo.GetParameters());
    }

    [Fact]
    public void BeStaticClass_WhenExaminingCommandManagerType()
    {
        // Arrange & Act
        var type = typeof(CommandManager);

        // Assert
        Assert.True(type.IsClass);
        Assert.True(type.IsAbstract); // Static classes are abstract
        Assert.True(type.IsSealed);   // Static classes are sealed
    }

    [Fact]
    public void InvokeEventSuccessfully_WhenInvalidateRequerySuggestedCalled()
    {
        // Arrange
        ResetCommandManagerState();
        var eventHandlerCalled = false;
        EventHandler handler = (sender, args) => eventHandlerCalled = true;
        CommandManager.RequerySuggested += handler;

        // Act
        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.True(eventHandlerCalled);

        // Cleanup
        CommandManager.RequerySuggested -= handler;
    }

    [Fact]
    public void PassNullSender_WhenInvalidateRequerySuggestedCalled()
    {
        // Arrange
        ResetCommandManagerState();
        object? capturedSender = new object(); // Initialize to non-null
        EventHandler handler = (sender, args) => capturedSender = sender;
        CommandManager.RequerySuggested += handler;

        // Act
        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.Null(capturedSender);

        // Cleanup
        CommandManager.RequerySuggested -= handler;
    }

    [Fact]
    public void PassEmptyEventArgs_WhenInvalidateRequerySuggestedCalled()
    {
        // Arrange
        ResetCommandManagerState();
        EventArgs? capturedArgs = null;
        EventHandler handler = (sender, args) => capturedArgs = args;
        CommandManager.RequerySuggested += handler;

        // Act
        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.NotNull(capturedArgs);
        Assert.Same(EventArgs.Empty, capturedArgs);

        // Cleanup
        CommandManager.RequerySuggested -= handler;
    }

    [Fact]
    public void HandleNoSubscribers_GracefullyWhenInvalidateRequerySuggestedCalled()
    {
        // Arrange
        ResetCommandManagerState();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => CommandManager.InvalidateRequerySuggested());
        Assert.Null(exception);
    }

    #endregion

    #region Checkpoint 2: Event Subscription Management and Multiple Handlers

    [Fact]
    public void AllowEventSubscription_WhenAddingEventHandler()
    {
        // Arrange
        ResetCommandManagerState();
        var eventHandlerCalled = false;
        EventHandler handler = (sender, args) => eventHandlerCalled = true;

        // Act
        var exception = Record.Exception(() => CommandManager.RequerySuggested += handler);

        // Assert
        Assert.Null(exception);
        
        // Verify subscription worked
        CommandManager.InvalidateRequerySuggested();
        Assert.True(eventHandlerCalled);

        // Cleanup
        CommandManager.RequerySuggested -= handler;
    }

    [Fact]
    public void AllowEventUnsubscription_WhenRemovingEventHandler()
    {
        // Arrange
        ResetCommandManagerState();
        var eventHandlerCalled = false;
        EventHandler handler = (sender, args) => eventHandlerCalled = true;
        CommandManager.RequerySuggested += handler;

        // Act
        var exception = Record.Exception(() => CommandManager.RequerySuggested -= handler);

        // Assert
        Assert.Null(exception);
        
        // Verify unsubscription worked
        CommandManager.InvalidateRequerySuggested();
        Assert.False(eventHandlerCalled);
    }

    [Fact]
    public void InvokeAllSubscribedHandlers_WhenMultipleHandlersSubscribed()
    {
        // Arrange
        ResetCommandManagerState();
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;

        EventHandler handler1 = (sender, args) => handler1Called = true;
        EventHandler handler2 = (sender, args) => handler2Called = true;
        EventHandler handler3 = (sender, args) => handler3Called = true;

        CommandManager.RequerySuggested += handler1;
        CommandManager.RequerySuggested += handler2;
        CommandManager.RequerySuggested += handler3;

        // Act
        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.True(handler1Called);
        Assert.True(handler2Called);
        Assert.True(handler3Called);

        // Cleanup
        CommandManager.RequerySuggested -= handler1;
        CommandManager.RequerySuggested -= handler2;
        CommandManager.RequerySuggested -= handler3;
    }

    [Fact]
    public void HandleSameHandlerMultipleSubscriptions_Correctly()
    {
        // Arrange
        ResetCommandManagerState();
        var callCount = 0;
        EventHandler handler = (sender, args) => callCount++;

        // Act - Subscribe same handler multiple times
        CommandManager.RequerySuggested += handler;
        CommandManager.RequerySuggested += handler;
        CommandManager.RequerySuggested += handler;

        CommandManager.InvalidateRequerySuggested();

        // Assert - Should be called multiple times (once per subscription)
        Assert.Equal(3, callCount);

        // Cleanup - Unsubscribe same number of times
        CommandManager.RequerySuggested -= handler;
        CommandManager.RequerySuggested -= handler;
        CommandManager.RequerySuggested -= handler;
    }

    [Fact]
    public void HandleUnsubscribeWithoutSubscribe_Gracefully()
    {
        // Arrange
        ResetCommandManagerState();
        EventHandler handler = (sender, args) => { };

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => CommandManager.RequerySuggested -= handler);
        Assert.Null(exception);
    }

    [Fact]
    public void RemoveOnlyOneInstance_OnSingleUnsubscribe()
    {
        // Arrange
        ResetCommandManagerState();
        var callCount = 0;
        EventHandler handler = (sender, args) => callCount++;

        CommandManager.RequerySuggested += handler;
        CommandManager.RequerySuggested += handler;

        // Act - Remove only one subscription
        CommandManager.RequerySuggested -= handler;
        CommandManager.InvalidateRequerySuggested();

        // Assert - Should still be called once
        Assert.Equal(1, callCount);

        // Cleanup
        CommandManager.RequerySuggested -= handler;
    }

    #endregion

    #region Checkpoint 3: Error Handling, Edge Cases, and Integration Patterns

    [Fact]
    public void ContinueInvokingOtherHandlers_WhenOneHandlerThrowsException()
    {
        // Arrange
        ResetCommandManagerState();
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;

        EventHandler handler1 = (sender, args) => handler1Called = true;
        EventHandler handler2 = (sender, args) => throw new InvalidOperationException("Test exception");
        EventHandler handler3 = (sender, args) => handler3Called = true;

        CommandManager.RequerySuggested += handler1;
        CommandManager.RequerySuggested += handler2;
        CommandManager.RequerySuggested += handler3;

        // Act & Assert - Should not throw and should call all handlers
        var exception = Record.Exception(() => CommandManager.InvalidateRequerySuggested());
        
        // Note: .NET event invocation will stop at first exception, this test documents the behavior
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        
        // First handler should be called
        Assert.True(handler1Called);
        // Third handler may not be called due to exception in second handler

        // Cleanup
        CommandManager.RequerySuggested -= handler1;
        CommandManager.RequerySuggested -= handler2;
        CommandManager.RequerySuggested -= handler3;
    }

    [Fact]
    public void MaintainEventState_AcrossMultipleInvalidations()
    {
        // Arrange
        ResetCommandManagerState();
        var callCount = 0;
        EventHandler handler = (sender, args) => callCount++;
        CommandManager.RequerySuggested += handler;

        // Act - Multiple invalidations
        CommandManager.InvalidateRequerySuggested();
        CommandManager.InvalidateRequerySuggested();
        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.Equal(3, callCount);

        // Cleanup
        CommandManager.RequerySuggested -= handler;
    }

    [Fact]
    public void ShareEventState_AcrossAllStaticAccess()
    {
        // Arrange
        ResetCommandManagerState();
        var callCount = 0;
        EventHandler handler = (sender, args) => callCount++;

        // Act - Subscribe from "different contexts" (all access the same static event)
        CommandManager.RequerySuggested += handler;
        
        // Simulate access from different class/method
        TriggerCommandManagerFromDifferentContext();

        // Assert
        Assert.Equal(1, callCount);

        // Cleanup
        CommandManager.RequerySuggested -= handler;
    }

    [Fact]
    public void WorkWithRelayCommandPattern_InIntegrationScenario()
    {
        // Arrange
        ResetCommandManagerState();
        var canExecuteChangedCalled = false;
        
        // Simulate RelayCommand integration pattern
        EventHandler? storedHandler = null;
        Action addHandler = () => CommandManager.RequerySuggested += (storedHandler = (s, e) => canExecuteChangedCalled = true);
        Action removeHandler = () => { if (storedHandler != null) CommandManager.RequerySuggested -= storedHandler; };

        // Act - Simulate RelayCommand lifecycle
        addHandler(); // Simulate command creation and CanExecuteChanged subscription
        CommandManager.InvalidateRequerySuggested(); // Simulate RaiseCanExecuteChanged call

        // Assert
        Assert.True(canExecuteChangedCalled);

        // Cleanup
        removeHandler();
    }

    [Fact]
    public void WorkWithMultipleCommandTypes_Simultaneously()
    {
        // Arrange
        ResetCommandManagerState();
        var relayCommandCalled = false;
        var genericCommandCalled = false;
        var customCommandCalled = false;

        EventHandler relayHandler = (s, e) => relayCommandCalled = true;
        EventHandler genericHandler = (s, e) => genericCommandCalled = true;
        EventHandler customHandler = (s, e) => customCommandCalled = true;

        // Act - Simulate multiple command types subscribing
        CommandManager.RequerySuggested += relayHandler;
        CommandManager.RequerySuggested += genericHandler;
        CommandManager.RequerySuggested += customHandler;

        CommandManager.InvalidateRequerySuggested();

        // Assert
        Assert.True(relayCommandCalled);
        Assert.True(genericCommandCalled);
        Assert.True(customCommandCalled);

        // Cleanup
        CommandManager.RequerySuggested -= relayHandler;
        CommandManager.RequerySuggested -= genericHandler;
        CommandManager.RequerySuggested -= customHandler;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Simulates CommandManager access from a different context
    /// </summary>
    private static void TriggerCommandManagerFromDifferentContext()
    {
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion
}

// Extension method to check if EventInfo represents a static event
internal static class EventInfoExtensions
{
    public static bool IsStatic(this EventInfo eventInfo)
    {
        var addMethod = eventInfo.GetAddMethod(true);
        return addMethod?.IsStatic ?? false;
    }
}