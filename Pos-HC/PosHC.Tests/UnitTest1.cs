namespace PosHC.Tests;

public class InvoiceTotalTests
{
    [Fact]
    public void Tax_is_rounded_in_the_invoice_currency()
    {
        var invoice = new PosHC.Domain.Entities.Invoice { Currency = "USD", DoctorFee = 10.05m, TaxRate = 11 };
        Assert.Equal(1.11m, invoice.Tax);
        Assert.Equal(11.16m, invoice.Total);
    }
}
