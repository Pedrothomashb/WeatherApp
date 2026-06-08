<template>
  <div class="app">
    <header class="header">
      <div class="header-inner">
        <div class="logo">
          <span class="logo-icon">⛅</span>
          <span class="logo-text">WeatherApp</span>
        </div>
        <p class="header-sub">Registro e histórico de temperaturas</p>
      </div>
    </header>

    <main class="main">
      <!-- Search Form -->
      <section class="card search-card">
        <h2 class="section-title">Registrar temperatura</h2>

        <div class="tabs">
          <button
            class="tab"
            :class="{ active: activeTab === 'city' }"
            @click="activeTab = 'city'"
          >Por cidade</button>
          <button
            class="tab"
            :class="{ active: activeTab === 'coords' }"
            @click="activeTab = 'coords'"
          >Por coordenadas</button>
        </div>

        <div v-if="activeTab === 'city'" class="form-group">
          <input
            v-model="cityInput"
            class="input"
            type="text"
            placeholder="Ex: Curitiba, São Paulo, Manaus..."
            @keyup.enter="handleCitySubmit"
          />
          <button class="btn-primary" :disabled="store.loading" @click="handleCitySubmit">
            <span v-if="store.loading" class="spinner" />
            <span v-else>Registrar</span>
          </button>
        </div>

        <div v-else class="form-row">
          <div class="form-group-inline">
            <label class="label">Latitude</label>
            <input v-model.number="latInput" class="input" type="number" step="0.0001" placeholder="-25.4284" />
          </div>
          <div class="form-group-inline">
            <label class="label">Longitude</label>
            <input v-model.number="lonInput" class="input" type="number" step="0.0001" placeholder="-49.2733" />
          </div>
          <button class="btn-primary coords-btn" :disabled="store.loading" @click="handleCoordsSubmit">
            <span v-if="store.loading" class="spinner" />
            <span v-else>Registrar</span>
          </button>
        </div>

        <div v-if="store.error" class="error-banner">
          ⚠️ {{ store.error }}
          <button class="error-close" @click="store.clearError">✕</button>
        </div>
      </section>

      <!-- Current reading -->
      <section v-if="store.lastReading" class="card reading-card">
        <div class="reading-header">
          <div>
            <h3 class="reading-city">{{ store.lastReading.cityName }}</h3>
            <p class="reading-time">{{ formatDate(store.lastReading.recordedAt) }}</p>
          </div>
          <div class="temp-badge">
            {{ store.lastReading.temperature.toFixed(1) }}<span class="deg">°C</span>
          </div>
        </div>
        <div class="reading-details">
          <div class="detail" v-if="store.lastReading.feelsLike !== null">
            <span class="detail-label">Sensação</span>
            <span class="detail-value">{{ store.lastReading.feelsLike?.toFixed(1) }}°C</span>
          </div>
          <div class="detail" v-if="store.lastReading.humidity !== null">
            <span class="detail-label">Umidade</span>
            <span class="detail-value">{{ store.lastReading.humidity }}%</span>
          </div>
          <div class="detail" v-if="store.lastReading.description">
            <span class="detail-label">Condição</span>
            <span class="detail-value capitalize">{{ store.lastReading.description }}</span>
          </div>
          <div class="detail">
            <span class="detail-label">Provedor</span>
            <span class="detail-value">{{ store.lastReading.provider }}</span>
          </div>
        </div>
      </section>

      <!-- History -->
      <section v-if="store.history && store.history.records.length > 0" class="card">
        <h2 class="section-title">
          Histórico — {{ store.history.cityName }}
          <span class="badge">{{ store.history.records.length }} registros</span>
        </h2>

        <!-- Chart -->
        <div class="chart-wrapper">
          <TemperatureChart :records="store.history.records" />
        </div>

        <!-- List -->
        <div class="history-list">
          <div v-for="rec in store.history.records" :key="rec.id" class="history-item">
            <div class="history-left">
              <span class="history-temp">{{ rec.temperature.toFixed(1) }}°C</span>
              <span v-if="rec.description" class="history-desc">{{ rec.description }}</span>
            </div>
            <div class="history-right">
              <span class="history-humidity" v-if="rec.humidity">💧 {{ rec.humidity }}%</span>
              <span class="history-date">{{ formatDate(rec.recordedAt) }}</span>
            </div>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useWeatherStore } from './stores/weather'
import TemperatureChart from './components/TemperatureChart.vue'

const store = useWeatherStore()
const activeTab = ref<'city' | 'coords'>('city')
const cityInput = ref('')
const latInput = ref<number | null>(null)
const lonInput = ref<number | null>(null)

async function handleCitySubmit() {
  const city = cityInput.value.trim()
  if (!city) return
  await store.registerByCity(city)
}

async function handleCoordsSubmit() {
  if (latInput.value === null || lonInput.value === null) return
  await store.registerByCoordinates(latInput.value, lonInput.value)
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}
</script>

<style>
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

:root {
  --bg: #0f1117;
  --surface: #1a1d27;
  --surface2: #22263a;
  --border: rgba(255,255,255,0.08);
  --accent: #4f8ef7;
  --accent2: #7c5cbf;
  --text: #e8eaf0;
  --muted: #8b90a8;
  --success: #3dd68c;
  --error: #f7657a;
  --radius: 14px;
  font-family: 'DM Sans', 'Segoe UI', system-ui, sans-serif;
}

body { background: var(--bg); color: var(--text); min-height: 100vh; }

.app { max-width: 760px; margin: 0 auto; padding: 0 20px 60px; }

.header { padding: 48px 0 32px; border-bottom: 1px solid var(--border); margin-bottom: 32px; }
.header-inner { display: flex; align-items: center; gap: 20px; flex-wrap: wrap; }
.logo { display: flex; align-items: center; gap: 10px; }
.logo-icon { font-size: 2rem; }
.logo-text { font-size: 1.5rem; font-weight: 700; letter-spacing: -0.02em; }
.header-sub { color: var(--muted); font-size: 0.9rem; margin-left: auto; }

.main { display: flex; flex-direction: column; gap: 20px; }

.card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 24px; }

.section-title { font-size: 1rem; font-weight: 600; margin-bottom: 18px; display: flex; align-items: center; gap: 10px; }
.badge { background: var(--surface2); color: var(--muted); font-size: 0.75rem; font-weight: 500; padding: 2px 8px; border-radius: 99px; }

.tabs { display: flex; gap: 4px; margin-bottom: 18px; background: var(--bg); border-radius: 8px; padding: 4px; width: fit-content; }
.tab { padding: 7px 18px; border: none; background: transparent; color: var(--muted); border-radius: 6px; cursor: pointer; font-size: 0.875rem; font-weight: 500; transition: all 0.15s; }
.tab.active { background: var(--surface2); color: var(--text); }

.form-group { display: flex; gap: 10px; }
.form-row { display: flex; gap: 10px; align-items: flex-end; flex-wrap: wrap; }
.form-group-inline { display: flex; flex-direction: column; gap: 6px; flex: 1; min-width: 140px; }
.label { font-size: 0.8rem; color: var(--muted); }

.input {
  flex: 1; padding: 11px 14px; background: var(--bg); border: 1px solid var(--border);
  border-radius: 9px; color: var(--text); font-size: 0.9rem; outline: none; transition: border 0.15s;
  min-width: 0;
}
.input:focus { border-color: var(--accent); }
.input::placeholder { color: var(--muted); }

.btn-primary {
  padding: 11px 22px; background: var(--accent); color: #fff; border: none;
  border-radius: 9px; font-size: 0.9rem; font-weight: 600; cursor: pointer;
  transition: opacity 0.15s; white-space: nowrap; display: flex; align-items: center; justify-content: center; min-width: 96px;
}
.btn-primary:hover { opacity: 0.88; }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
.coords-btn { align-self: flex-end; }

.spinner {
  width: 16px; height: 16px; border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff; border-radius: 50%; animation: spin 0.6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.error-banner {
  margin-top: 14px; padding: 10px 14px; background: rgba(247,101,122,0.1);
  border: 1px solid rgba(247,101,122,0.3); border-radius: 8px; color: var(--error);
  font-size: 0.875rem; display: flex; align-items: center; justify-content: space-between;
}
.error-close { background: none; border: none; color: var(--error); cursor: pointer; font-size: 1rem; padding: 0 0 0 12px; }

.reading-card { background: linear-gradient(135deg, #1a2540 0%, #1a1d27 100%); }
.reading-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
.reading-city { font-size: 1.3rem; font-weight: 700; }
.reading-time { color: var(--muted); font-size: 0.8rem; margin-top: 4px; }
.temp-badge { font-size: 2.8rem; font-weight: 800; color: var(--accent); line-height: 1; }
.deg { font-size: 1.5rem; }
.reading-details { display: flex; gap: 24px; flex-wrap: wrap; }
.detail { display: flex; flex-direction: column; gap: 3px; }
.detail-label { font-size: 0.75rem; color: var(--muted); text-transform: uppercase; letter-spacing: 0.05em; }
.detail-value { font-size: 0.95rem; font-weight: 500; }
.capitalize { text-transform: capitalize; }

.chart-wrapper { margin-bottom: 20px; }

.history-list { display: flex; flex-direction: column; gap: 1px; border-radius: 9px; overflow: hidden; }
.history-item {
  display: flex; justify-content: space-between; align-items: center;
  padding: 12px 16px; background: var(--bg); transition: background 0.1s;
}
.history-item:hover { background: var(--surface2); }
.history-left { display: flex; align-items: center; gap: 14px; }
.history-temp { font-weight: 700; font-size: 1.05rem; min-width: 56px; }
.history-desc { color: var(--muted); font-size: 0.85rem; text-transform: capitalize; }
.history-right { display: flex; align-items: center; gap: 14px; }
.history-humidity { color: var(--muted); font-size: 0.85rem; }
.history-date { color: var(--muted); font-size: 0.8rem; }

@media (max-width: 500px) {
  .form-group { flex-direction: column; }
  .btn-primary { width: 100%; }
  .header-inner { flex-direction: column; align-items: flex-start; }
}
</style>
