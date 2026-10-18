<template>
  <RouterView></RouterView>
</template>

<script setup lang="ts">
import { useUserStore } from './stores/user.ts';

const userStore = useUserStore();
</script>

<style scoped></style>
