import { defineStore } from 'pinia'
import { ref } from 'vue'
import { weatherApi } from '../services/weatherApi'
import type { TemperatureResponse, HistoryResponse } from '../types/weather'

export const useWeatherStore = defineStore('weather', () => {
  const lastReading = ref<TemperatureResponse | null>(null)
  const history = ref<HistoryResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function registerByCity(cityName: string) {
    loading.value = true
    error.value = null
    try {
      lastReading.value = await weatherApi.registerByCity(cityName)
      await fetchHistory(cityName)
    } catch (e: any) {
      error.value = e?.response?.data?.error ?? 'Erro ao consultar temperatura.'
    } finally {
      loading.value = false
    }
  }

  async function registerByCoordinates(lat: number, lon: number) {
    loading.value = true
    error.value = null
    try {
      lastReading.value = await weatherApi.registerByCoordinates(lat, lon)
      await fetchHistory(undefined, lat, lon)
    } catch (e: any) {
      error.value = e?.response?.data?.error ?? 'Erro ao consultar temperatura.'
    } finally {
      loading.value = false
    }
  }

  async function fetchHistory(city?: string, lat?: number, lon?: number) {
    try {
      history.value = await weatherApi.getHistory(city, lat, lon)
    } catch {
      // silently fail history load
    }
  }

  function clearError() { error.value = null }

  return { lastReading, history, loading, error, registerByCity, registerByCoordinates, fetchHistory, clearError }
})
