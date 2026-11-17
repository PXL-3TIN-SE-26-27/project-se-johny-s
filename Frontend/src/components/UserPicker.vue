<script setup>
import { computed } from 'vue'
import { ChevronDown } from 'lucide-vue-next'
import { useUser } from '../composables/useUser'

const { users, userId, currentUser, error } = useUser()

const roleEmoji = { student: '🎓', staff: '🧑‍🏫', association: '🎉' }

const initials = computed(() =>
  (currentUser.value?.name || '?')
    .split(' ')
    .map((p) => p[0])
    .slice(0, 2)
    .join('')
    .toUpperCase(),
)
</script>

<template>
  <div class="flex items-center gap-2.5 rounded-2xl border border-white/10 bg-white/5 py-1.5 pr-2 pl-1.5">
    <span class="grid h-8 w-8 place-items-center rounded-xl bg-linear-to-br from-fuchsia-500 to-violet-600 text-xs font-bold text-white">
      {{ initials }}
    </span>
    <div class="relative">
      <select
        v-model.number="userId"
        class="cursor-pointer appearance-none bg-transparent pr-6 text-sm font-medium text-slate-100 outline-none"
        :title="error || 'Wie ben jij?'"
      >
        <option v-if="!users.length" :value="userId" class="bg-slate-900">{{ error ? 'API offline 😵' : 'Laden...' }}</option>
        <option v-for="u in users" :key="u.id" :value="u.id" class="bg-slate-900">
          {{ roleEmoji[u.role] || '👤' }} {{ u.name }}
        </option>
      </select>
      <ChevronDown class="pointer-events-none absolute top-1/2 right-0 h-4 w-4 -translate-y-1/2 text-slate-400" />
    </div>
  </div>
</template>
