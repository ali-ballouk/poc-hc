
# Web API Controllers Documentation

This document describes the controllers inside **PosHCExternal.web/Controllers**.  
All APIs are extracted directly from the codebase, without additions.

---

## 1. CatalogItemController
**Route Base:** `api/catalogtitem`  
**Dependency:** `ICatalogItemService`

### Endpoints
- **GET /api/catalogtitem**
```csharp
[HttpGet]
public async Task<IActionResult> GetAllItems(CancellationToken cancellationToken)
```
- **Description:** Returns all catalog items.  
- **Request Body:** None  
- **Response:** List of catalog items DTOs.

---

## 2. DoctorController
**Route Base:** `api/doctor`  
**Dependency:** `IDoctorService`

### Endpoints
- **GET /api/doctor/lookup**
```csharp
[HttpGet("lookup")]
public async Task<IActionResult> GetAllDoctorInfo(CancellationToken cancellationToken)
```
- **Description:** Returns doctors lookup list.  
- **Request Body:** None  
- **Response:** List of doctors DTOs.

---

## 3. InvoiceController
**Class Name:** `InvoiceControllerController` (⚠️ redundant `Controller`)  
**Route Base:** `api/invoice`  
**Dependencies:**  
- `IInvoiceService _invoiceServiceService`  
- `InvoiceForPrintService _invoiceForPrintService`  
- `IInvoicePdfGenerator _iInvoicePdfGenerator`

### Endpoints
1. **GET /api/invoice**
```csharp
[HttpGet]
public async Task<IActionResult> GetAllInvoice(CancellationToken cancellationToken)
```
- **Description:** Returns all invoices.  
- **Request Body:** None  

---

2. **POST /api/invoice**
```csharp
[HttpPost]
public async Task<IActionResult> CreatInvoice(CreateInvoiceDto invoice, CancellationToken cancellationToken)
```
- **Description:** Creates a new invoice.  
- **Request Body (JSON sample):**
```json
{
  "doctorId": "11111111-1111-1111-1111-111111111111",
  "patientId": "22222222-2222-2222-2222-222222222222",
  "discount": 10.0,
  "doctorFee": 50.0,
  "items": [
    {
      "catalogItemId": "33333333-3333-3333-3333-333333333333",
      "quantity": 2,
      "unitPrice": 30.0
    }
  ]
}
```

---

3. **GET /api/invoice/{id}/print**
```csharp
[HttpGet("{id:guid}/print")]
public async Task<IActionResult> Print(Guid id, CancellationToken ct)
```
- **Description:** Returns PDF file of the invoice.  
- **Request Body:** None  
- **Response:** PDF file download.

---

## 4. PatientController
**Route Base:** `api/patient`  
**Dependency:** `IPatientService`

### Endpoints
- **GET /api/patient/lookup**
```csharp
[HttpGet("lookup")]
public async Task<IActionResult> GetAllPatientInfo(CancellationToken cancellationToken)
```
- **Description:** Returns patients lookup list.  
- **Request Body:** None  
- **Response:** List of patient DTOs.

---

## 5. PaymentController
**Route Base:** `api/payment`  
**Dependency:** `IPaymentService`

### Endpoints
- **POST /api/payment**
```csharp
[HttpPost]
public async Task<IActionResult> SavePayment(PaymentRequestDto paymentRequestDto, CancellationToken cancellationToken)
```
- **Description:** Saves a new payment for an invoice.  
- **Request Body (JSON sample):**
```json
{
  "invoiceId": "44444444-4444-4444-4444-444444444444",
  "paymentTypeId": "55555555-5555-5555-5555-555555555555",
  "settings": "{ \"transactionId\": \"TX123456\", \"last4\": \"4242\" }"
}
```
- **Response:** Payment confirmation DTO.

---

## 6. PaymentTypeController
**Route Base:** `api/paymenttype`  
**Dependency:** `IPaymentTypeService`

### Endpoints
- **GET /api/paymenttype/lookup**
```csharp
[HttpGet("lookup")]
public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
```
- **Description:** Returns list of payment types (Cash, Card, etc.).  
- **Request Body:** None  

---

