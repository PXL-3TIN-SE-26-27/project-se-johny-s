<script setup>
import { computed } from 'vue'
import { typeInfo } from '../lib/format'

const props = defineProps({
  type: { type: String, required: true },
  size: { type: String, default: 'md' },
})
const info = computed(() => typeInfo(props.type))
const box = computed(() => (props.size === 'lg' ? 'h-14 w-14 rounded-2xl' : 'h-11 w-11 rounded-xl'))
const icon = computed(() => (props.size === 'lg' ? 'h-7 w-7' : 'h-5 w-5'))
</script>

<template>
  <span class="grid shrink-0 place-items-center bg-linear-to-br shadow-lg shadow-fuchsia-500/20" :class="[box, info.gradient]">
    <component :is="info.icon" class="text-white" :class="icon" />
  </span>
</template>
