import { TestBed } from '@angular/core/testing';
import { Pagination } from './pagination';

describe('Pagination', () => {
  async function render(page: number, total: number, pageSize = 10) {
    const fixture = TestBed.createComponent(Pagination);
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('pageSize', pageSize);
    fixture.componentRef.setInput('total', total);
    await fixture.whenStable();
    return fixture;
  }

  const buttons = (el: HTMLElement) => [...el.querySelectorAll('button')] as HTMLButtonElement[];

  it('is hidden when everything fits on one page', async () => {
    const fixture = await render(1, 10);

    expect(fixture.nativeElement.querySelector('nav')).toBeNull();
  });

  it('shows the range and disables Prev on the first page', async () => {
    const fixture = await render(1, 57);
    const el = fixture.nativeElement as HTMLElement;

    expect(el.textContent).toContain('1–10 of 57');
    expect(el.textContent).toContain('Page 1 of 6');
    expect(buttons(el)[0].disabled).toBe(true);
    expect(buttons(el)[1].disabled).toBe(false);
  });

  it('shows a partial last page and emits the requested page', async () => {
    const fixture = await render(6, 57);
    const el = fixture.nativeElement as HTMLElement;
    const requested: number[] = [];
    fixture.componentInstance.pageChange.subscribe((p) => requested.push(p));

    expect(el.textContent).toContain('51–57 of 57');
    expect(buttons(el)[1].disabled).toBe(true);
    buttons(el)[0].click();
    expect(requested).toEqual([5]);
  });
});
