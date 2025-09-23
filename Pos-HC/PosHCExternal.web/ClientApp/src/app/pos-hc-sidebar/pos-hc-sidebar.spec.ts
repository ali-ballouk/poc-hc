import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHcSidebar } from './pos-hc-sidebar';

describe('PosHcSidebar', () => {
  let component: PosHcSidebar;
  let fixture: ComponentFixture<PosHcSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHcSidebar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHcSidebar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
