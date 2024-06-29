<template>
  <v-app>
    <AppBar v-model:snackbar-inform-user="snackbarInformUser" v-model:snackbar-inform-user-text="snackbarInformUserText"
      v-model:login-status="loginStatus" />
    <NuxtPage style="padding-top: 64px;" v-model:login-status="loginStatus"
      v-model:snackbar-inform-user="snackbarInformUser"
      v-model:snackbar-inform-user-text="snackbarInformUserText" />
    <v-snackbar :timeout="2000" rounded="pill" v-model="snackbarInformUser">
      {{ snackbarInformUserText }}
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import AppBar from './components/AppBar.client.vue';
const loginStatus = ref(false);
const snackbarInformUser = ref(false);
const snackbarInformUserText = ref("")
watch(loginStatus, (newState) => {
  loginStatus.value = newState;
});
watch(snackbarInformUser, (newState) => {
  snackbarInformUser.value = newState;
});
watch(snackbarInformUserText, (newState) => {
  snackbarInformUserText.value = newState;
});
</script>
