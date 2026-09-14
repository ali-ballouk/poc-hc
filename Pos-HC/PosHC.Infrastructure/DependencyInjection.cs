using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PosHC.Application.Interfaces;
using PosHC.Infrastructure.Pdf;
using PosHC.Infrastructure.Persistence;
using PosHC.Infrastructure.Repositories;
using QuestPDF.Infrastructure;

namespace PosHC.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IPOSHCRepository, POSHCRepository>();
            services.AddScoped<IClinicStore, ClinicStore>();
            services.AddScoped<IInvoiceReader, InvoiceReader>();
            services.AddScoped<IPatientVisitReader, PatientVisitReader>();
            services.AddScoped<IClinicReportReader, ClinicReportReader>();
            services.AddScoped<IPasswordCodec, PasswordCodec>();
            services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
            services.AddScoped<IReceiptPdfGenerator, ReceiptPdfGenerator>();
            QuestPDF.Settings.License = LicenseType.Community;
            return services;
        }
    }
}
