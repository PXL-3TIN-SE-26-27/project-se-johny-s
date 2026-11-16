<script setup>
import NavBar from './components/NavBar.vue'
import ToastHost from './components/ToastHost.vue'
</script>

<template>
  <div class="relative min-h-screen overflow-x-hidden">
    <div class="pointer-events-none fixed inset-0 -z-10">
      <div class="blob absolute -top-40 -left-32 h-[28rem] w-[28rem] rounded-full bg-violet-600/30 blur-3xl"></div>
      <div class="blob absolute top-1/3 -right-40 h-[32rem] w-[32rem] rounded-full bg-fuchsia-600/20 blur-3xl" style="animation-delay: -5s"></div>
      <div class="blob absolute -bottom-40 left-1/3 h-[24rem] w-[24rem] rounded-full bg-indigo-600/25 blur-3xl" style="animation-delay: -9s"></div>
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_1px_1px,rgba(255,255,255,0.06)_1px,transparent_0)] [background-size:28px_28px]"></div>
    </div>

    <NavBar />

    <main class="mx-auto max-w-6xl px-4 pt-8 pb-24 sm:px-6">
      <RouterView v-slot="{ Component }">
        <Transition name="page" mode="out-in">
          <component :is="Component" />
        </Transition>
      </RouterView>
    </main>

    <footer class="pb-10 text-center text-sm text-slate-500">
      Gemaakt met 💜 voor de campus · CampusBook
    </footer>

    <ToastHost />
  </div>
</template>
