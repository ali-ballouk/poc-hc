
# Infrastructure Layer Documentation

The **Infrastructure Layer** provides technical implementations for persistence and external services.  
It bridges the **Application Layer** with the database and external systems like PDF generation.

---

## Persistence

### ApplicationDbContext
Implements `DbContext` from **Entity Framework Core**.

**DbSets:**
- `CatalogItems`
- `Doctors`
- `Invoices`
- `InvoiceItems`
- `Patients`
- `Payments`
- `PaymentTypes`

**Responsibilities:**
- Maps domain entities to database tables.
- Manages entity relationships (Invoice ↔ Items, Payments).
- Configures EF Core conventions.

**Usage:** Centralized entry point for database operations.

---

## Repository

### POSHCRepository
Implements `IPOSHCRepository` interface from the Application layer.

**Responsibilities:**
- Abstracts data access from business logic.
- Provides methods like:
  - `GetById<T>()`
  - `Add<T>()`
  - `Update<T>()`
  - `Delete<T>()`
  - `SaveChanges()`

**Usage:** Called by services (Application layer) to interact with EF Core DbContext.

---

## PDF Generator

### InvoicePdfGenerator
Implements `IInvoicePdfGenerator` interface.

**Responsibilities:**
- Generates **invoice PDF documents**.
- Uses third-party libraries (e.g., QuestPDF).
- Takes invoice DTO/entity and produces formatted PDF.

**Example Workflow:**
1. InvoiceService calls `IInvoicePdfGenerator`.
2. Generator creates a styled PDF with patient, doctor, items, totals.
3. Returns PDF as byte stream for download or storage.

---

## Flow Overview

1. **Application Layer** services request data.  
2. **POSHCRepository** queries `ApplicationDbContext`.  
3. **InvoicePdfGenerator** is used when invoice export is needed.  
4. All implementations remain hidden behind interfaces.

---

