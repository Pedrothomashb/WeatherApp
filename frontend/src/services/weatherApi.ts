import axios from 'axios'
import type { TemperatureResponse, HistoryResponse } from '../types/weather'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
})

export const weatherApi = {
  registerByCity(cityName: string): Promise<TemperatureResponse> {
    return api.post<TemperatureResponse>('/weather/city', { cityName }).then(r => r.data)
  },

  registerByCoordinates(latitude: number, longitude: number): Promise<TemperatureResponse> {
    return api.post<TemperatureResponse>('/weather/coordinates', { latitude, longitude }).then(r => r.data)
  },

  getHistory(city?: string, lat?: number, lon?: number): Promise<HistoryResponse> {
    const params: Record<string, string | number> = {}
    if (city) params.city = city
    if (lat !== undefined) params.lat = lat
    if (lon !== undefined) params.lon = lon
    return api.get<HistoryResponse>('/weather/history', { params }).then(r => r.data)
  }
}
