using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Attachments.Application.Interfaces;
using TaskManager.Attachments.Application.Services;
using TaskManager.Attachments.Application.Validators;
using TaskManager.Attachments.Domain.Interfaces;
using TaskManager.Attachments.Infrastructure.Data;
using TaskManager.Attachments.Infrastructure.Data.Repositories;
using TaskManager.Attachments.Infrastructure.Storage;

namespace TaskManager.Attachments.Api.Extensions;

/// <summary>
/// Extension methods for configuring attachments module services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all attachments module services with DI container.
    /// </summary>
    public static IServiceCollection AddAttachmentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AttachmentsDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<ITaskReadOnlyRepository, TaskReadOnlyRepository>();

        // Services
        services.AddScoped<IAttachmentService, AttachmentService>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<UploadAttachmentRequestValidator>();

        // File storage
        var fileStorageBasePath = configuration["FileStorage:BasePath"] 
            ?? throw new InvalidOperationException("FileStorage:BasePath configuration is missing");
        
        services.AddSingleton<IFileStorageService>(sp => 
            new LocalFileStorageService(fileStorageBasePath));

        return services;
    }
}
