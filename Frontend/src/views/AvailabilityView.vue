<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { ArrowRight, CalendarDays, Clock, MapPin, RefreshCw, Zap } from 'lucide-vue-next'
import { api } from '../api'
import { money, resourceTypes, todayIso, typeInfo } from '../lib/format'
import TypeIcon from '../components/TypeIcon.vue'

const day = ref(todayIso())
const resources = ref([])
const loading = ref(true)
const error = ref('')
const filter = ref('all')

const filters = computed(() => [
  { key: 'all', label: 'Alles', emoji: '✨' },
  ...Object.entries(resourceTypes).map(([key, t]) => ({ key, label: t.plural, emoji: null })),
])

const visible = computed(() =>
  filter.value === 'all' ? resources.value : resources.value.filter((r) => r.type === filter.value),
)

const freeCount = computed(() => resources.value.filter((r) => r.available).length)

async function load() {
  loading.value = true
  error.value = ''
  try {
    resources.value = await api.resources(day.value)
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

watch(day, load)
onMounted(load)
</script>

<template>
  <section>
    <div class="mx-auto max-w-3xl pt-6 pb-12 text-center">
      <span class="inline-flex items-center gap-2 rounded-full border border-fuchsia-400/30 bg-fuchsia-500/10 px-4 py-1.5 text-sm font-medium text-fuchsia-200">
        <Zap class="h-4 w-4" /> Nieuw: realtime beschikbaarheid
      </span>
      <h1 class="mt-6 text-5xl font-extrabold tracking-tight sm:text-6xl">
        Boek alles op campus.
        <span class="gradient-text block">Instant. 🚀</span>
      </h1>
      <p class="mt-5 text-lg text-slate-400">
        Lokalen, laptops, camera's en 3D-printers. Kies een dag, vind wat vrij is en reserveer in een paar klikken.
      </p>
    </div>

    <div class="glass mb-8 flex flex-col gap-4 p-4 sm:flex-row sm:items-center sm:justify-between">
      <div class="flex items-center gap-3">
        <span class="grid h-11 w-11 place-items-center rounded-xl bg-violet-500/20 text-violet-300">
          <CalendarDays class="h-5 w-5" />
        </span>
        <input v-model="day" type="date" class="input !w-auto !py-2.5" />
        <button class="btn-ghost !p-2.5" :disabled="loading" @click="load" aria-label="Vernieuwen">
          <RefreshCw class="h-4 w-4" :class="{ 'animate-spin': loading }" />
        </button>
      </div>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="f in filters"
          :key="f.key"
          class="rounded-full px-4 py-2 text-sm font-medium transition-all duration-200"
          :class="filter === f.key
            ? 'bg-linear-to-r from-violet-600 to-fuchsia-600 text-white shadow-lg shadow-fuchsia-500/30'
            : 'bg-white/5 text-slate-300 hover:bg-white/10'"
          @click="filter = f.key"
        >
          <span v-if="f.emoji">{{ f.emoji }}</span> {{ f.label }}
        </button>
      </div>
    </div>

    <div v-if="!loading && !error" class="mb-5 text-sm text-slate-400">
      <span class="font-semibold text-emerald-300">{{ freeCount }}</span> van {{ resources.length }} items zijn vrij op deze dag 🎯
    </div>

    <div v-if="error" class="glass p-10 text-center">
      <div class="text-5xl">😵‍💫</div>
      <p class="mt-4 font-semibold">Oeps, de API antwoordt niet.</p>
      <p class="mt-1 text-sm text-slate-400">{{ error }}</p>
      <button class="btn-primary mt-6" @click="load"><RefreshCw class="h-4 w-4" /> Opnieuw proberen</button>
    </div>

    <div v-else class="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
      <template v-if="loading">
        <div v-for="n in 6" :key="n" class="glass h-56 animate-pulse"></div>
      </template>

      <template v-else>
      <article
        v-for="r in visible"
        :key="r.id"
        class="glass group relative overflow-hidden p-6 transition-all duration-300 hover:-translate-y-1 hover:border-fuchsia-400/40 hover:shadow-fuchsia-500/20"
      >
        <div class="absolute -top-16 -right-16 h-40 w-40 rounded-full bg-fuchsia-500/10 blur-2xl transition-opacity duration-300 group-hover:opacity-100 opacity-0"></div>

        <div class="flex items-start justify-between gap-3">
          <TypeIcon :type="r.type" />
          <span
            v-if="r.available"
            class="inline-flex items-center gap-2 rounded-full bg-emerald-500/15 px-3 py-1 text-xs font-semibold text-emerald-300 ring-1 ring-emerald-400/30"
          >
            <span class="relative flex h-2 w-2">
              <span class="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75"></span>
              <span class="relative inline-flex h-2 w-2 rounded-full bg-emerald-400"></span>
            </span>
            Beschikbaar
          </span>
          <span v-else class="rounded-full bg-rose-500/15 px-3 py-1 text-xs font-semibold text-rose-300 ring-1 ring-rose-400/30">
            Deels bezet
          </span>
        </div>

        <h3 class="mt-5 text-lg font-bold">{{ r.name }}</h3>
        <p class="mt-1 flex items-center gap-1.5 text-sm text-slate-400">
          <MapPin class="h-3.5 w-3.5" /> {{ r.location || 'Campus' }} · {{ typeInfo(r.type).label }}
        </p>

        <div v-if="r.busySlots && r.busySlots.length" class="mt-3 flex flex-wrap gap-1.5">
          <span
            v-for="slot in r.busySlots"
            :key="slot"
            class="inline-flex items-center gap-1 rounded-lg bg-white/5 px-2 py-0.5 text-xs text-slate-300"
          >
            <Clock class="h-3 w-3" /> {{ slot }}
          </span>
        </div>

        <div class="mt-6 flex items-end justify-between">
          <div>
            <div class="text-2xl font-extrabold">{{ money(r.hourlyRate) }}<span class="text-sm font-medium text-slate-400"> /uur</span></div>
            <div class="text-xs text-slate-500">{{ money(r.dayRate) }} per dag</div>
          </div>
          <RouterLink
            :to="{ path: '/reserveren', query: { resource: r.id, day } }"
            class="inline-flex items-center gap-1.5 rounded-xl bg-white/10 px-4 py-2 text-sm font-semibold transition-all group-hover:bg-linear-to-r group-hover:from-violet-600 group-hover:to-fuchsia-600 group-hover:shadow-lg group-hover:shadow-fuchsia-500/30"
          >
            Reserveer <ArrowRight class="h-4 w-4 transition-transform group-hover:translate-x-0.5" />
          </RouterLink>
        </div>
      </article>
      </template>
    </div>
  </section>
</template>
