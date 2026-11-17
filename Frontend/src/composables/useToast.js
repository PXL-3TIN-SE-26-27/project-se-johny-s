import { ref } from 'vue'

const toasts = ref([])
let nextId = 1

function push(type, text) {
  const id = nextId++
  toasts.value.push({ id, type, text })
  setTimeout(() => dismiss(id), type === 'error' ? 6000 : 3500)
}

function dismiss(id) {
  toasts.value = toasts.value.filter((t) => t.id !== id)
}

export function useToast() {
  return {
    toasts,
    success: (text) => push('success', text),
    error: (text) => push('error', text),
    dismiss,
  }
}
