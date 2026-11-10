import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '@core/auth/auth.service';
import { Permissions } from '@core/auth/permissions';
import { AiSuggestion } from '@models/task';
import { of } from 'rxjs';
import { AiApi } from './ai-api';
import { AiSuggest } from './ai-suggest';

describe('AiSuggest', () => {
  const permissions = signal<Record<string, number>>({});
  const ai = { improve: vi.fn() };

  function render(description = 'fix the invoice thing') {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { permissions, me: signal({ id: 'me' }) } },
        { provide: AiApi, useValue: ai },
      ],
    });
    const fixture = TestBed.createComponent(AiSuggest);
    fixture.componentRef.setInput('title', 'Invoice');
    fixture.componentRef.setInput('description', description);
    return fixture;
  }

  const button = (el: HTMLElement) => el.querySelector<HTMLButtonElement>('button');

  beforeEach(() => {
    permissions.set({ [Permissions.Ai.EnhanceDescription]: 8 });
    ai.improve.mockReset();
  });

  it('renders nothing without the AI permission', async () => {
    permissions.set({});
    const fixture = render();
    await fixture.whenStable();

    expect(button(fixture.nativeElement)).toBeNull();
  });

  it('is disabled until there is a description to improve', async () => {
    const fixture = render('   ');
    await fixture.whenStable();

    expect(button(fixture.nativeElement)?.disabled).toBe(true);
  });

  it('previews the suggestion and emits it only on Apply', async () => {
    const suggestion: AiSuggestion = { title: 'Export invoices as PDF', description: 'Acceptance criteria…' };
    ai.improve.mockReturnValue(of(suggestion));
    const fixture = render();
    const applied: AiSuggestion[] = [];
    fixture.componentInstance.apply.subscribe((s) => applied.push(s));
    await fixture.whenStable();

    button(fixture.nativeElement)?.click();
    await fixture.whenStable();

    expect(ai.improve).toHaveBeenCalledWith('Invoice', 'fix the invoice thing');
    expect(fixture.nativeElement.textContent).toContain('Export invoices as PDF');
    expect(applied).toEqual([]);

    button(fixture.nativeElement)?.click(); // Apply is the first button in the preview
    await fixture.whenStable();

    expect(applied).toEqual([suggestion]);
  });
});
