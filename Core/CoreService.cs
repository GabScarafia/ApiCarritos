using Core.Features.Usuarios.Login;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Core;

public static class CoreService
{
    public static IServiceCollection AddCoreService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DbConnection:ConnectionString"];

        var isInMemory = false;
        var isInMemoryRaw = configuration["DbConnection:IsInMemory"];
        if (!string.IsNullOrEmpty(isInMemoryRaw))
        {
            SQLitePCL.Batteries.Init();
            bool.TryParse(isInMemoryRaw, out isInMemory);
        }

        if (isInMemory) { 
            services.AddSingleton<Core.Database.IDbContext>(sp => new Core.Database.DbContext(connectionString, isInMemory: true));
        }
        else 
            services.AddScoped<Core.Database.IDbContext>(sp => new Core.Database.DbContext(connectionString, isInMemory: isInMemory));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());	
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        return services;
    }
}
