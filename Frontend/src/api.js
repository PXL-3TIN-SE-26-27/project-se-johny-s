const BASE = (import.meta.env.VITE_API_BASE || '').replace(/\/$/, '')

async function request(path, options = {}) {
  const res = await fetch(BASE + path, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })
  const text = await res.text()
  let data = text
  try {
    data = JSON.parse(text)
  } catch {
    // plain text response
  }
  if (!res.ok) {
    let msg = `Er ging iets mis (HTTP ${res.status})`
    if (typeof data === 'string' && data.trim() && !data.trim().startsWith('<')) {
      msg = data.split('\n')[0].slice(0, 200)
    } else if (data && typeof data === 'object' && data.title) {
      msg = data.title
    }
    throw new Error(msg)
  }
  return data
}

export const api = {
  users: () => request('/api/users'),
  resources: (date) => request(`/api/resources?date=${encodeURIComponent(date)}`),
  reservations: (userId) => request(`/api/reservations?userId=${userId}`),
  createReservation: (body) => request('/api/reservations', { method: 'POST', body: JSON.stringify(body) }),
  action: (id, action) => request(`/api/reservations/${id}/${action}`, { method: 'POST' }),
  stats: () => request('/api/stats'),
  exportUrl: (userId) => `${BASE}/api/reservations/export?userId=${userId}`,
}
