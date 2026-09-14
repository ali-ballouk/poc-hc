using PosHC.Application.Interfaces;
using PosHC.Application.Services;

namespace PosHCExternal.web.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IPatientVisitService, PatientVisitService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IClinicSettingsService, ClinicSettingsService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<ICashShiftService, CashShiftService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<ICatalogItemService, CatalogItemService>();
            services.AddScoped<IInvoiceForPrintService, InvoiceForPrintService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentTypeService, PaymentTypeService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IReportsService, ReportsService>();
            return services;
        }
    }
}
