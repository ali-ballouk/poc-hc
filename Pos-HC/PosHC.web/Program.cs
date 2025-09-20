using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Infrastructure.Persistence;
using PosHC.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

// Replace this block:

// With this block:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();

// Register Query Handlers
builder.Services.AddScoped<IDoctorService, DoctorService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // keep original casing
});

builder.Services.AddSpaStaticFiles(config =>
{
    config.RootPath = "ClientApp/dist";
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapWhen(context =>
    !context.Request.Path.StartsWithSegments("/api"), spaApp =>
    {
        spaApp.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (app.Environment.IsDevelopment())
            {
                spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            }

        });
    });

app.Run();
