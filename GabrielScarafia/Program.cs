using Api.Endpoints;
using Core;
using Core.Database;
using Core.Exceptions;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreService(builder.Configuration);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapCarrosEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapGet("/", () => Results.Redirect("/scalar/")).ExcludeFromDescription();

    app.MapGet("/debug/tabla/{nombre}", async (string nombre, IDbContext db) =>
    {
        var result = await db.Connection.QueryAsync($"SELECT * FROM {nombre} ORDER BY 1 DESC");
        return Results.Ok(result);
    }).WithTags("debug");

    app.MapGet("/debug/tablas", async (IDbContext db) =>
    {
        var tablas = await db.Connection.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table'");
        return Results.Ok(tablas);
    }).WithTags("debug");
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Mi API de HealthTech")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        (int status, string mensaje) = ex switch
        {
            NotFoundException e => (404, e.Message),
            ValidationException e => (400, e.Message),
            _ => (500, "Error interno del servidor")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { error = mensaje });
    });
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
    var isInMemoryRaw = app.Configuration["DbConnection:IsInMemory"];
    if (bool.TryParse(isInMemoryRaw, out var isInMemory) && isInMemory)
    {
        Core.Database.DbInitializer.Initialize(dbContext.Connection, true);
    }
}

app.Run();