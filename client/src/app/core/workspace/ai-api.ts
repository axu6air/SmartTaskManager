import { HttpContext } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/http/api.service';
import { SKIP_LOADING } from '@core/http/http-context';
import { AiSuggestion } from '@models/task';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AiApi {
  private readonly api = inject(ApiService);

  /** A suggestion only; nothing is saved. The caller shows its own spinner, so no global progress bar. */
  improve(title: string | null, description: string): Observable<AiSuggestion> {
    return this.api.post('/ai/task-description', { title, description }, {
      context: new HttpContext().set(SKIP_LOADING, true),
    });
  }
}
