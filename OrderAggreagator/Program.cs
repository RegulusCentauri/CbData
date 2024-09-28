using OrderAggregator;

var utc = DateTime.UtcNow;
Console.WriteLine($".NET={Environment.Version} CULT:{System.Globalization.CultureInfo.CurrentCulture} UTC:{utc} LOC:{utc.ToLocalTime()}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IOrderCache, OrderCache>();
builder.Services.AddHostedService<OrderAggregationService>();
builder.Services.AddControllers();
builder.Services.AddLogging(logging => {
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();
app.MapControllers();

app.Run();