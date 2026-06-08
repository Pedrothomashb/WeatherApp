export interface TemperatureResponse {
  id: string
  cityName: string
  temperature: number
  feelsLike: number | null
  humidity: number | null
  description: string | null
  provider: string | null
  recordedAt: string
}

export interface HistoryResponse {
  cityName: string
  latitude: number | null
  longitude: number | null
  records: TemperatureResponse[]
}

export interface RegisterByCityRequest {
  cityName: string
}

export interface RegisterByCoordinatesRequest {
  latitude: number
  longitude: number
}

export interface ApiError {
  error: string
}
