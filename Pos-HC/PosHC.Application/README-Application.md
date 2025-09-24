
# Application Layer Documentation

The **Application Layer** orchestrates the business logic of the POS Healthcare system.  
It defines **DTOs**, **interfaces**, **services**, and **contracts** to ensure separation of concerns between **Domain** (business rules) and **Infrastructure** (data access).

---

## DTOs (Data Transfer Objects)

### 1. CatalogItemDto
Represents catalog item data for transport between layers.
- `Id (Guid)`  
- `Name (string)`  
- `UnitPrice (decimal)`  
- `Type (int)`  

**Usage:** Exposes catalog item info safely without domain coupling.

---

### 2. DoctorDto
Represents doctor information for API communication.
- `Id (Guid)`  
- `FirstName (string)`  
- `LastName (string)`  
- `Fee (decimal)`  

---

### 3. InvoiceDto
Invoice data for presentation or API transfer.
- `Id (Guid)`  
- `InvoiceDate (DateTime)`  
- `DoctorId (Guid)` / `DoctorName (string)`  
- `PatientId (Guid)` / `PatientName (string)`  
- `Discount (decimal)`  
- `DoctorFee (decimal)`  
- `Total (decimal)`  
- `Items (List<InvoiceItemDto>)`  
- `Payments (List<PaymentDto>)`  

---

### 4. PatientDto
Represents patient information for the API.
- `Id (Guid)`  
- `FirstName (string)`  
- `LastName (string)`  

---

### 5. PaymentRequestDto
Used when registering a new payment.
- `InvoiceId (Guid)`  
- `PaymentTypeId (Guid)`  
- `Settings (string/json)`: Dynamic payment details (card token, transfer reference, etc.)

---

## Interfaces

Define contracts for service implementations.

- **ICatalogItemService** → CRUD operations for catalog items.  
- **IDoctorService** → Manage doctors.  
- **IInvoiceService** → Handle invoice creation, retrieval, totals.  
- **IPatientService** → Manage patients.  
- **IPaymentService** → Register payments.  
- **IPaymentTypeService** → CRUD operations for payment types.  
- **IInvoicePdfGenerator** → Defines contract for generating invoice PDFs.  
- **IPOSHCRepository** → Abstraction for repository operations.  

---

## Services (Business Logic)

Concrete implementations of the interfaces, typically using **repositories** and **DbContext**.

### 1. CatalogItemService
Implements catalog operations:
- Add, update, delete items.
- Retrieve all catalog entries.

### 2. DoctorService
Handles doctor CRUD operations.

### 3. InvoiceService
Business logic for invoices:
- Create invoices.
- Calculate totals.
- Apply doctor fees and discounts.
- Fetch invoice history.

### 4. PatientService
Manages patient records.

### 5. PaymentService
Processes payments:
- Validates invoice existence.
- Links payments with payment types.
- Stores flexible details in `Settings`.

### 6. PaymentTypeService
CRUD operations for payment types.

---

## Contract

### GetInvoiceGenerator
Defines queries/commands for invoice generation (part of CQRS design).  
Used to request PDF generation through `IInvoicePdfGenerator`.

---

## Flow Overview
1. **DTOs** → Data sent/received via API.  
2. **Interfaces** → Define service contracts.  
3. **Services** → Implement business logic (using repository & DbContext).  
4. **Contracts** → Encapsulate specific operations like invoice PDF generation.  

---

