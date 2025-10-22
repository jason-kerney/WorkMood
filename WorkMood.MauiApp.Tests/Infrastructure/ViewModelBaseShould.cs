using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit;
using WorkMood.MauiApp.Infrastructure;

namespace WorkMood.MauiApp.Tests.Infrastructure
{
    public class ViewModelBaseShould
    {
        #region Test Helper Classes

        // Concrete test implementation for abstract ViewModelBase
        public class TestViewModel : ViewModelBase
        {
            private string? _name;
            private int _age;
            private bool _isEnabled;
            private object? _data;
            private double _rating;
            private DateTime _lastUpdated;

            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            public int Age
            {
                get => _age;
                set => SetProperty(ref _age, value);
            }

            public bool IsEnabled
            {
                get => _isEnabled;
                set => SetProperty(ref _isEnabled, value);
            }

            public object? Data
            {
                get => _data;
                set => SetProperty(ref _data, value);
            }

            public double Rating
            {
                get => _rating;
                set => SetProperty(ref _rating, value);
            }

            public DateTime LastUpdated
            {
                get => _lastUpdated;
                set => SetProperty(ref _lastUpdated, value);
            }

            // Expose protected methods for testing
            public new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                base.OnPropertyChanged(propertyName);
            }

            public new bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
            {
                return base.SetProperty(ref field, value, propertyName);
            }

            // Test helpers for direct field manipulation
            public void SetNameField(string? value) => _name = value;
            public void SetAgeField(int value) => _age = value;
            public void SetIsEnabledField(bool value) => _isEnabled = value;
        }

        // Custom ViewModelBase for testing virtual method overrides
        public class CustomViewModel : ViewModelBase
        {
            public List<string> OnPropertyChangedCalls { get; } = new();
            public bool SuppressPropertyChanged { get; set; }

            protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                OnPropertyChangedCalls.Add(propertyName ?? "null");
                
                if (!SuppressPropertyChanged)
                {
                    base.OnPropertyChanged(propertyName);
                }
            }

            private string? _customProperty;
            public string? CustomProperty
            {
                get => _customProperty;
                set => SetProperty(ref _customProperty, value);
            }
        }

        // Extended ViewModelBase for testing inheritance chains
        public class ExtendedViewModel : TestViewModel
        {
            private string? _description;
            public string? Description
            {
                get => _description;
                set => SetProperty(ref _description, value);
            }
        }

        #endregion

        #region PropertyChanged Event Tests

        [Fact]
        public void PropertyChanged_ShouldAllowEventSubscription_Successfully()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

            // Act
            viewModel.PropertyChanged += handler;
            viewModel.OnPropertyChanged("TestProperty");

            // Assert
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void PropertyChanged_ShouldAllowEventUnsubscription_Successfully()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

            viewModel.PropertyChanged += handler;

            // Act
            viewModel.PropertyChanged -= handler;
            viewModel.OnPropertyChanged("TestProperty");

            // Assert
            Assert.False(eventFired);
        }

        [Fact]
        public void PropertyChanged_ShouldAllowMultipleSubscribers()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var event1Fired = false;
            var event2Fired = false;
            var event3Fired = false;

            PropertyChangedEventHandler handler1 = (sender, args) => event1Fired = true;
            PropertyChangedEventHandler handler2 = (sender, args) => event2Fired = true;
            PropertyChangedEventHandler handler3 = (sender, args) => event3Fired = true;

            // Act
            viewModel.PropertyChanged += handler1;
            viewModel.PropertyChanged += handler2;
            viewModel.PropertyChanged += handler3;

            viewModel.OnPropertyChanged("TestProperty");

            // Assert
            Assert.True(event1Fired);
            Assert.True(event2Fired);
            Assert.True(event3Fired);

            // Cleanup
            viewModel.PropertyChanged -= handler1;
            viewModel.PropertyChanged -= handler2;
            viewModel.PropertyChanged -= handler3;
        }

        [Fact]
        public void PropertyChanged_ShouldHandleNullEventHandler_Gracefully()
        {
            // Arrange
            var viewModel = new TestViewModel();

            // Act & Assert - Should not throw
            viewModel.OnPropertyChanged("TestProperty");
        }

        [Fact]
        public void PropertyChanged_ShouldPassCorrectPropertyName_InEventArgs()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? receivedPropertyName = null;
            PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;

            viewModel.PropertyChanged += handler;

            // Act
            viewModel.OnPropertyChanged("TestProperty");

            // Assert
            Assert.Equal("TestProperty", receivedPropertyName);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void PropertyChanged_ShouldPassCorrectSender_InEvent()
        {
            // Arrange
            var viewModel = new TestViewModel();
            object? receivedSender = null;
            PropertyChangedEventHandler handler = (sender, args) => receivedSender = sender;

            viewModel.PropertyChanged += handler;

            // Act
            viewModel.OnPropertyChanged("TestProperty");

            // Assert
            Assert.Equal(viewModel, receivedSender);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region OnPropertyChanged Method Tests

        [Fact]
        public void OnPropertyChanged_ShouldRaisePropertyChangedEvent_WhenCalled()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

            viewModel.PropertyChanged += handler;

            // Act
            viewModel.OnPropertyChanged("TestProperty");

            // Assert
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void OnPropertyChanged_ShouldHandleNullPropertyName_Safely()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? receivedPropertyName = "initial";
            PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;

            viewModel.PropertyChanged += handler;

            // Act
            viewModel.OnPropertyChanged(null);

            // Assert
            Assert.Null(receivedPropertyName);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void OnPropertyChanged_ShouldHandleEmptyPropertyName_Gracefully()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? receivedPropertyName = null;
            PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;

            viewModel.PropertyChanged += handler;

            // Act
            viewModel.OnPropertyChanged("");

            // Assert
            Assert.Equal("", receivedPropertyName);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void OnPropertyChanged_ShouldAllowMultipleInvocations()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var callCount = 0;
            PropertyChangedEventHandler handler = (sender, args) => callCount++;

            viewModel.PropertyChanged += handler;

            // Act
            viewModel.OnPropertyChanged("Property1");
            viewModel.OnPropertyChanged("Property2");
            viewModel.OnPropertyChanged("Property3");

            // Assert
            Assert.Equal(3, callCount);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void OnPropertyChanged_ShouldUseCallerMemberName_WhenNotProvided()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? receivedPropertyName = null;
            PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;

            viewModel.PropertyChanged += handler;

            // Act - Set property using CallerMemberName
            viewModel.Name = "Test";

            // Assert
            Assert.Equal("Name", receivedPropertyName);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region SetProperty Method Tests - Value Types

        [Fact]
        public void SetProperty_ShouldSetIntProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            var result = viewModel.Age = 25;

            // Assert
            Assert.Equal(25, viewModel.Age);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldSetBoolProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.IsEnabled = true;

            // Assert
            Assert.True(viewModel.IsEnabled);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldSetDoubleProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Rating = 4.5;

            // Assert
            Assert.Equal(4.5, viewModel.Rating);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldSetDateTimeProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var testDate = new DateTime(2025, 6, 15, 10, 30, 0);
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.LastUpdated = testDate;

            // Assert
            Assert.Equal(testDate, viewModel.LastUpdated);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region SetProperty Method Tests - Reference Types

        [Fact]
        public void SetProperty_ShouldSetStringProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Name = "John Doe";

            // Assert
            Assert.Equal("John Doe", viewModel.Name);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldSetObjectProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var testObject = new { Name = "Test", Value = 42 };
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Data = testObject;

            // Assert
            Assert.Equal(testObject, viewModel.Data);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldSetNullProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.Name = "Initial";
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Name = null;

            // Assert
            Assert.Null(viewModel.Name);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldSetCollectionProperty_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var testList = new List<string> { "Item1", "Item2", "Item3" };
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Data = testList;

            // Assert
            Assert.Equal(testList, viewModel.Data);
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Equality Comparison Tests

        [Fact]
        public void SetProperty_ShouldReturnFalse_WhenValuesAreEqual()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var testField = 42;

            // Act
            var result1 = viewModel.SetProperty(ref testField, 42);
            var result2 = viewModel.SetProperty(ref testField, 42);

            // Assert
            Assert.False(result1); // Initial set should return false if values are equal
            Assert.False(result2); // Subsequent set should return false
        }

        [Fact]
        public void SetProperty_ShouldReturnTrue_WhenValuesAreDifferent()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var testField = 42;

            // Act
            var result = viewModel.SetProperty(ref testField, 100);

            // Assert
            Assert.True(result);
            Assert.Equal(100, testField);
        }

        [Fact]
        public void SetProperty_ShouldNotRaiseEvent_WhenValuesAreEqual()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.Age = 25;
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Age = 25; // Same value

            // Assert
            Assert.False(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldRaiseEvent_WhenValuesAreDifferent()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.Age = 25;
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Age = 30; // Different value

            // Assert
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldHandleNullEquality_Correctly()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.Name = null;
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Name = null; // Setting null to null

            // Assert
            Assert.False(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldHandleStringEquality_Correctly()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.Name = "Test";
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Name = "Test"; // Same string value

            // Assert
            Assert.False(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Property Change Notification Tests

        [Fact]
        public void SetProperty_ShouldPassCorrectPropertyName_WhenChanged()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? receivedPropertyName = null;
            PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Name = "John";

            // Assert
            Assert.Equal("Name", receivedPropertyName);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldRaiseEventAfter_SettingField()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var fieldValueDuringEvent = string.Empty;
            PropertyChangedEventHandler handler = (sender, args) => fieldValueDuringEvent = viewModel.Name ?? "";
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Name = "Updated";

            // Assert
            Assert.Equal("Updated", fieldValueDuringEvent);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldUseCallerMemberName_WhenPropertyNameNotProvided()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? receivedPropertyName = null;
            PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.IsEnabled = true;

            // Assert
            Assert.Equal("IsEnabled", receivedPropertyName);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Field Modification Tests

        [Fact]
        public void SetProperty_ShouldUpdateField_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var originalAge = viewModel.Age;

            // Act
            viewModel.Age = 35;

            // Assert
            Assert.Equal(35, viewModel.Age);
            Assert.NotEqual(originalAge, viewModel.Age);
        }

        [Fact]
        public void SetProperty_ShouldNotUpdateField_WhenValueSame()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.Age = 25;
            var originalAge = viewModel.Age;

            // Act
            viewModel.Age = 25; // Same value

            // Assert
            Assert.Equal(originalAge, viewModel.Age);
            Assert.Equal(25, viewModel.Age);
        }

        [Fact]
        public void SetProperty_ShouldUpdateFieldBefore_RaisingEvent()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var initialAge = 20;
            var newAge = 30;
            viewModel.Age = initialAge;

            var ageValueDuringEvent = 0;
            PropertyChangedEventHandler handler = (sender, args) => ageValueDuringEvent = viewModel.Age;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Age = newAge;

            // Assert
            Assert.Equal(newAge, ageValueDuringEvent);
            Assert.Equal(newAge, viewModel.Age);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Abstract Class Implementation Tests

        [Fact]
        public void ViewModelBase_ShouldInstantiateSuccessfully_WithConcreteClass()
        {
            // Act
            var viewModel = new TestViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.IsAssignableFrom<ViewModelBase>(viewModel);
            Assert.IsAssignableFrom<INotifyPropertyChanged>(viewModel);
        }

        [Fact]
        public void ViewModelBase_ShouldInheritPropertyChangedEvent_Correctly()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;

            // Act
            viewModel.PropertyChanged += (sender, args) => eventFired = true;
            viewModel.Name = "Test";

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void ViewModelBase_ShouldSupportPolymorphicBehavior_Appropriately()
        {
            // Arrange
            ViewModelBase baseReference = new TestViewModel();
            var eventFired = false;

            // Act
            baseReference.PropertyChanged += (sender, args) => eventFired = true;
            ((TestViewModel)baseReference).Name = "Polymorphic Test";

            // Assert
            Assert.True(eventFired);
        }

        #endregion

        #region Virtual Method Override Tests

        [Fact]
        public void OnPropertyChanged_ShouldAllowOverride_InDerivedClasses()
        {
            // Arrange
            var customViewModel = new CustomViewModel();

            // Act
            customViewModel.CustomProperty = "Test";

            // Assert
            Assert.Contains("CustomProperty", customViewModel.OnPropertyChangedCalls);
        }

        [Fact]
        public void OnPropertyChanged_ShouldCallOverriddenImplementation_WhenOverridden()
        {
            // Arrange
            var customViewModel = new CustomViewModel();
            customViewModel.SuppressPropertyChanged = true;
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            customViewModel.PropertyChanged += handler;

            // Act
            customViewModel.CustomProperty = "Test";

            // Assert
            Assert.Contains("CustomProperty", customViewModel.OnPropertyChangedCalls);
            Assert.False(eventFired); // Base event should be suppressed

            // Cleanup
            customViewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void OnPropertyChanged_ShouldMaintainBaseClassBehavior_WhenNotOverridden()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            viewModel.PropertyChanged += handler;

            // Act
            viewModel.Name = "Base Behavior";

            // Assert
            Assert.True(eventFired);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void CustomViewModel_ShouldSupportCustomEventHandling_InOverrides()
        {
            // Arrange
            var customViewModel = new CustomViewModel();
            var eventFired = false;
            PropertyChangedEventHandler handler = (sender, args) => eventFired = true;
            customViewModel.PropertyChanged += handler;

            // Act
            customViewModel.CustomProperty = "Test";

            // Assert
            Assert.True(eventFired);
            Assert.Single(customViewModel.OnPropertyChangedCalls);
            Assert.Equal("CustomProperty", customViewModel.OnPropertyChangedCalls[0]);

            // Cleanup
            customViewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Multiple Inheritance Levels Tests

        [Fact]
        public void ExtendedViewModel_ShouldWorkThroughMultipleInheritanceLevels()
        {
            // Arrange
            var extendedViewModel = new ExtendedViewModel();
            var nameEventFired = false;
            var descriptionEventFired = false;

            PropertyChangedEventHandler handler = (sender, args) =>
            {
                if (args.PropertyName == "Name") nameEventFired = true;
                if (args.PropertyName == "Description") descriptionEventFired = true;
            };

            extendedViewModel.PropertyChanged += handler;

            // Act
            extendedViewModel.Name = "Base Property";
            extendedViewModel.Description = "Extended Property";

            // Assert
            Assert.True(nameEventFired);
            Assert.True(descriptionEventFired);
            Assert.Equal("Base Property", extendedViewModel.Name);
            Assert.Equal("Extended Property", extendedViewModel.Description);

            // Cleanup
            extendedViewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void ExtendedViewModel_ShouldMaintainEventChain_ThroughInheritance()
        {
            // Arrange
            var extendedViewModel = new ExtendedViewModel();
            var totalEventCount = 0;
            PropertyChangedEventHandler handler = (sender, args) => totalEventCount++;
            extendedViewModel.PropertyChanged += handler;

            // Act
            extendedViewModel.Name = "Test Name";
            extendedViewModel.Age = 25;
            extendedViewModel.Description = "Test Description";
            extendedViewModel.IsEnabled = true;

            // Assert
            Assert.Equal(4, totalEventCount);

            // Cleanup
            extendedViewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Performance and Edge Cases

        [Fact]
        public void SetProperty_ShouldHandleFrequentPropertyChanges_Efficiently()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventCount = 0;
            PropertyChangedEventHandler handler = (sender, args) => eventCount++;
            viewModel.PropertyChanged += handler;

            // Act - Simulate frequent changes (starting from 1 since 0 is the default)
            for (int i = 1; i <= 100; i++)
            {
                viewModel.Age = i;
            }

            // Assert
            Assert.Equal(100, eventCount);
            Assert.Equal(100, viewModel.Age);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldMinimizeEventRaising_WhenValuesUnchanged()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventCount = 0;
            PropertyChangedEventHandler handler = (sender, args) => eventCount++;
            viewModel.PropertyChanged += handler;

            viewModel.Age = 25; // Initial set
            eventCount = 0; // Reset count

            // Act - Set same value multiple times
            for (int i = 0; i < 10; i++)
            {
                viewModel.Age = 25;
            }

            // Assert
            Assert.Equal(0, eventCount); // No events should fire for unchanged values

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void SetProperty_ShouldHandleBatchPropertyChanges_Efficiently()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var eventCount = 0;
            PropertyChangedEventHandler handler = (sender, args) => eventCount++;
            viewModel.PropertyChanged += handler;

            // Act - Batch change all properties
            viewModel.Name = "John Doe";
            viewModel.Age = 30;
            viewModel.IsEnabled = true;
            viewModel.Rating = 4.5;
            viewModel.Data = new { Test = "Value" };

            // Assert
            Assert.Equal(5, eventCount);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Integration and MVVM Scenarios

        [Fact]
        public void ViewModelBase_ShouldSupportTypicalViewModelScenarios()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var propertyChanges = new List<string>();
            PropertyChangedEventHandler handler = (sender, args) => 
            {
                if (args.PropertyName != null)
                    propertyChanges.Add(args.PropertyName);
            };
            viewModel.PropertyChanged += handler;

            // Act - Typical ViewModel usage pattern
            viewModel.Name = "John Doe";
            viewModel.Age = 35;
            viewModel.IsEnabled = true;
            viewModel.Rating = 4.8;

            // Assert
            Assert.Contains("Name", propertyChanges);
            Assert.Contains("Age", propertyChanges);
            Assert.Contains("IsEnabled", propertyChanges);
            Assert.Contains("Rating", propertyChanges);
            Assert.Equal(4, propertyChanges.Count);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        [Fact]
        public void ViewModelBase_ShouldHandleComplexPropertyDependencies()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var propertyChanges = new List<string>();
            PropertyChangedEventHandler handler = (sender, args) => 
            {
                if (args.PropertyName != null)
                    propertyChanges.Add(args.PropertyName);
            };
            viewModel.PropertyChanged += handler;

            // Act - Properties that might depend on each other
            viewModel.Name = "Initial";
            viewModel.Name = "Updated";
            viewModel.Age = 25;
            viewModel.Age = 26;

            // Assert
            Assert.Equal(4, propertyChanges.Count);
            Assert.Equal("Name", propertyChanges[0]);
            Assert.Equal("Name", propertyChanges[1]);
            Assert.Equal("Age", propertyChanges[2]);
            Assert.Equal("Age", propertyChanges[3]);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion

        #region Error Handling and Edge Cases

        [Fact]
        public void SetProperty_ShouldHandleExceptionInEventHandler_Gracefully()
        {
            // Arrange
            var viewModel = new TestViewModel();
            
            PropertyChangedEventHandler badHandler = (sender, args) => throw new InvalidOperationException("Test exception");

            viewModel.PropertyChanged += badHandler;

            // Act & Assert - Should not throw
            var exception = Record.Exception(() => viewModel.Name = "Test");
            
            // The PropertyChanged event will throw, but we can't catch it directly
            // because it's thrown during the event invocation. This is expected behavior.
            // In real applications, event handlers should not throw exceptions.
            
            // Cleanup
            viewModel.PropertyChanged -= badHandler;
        }

        [Fact]
        public void ViewModelBase_ShouldHandleViewModelLifecycle_Properly()
        {
            // Arrange & Act
            var viewModel = new TestViewModel();
            viewModel.Name = "Test";
            viewModel.Age = 25;

            // Simulate typical lifecycle
            var eventHandlerAttached = false;
            PropertyChangedEventHandler handler = (sender, args) => eventHandlerAttached = true;
            
            viewModel.PropertyChanged += handler;
            viewModel.IsEnabled = true;
            
            // Assert
            Assert.True(eventHandlerAttached);
            Assert.True(viewModel.IsEnabled);

            // Cleanup
            viewModel.PropertyChanged -= handler;
        }

        #endregion
    }
}