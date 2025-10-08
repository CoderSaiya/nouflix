export interface GlobalResponse<T> {
    data: T
    message: string
    statusCode: number
    isSuccess: boolean
}

export interface ApiError {
    message: string
    statusCode: number
}

