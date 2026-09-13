# Generic grid

`GenericGridComponent` renders Bootstrap-styled rows with client-side search,
sorting, paging, column visibility, resizing, optional row selection, and actions.

The app uses two supported configuration styles:

- Visit items pass typed `GridColumn<VisitRow>[]` and `GridAction<VisitRow>[]` inputs.
  The total column uses a value accessor. Each added line has its own `rowId`, so
  duplicate catalog items remain independently removable. API payloads retain the
  catalog ID and quantity; the local row ID is not sent to the backend.
- Invoices project `app-generic-grid-column` components. A cell template retains
  date and time formatting. Currency columns and Download PDF use built-in column
  formatting and the actions input.

Pass an immutable `data` array and a stable `rowId` function. Both app grids set
`selectable` to false because their workflows use row actions rather than bulk
selection. Set `label`, `emptyMessage`, and `loading` for the surrounding workflow.

Column menus and form IDs are isolated per instance. Column visibility overrides
do not mutate the caller's definitions. Resizing supports pointer dragging and
left/right arrow keys on the resize handle. Date objects sort chronologically.
Search and page-size controls are standalone, so grids can be nested in forms.

Run `npm test` for shared component and app integration tests.

## Backend paging

Billing and clinic management lists use `serverPaging`. Pass the current API page
as `data`; the grid renders it without filtering, sorting, or slicing it locally.
The existing search form submits a backend search and resets the page to 1.
Local sort buttons are disabled in this mode because these endpoints do not
support sorting across the full result set.

```html
<app-generic-grid [data]="rows" [columns]="columns" [serverPaging]="true" [totalRecords]="total" [pageIndex]="page - 1" [pageSize]="pageSize" [pageSizeOptions]="[10, 20, 50, 100]" [loading]="loading" (pageChange)="onPageChange($event)"></app-generic-grid>
```

`pageChange` emits `GridPageChange` with a zero-based `pageIndex` and `pageSize`.
The parent converts the index to the API's one-based `page`, cancels the previous
request, and requests `?page=2&pageSize=20` with the current search and filters.
Responses contain `Items`, `Total`, `PageSize`, and the effective `Page`. Apply all
four values so the UI follows backend page clamping when result counts shrink.
The API defaults to 50 rows for existing callers and limits page sizes to 1–100.

Page controls remain visible for empty results and are disabled while loading.
Replacing data does not reset a server page. Selection remains scoped to the
loaded page and is pruned when data is replaced. Omit `serverPaging` for complete
in-memory arrays such as visit items, invoice-detail tables and report summaries.
