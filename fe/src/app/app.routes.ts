import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: "",
        loadComponent: () => import("./pages/home/home.component").then((m) => m.HomeComponent),
        title: "NouFlix - Xem Phim Online Chất Lượng Cao",
      },
      {
        path: "movie/:slug",
        loadComponent: () => import("./pages/movie-detail/movie-detail.component").then((m) => m.MovieDetailComponent),
        title: "Chi Tiết Phim - NouFlix",
      },
      {
        path: "watch/:slug",
        loadComponent: () => import("./pages/watch/watch.component").then((m) => m.WatchComponent),
        title: "Xem Phim - NouFlix",
      },
      {
        path: "search",
        loadComponent: () => import("./pages/search/search.component").then((m) => m.SearchComponent),
        title: "Tìm Kiếm - NouFlix",
      },
      {
        path: "genre/:genre",
        loadComponent: () => import("./pages/genre/genre.component").then((m) => m.GenreComponent),
        title: "Thể Loại - NouFlix",
      },
      {
        path: "login",
        loadComponent: () => import("./pages/auth/login/login.component").then((m) => m.LoginComponent),
        title: "Đăng Nhập - NouFlix",
      },
      {
        path: "register",
        loadComponent: () => import("./pages/auth/register/register.component").then((m) => m.RegisterComponent),
        title: "Đăng Ký - NouFlix",
      },
      {
        path: "auth/sso/success",
        loadComponent: () => import("./pages/auth/sso/sso-success.component").then(m => m.SsoSuccessComponent)
      },
      {
        path: "profile",
        loadComponent: () => import("./pages/profile/profile.component").then((m) => m.ProfileComponent),
        title: "Thông Tin Cá Nhân - NouFlix",
      },
      {
        path: "not-found",
        loadComponent: () => import("./pages/not-found/not-found.component").then((m) => m.NotFoundComponent),
        title: "404 - Trang Không Tồn Tại",
      }
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    children: [
      {
        path: "dashboard",
        loadComponent: () => import("./pages/admin/dashboard/dashboard.component").then((m) => m.DashboardComponent),
        title: "Dashboard - NouFlix",
      },
      {
        path: "movies",
        loadComponent: () => import("./pages/admin/movies/movies.component").then((m) => m.MoviesListComponent),
        title: "Danh sách phim",
      },
      {
        path: "genres",
        loadComponent: () => import("./pages/admin/genres/genres.component").then((m) => m.GenresListComponent),
      },
      {
        path: "studios",
        loadComponent: () => import("./pages/admin/studios/studios.component").then((m) => m.StudiosListComponent),
      },
      {
        path: "users",
        loadComponent: () => import("./pages/admin/users/users.component").then((m) => m.UsersListComponent),
      },
      {
        path: "orders",
        loadComponent: () => import("./pages/admin/orders/orders.component").then((m) => m.OrdersListComponent),
      },
      // {
      //   path: "media-library",
      //   loadChildren: () => import("./features/media-library/media-library.module").then((m) => m.MediaLibraryModule),
      // },
      {
        path: "settings",
        loadComponent: () => import("./pages/admin/settings/settings.component").then((m) => m.SettingsComponent),
      },
      {
        path: "audit-logs",
        loadComponent: () => import("./pages/admin/audit-logs/audit-logs.component").then((m) => m.AuditLogsComponent),
      },
      { path: "", redirectTo: "dashboard", pathMatch: "full" },
    ]
  },
  { path: '**', redirectTo: '/not-found' },
];
