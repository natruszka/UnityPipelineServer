using UnityPipelineWebApi.Services;

var corsPolicy = "CorsPolicy";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache(); 
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<BuildService>();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy,
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.MapControllers();
app.Run();