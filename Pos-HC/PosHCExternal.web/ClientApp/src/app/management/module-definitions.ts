import { GridColumn } from '../ui-shared/components/generic-grid/generic-grid.models';
export interface Field {
  key: string;
  label: string;
  type?: string;
  required?: boolean;
  options?: { Id: any; Name: string }[];
  lookup?: string;
  min?: number;
  step?: number;
}
export interface ModuleDefinition {
  title: string;
  fields: Field[];
  columns: GridColumn<any>[];
  roles: string[];
  writers: string[];
  defaults?: Record<string, any>;
  createOnly?: boolean;
}
const text = (key: string, label: string, required = false): Field => ({
  key,
  label,
  required,
});
const option = (key: string, label: string, values: string[]): Field => ({
  key,
  label,
  required: true,
  type: 'select',
  options: values.map((x) => ({ Id: x, Name: x })),
});
const active: Field = { key: 'IsActive', label: 'Active', type: 'checkbox' };
const names = [
  text('FirstName', 'First name', true),
  text('LastName', 'Last name', true),
];
const columns = (...keys: string[]): GridColumn<any>[] =>
  keys.map((key) => ({ key, header: key.replace(/([a-z])([A-Z])/g, '$1 $2') }));
export const currencies = option('Currency', 'Currency', ['USD', 'LBP']);
export const amount: Field = {
  key: 'Amount',
  label: 'Amount',
  type: 'number',
  required: true,
  step: 0.01,
};
export const patient: Field = {
  key: 'PatientId',
  label: 'Patient',
  type: 'select',
  lookup: 'api/patient/lookup',
  required: true,
};
export const doctor: Field = {
  key: 'DoctorId',
  label: 'Doctor',
  type: 'select',
  lookup: 'api/doctor/lookup',
  required: true,
};
const times: Field[] = [
  {
    key: 'StartsAt',
    label: 'Starts at (device local time)',
    type: 'datetime-local',
    required: true,
  },
  {
    key: 'EndsAt',
    label: 'Ends at (device local time)',
    type: 'datetime-local',
    required: true,
  },
];
export const modules: Record<string, ModuleDefinition> = {
  patients: {
    title: 'Patients',
    roles: ['Administrator', 'Receptionist', 'Doctor'],
    writers: ['Administrator', 'Receptionist'],
    fields: [
      ...names,
      text('Phone', 'Phone'),
      { ...text('Email', 'Email'), type: 'email' },
      { key: 'DateOfBirth', label: 'Date of birth', type: 'date' },
      text('Address', 'Address'),
      active,
    ],
    columns: columns(
      'FirstName',
      'LastName',
      'Phone',
      'Email',
      'DateOfBirth',
      'IsActive',
    ),
    defaults: { IsActive: true },
  },
  doctors: {
    title: 'Doctors',
    roles: ['Administrator', 'Receptionist', 'Cashier', 'Doctor'],
    writers: ['Administrator'],
    fields: [
      ...names,
      text('Phone', 'Phone'),
      text('Specialty', 'Specialty'),
      {
        key: 'Fee',
        label: 'Consultation fee (USD)',
        type: 'number',
        min: 0,
        step: 0.01,
        required: true,
      },
      active,
    ],
    columns: columns('FirstName', 'LastName', 'Specialty', 'Fee', 'IsActive'),
    defaults: { IsActive: true, Fee: 0 },
  },
  catalog: {
    title: 'Services and products',
    roles: ['Administrator', 'Receptionist', 'Cashier', 'Doctor'],
    writers: ['Administrator'],
    fields: [
      text('Name', 'Name', true),
      {
        key: 'UnitPrice',
        label: 'Base price (USD)',
        type: 'number',
        min: 0,
        step: 0.01,
        required: true,
      },
      {
        key: 'Type',
        label: 'Type',
        type: 'select',
        required: true,
        options: [
          { Id: 1, Name: 'Product' },
          { Id: 2, Name: 'Service' },
        ],
      },
      active,
    ],
    columns: columns('Name', 'UnitPrice', 'Type', 'IsActive'),
    defaults: { IsActive: true, Type: 2, UnitPrice: 0 },
  },
  staff: {
    title: 'Staff accounts',
    roles: ['Administrator'],
    writers: ['Administrator'],
    fields: [
      text('Username', 'Username', true),
      text('DisplayName', 'Display name', true),
      option('Role', 'Role', [
        'Administrator',
        'Receptionist',
        'Cashier',
        'Doctor',
      ]),
      {
        key: 'Password',
        label: 'Password (12+ characters; leave blank to keep existing)',
        type: 'password',
      },
      active,
    ],
    columns: columns('Username', 'DisplayName', 'Role', 'IsActive'),
    defaults: { IsActive: true, Role: 'Receptionist' },
  },
  appointments: {
    title: 'Appointments and waiting queue',
    roles: ['Administrator', 'Receptionist', 'Doctor'],
    writers: ['Administrator', 'Receptionist', 'Doctor'],
    fields: [
      patient,
      doctor,
      ...times,
      option('Status', 'Status', [
        'Booked',
        'CheckedIn',
        'InProgress',
        'Completed',
        'Cancelled',
        'NoShow',
      ]),
      text('Reason', 'Visit reason'),
    ],
    columns: [
      ...columns('PatientName', 'DoctorName'),
      { key: 'StartsAt', header: 'Starts at', type: 'date' },
      ...columns('Status', 'Reason'),
    ],
    defaults: { Status: 'Booked' },
  },
  availability: {
    title: 'Doctor availability',
    roles: ['Administrator', 'Receptionist', 'Doctor'],
    writers: ['Administrator', 'Receptionist'],
    fields: [doctor, ...times],
    columns: [
      ...columns('DoctorName'),
      { key: 'StartsAt', header: 'Starts at', type: 'date' },
      { key: 'EndsAt', header: 'Ends at', type: 'date' },
    ],
    createOnly: true,
  },
  shifts: {
    title: 'Cash shifts',
    roles: ['Administrator', 'Cashier'],
    writers: ['Administrator', 'Cashier'],
    fields: [
      currencies,
      {
        key: 'OpeningAmount',
        label: 'Opening cash',
        type: 'number',
        min: 0,
        step: 0.01,
        required: true,
      },
    ],
    columns: [
      ...columns(
        'Currency',
        'OpeningAmount',
        'ExpectedAmount',
        'CountedAmount',
        'Variance',
      ),
      { key: 'OpenedAt', header: 'Opened', type: 'date' },
      { key: 'ClosedAt', header: 'Closed', type: 'date' },
    ],
    defaults: { Currency: 'USD', OpeningAmount: 0 },
    createOnly: true,
  },
  audit: {
    title: 'Audit history',
    roles: ['Administrator'],
    writers: [],
    fields: [],
    columns: [
      { key: 'CreatedAt', header: 'Time', type: 'date' },
      ...columns('Actor', 'Action', 'Entity', 'EntityId', 'Details'),
    ],
  },
};
