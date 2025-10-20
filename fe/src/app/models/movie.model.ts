export interface Movie extends MovieItem {
  alternateTitle: string
  overview: string
  backdropUrl: string
  runtime: number
  avgRating: number
  voteCount: number
  popularity: number
  // productionCountries: ProductionCountry[]
  // spokenLanguages: SpokenLanguage[]
  // budget: number
  // revenue: number
  status: PublishStatus
  tagline: string
  adult: boolean
  video: boolean
  type: MovieType
  episodes?: Episode[]
  numberOfSeasons?: number
  numberOfEpisodes?: number
}

export interface MovieItem {
  id: number
  slug: string
  title: string
  posterUrl: string
  avgRating: number
  releaseDate: string
  genres: Genre[]
}

export interface Genre {
  id: number
  name: string
}

export interface ProductionCountry {
  iso_3166_1: string
  name: string
}

export interface SpokenLanguage {
  iso_639_1: string
  name: string
}

export interface Cast {
  id: number
  name: string
  character: string
  profilePath: string
  order: number
}

export interface Crew {
  id: number
  name: string
  job: string
  department: string
  profilePath: string
}

export interface Review {
  id: string
  author: string
  authorDetails: {
    name: string
    username: string
    avatarUrl: string
    rating: number
  }
  content: string
  createdAt: string
  updatedAt: string
}

export interface Video {
  id: string
  key: string
  name: string
  site: string
  type: string
  official: boolean
  publishedAt: string
}

export interface Season {
  id: number
  number: number
  title: string
  year: string
  episodeCount: number
  episodes: Episode[]
}

export interface Episode {
  id: number
  number: number
  seasonNumber: number
  title: string
  overview: string
  releaseDate: string
  runtime: number
}

export enum MovieType {
  Single = 1,
  Series
}

export enum QualityLevel {
  Low = 1,
  Medium,
  High
}

export enum PublishStatus {
  Draft = 1,
  Published,
  Hidden,
  InReview
}
