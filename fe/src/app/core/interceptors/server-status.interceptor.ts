import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ServerStatusService } from '../services/server-status.service';
import {catchError} from 'rxjs/operators';
import {throwError} from 'rxjs';

export const ServerStatusInterceptor: HttpInterceptorFn = (req, next) => {
  const status = inject(ServerStatusService);

  return next(req).pipe(
    catchError((err: unknown) => {
      const httpErr = err as HttpErrorResponse;

      if (httpErr.status === 0) {
        status.show('Không thể kết nối tới server. Có thể Docker/API chưa bật.');
      } else if ([502, 503, 504].includes(httpErr.status)) {
        status.show('Server đang bận hoặc bảo trì. Vui lòng thử lại sau.');
      }

      return throwError(() => err);
    })
  );
};
