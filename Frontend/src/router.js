import { createRouter, createWebHistory } from 'vue-router'
import AvailabilityView from './views/AvailabilityView.vue'
import ReserveView from './views/ReserveView.vue'
import MyReservationsView from './views/MyReservationsView.vue'
import StatsView from './views/StatsView.vue'

export default createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'availability', component: AvailabilityView },
    { path: '/reserveren', name: 'reserve', component: ReserveView },
    { path: '/mijn-reservaties', name: 'mine', component: MyReservationsView },
    { path: '/stats', name: 'stats', component: StatsView },
  ],
  scrollBehavior: () => ({ top: 0 }),
})
