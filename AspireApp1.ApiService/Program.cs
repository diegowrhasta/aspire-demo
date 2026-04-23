var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults(hasRabbit: true);

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServer")
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries =
    ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/users", async (AppDbContext db) => await db.Users.ToListAsync());

app.MapPost("/users", async (AppDbContext db, User user) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});

app.MapGet("/rabbit", async (IConfiguration configuration) =>
{
    using var activity = Extensions.MessagingObservability.Source.StartActivity("Publish Message");
    
    var connectionString = configuration.GetConnectionString("rabbitmq");
    
    var factory = new ConnectionFactory
    {
        Uri = new Uri(connectionString!)
    };

    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();
    
    await channel.QueueDeclareAsync(
        queue: "task_queue",
        durable: false,
        exclusive: false,
        autoDelete: false
    );
    
    var message = new { Text = "Hello World", Timestamp = DateTime.UtcNow };
    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    
    var properties = new BasicProperties
    {
        Persistent = false,  // Message survives broker restart
        ContentType = "application/json",
        MessageId = Guid.NewGuid().ToString(),
        Headers = new Dictionary<string, object>()!
    };
    
    var propagator = Propagators.DefaultTextMapPropagator;
    
    propagator.Inject(
        new PropagationContext(activity!.Context, Baggage.Current),
        properties.Headers,
        (headers, key, value) =>
        {
            headers[key] = Encoding.UTF8.GetBytes(value);
        });

    await channel.BasicPublishAsync(
        exchange: "",           // Empty = default exchange
        routingKey: "task_queue", // Queue name when using default exchange
        mandatory: true,       // Return if can't route
        basicProperties: properties,
        body: body
    );
    
    return Results.Ok(new { Message = "Published successfully" });
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}