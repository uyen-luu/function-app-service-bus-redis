using IPS.Grow.Infra.Extensions;
using IPS.Grow.Web.Server.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(ConfigFactory.ConfigureServer);
// Add services to the container.
builder.ConfigureAuth();
// DâtProtection
//builder.Services.AddAntiforgery(); //TODO
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole()
                  .AddDebug();
});
builder.Services.AddRateLimiter(o => o
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        //options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys"))
    .SetApplicationName("YourAppName");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://example.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
builder.Services.AddControllers()
    .AddJsonOptions(ConfigFactory.ConfigureJson);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
//
app.UseRateLimiter();
app.UseCors("AllowSpecificOrigin");
//
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; img-src *; media-src media1.com media2.com; script-src trustedscripts.com");
    await next();
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");
app.Run();
