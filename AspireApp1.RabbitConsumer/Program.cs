var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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

app.MapGet("/message", async (IConfiguration configuration) =>
{
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
    
    // Set prefetch - how many messages to grab at once
    await channel.BasicQosAsync(
        prefetchSize: 0,
        prefetchCount: 1,  // Process one message at a time
        global: false
    );
    
    var consumer = new AsyncEventingBasicConsumer(channel);
    
    consumer.ReceivedAsync += async (sender, args) =>
    {
        var body = args.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        
        try
        {
            // Process your message
            Console.WriteLine($"Received: {message}");
            
            // Acknowledge successful processing
            await channel.BasicAckAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false
            );
        }
        catch (Exception ex)
        {
            // Reject and optionally requeue
            await channel.BasicNackAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                requeue: true  // false = send to dead letter queue
            );
        }
    };
    
    await channel.BasicConsumeAsync(
        queue: "task_queue",
        autoAck: false,  // Manual ack for reliability
        consumer: consumer
    );
    
    return Results.Ok(new { Message = "Consumer started" });
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}