import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { FeedbackService } from '../services/feedback.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const feedback = inject(FeedbackService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const { title, detail } = extractMessage(err);
      feedback.error(title, detail);
      return throwError(() => err);
    })
  );
};

function extractMessage(err: HttpErrorResponse): { title: string; detail?: string } {
  if (err.status === 0) {
    return {
      title: 'Sin conexión',
      detail: 'No se pudo conectar con el servidor. Verifica tu conexión a internet.'
    };
  }

  const body = err.error;
  if (body && typeof body === 'object') {
    const title  = typeof body.title  === 'string' ? body.title  : null;
    const detail = typeof body.detail === 'string' ? body.detail : null;

    if (body.errors && typeof body.errors === 'object') {
      const flat = Object.values(body.errors as Record<string, string[]>)
        .flat()
        .filter(Boolean);
      if (flat.length > 0) {
        return {
          title: title ?? 'Datos inválidos',
          detail: flat.join(' • ')
        };
      }
    }

    if (title || detail) {
      return {
        title:  title  ?? defaultTitle(err.status),
        detail: detail ?? undefined
      };
    }
  }

  return {
    title: defaultTitle(err.status),
    detail: err.statusText || undefined
  };
}

function defaultTitle(status: number): string {
  if (status >= 500) return 'Error del servidor';
  if (status === 404) return 'Recurso no encontrado';
  if (status === 409) return 'Conflicto';
  if (status === 422) return 'Operación no permitida';
  if (status === 400) return 'Datos inválidos';
  return `Error ${status}`;
}
