import { Routes } from '@angular/router';

export const routes: Routes = [
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
    loadComponent: () => import("./pages/login/login.component").then((m) => m.LoginComponent),
    title: "Đăng Nhập - NouFlix",
  },
  {
    path: "register",
    loadComponent: () => import("./pages/register/register.component").then((m) => m.RegisterComponent),
    title: "Đăng Ký - NouFlix",
  },
  {
    path: "profile",
    loadComponent: () => import("./pages/profile/profile.component").then((m) => m.ProfileComponent),
    title: "Thông Tin Cá Nhân - NouFlix",
  },
  {
    path: "**",
    redirectTo: "",
  },
];
