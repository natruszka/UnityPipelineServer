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
        policyBuilder => policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.MapControllers();
app.Run();