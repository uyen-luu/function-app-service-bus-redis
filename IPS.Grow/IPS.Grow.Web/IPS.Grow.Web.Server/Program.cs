using IPS.Grow.Infra.Extensions;
using IPS.Grow.Web.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(ConfigFactory.ConfigureServer);
// Add services to the container.
builder.ConfigureAuth();
//builder.Services.AddAntiforgery(); //TODO
builder.Services.AddRateLimiter(options =>
{
    //options.AddPolicy("", policy => new r)
    //options.AddPolicy("default", policy =>
    //{
    //    policy.Limit = 100; // Limit to 100 requests
    //    policy.Period = TimeSpan.FromMinutes(1); // Per 1 minute
    //});
});
builder.Services.AddControllers()
    .AddJsonOptions(ConfigFactory.ConfigureJson);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
//

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");
app.Run();
