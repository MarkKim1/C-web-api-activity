using System.Net;
using Serilog;
var builder = WebApplication.CreateBuilder();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddSwaggerGen();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;

    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

Log.Logger = new LoggerConfiguration()
        .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception occurred while processing request.");
        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var error = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "an unexpected error occurred",
                Detail = app.Environment.IsDevelopment() ? ex.Message : ""
            };
            await context.Response.WriteAsJsonAsync(error);
        }
        else
        {
            context.Abort();
        }
    }
});

app.UseHttpLogging();

app.Use(async (context, next) =>
{
    DateTime start = DateTime.UtcNow;
    await next.Invoke();
    var end = DateTime.UtcNow - start;
    System.Console.WriteLine($"Execution time: {end}");
    logger.LogInformation($"Execution time: {end}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    var isAuthenticated = context.Request.Query["authenticated"] == "true";
    if (!isAuthenticated)
    {
        logger.LogWarning("403 Forbidden - Unauthenticated request. Path = {Path}, Query = {Query}",
            context.Request.Path, context.Request.QueryString.ToString());

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        if (!context.Response.HasStarted)
        {
            await context.Response.WriteAsync("Access denied: authenticated = false");
            return;
        }
    }
    else
    {
        await next();
    }

});

app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Put || context.Request.Method == HttpMethods.Post)
    {
        var isAuthorized = context.Request.Query["authorized"] == "true";
        if (!isAuthorized)
        {
            logger.LogWarning("403 Forbidden - Unauthorized request. Path = {Path}, Query = {Query}",
                context.Request.Path, context.Request.QueryString.ToString());

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            if (!context.Response.HasStarted)
            {
                await context.Response.WriteAsync("Access denied: only authrized personal can update the data");
                return;
            }
        }
    }
    else
    {
        await next();
    }

});

app.MapControllers();

app.MapFallback(() => Results.NotFound("Sorry we couldn't find that page"));
app.Run();