import { inject, Injectable, signal } from "@angular/core"
import { Router } from "@angular/router"
import { type Observable, of, delay, throwError, tap } from "rxjs"
import type {
  User,
  LoginRequest,
  RegisterRequest,
  UpdateProfileRequest,
  ChangePasswordRequest,
  AuthResponse,
  RefreshResponse,
} from "../../models/user.model"
import { GlobalResponse } from '../../models/api-response.model';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private router = inject(Router)
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/api/auth`
  private userApi = `${environment.apiUrl}/api/user`

  private currentUserSignal = signal<User | null>(null)
  private tokenSignal = signal<string | null>(null)

  currentUser = this.currentUserSignal.asReadonly()
  token = this.tokenSignal.asReadonly()

  constructor() {
    this.loadUserFromStorage()
  }

  private loadUserFromStorage(): void {
    const token = localStorage.getItem("access_token")
    const userJson = localStorage.getItem("current_user")

    if (token && userJson) {
      this.tokenSignal.set(token)
      this.currentUserSignal.set(JSON.parse(userJson))
    }
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    // Mock login
    // return of({
    //   user: {
    //     userId: "1",
    //     email: request.email,
    //     firstName: "John",
    //     lastName: "Doe",
    //     dateOfBirth: "1990-01-15",
    //     avatar: "/diverse-user-avatars.png",
    //     createdAt: new Date().toISOString(),
    //     updatedAt: new Date().toISOString(),
    //   },
    //   accessToken: "mock-jwt-token-" + Date.now(),
    // }).pipe(
    //   delay(1000),
    //   // Simulate error for demo
    //   // throwError(() => new Error('Invalid credentials'))
    // )

    const form = new FormData();
    form.append('email', request.email);
    form.append('password', request.password);

    return this.http.post<GlobalResponse<AuthResponse>>(
      `${this.apiUrl}/login`,
      form,
    ).pipe(map(res => res.data));
  }

  register(request: RegisterRequest): Observable<string> {
    // return of({
    //   user: {
    //     userId: "1",
    //     email: request.email,
    //     firstName: null,
    //     lastName: null,
    //     dateOfBirth: null,
    //     avatar: null,
    //     createdAt: new Date().toISOString(),
    //     updatedAt: new Date().toISOString(),
    //   },
    //   accessToken: "mock-jwt-token-" + Date.now(),
    // }).pipe(delay(1000))

    const form = new FormData();
    form.append('email', request.email);
    form.append('password', request.password);

    return this.http.post<GlobalResponse<string>>(
      `${this.apiUrl}/register`,
      form,
    ).pipe(map(res => res.data));
  }

  loginWithProvider(provider: 'google' | 'facebook', returnPath = '/auth/sso/success') {
    const returnUrl = `${window.location.origin}${returnPath}`;
    window.location.href = `${this.apiUrl}/external/${provider}/start?returnUrl=${encodeURIComponent(returnUrl)}`;
  }

  storeAccessTokenFromFragment(hash: string): boolean {
    if (!hash) return false;
    if (!hash.startsWith('#')) hash = '#' + hash;
    const params = new URLSearchParams(hash.substring(1));

    const authPayload = params.get('auth');
    if (authPayload) {
      try {
        const json = JSON.parse(
          decodeURIComponent(escape(atob(authPayload.replace(/-/g, '+').replace(/_/g, '/'))))
        );

        const token = json.access_token as string | undefined;

        if (!token) return false;

        this.tokenSignal.set(token);
        this.currentUserSignal.set(json.user);
        localStorage.setItem("access_token", token)
        localStorage.setItem("current_user", JSON.stringify(json.user))
        return true;
      } catch {
        return false;
      }
    }
    return false;
  }

  setAuthData(response: AuthResponse): void {
    this.tokenSignal.set(response.accessToken)
    this.currentUserSignal.set(response.user)
    localStorage.setItem("access_token", response.accessToken)
    localStorage.setItem("current_user", JSON.stringify(response.user))
  }

  logout(): Observable<void> {
    this.tokenSignal.set(null)
    this.currentUserSignal.set(null)
    localStorage.removeItem("access_token")
    localStorage.removeItem("current_user")

    return this.http.post<void>(
      `${this.apiUrl}/logout`, {}
    ).pipe(
      tap(() => {
        this.router.navigate(["/"])
      })
    );
  }

  updateProfile(request: UpdateProfileRequest): Observable<User> {
    const currentUser = this.currentUserSignal()
    if (!currentUser) {
      return throwError(() => new Error("Not authenticated"))
    }

    const form = new FormData();
    if (request.firstName !== undefined && request.firstName !== null) {
      form.append('firstName', request.firstName);
    }
    if (request.lastName !== undefined && request.lastName !== null) {
      form.append('lastName', request.lastName);
    }
    if (request.dateOfBirth !== undefined && request.dateOfBirth !== null) {
      form.append('dateOfBirth', request.dateOfBirth); // dạng 'YYYY-MM-DD'
    }
    if (request.avatar) {
      form.append('avatar', request.avatar);
    }

    // const updatedUser: User = {
    //   ...currentUser,
    //   firstName: request.firstName,
    //   lastName: request.lastName,
    //   dateOfBirth: request.dateOfBirth,
    //   avatar: request.avatar ? URL.createObjectURL(request.avatar) : currentUser.avatar,
    //   updatedAt: new Date().toISOString(),
    // }

    // return of(updatedUser).pipe(delay(1000))

    return this.http.put<GlobalResponse<User>>(
      `${this.userApi}/profile/${currentUser.userId}`,
      form
    ).pipe(
      map(res => res.data),
      tap((updatedUser) => this.updateUserData(updatedUser))
    );
  }

  updateUserData(user: User): void {
    this.currentUserSignal.set(user)
    localStorage.setItem("current_user", JSON.stringify(user))
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    if (request.newPassword.length < 8) {
      return throwError(() => new Error("Password must be at least 8 characters"))
    }

    return of(void 0).pipe(delay(1000))
  }

  isAuthenticated(): boolean {
    return this.tokenSignal() !== null
  }

  refreshToken(): Observable<string> {
    return this.http.post<GlobalResponse<RefreshResponse>>(
      `${this.apiUrl}/refresh-token`,
      {}, // server đọc refresh token từ cookie HttpOnly "rt"
      { withCredentials: true }
    ).pipe(
      tap(res => {
        const newToken = res.data?.accessToken;
        if (!newToken) {
          throw new Error('No access token in refresh response');
        }
        this.tokenSignal.set(newToken);
        localStorage.setItem("access_token", newToken);
      }),
      map(res => res.data!.accessToken)
    );
  }

  clearSession() {
    this.tokenSignal.set(null);
    this.currentUserSignal.set(null);
  }

  fetchMe(): Observable<User> {
    return this.http.get<GlobalResponse<User>>(
      `${this.apiUrl}/me`,
      { withCredentials: true }
    ).pipe(
      map(res => res.data as User),
      tap(user => this.currentUserSignal.set(user))
    );
  }
}
