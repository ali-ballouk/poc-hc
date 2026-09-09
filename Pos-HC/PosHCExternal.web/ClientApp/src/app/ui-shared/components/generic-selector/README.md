# Generic Selector Component

A reusable, type-safe Angular dropdown selector component that supports both single and multi-selection modes. Built as a standalone component with minimal dependencies and flexible configuration.

## Features

- ✅ **Generic typing** — Works with any entity type using `valueField` and `textField` inputs
- ✅ **Single & Multi-selection modes** — Toggle between single selection and multi-select with checkboxes
- ✅ **Search/Filter** — Built-in search functionality to filter options by text
- ✅ **Selection state preservation** — Selected items persist when dropdown is reopened
- ✅ **Bootstrap styled** — Uses Bootstrap 5.3 for consistent UI
- ✅ **OnPush change detection** — Optimized performance
- ✅ **Accessibility** — ARIA roles and keyboard-friendly
- ✅ **Type-safe** — Full TypeScript support with generics

## Installation

The component is already integrated into the demo. To use it in your own Angular application:

1. Copy the `generic-selector` folder to your components directory
2. Import `GenericSelectorComponent` in your standalone component or module

## Usage

### POS-HC integration

Doctor, patient, payment-type and catalog-item selectors now use this component.
The entity wrappers retain API loading and emit IDs to their parents; this component
emits the selected entity (or `null`), so wrappers map the event to `item?.Id ?? null`.

Bind `initialSelection` to the current ID (or array of IDs in multi mode). Despite
its historical name, this input is reactive: parent resets and asynchronous data
updates refresh the selected label without emitting another selection event.
Use `label` for an accessible field label and optionally `inputId` for a stable ID;
otherwise each instance generates a unique ID.

Search only filters visible options. Multi-selection events include every selected
entity, including those outside the filter. Select-all affects matching options.
Only one selector can remain open. Arrow keys move through options; Escape closes
the menu and returns focus to its trigger. Controls use standalone Angular forms
bindings so the selector can be nested inside the visit form.

The stylesheet keeps dropdowns inside the scrollable Bootstrap payment modal.

### Single Selection Mode

```html
<app-generic-selector [data]="users" valueField="id" textField="name" mode="single" placeholder="Select a user..." (selectionChanged)="onUserSelected($event)"></app-generic-selector>
```

```typescript
onUserSelected(user: User | null): void {
  if (user) {
	console.log('Selected user:', user);
  }
}
```

### Multi-Selection Mode

```html
<app-generic-selector [data]="users" valueField="id" textField="name" mode="multi" placeholder="Select multiple users..." (selectionChanged)="onUsersSelected($event)"></app-generic-selector>
```

```typescript
onUsersSelected(users: User[]): void {
  console.log('Selected users:', users);
}
```

## Inputs

| Input              | Type                  | Required | Default                 | Description                                 |
| ------------------ | --------------------- | -------- | ----------------------- | ------------------------------------------- |
| `data`             | `readonly T[]`        | ✅       | —                       | Array of items to display in dropdown       |
| `valueField`       | `keyof T \| string`   | ✅       | —                       | Property name to use as the unique value/id |
| `textField`        | `keyof T \| string`   | ✅       | —                       | Property name to use as display text        |
| `mode`             | `'single' \| 'multi'` | ✅       | —                       | Selection mode                              |
| `placeholder`      | `string`              | ❌       | `'Select an option...'` | Placeholder text when nothing is selected   |
| `disabled`         | `boolean`             | ❌       | `false`                 | Disable the selector                        |
| `initialSelection` | `any \| any[]`        | ❌       | `null` or `[]`          | Initial selected value(s)                   |

## Outputs

| Output             | Type                         | Description                                       |
| ------------------ | ---------------------------- | ------------------------------------------------- |
| `selectionChanged` | `EventEmitter<any \| any[]>` | Emits the selected item(s) when selection changes |

## API Methods

| Method               | Parameters | Returns  | Description                        |
| -------------------- | ---------- | -------- | ---------------------------------- |
| `openDropdown()`     | —          | `void`   | Opens the dropdown                 |
| `closeDropdown()`    | —          | `void`   | Closes the dropdown                |
| `toggleDropdown()`   | —          | `void`   | Toggles dropdown open/close state  |
| `clearSelection()`   | —          | `void`   | Clears all selections              |
| `clearAll()`         | —          | `void`   | Clears all selections (multi-mode) |
| `getSelectedCount()` | —          | `number` | Returns count of selected items    |

## Component Structure

```
generic-selector/
├── generic-selector.component.ts       # Main component logic
├── generic-selector.component.html     # Template
├── generic-selector.component.scss     # Styles
├── generic-selector.models.ts          # TypeScript interfaces & types
└── README.md                           # This file
```

## Models

### `SelectorMode`

```typescript
type SelectorMode = "single" | "multi";
```

### `SelectorConfig<T>`

```typescript
interface SelectorConfig<T> {
  valueField: keyof T | string;
  textField: keyof T | string;
  mode: SelectorMode;
}
```

### `SelectedItem<T>`

```typescript
interface SelectedItem<T> {
  value: any;
  text: string;
  item: T;
}
```

## Examples

### Example 1: User Selector (Single)

```typescript
@Component({
  selector: "app-user-form",
  template: ` <app-generic-selector [data]="users$ | async" valueField="id" textField="name" mode="single" placeholder="Choose assigned user..." (selectionChanged)="form.patchValue({ userId: $event?.id })"></app-generic-selector> `,
})
export class UserFormComponent {
  users$ = this.userService.getUsers();

  constructor(private userService: UserService) {}
}
```

### Example 2: Product Selector (Multi)

```typescript
@Component({
  selector: "app-cart",
  template: ` <app-generic-selector [data]="products" valueField="sku" textField="productName" mode="multi" placeholder="Add products to cart..." (selectionChanged)="addToCart($event)"></app-generic-selector> `,
})
export class CartComponent {
  products: Product[] = [];

  addToCart(products: Product[]): void {
    products.forEach((p) => this.cart.add(p));
  }
}
```

### Example 3: Role Selector (Multi with Initial Selection)

```typescript
@Component({
  selector: "app-permissions",
  template: ` <app-generic-selector [data]="availableRoles" valueField="id" textField="roleName" mode="multi" [initialSelection]="user.roleIds" (selectionChanged)="onRolesSelected($event)"></app-generic-selector> `,
})
export class PermissionsComponent {
  @Input() user!: User;
  availableRoles: Role[] = [];

  onRolesSelected(roles: Role[]): void {
    this.user.roles = roles;
  }
}
```

## Styling

The component uses Bootstrap 5.3 classes for styling. You can customize the appearance by:

1. **Overriding SCSS variables** in your global styles
2. **Extending the component** and modifying `generic-selector.component.scss`
3. **Using CSS classes** to override specific elements

### Key CSS Classes

- `.generic-selector-wrapper` — Container
- `.dropdown` — Dropdown container
- `.dropdown-toggle` — Button that opens/closes dropdown
- `.dropdown-menu` — Menu container
- `.dropdown-options` — Single mode options list
- `.dropdown-options-multi` — Multi mode checkboxes
- `.selected-items-panel` — Multi mode selected items display

## Performance Considerations

- **trackBy functions** are implemented for efficient list rendering
- **OnPush change detection** minimizes unnecessary checks
- **Search filtering** is performed client-side (suitable for < 1000 items)
- **Selection state** is managed with a Set for O(1) lookups

For large datasets (1000+ items), consider:

- Implementing virtual scrolling (ng-virtual-scroll)
- Server-side filtering
- Lazy loading options

## Accessibility

The component includes:

- ✅ ARIA roles (`listbox`, `option`, `listbox`)
- ✅ ARIA attributes (`aria-expanded`, `aria-selected`, `aria-label`)
- ✅ Keyboard navigation support
- ✅ Focus management
- ✅ Screen reader announcements

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Dependencies

- Angular 21+
- Bootstrap 5.3.8
- TypeScript 5.9+

## Testing

To test the component in your application:

```typescript
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { GenericSelectorComponent } from "./generic-selector.component";

describe("GenericSelectorComponent", () => {
  let component: GenericSelectorComponent;
  let fixture: ComponentFixture<GenericSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenericSelectorComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(GenericSelectorComponent);
    component = fixture.componentInstance;
    component.data = [
      { id: 1, name: "Alice" },
      { id: 2, name: "Bob" },
    ];
    component.valueField = "id";
    component.textField = "name";
    component.mode = "single";

    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("should emit selection when option is selected", (done) => {
    component.selectionChanged.subscribe((selection) => {
      expect(selection.id).toBe(1);
      done();
    });

    component.onOptionClicked({ value: 1, text: "Alice", item: component.data[0] });
  });
});
```

## Changelog

### v1.0.0

- Initial release
- Single and multi-selection modes
- Search functionality
- Bootstrap 5.3 styling
- Full TypeScript support

## Contributing

Found a bug or want to improve the component? Please submit an issue or pull request to the repository.

## License

MIT
