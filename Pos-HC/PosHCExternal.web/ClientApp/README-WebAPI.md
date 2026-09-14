# Angular API integration

See [API-ROUTES.md](../../../API-ROUTES.md) for the current routes and migration table, and [ARCHITECTURE.md](../../../ARCHITECTURE.md) for backend ownership and conventions.

Management modules declare their endpoint in src/app/management/module-definitions.ts. The shared BaseAPI client handles HTTP calls and PDF language selection. Invoice actions use api/invoice, payments use api/payment, and refunds use api/payment/invoice/{invoiceId}/refunds.

Request and response models are defined by the Application layer. Keep frontend endpoint tests in sync when changing a controller route. Deploy frontend and backend together.
