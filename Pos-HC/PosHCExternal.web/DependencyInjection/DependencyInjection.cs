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
        builder.Services.AddScoped<IClinicStore, ClinicStore>();
        builder.Services.AddScoped<IPatientVisitReader, PatientVisitReader>();
        builder.Services.AddScoped<IClinicReports, ClinicReports>();
        builder.Services.AddScoped<IPasswordCodec, PasswordCodec>();

        return builder;
    }
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<BillingService>();
        services.AddScoped<PatientVisitService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IClinicSettingsService, ClinicSettingsService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<ICashShiftService, CashShiftService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<ICatalogItemService, CatalogItemService>();

        // PDF Generator
        services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
        services.AddScoped<IReceiptPdfGenerator, ReceiptPdfGenerator>();

        // Use Cases
        services.AddScoped<InvoiceForPrintService>();

        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentTypeService, PaymentTypeService>();
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}
