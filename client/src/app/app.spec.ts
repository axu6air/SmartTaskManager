import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  it('renders the router outlet and the toast host', async () => {
    TestBed.configureTestingModule({ imports: [App], providers: [provideRouter([])] });

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const root = fixture.nativeElement as HTMLElement;

    expect(root.querySelector('router-outlet')).not.toBeNull();
    expect(root.querySelector('app-toast-host')).not.toBeNull();
  });
});
