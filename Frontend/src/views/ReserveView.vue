<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowRight, CalendarClock, Loader, MessageSquare, Rocket } from 'lucide-vue-next'
import { api } from '../api'
import { money, todayIso, typeInfo } from '../lib/format'
import { useUser } from '../composables/useUser'
import { useToast } from '../composables/useToast'
import TypeIcon from '../components/TypeIcon.vue'

const route = useRoute()
const { userId, currentUser } = useUser()
const toast = useToast()

const day = typeof route.query.day === 'string' ? route.query.day : todayIso()

const resources = ref([])
const submitting = ref(false)
const created = ref(null)

const form = reactive({
  resourceId: route.query.resource ? Number(route.query.resource) : null,
  start: `${day}T10:00`,
  end: `${day}T12:00`,
  insurance: false,
  delivery: false,
  technician: false,
  cleaning: false,
  extendedReturn: false,
  notes: '',
})

const extras = [
  { key: 'insurance', emoji: '🛡️', label: 'Verzekering', hint: 'Zorgeloos bij schade' },
  { key: 'delivery', emoji: '🚚', label: 'Levering', hint: 'Tot aan je lokaal' },
  { key: 'technician', emoji: '🧑‍🔧', label: 'Technieker', hint: 'Hulp bij opstelling' },
  { key: 'cleaning', emoji: '🧽', label: 'Schoonmaak', hint: 'Wij ruimen op' },
  { key: 'extendedReturn', emoji: '⏰', label: 'Verlengde terugbreng', hint: 'Extra tijd om terug te brengen' },
]

const selected = computed(() => resources.value.find((r) => r.id === form.resourceId) || null)

const steps = [
  { emoji: '🔎', title: 'Kies', text: 'Selecteer wat je nodig hebt en wanneer.' },
  { emoji: '💳', title: 'Betaal', text: 'Betaal veilig via onze betaalpartner.' },
  { emoji: '✅', title: 'Bevestig', text: 'Bevestig en haal op aan de balie.' },
]

onMounted(async () => {
  try {
    resources.value = await api.resources(day)
    if (!form.resourceId && resources.value.length) form.resourceId = resources.value[0].id
  } catch (e) {
    toast.error(e.message)
  }
})

async function submit() {
  if (!userId.value) {
    toast.error('Kies eerst wie je bent (rechtsboven) 👆')
    return
  }
  submitting.value = true
  created.value = null
  try {
    const result = await api.createReservation({ ...form, userId: userId.value })
    created.value = result
    toast.success(`Reservatie #${result.id} is aangemaakt! 🎉`)
  } catch (e) {
    toast.error(e.message)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <section class="grid gap-8 lg:grid-cols-5">
    <div class="lg:col-span-3">
      <h1 class="text-4xl font-extrabold tracking-tight">
        Nieuwe <span class="gradient-text">reservatie</span> ✨
      </h1>
      <p class="mt-2 text-slate-400">
        Reserveren als <span class="font-semibold text-slate-200">{{ currentUser?.name || '...' }}</span>
      </p>

      <form class="glass mt-8 space-y-6 p-6 sm:p-8" @submit.prevent="submit">
        <div>
          <label class="label">Wat wil je reserveren?</label>
          <select v-model.number="form.resourceId" class="input" required>
            <option v-for="r in resources" :key="r.id" :value="r.id" class="bg-slate-900">
              {{ r.name }} · {{ typeInfo(r.type).label }}
            </option>
          </select>
        </div>

        <div class="grid gap-4 sm:grid-cols-2">
          <div>
            <label class="label">Van</label>
            <input v-model="form.start" type="datetime-local" class="input" required />
          </div>
          <div>
            <label class="label">Tot</label>
            <input v-model="form.end" type="datetime-local" class="input" required />
          </div>
        </div>

        <div>
          <label class="label">Extra's</label>
          <div class="grid gap-3 sm:grid-cols-2">
            <label
              v-for="x in extras"
              :key="x.key"
              class="flex cursor-pointer items-center gap-3 rounded-2xl border p-4 transition-all duration-200"
              :class="form[x.key]
                ? 'border-fuchsia-400/60 bg-fuchsia-500/10 shadow-lg shadow-fuchsia-500/10'
                : 'border-white/10 bg-white/[0.03] hover:border-white/20'"
            >
              <input v-model="form[x.key]" type="checkbox" class="sr-only" />
              <span class="text-2xl">{{ x.emoji }}</span>
              <span class="flex-1">
                <span class="block text-sm font-semibold">{{ x.label }}</span>
                <span class="block text-xs text-slate-400">{{ x.hint }}</span>
              </span>
              <span
                class="grid h-5 w-5 place-items-center rounded-md border text-xs transition"
                :class="form[x.key] ? 'border-fuchsia-400 bg-fuchsia-500 text-white' : 'border-white/20'"
              >
                <span v-if="form[x.key]">✓</span>
              </span>
            </label>
          </div>
        </div>

        <div>
          <label class="label flex items-center gap-1.5"><MessageSquare class="h-4 w-4" /> Opmerking (optioneel)</label>
          <textarea v-model="form.notes" rows="3" class="input resize-none" placeholder="Bv. 'voor de kerstquiz van de kring' 🎄"></textarea>
        </div>

        <button type="submit" class="btn-primary w-full !py-4 text-lg" :disabled="submitting || !form.resourceId">
          <Loader v-if="submitting" class="h-5 w-5 animate-spin" />
          <Rocket v-else class="h-5 w-5" />
          {{ submitting ? 'Bezig...' : 'Reserveer nu' }}
        </button>
      </form>
    </div>

    <aside class="space-y-6 lg:col-span-2 lg:pt-24">
      <div v-if="selected" class="glass p-6">
        <div class="flex items-center gap-4">
          <TypeIcon :type="selected.type" size="lg" />
          <div>
            <div class="text-lg font-bold">{{ selected.name }}</div>
            <div class="text-sm text-slate-400">{{ selected.location }}</div>
          </div>
        </div>
        <div class="mt-5 grid grid-cols-2 gap-3 text-center">
          <div class="rounded-2xl bg-white/5 p-3">
            <div class="text-xl font-bold">{{ money(selected.hourlyRate) }}</div>
            <div class="text-xs text-slate-400">per uur</div>
          </div>
          <div class="rounded-2xl bg-white/5 p-3">
            <div class="text-xl font-bold">{{ money(selected.dayRate) }}</div>
            <div class="text-xs text-slate-400">per dag</div>
          </div>
        </div>
        <p class="mt-4 flex items-center gap-2 text-xs text-slate-500">
          <CalendarClock class="h-4 w-4" /> De definitieve prijs zie je na het reserveren.
        </p>
      </div>

      <Transition
        enter-active-class="transition duration-500 ease-out"
        enter-from-class="opacity-0 scale-90"
      >
        <div v-if="created" class="glass relative overflow-hidden p-6 ring-1 ring-emerald-400/40">
          <div class="absolute -top-10 -right-10 h-32 w-32 rounded-full bg-emerald-500/20 blur-2xl"></div>
          <div class="text-4xl">🎉</div>
          <div class="mt-3 text-xl font-bold">Reservatie #{{ created.id }} staat klaar!</div>
          <div class="mt-4 space-y-2 text-sm">
            <div class="flex justify-between"><span class="text-slate-400">Prijs</span><span class="font-semibold">{{ money(created.price) }}</span></div>
            <div v-if="created.deposit > 0" class="flex justify-between"><span class="text-slate-400">Waarborg</span><span class="font-semibold">{{ money(created.deposit) }}</span></div>
            <div v-if="created.extras" class="flex justify-between gap-4"><span class="text-slate-400">Extra's</span><span class="text-right">{{ created.extras }}</span></div>
          </div>
          <RouterLink to="/mijn-reservaties" class="btn-primary mt-6 w-full">
            Naar mijn reservaties <ArrowRight class="h-4 w-4" />
          </RouterLink>
        </div>
      </Transition>

      <div class="glass p-6">
        <div class="mb-4 font-semibold">Zo simpel werkt het</div>
        <ol class="space-y-4">
          <li v-for="(s, i) in steps" :key="s.title" class="flex gap-3">
            <span class="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-white/5 text-xl">{{ s.emoji }}</span>
            <div>
              <div class="text-sm font-semibold">{{ i + 1 }}. {{ s.title }}</div>
              <div class="text-sm text-slate-400">{{ s.text }}</div>
            </div>
          </li>
        </ol>
      </div>
    </aside>
  </section>
</template>
