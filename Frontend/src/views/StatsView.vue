<script setup>
import { computed, onMounted, ref } from 'vue'
import { CalendarCheck, Euro, RefreshCw, TrendingUp } from 'lucide-vue-next'
import { api } from '../api'
import { money, statusInfo, typeInfo } from '../lib/format'

const stats = ref(null)
const loading = ref(true)
const error = ref('')

async function load() {
  loading.value = true
  error.value = ''
  try {
    stats.value = await api.stats()
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(load)

const cards = computed(() => {
  if (!stats.value) return []
  const confirmations = Object.values(stats.value.confirmationsSinceStartup || {}).reduce((a, b) => a + b, 0)
  return [
    { label: 'Reservaties deze maand', value: stats.value.reservationsThisMonth, icon: CalendarCheck, gradient: 'from-violet-500 to-indigo-500' },
    { label: 'Omzet deze maand', value: money(stats.value.revenueThisMonth), icon: Euro, gradient: 'from-fuchsia-500 to-pink-500' },
    { label: 'Bevestigd sinds opstart', value: confirmations, icon: TrendingUp, gradient: 'from-purple-500 to-fuchsia-500' },
    { label: 'Bedrag sinds opstart', value: money(stats.value.confirmedAmountSinceStartup), icon: Euro, gradient: 'from-indigo-500 to-cyan-500' },
  ]
})

const statusRows = computed(() => {
  const byStatus = stats.value?.byStatus || {}
  const max = Math.max(1, ...Object.values(byStatus))
  return Object.entries(byStatus)
    .sort((a, b) => b[1] - a[1])
    .map(([status, count]) => ({ status, count, info: statusInfo(status), pct: Math.round((count / max) * 100) }))
})

const typeRows = computed(() =>
  Object.entries(stats.value?.confirmationsSinceStartup || {}).map(([type, count]) => ({ type, count, info: typeInfo(type) })),
)
</script>

<template>
  <section>
    <div class="flex items-end justify-between gap-4">
      <div>
        <h1 class="text-4xl font-extrabold tracking-tight">
          <span class="gradient-text">Dashboard</span> 📊
        </h1>
        <p class="mt-2 text-slate-400">Live inzicht in alles wat er op de campus gereserveerd wordt.</p>
      </div>
      <button class="btn-ghost !px-4 !py-2.5" :disabled="loading" @click="load">
        <RefreshCw class="h-4 w-4" :class="{ 'animate-spin': loading }" /> Vernieuwen
      </button>
    </div>

    <div v-if="error" class="glass mt-8 p-10 text-center">
      <div class="text-5xl">📉</div>
      <p class="mt-4 font-semibold">Stats konden niet geladen worden.</p>
      <p class="mt-1 text-sm text-slate-400">{{ error }}</p>
    </div>

    <template v-else>
      <div class="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <template v-if="loading && !stats">
          <div v-for="n in 4" :key="n" class="glass h-36 animate-pulse"></div>
        </template>
        <div v-for="c in cards" :key="c.label" class="glass group relative overflow-hidden p-6">
          <div class="absolute -right-8 -bottom-8 h-28 w-28 rounded-full bg-linear-to-br opacity-20 blur-2xl transition-opacity group-hover:opacity-40" :class="c.gradient"></div>
          <span class="grid h-11 w-11 place-items-center rounded-xl bg-linear-to-br shadow-lg shadow-fuchsia-500/20" :class="c.gradient">
            <component :is="c.icon" class="h-5 w-5 text-white" />
          </span>
          <div class="mt-4 text-3xl font-extrabold">{{ c.value }}</div>
          <div class="text-sm text-slate-400">{{ c.label }}</div>
        </div>
      </div>

      <div v-if="stats" class="mt-8 grid gap-6 lg:grid-cols-2">
        <div class="glass p-6">
          <div class="mb-5 font-semibold">Reservaties per status</div>
          <div class="space-y-4">
            <div v-for="row in statusRows" :key="row.status">
              <div class="mb-1.5 flex justify-between text-sm">
                <span>{{ row.info.emoji }} {{ row.info.label }}</span>
                <span class="font-semibold">{{ row.count }}</span>
              </div>
              <div class="h-2.5 overflow-hidden rounded-full bg-white/5">
                <div
                  class="h-full rounded-full bg-linear-to-r from-violet-500 to-fuchsia-500 transition-all duration-700"
                  :style="{ width: row.pct + '%' }"
                ></div>
              </div>
            </div>
          </div>
        </div>

        <div class="glass p-6">
          <div class="mb-5 font-semibold">Bevestigingen sinds opstart</div>
          <div v-if="!typeRows.length" class="py-8 text-center text-slate-400">
            Nog niets bevestigd sinds de server opstartte 😴
          </div>
          <div v-else class="grid grid-cols-2 gap-3">
            <div v-for="row in typeRows" :key="row.type" class="flex items-center gap-3 rounded-2xl bg-white/5 p-4">
              <span class="grid h-10 w-10 place-items-center rounded-xl bg-linear-to-br" :class="row.info.gradient">
                <component :is="row.info.icon" class="h-5 w-5 text-white" />
              </span>
              <div>
                <div class="text-xl font-bold">{{ row.count }}</div>
                <div class="text-xs text-slate-400">{{ row.info.plural }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </section>
</template>
