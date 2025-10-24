using Microsoft.EntityFrameworkCore;
using RTMAuthServer.Data;
using RTMAuthServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminPanel",
        policy =>
        {
            policy.WithOrigins("http://localhost:8000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                .AllowCredentials();
        }); 
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAdminPanel");
app.UseAuthorization();
app.MapControllers();

app.MapHub<UserHub>("/userHub");
app.Run();
