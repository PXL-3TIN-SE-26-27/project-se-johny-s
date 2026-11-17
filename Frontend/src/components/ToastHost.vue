<script setup>
import { CircleAlert, PartyPopper, X } from 'lucide-vue-next'
import { useToast } from '../composables/useToast'

const { toasts, dismiss } = useToast()
</script>

<template>
  <div class="fixed right-4 bottom-4 z-50 flex w-[22rem] max-w-[calc(100vw-2rem)] flex-col gap-3">
    <TransitionGroup
      enter-active-class="transition duration-300 ease-out"
      enter-from-class="translate-y-4 opacity-0 scale-95"
      leave-active-class="transition duration-200 ease-in"
      leave-to-class="translate-x-8 opacity-0"
    >
      <div
        v-for="t in toasts"
        :key="t.id"
        class="glass flex items-start gap-3 !rounded-2xl p-4"
        :class="t.type === 'error' ? 'ring-1 ring-rose-400/40' : 'ring-1 ring-emerald-400/40'"
      >
        <span
          class="grid h-9 w-9 shrink-0 place-items-center rounded-xl"
          :class="t.type === 'error' ? 'bg-rose-500/20 text-rose-300' : 'bg-emerald-500/20 text-emerald-300'"
        >
          <CircleAlert v-if="t.type === 'error'" class="h-5 w-5" />
          <PartyPopper v-else class="h-5 w-5" />
        </span>
        <div class="flex-1 pt-1.5 text-sm leading-snug text-slate-100">{{ t.text }}</div>
        <button class="text-slate-400 hover:text-white" @click="dismiss(t.id)" aria-label="Sluiten">
          <X class="h-4 w-4" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>
