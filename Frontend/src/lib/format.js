import { Camera, DoorOpen, Laptop, Package, Printer } from 'lucide-vue-next'

export const resourceTypes = {
  room: { label: 'Lokaal', plural: "Lokalen", icon: DoorOpen, gradient: 'from-violet-500 to-indigo-500' },
  laptop: { label: 'Laptop', plural: 'Laptops', icon: Laptop, gradient: 'from-fuchsia-500 to-pink-500' },
  camera: { label: 'Camera', plural: "Camera's", icon: Camera, gradient: 'from-purple-500 to-fuchsia-500' },
  printer: { label: '3D-printer', plural: 'Printers', icon: Printer, gradient: 'from-indigo-500 to-cyan-500' },
}

export function typeInfo(type) {
  return resourceTypes[type] || { label: type, plural: type, icon: Package, gradient: 'from-slate-500 to-slate-600' }
}

export const statuses = {
  Pending: { label: 'In afwachting', emoji: '⏳', classes: 'bg-amber-500/15 text-amber-300 ring-amber-400/30' },
  Confirmed: { label: 'Bevestigd', emoji: '✅', classes: 'bg-violet-500/15 text-violet-300 ring-violet-400/30' },
  CheckedOut: { label: 'Afgehaald', emoji: '📦', classes: 'bg-sky-500/15 text-sky-300 ring-sky-400/30' },
  Returned: { label: 'Teruggebracht', emoji: '🎉', classes: 'bg-emerald-500/15 text-emerald-300 ring-emerald-400/30' },
  Cancelled: { label: 'Geannuleerd', emoji: '🚫', classes: 'bg-rose-500/15 text-rose-300 ring-rose-400/30' },
}

export function statusInfo(status) {
  return statuses[status] || { label: status, emoji: '❔', classes: 'bg-slate-500/15 text-slate-300 ring-slate-400/30' }
}

export function money(value) {
  return new Intl.NumberFormat('nl-BE', { style: 'currency', currency: 'EUR' }).format(Number(value) || 0)
}

export function dateTime(value) {
  const d = new Date(value)
  return d.toLocaleString('nl-BE', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })
}

export function todayIso() {
  const d = new Date()
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}
