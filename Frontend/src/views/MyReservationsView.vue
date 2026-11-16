<script setup>
import { computed, ref, watch } from 'vue'
import { Ban, CalendarX, Check, CreditCard, Download, PackageCheck, PackageOpen, RefreshCw } from 'lucide-vue-next'
import { api } from '../api'
import { dateTime, money } from '../lib/format'
import { useUser } from '../composables/useUser'
import { useToast } from '../composables/useToast'
import StatusBadge from '../components/StatusBadge.vue'
import TypeIcon from '../components/TypeIcon.vue'

const { userId, currentUser } = useUser()
const toast = useToast()

const items = ref([])
const loading = ref(false)
const busy = ref('')

const actions = [
  { key: 'pay', label: 'Betaal', icon: CreditCard, done: 'Betaling gelukt 💸' },
  { key: 'confirm', label: 'Bevestig', icon: Check, done: 'Bevestigd ✅' },
  { key: 'checkout', label: 'Afhalen', icon: PackageOpen, done: 'Veel plezier ermee 📦' },
  { key: 'return', label: 'Terugbrengen', icon: PackageCheck, done: 'Bedankt om terug te brengen 🙌' },
  { key: 'cancel', label: 'Annuleer', icon: Ban, done: 'Reservatie geannuleerd' },
]

const summary = computed(() => [
  { label: 'Totaal', value: items.value.length, emoji: '📋' },
  {
    label: 'Lopend',
    value: items.value.filter((r) => ['Pending', 'Confirmed', 'CheckedOut'].includes(r.status)).length,
    emoji: '⚡',
  },
  { label: 'Onbetaald', value: items.value.filter((r) => !r.paid && r.status !== 'Cancelled').length, emoji: '💳' },
])

const exportUrl = computed(() => api.exportUrl(userId.value))

async function load() {
  if (!userId.value) return
  loading.value = true
  try {
    items.value = await api.reservations(userId.value)
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

async function run(item, action) {
  busy.value = `${item.id}-${action.key}`
  try {
    await api.action(item.id, action.key)
    toast.success(`#${item.id}: ${action.done}`)
    await load()
  } catch (e) {
    toast.error(`#${item.id}: ${e.message}`)
  } finally {
    busy.value = ''
  }
}

watch(userId, load, { immediate: true })
</script>

<template>
  <section>
    <div class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <h1 class="text-4xl font-extrabold tracking-tight">
          Mijn <span class="gradient-text">reservaties</span> 📚
        </h1>
        <p class="mt-2 text-slate-400">Alles van {{ currentUser?.name || '...' }} op een rij.</p>
      </div>
      <div class="flex gap-2">
        <button class="btn-ghost !px-4 !py-2.5" :disabled="loading" @click="load">
          <RefreshCw class="h-4 w-4" :class="{ 'animate-spin': loading }" /> Vernieuwen
        </button>
        <a :href="exportUrl" class="btn-primary !py-2.5" download>
          <Download class="h-4 w-4" /> Exporteer CSV
        </a>
      </div>
    </div>

    <div class="mt-8 grid grid-cols-3 gap-4">
      <div v-for="s in summary" :key="s.label" class="glass p-5">
        <div class="text-2xl">{{ s.emoji }}</div>
        <div class="mt-2 text-3xl font-extrabold">{{ s.value }}</div>
        <div class="text-sm text-slate-400">{{ s.label }}</div>
      </div>
    </div>

    <div v-if="loading && !items.length" class="mt-8 space-y-4">
      <div v-for="n in 3" :key="n" class="glass h-36 animate-pulse"></div>
    </div>

    <div v-else-if="!items.length" class="glass mt-8 p-12 text-center">
      <CalendarX class="mx-auto h-12 w-12 text-fuchsia-300" />
      <p class="mt-4 text-lg font-semibold">Nog geen reservaties 👀</p>
      <p class="mt-1 text-slate-400">Tijd om iets leuks te boeken!</p>
      <RouterLink to="/" class="btn-primary mt-6">Ontdek wat vrij is ✨</RouterLink>
    </div>

    <TransitionGroup v-else tag="div" class="mt-8 space-y-4" move-class="transition duration-300">
      <article
        v-for="r in items"
        :key="r.id"
        class="glass p-5 transition-all duration-300 hover:border-fuchsia-400/30 sm:p-6"
      >
        <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div class="flex items-center gap-4">
            <TypeIcon :type="r.resourceType || 'other'" />
            <div>
              <div class="flex flex-wrap items-center gap-2">
                <span class="text-lg font-bold">{{ r.resourceName }}</span>
                <span class="text-sm text-slate-500">#{{ r.id }}</span>
                <StatusBadge :status="r.status" />
              </div>
              <div class="mt-1 text-sm text-slate-400">
                {{ dateTime(r.start) }} → {{ dateTime(r.end) }}
                <span v-if="r.extras" class="text-slate-500"> · {{ r.extras }}</span>
              </div>
            </div>
          </div>
          <div class="flex items-center gap-6 md:text-right">
            <div>
              <div class="text-xl font-extrabold">{{ money(r.price) }}</div>
              <div v-if="r.deposit > 0" class="text-xs text-slate-400">+ {{ money(r.deposit) }} waarborg</div>
            </div>
            <span
              class="rounded-full px-3 py-1 text-xs font-semibold"
              :class="r.paid ? 'bg-emerald-500/15 text-emerald-300' : 'bg-amber-500/15 text-amber-300'"
            >
              {{ r.paid ? 'Betaald 💚' : 'Open' }}
            </span>
          </div>
        </div>

        <div class="mt-5 flex flex-wrap gap-2 border-t border-white/5 pt-4">
          <button
            v-for="a in actions"
            :key="a.key"
            class="btn-ghost"
            :disabled="busy !== ''"
            @click="run(r, a)"
          >
            <RefreshCw v-if="busy === `${r.id}-${a.key}`" class="h-3.5 w-3.5 animate-spin" />
            <component :is="a.icon" v-else class="h-3.5 w-3.5" />
            {{ a.label }}
          </button>
        </div>
      </article>
    </TransitionGroup>
  </section>
</template>
