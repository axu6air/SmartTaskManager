import { pageFrom, toParams } from './query-params';

describe('query param helpers', () => {
  it('toParams drops empty values but keeps zero and false', () => {
    expect(
      toParams({ page: 1, pageSize: 10, search: '  ', sortBy: undefined, status: null, priority: 0, flag: false, q: 'pdf' }),
    ).toEqual({ page: 1, pageSize: 10, priority: 0, flag: false, q: 'pdf' });
  });

  it('pageFrom accepts only positive integers', () => {
    expect(pageFrom('3')).toBe(3);
    expect(pageFrom(undefined)).toBe(1);
    expect(pageFrom('0')).toBe(1);
    expect(pageFrom('-2')).toBe(1);
    expect(pageFrom('2.5')).toBe(1);
    expect(pageFrom('abc')).toBe(1);
  });
});
