
# Domain Layer Documentation

This document describes the **Domain Layer** of the `poc-hc` project, which defines the core business entities. 
These entities represent the healthcare Point-of-Sale (POS) system concepts such as doctors, patients, invoices, catalog items, and payments. 
The domain layer is independent and contains no infrastructure or external dependencies.

---

## Entities

### 1. CatalogItem
Represents an item that can be billed in the POS system (either a product or service).

**Properties:**
- `Id (Guid)`: Unique identifier.
- `Name (string)`: Item name.
- `UnitPrice (decimal)`: Price per unit.
- `Type (int / enum)`: Indicates if it’s a Product or Service.
- `Settings (string/json)`: Flexible metadata storage.

**Usage:** Used in invoices to define billable items.

---

### 2. Doctor
Represents a doctor who provides consultations or services.

**Properties:**
- `Id (Guid)`: Unique identifier.
- `FirstName (string)` / `LastName (string)`: Doctor’s name.
- `Fee (decimal)`: Consultation fee.

**Usage:** Linked with invoices as the service provider.

---

### 3. Invoice
Represents a complete invoice issued to a patient.

**Properties:**
- `Id (Guid)`: Invoice identifier.
- `InvoiceDate (DateTime)`: Creation date.
- `DoctorId (Guid)`: Reference to doctor.
- `PatientId (Guid)`: Reference to patient.
- `Discount (decimal)`: Optional discount.
- `DoctorFee (decimal)`: Applied doctor fee.
- `Total (decimal)`: Final invoice total.
- `Items (List<InvoiceItem>)`: Detailed items in the invoice.
- `Payments (List<Payment>)`: Payments made toward the invoice.

**Usage:** Core entity that ties together doctor, patient, items, and payments.

---

### 4. InvoiceItem
Represents each line item inside an invoice.

**Properties:**
- `Id (Guid)`: Identifier.
- `InvoiceId (Guid)`: Belongs to which invoice.
- `CatalogItemId (Guid)`: Reference to catalog item.
- `Quantity (int)`: Number of units.
- `UnitPrice (decimal)`: Price per unit at the time of invoice.
- `LineTotal (decimal)`: Quantity × UnitPrice.

**Usage:** Ensures historical integrity — invoice keeps its own copy of item prices.

---

### 5. Patient
Represents a patient receiving services/products.

**Properties:**
- `Id (Guid)`: Unique identifier.
- `FirstName (string)` / `LastName (string)`: Patient’s name.

**Usage:** Linked to invoices for billing.

---

### 6. Payment
Represents a payment transaction linked to an invoice.

**Properties:**
- `Id (Guid)`: Identifier.
- `InvoiceId (Guid)`: Related invoice.
- `PaymentTypeId (Guid)`: Reference to payment type.
- `PaymentDate (DateTime)`: Date of payment.
- `Settings (string/json)`: Flexible storage for type-specific details (e.g., card token, transfer reference).

**Usage:** Flexible design to support multiple payment methods.

---

### 7. PaymentType
Defines available payment methods (Cash, Card, Transfer, On-Account).

**Properties:**
- `Id (int)`: Identifier.
- `Name (string)`: Payment type name (unique).

**Usage:** Lookup table for `Payment`.

---

## Relationships Summary

- **Invoice ↔ Patient** → Each invoice belongs to a patient.  
- **Invoice ↔ Doctor** → Each invoice is issued by a doctor.  
- **Invoice ↔ InvoiceItems** → Line items linked by `InvoiceId`.  
- **Invoice ↔ Payments** → Payments linked by `InvoiceId`.  
- **InvoiceItem ↔ CatalogItem** → Each line references a catalog item.  
- **Payment ↔ PaymentType** → Defines the method used.  

---

