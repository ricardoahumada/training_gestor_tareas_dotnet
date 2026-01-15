using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskManager.Api;
using TaskManager.Infrastructure.Data;
using TaskManager.Tests.Fixtures;

namespace TaskManager.Tests.Integration
{
    /// <summary>
    /// Tests de integración para TasksController.
    /// </summary>
    public class TasksControllerIntegrationTests : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly TestDatabaseFixture _databaseFixture;
        private readonly HttpClient _client;

        public TasksControllerIntegrationTests(TestDatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                        
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestTaskManagerDb");
                        });

                        // Asegurarse de que IMemoryCache esté registrado
                        services.AddMemoryCache();
                    });
                });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Get_AllTasks_ShouldReturnEmptyList_WhenNoTasksExist()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/tasks");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("items", content);
        }

        [Fact]
        public async Task Get_TaskById_ShouldReturn404_WhenTaskNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/tasks/{nonExistentId}");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateTask_ShouldReturn201_WhenDataIsValid()
        {
            // Arrange
            var request = new
            {
                title = "Integration Test Task",
                description = "Testing integration",
                priority = 1,
                dueDate = DateTime.UtcNow.AddDays(7).ToString("o")
            };

            // Act
            var response = await _client.PostAsync("/api/v1/tasks",
                new StringContent(System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"));

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateTask_ShouldReturn400_WhenTitleIsEmpty()
        {
            // Arrange
            var request = new
            {
                title = "",
                description = "Testing integration"
            };

            // Act
            var response = await _client.PostAsync("/api/v1/tasks",
                new StringContent(System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"));

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _factory.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
