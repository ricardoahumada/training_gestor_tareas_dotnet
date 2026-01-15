using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Tests.Unit
{
    /// <summary>
    /// Tests unitarios para TaskService.
    /// </summary>
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILabelRepository> _labelRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITaskNotifier> _taskNotifierMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _labelRepositoryMock = new Mock<ILabelRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _taskNotifierMock = new Mock<ITaskNotifier>();

            _taskService = new TaskService(
                _taskRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _userRepositoryMock.Object,
                _labelRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _taskNotifierMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldCreateTask()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Test Task",
                Description = "Test Description",
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var createdByUserId = Guid.NewGuid();

            _projectRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Task>()))
                .ReturnsAsync((Domain.Entities.Task t) => t);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _taskService.CreateAsync(request, createdByUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
            Assert.Equal(TaskStatus.Pending, result.Status);
            _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistentProject_ShouldThrowException()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Test Task",
                ProjectId = Guid.NewGuid()
            };

            _projectRepositoryMock.Setup(x => x.ExistsAsync(request.ProjectId.Value))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _taskService.CreateAsync(request, Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAsync_WithPastDueDate_ShouldThrowException()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Title = "Test Task",
                DueDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _taskService.CreateAsync(request, Guid.NewGuid()));
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingTask_ShouldReturnTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new Domain.Entities.Task
            {
                Id = taskId,
                Title = "Test Task",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                CreatedByUserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _taskService.GetByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
            Assert.Equal("Test Task", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingTask_ShouldReturnNull()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync((Domain.Entities.Task?)null);

            // Act
            var result = await _taskService.GetByIdAsync(taskId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateStatusAsync_WithValidTransition_ShouldUpdateStatus()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new Domain.Entities.Task
            {
                Id = taskId,
                Title = "Test Task",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                CreatedByUserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);
            _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
                .ReturnsAsync((Domain.Entities.Task t) => t);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _taskService.UpdateStatusAsync(taskId, TaskStatus.InProgress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TaskStatus.InProgress, result.Status);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_FromCompleted_ShouldThrowException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new Domain.Entities.Task
            {
                Id = taskId,
                Title = "Test Task",
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Medium,
                CreatedByUserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _taskService.UpdateStatusAsync(taskId, TaskStatus.InProgress));
        }

        [Fact]
        public async Task DeleteAsync_ShouldPerformSoftDelete()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new Domain.Entities.Task
            {
                Id = taskId,
                Title = "Test Task",
                Status = TaskStatus.Pending,
                IsActive = true
            };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);
            _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
                .ReturnsAsync((Domain.Entities.Task t) => t);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _taskService.DeleteAsync(taskId);

            // Assert
            Assert.False(task.IsActive);
            _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
