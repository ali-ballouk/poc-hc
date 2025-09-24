
# POS-HC (Healthcare Point of Sale System)

**POS-HC** is a **Point of Sale system for healthcare clinics**, built with **.NET 8 (Clean Architecture + DDD)** and **Angular 20**.  
It manages doctors, patients, invoices, catalog items, and payments with full **API + Frontend integration**.

---

## 🚀 Tech Stack
- **Backend:** ASP.NET Core 8, Clean Architecture, EF Core 8  
- **Frontend:** Angular 20, Angular Material, TailwindCSS  
- **Database:** SQL Server  
- **PDF Generation:** QuestPDF  

---

## 🏗 Architecture
The project follows **Clean Architecture** principles with four layers:

1. **Domain** → Core business entities (Doctor, Patient, Invoice, Payment, CatalogItem).  
2. **Application** → Business logic (DTOs, services, interfaces, contracts).  
3. **Infrastructure** → EF Core DbContext, repositories, PDF invoice generator.  
4. **Web (API + Angular)** →  
   - **Web API:** Controllers exposing endpoints for invoices, doctors, patients, catalog, and payments.  
   - **ClientApp:** Angular 20 frontend consuming the APIs.  

---

## 🔑 Features
- Manage **Doctors** & **Patients**  
- Create **Invoices** with items and doctor fees  
- Record **Payments** (Cash, Card, Transfer, On-Account)  
- Print invoices as **PDF**  
- Angular frontend with Material UI for seamless user experience  

---

## 📂 Documentation
Detailed documentation for each layer:  
- [Domain Layer](README-Domain.md)  
- [Application Layer](README-Application.md)  
- [Infrastructure Layer](README-Infrastructure.md)  
- [Web API](README-WebAPI.md)  
- [ClientApp (Angular Frontend)](README-ClientApp.md)  

---
