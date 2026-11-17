import { computed, ref, watch } from 'vue'
import { api } from '../api'

function readStored() {
  try {
    return Number(localStorage.getItem('cb_user')) || 0
  } catch {
    return 0
  }
}

const users = ref([])
const userId = ref(readStored())
const loading = ref(false)
const error = ref('')
let started = false

watch(userId, (value) => {
  try {
    localStorage.setItem('cb_user', String(value))
  } catch {
    // storage not available
  }
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    const list = await api.users()
    users.value = list
    if (list.length && !list.some((u) => u.id === userId.value)) {
      userId.value = list[0].id
    }
  } catch (e) {
    error.value = e.message
    started = false
  } finally {
    loading.value = false
  }
}

export function useUser() {
  if (!started) {
    started = true
    load()
  }
  const currentUser = computed(() => users.value.find((u) => u.id === userId.value) || null)
  return { users, userId, currentUser, loading, error, reload: load }
}
