import { TestBed } from '@angular/core/testing';

import { PosHsDialogService } from './pos-hs-dialog.service';

describe('PosHsDialogService', () => {
  let service: PosHsDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PosHsDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
