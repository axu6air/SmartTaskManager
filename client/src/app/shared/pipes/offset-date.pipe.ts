import { Pipe, PipeTransform } from '@angular/core';
import { OffsetDateString } from '@models/api';
import { formatShortDate } from '@shared/utils/date';

/** {{ task.dueDate | offsetDate }} → "15 Nov". Empty for null. */
@Pipe({ name: 'offsetDate' })
export class OffsetDatePipe implements PipeTransform {
  transform(value: OffsetDateString | null | undefined): string {
    return value ? formatShortDate(value) : '';
  }
}
