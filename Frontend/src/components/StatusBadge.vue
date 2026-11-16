<script setup>
import { computed } from 'vue'
import { statusInfo } from '../lib/format'

const props = defineProps({ status: { type: String, required: true } })
const info = computed(() => statusInfo(props.status))
</script>

<template>
  <span class="inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold ring-1" :class="info.classes">
    <span>{{ info.emoji }}</span>
    {{ info.label }}
  </span>
</template>
