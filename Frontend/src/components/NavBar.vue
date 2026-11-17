<script setup>
import { ref } from 'vue'
import { CalendarPlus, LayoutDashboard, ListChecks, Menu, Search, Sparkles, X } from 'lucide-vue-next'
import UserPicker from './UserPicker.vue'

const open = ref(false)

const links = [
  { to: '/', label: 'Ontdek', icon: Search },
  { to: '/reserveren', label: 'Reserveren', icon: CalendarPlus },
  { to: '/mijn-reservaties', label: 'Mijn reservaties', icon: ListChecks },
  { to: '/stats', label: 'Stats', icon: LayoutDashboard },
]
</script>

<template>
  <header class="sticky top-0 z-40 border-b border-white/10 bg-slate-950/60 backdrop-blur-xl">
    <div class="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:px-6">
      <RouterLink to="/" class="group flex items-center gap-2.5">
        <span class="grid h-10 w-10 place-items-center rounded-2xl bg-linear-to-br from-violet-500 via-fuchsia-500 to-pink-500 shadow-lg shadow-fuchsia-500/40 transition-transform duration-300 group-hover:rotate-12">
          <Sparkles class="h-5 w-5 text-white" />
        </span>
        <span class="text-xl font-extrabold tracking-tight">
          Campus<span class="gradient-text">Book</span>
        </span>
      </RouterLink>

      <nav class="hidden items-center gap-1 md:flex">
        <RouterLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          class="flex items-center gap-2 rounded-xl px-3.5 py-2 text-sm font-medium text-slate-300 transition hover:bg-white/5 hover:text-white"
          exact-active-class="!bg-linear-to-r !from-violet-600/30 !to-fuchsia-600/30 !text-white ring-1 ring-fuchsia-400/30"
        >
          <component :is="link.icon" class="h-4 w-4" />
          {{ link.label }}
        </RouterLink>
      </nav>

      <div class="hidden md:block">
        <UserPicker />
      </div>

      <button class="btn-ghost md:hidden" @click="open = !open" aria-label="Menu">
        <X v-if="open" class="h-5 w-5" />
        <Menu v-else class="h-5 w-5" />
      </button>
    </div>

    <div v-if="open" class="space-y-1 border-t border-white/10 px-4 py-4 md:hidden">
      <RouterLink
        v-for="link in links"
        :key="link.to"
        :to="link.to"
        class="flex items-center gap-2 rounded-xl px-3 py-2.5 text-slate-200 hover:bg-white/5"
        exact-active-class="bg-fuchsia-500/15 text-white"
        @click="open = false"
      >
        <component :is="link.icon" class="h-4 w-4" />
        {{ link.label }}
      </RouterLink>
      <div class="pt-3">
        <UserPicker />
      </div>
    </div>
  </header>
</template>
