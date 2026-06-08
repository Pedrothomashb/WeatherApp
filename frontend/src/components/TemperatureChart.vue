<template>
  <div class="chart-container">
    <canvas ref="chartCanvas" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import {
  Chart,
  LineElement, PointElement, LineController,
  CategoryScale, LinearScale,
  Tooltip, Filler
} from 'chart.js'
import type { TemperatureResponse } from '../types/weather'

Chart.register(LineElement, PointElement, LineController, CategoryScale, LinearScale, Tooltip, Filler)

const props = defineProps<{ records: TemperatureResponse[] }>()
const chartCanvas = ref<HTMLCanvasElement | null>(null)
let chartInstance: Chart | null = null

function buildChart() {
  if (!chartCanvas.value) return

  const sorted = [...props.records].reverse()
  const labels = sorted.map(r =>
    new Date(r.recordedAt).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })
  )
  const temps = sorted.map(r => r.temperature)

  if (chartInstance) chartInstance.destroy()

  chartInstance = new Chart(chartCanvas.value, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: 'Temperatura (°C)',
        data: temps,
        borderColor: '#4f8ef7',
        backgroundColor: 'rgba(79,142,247,0.12)',
        borderWidth: 2,
        pointRadius: 4,
        pointHoverRadius: 6,
        pointBackgroundColor: '#4f8ef7',
        tension: 0.4,
        fill: true
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: '#22263a',
          titleColor: '#e8eaf0',
          bodyColor: '#8b90a8',
          borderColor: 'rgba(255,255,255,0.08)',
          borderWidth: 1,
          callbacks: {
            label: ctx => ` ${ctx.parsed.y.toFixed(1)}°C`
          }
        }
      },
      scales: {
        x: {
          ticks: { color: '#8b90a8', font: { size: 11 }, maxRotation: 45 },
          grid: { color: 'rgba(255,255,255,0.05)' }
        },
        y: {
          ticks: { color: '#8b90a8', callback: v => `${v}°` },
          grid: { color: 'rgba(255,255,255,0.05)' }
        }
      }
    }
  })
}

onMounted(buildChart)
watch(() => props.records, buildChart, { deep: true })
</script>

<style scoped>
.chart-container { height: 220px; position: relative; }
</style>
