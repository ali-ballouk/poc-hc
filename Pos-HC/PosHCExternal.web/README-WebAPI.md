# Web API documentation

The current controller responsibilities, endpoints and migration table are maintained in [API-ROUTES.md](../../API-ROUTES.md). The layer boundaries and coding conventions are in [ARCHITECTURE.md](../../ARCHITECTURE.md).

Controllers handle HTTP and authorization and delegate to Application service interfaces. Doctor, patient, catalog, invoice and payment operations each have a single controller owner. Request and response models live in PosHC.Application/DTOs; database entities are not API contracts.

In development, Swagger lists the current endpoints and their request schemas. Cookie authentication and CSRF validation remain required. Invoice and receipt downloads accept language=en or language=ar. Cash is the only supported new payment method.

Deploy the matching backend and Angular build together. See the route migration table before updating an external client.
