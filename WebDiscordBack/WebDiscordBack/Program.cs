using WebDiscordBack;
using WebDiscordBack.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IDictionary<string, UserConnection>>(opts => new Dictionary<string, UserConnection>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors(builder => builder.WithOrigins("http://localhost:5174", "http://localhost:5173"
    , "https://darling-travesseiro-5ae1c0.netlify.app")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod());

app.MapHub<ChatHub>("/chat");

app.Run();
