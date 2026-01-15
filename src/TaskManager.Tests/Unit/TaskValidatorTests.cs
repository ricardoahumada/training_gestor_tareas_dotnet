using Moq;
using FluentValidation;
using Xunit;
using TaskManager.Application.Validators;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Tests.Unit
{
    /// <summary>
    /// Tests unitarios para validadores.
    /// </summary>
    public class TaskValidatorTests
    {
        private readonly CreateTaskValidator _createTaskValidator;
        private readonly UpdateTaskValidator _updateTaskValidator;

        public TaskValidatorTests()
        {
            _createTaskValidator = new CreateTaskValidator();
            _updateTaskValidator = new UpdateTaskValidator();
        }

        [Fact]
        public void CreateTaskValidator_WithValidData_ShouldPass()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Valid Task Title",
                Description = "Valid description",
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = _createTaskValidator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CreateTaskValidator_WithEmptyTitle_ShouldFail()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "",
                Description = "Valid description"
            };

            // Act
            var result = _createTaskValidator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("título"));
        }

        [Fact]
        public void CreateTaskValidator_WithTitleTooLong_ShouldFail()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = new string('a', 201),
                Description = "Valid description"
            };

            // Act
            var result = _createTaskValidator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("200"));
        }

        [Fact]
        public void CreateTaskValidator_WithPastDueDate_ShouldFail()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Valid Task",
                Description = "Valid description",
                DueDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var result = _createTaskValidator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("fecha límite"));
        }

        [Fact]
        public void CreateTaskValidator_WithInvalidPriority_ShouldFail()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Valid Task",
                Description = "Valid description",
                Priority = (TaskPriority)99
            };

            // Act
            var result = _createTaskValidator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("prioridad"));
        }

        [Fact]
        public void UpdateTaskValidator_WithValidData_ShouldPass()
        {
            // Arrange
            var request = new UpdateTaskRequest
            {
                Title = "Valid Updated Title",
                Description = "Valid updated description",
                Priority = TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(5)
            };

            // Act
            var result = _updateTaskValidator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void UpdateTaskValidator_WithEmptyTitle_ShouldFail()
        {
            // Arrange
            var request = new UpdateTaskRequest
            {
                Title = "",
                Description = "Valid description"
            };

            // Act
            var result = _updateTaskValidator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("título"));
        }
    }
}
