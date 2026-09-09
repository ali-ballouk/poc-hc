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
