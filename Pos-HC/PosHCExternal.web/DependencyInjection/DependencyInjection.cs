using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Application.Invoices.Queries;
using PosHC.Application.Services;
using PosHC.Infrastructure.Pdf;
using PosHC.Infrastructure.Persistence;
using PosHC.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddRepository(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
         options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<IPOSHCRepository, POSHCRepository>();

        return builder;
    }
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<ICatalogItemService, CatalogItemService>();

        // PDF Generator
        services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();

        // Use Cases
        services.AddScoped<InvoiceForPrintService>();

        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentTypeService, PaymentTypeService>();
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}