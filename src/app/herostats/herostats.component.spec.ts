import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HerostatsComponent } from './herostats.component';

describe('HerostatsComponent', () => {
  let component: HerostatsComponent;
  let fixture: ComponentFixture<HerostatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HerostatsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HerostatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
