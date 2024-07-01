<template>
    <v-app-bar>
        <v-toolbar-items>
            <div style="padding: 5px">
                <v-switch class="switch" @click="toggleTheme()" dense v-model="isDark"
                    append-icon="mdi-theme-light-dark"></v-switch>
            </div>
        </v-toolbar-items>
        <v-spacer />
        <v-btn v-if="loginStatus" @click="logout()">Logout</v-btn>
        <v-btn v-else :href="config.public.discordUrl">Login</v-btn>
    </v-app-bar>
</template>

<script setup lang="ts">
import { useTheme } from 'vuetify'
import { UserHandling } from './UserHandling';

const theme = useTheme()
const config = useRuntimeConfig();
const route = useRoute();
const snackbarInformUser = defineModel<boolean>("snackbarInformUser", { required: true });
const snackbarInformUserText = defineModel<string>("snackbarInformUserText", { required: true });
const loginStatus = defineModel<boolean>("loginStatus", { required: true });
const isDark = ref(true);

onBeforeMount(async () => {
    const login_code = route.query.code;
    // Weird workaround... before a request is executed correctly.
    await nextTick()

    // Check code
    if (login_code) {
        UserHandling.removeStorageItems();
        var result = await UserHandling.getLogin(config.public.apiUrl, String(login_code));
        if (result && result.loginStatus) {
            loginStatus.value = result.loginStatus;
            snackbarInformUserText.value = "User logged in successfully."
            snackbarInformUser.value = true;
            navigateTo("/");
            return
        }
    }

    // Check user has a token
    if (localStorage.getItem("token") === null) {
        return
    }

    // If user has token, validate creds
    await validateLogin();
})

onMounted(() => {
    setThemeLocalstorage();
})

async function validateLogin() {
    var refresh = await UserHandling.validateLogin();
    if (refresh && refresh.refreshToken !== null && !refresh.refreshToken) {
        var result = await UserHandling.refreshToken(config.public.apiUrl);
        loginStatus.value = result.loginStatus;
        if (result.loginStatus) {
            snackbarInformUserText.value = "User credentials refreshed successfully."
            snackbarInformUser.value = true;
        } else {
            snackbarInformUserText.value = "User credentials refresh failed. Please login again."
            snackbarInformUser.value = true;
        }
    }
}

function setThemeLocalstorage() {
    if (
        import.meta.client && localStorage.getItem("dark") == "undefined"
    ) {
        localStorage.setItem("dark", String(isDark.value));
        theme.global.name.value = "dark";
    }
    if (import.meta.client && localStorage.getItem("dark") !== null) {
        isDark.value = localStorage.getItem("dark") == "true"
    }
    if (!isDark.value) {
        theme.global.name.value = "light";
    }
}

function toggleTheme() {
    theme.global.name.value = theme.global.current.value.dark ? 'light' : 'dark'
    isDark.value = theme.global.current.value.dark;
    if (import.meta.client) {
        localStorage.setItem("dark", String(isDark.value));
    }
}

function logout() {
    var result = UserHandling.setLogout();
    if (!result.loginStatus) {
        loginStatus.value = false;
        snackbarInformUserText.value = "User logged out successfully."
        snackbarInformUser.value = true;
    }
}

</script>