using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Mapping;
using PosHC.Domain.Entities;
using PosHCExternal.web.Controllers;

namespace PosHC.Tests
{
    public class ArchitectureTests
    {
        [Fact]
        public void Inner_layers_do_not_reference_outer_layers_or_framework_adapters()
        {
            var domain = typeof(Invoice).Assembly;
            var application = typeof(IInvoiceService).Assembly;
            Assert.DoesNotContain(domain.GetReferencedAssemblies(), reference =>
                reference.Name!.StartsWith("PosHC.") || reference.Name.StartsWith("Microsoft.EntityFrameworkCore") ||
                reference.Name.StartsWith("Microsoft.AspNetCore") || reference.Name.StartsWith("QuestPDF") ||
                reference.Name == "System.Text.Json" || reference.Name.Contains("Annotations"));
            Assert.DoesNotContain(application.GetReferencedAssemblies(), reference =>
                reference.Name!.StartsWith("PosHC.Infrastructure") || reference.Name.StartsWith("PosHCExternal") ||
                reference.Name.StartsWith("Microsoft.EntityFrameworkCore") || reference.Name.StartsWith("Microsoft.AspNetCore") ||
                reference.Name.StartsWith("QuestPDF"));
        }

        [Fact]
        public void Controllers_depend_on_service_interfaces_and_accept_no_domain_entities()
        {
            var controllers = typeof(DoctorController).Assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract).ToList();
            Assert.NotEmpty(controllers);
            foreach (var controller in controllers)
            {
                foreach (var parameter in controller.GetConstructors().SelectMany(constructor => constructor.GetParameters()))
                {
                    Assert.True(parameter.ParameterType.IsInterface, controller.Name + " must depend on an interface.");
                    Assert.DoesNotContain(parameter.ParameterType.Name, new[] { nameof(IClinicStore), nameof(IPOSHCRepository), nameof(IInvoiceReader), nameof(IClinicReportReader) });
                }

                foreach (var method in controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    Assert.DoesNotContain(method.GetParameters(), parameter => parameter.ParameterType.Assembly == typeof(Invoice).Assembly);
                }
            }
        }

        [Theory]
        [InlineData(typeof(DoctorController), "api/doctor")]
        [InlineData(typeof(PatientController), "api/patient")]
        [InlineData(typeof(CatalogItemController), "api/catalogitem")]
        [InlineData(typeof(InvoiceController), "api/invoice")]
        [InlineData(typeof(PaymentController), "api/payment")]
        public void Resource_actions_stay_under_their_own_controller_route(Type controller, string route)
        {
            Assert.Equal(route, controller.GetCustomAttribute<RouteAttribute>()!.Template);
            var actions = controller.GetMethods().SelectMany(method => method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>());
            Assert.All(actions, action => Assert.False(action.Template?.StartsWith('/') == true));
            Assert.DoesNotContain(controller.Assembly.GetTypes(), type =>
                type.Name is "BillingController" or "DoctorManagementController" or "PatientManagementController" or "CatalogManagementController" or "PatientVisitsController");
        }

        [Fact]
        public void Public_models_exclude_staff_secrets_and_clinical_notes()
        {
            var staff = new StaffUser { PasswordHash = "private hash", SecurityStamp = "private stamp", ResetTokenHash = "private reset" };
            var invoice = new Invoice { VisitDescription = "private visit", Diagnosis = "private diagnosis", VisitNotesUpdatedBy = "private author" };
            var staffJson = JsonSerializer.Serialize(ClinicDtoMapper.ToDto(staff));
            var invoiceJson = JsonSerializer.Serialize(ClinicDtoMapper.ToDto(invoice));
            Assert.DoesNotContain("private", staffJson);
            Assert.DoesNotContain("PasswordHash", staffJson);
            Assert.DoesNotContain("SecurityStamp", staffJson);
            Assert.DoesNotContain("private", invoiceJson);
            Assert.DoesNotContain("Diagnosis", invoiceJson);
            Assert.DoesNotContain("VisitDescription", invoiceJson);
            Assert.DoesNotContain(typeof(StaffUserDetailsDto).GetProperties(), property => property.Name == "ResetTokenHash");
        }
    }
}
