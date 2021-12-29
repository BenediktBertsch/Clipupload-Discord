<template>
  <v-app dark>
    <v-app-bar fixed app>
      <v-toolbar-items>
        <div style="padding: 20px">
          <v-switch
            class="switch"
            dense
            v-model="darkmode"
            append-icon="mdi-theme-light-dark"
          ></v-switch>
        </div>
      </v-toolbar-items>
      <v-spacer />
      <v-btn v-if="loginStatus" @click="setLogout" text>Logout</v-btn>
      <v-btn v-else text :href="loginURL">Login</v-btn>
    </v-app-bar>
    <v-main>
      <Upload
        v-if="
          loginStatus !== null &&
          snackBarLogin !== null &&
          refreshTokenReady === true
        "
        :loginStatus="loginStatus"
        :snackBarLogin="snackBarLogin"
        :snackBarLoginText="snackBarLoginText"
      />
    </v-main>
  </v-app>
</template>

<script>
import Upload from "../pages/index";
export default {
  components: {
    Upload,
  },
  data() {
    return {
      darkmode: null,
      loginStatus: null,
      loginURL: null,
      apiURL: null,
      snackBarLogin: null,
      snackBarLoginText: null,
      refreshTokenReady: null,
    };
  },
  async mounted() {
    this.loginURL = this.$config.discordURL;
    this.apiURL = this.$config.apiURL;
    this.loginStatus = false;
    if (localStorage.getItem("token")) {
      await this.validateLogin();
    }
    if (this.$route.query.code && this.loginStatus === false) {
      await this.getLogin();
    } else {
      this.snackBarLogin = false;
      this.refreshTokenReady = true;
    }
    if (
      localStorage.getItem("dark") == "undefined" &&
      localStorage.getItem("dark")
    ) {
      localStorage.setItem("dark", "false");
    } else {
      this.darkmode = localStorage.getItem("dark") == "true";
    }
    this.$nuxt.$vuetify.theme.dark = this.darkmode;
  },
  watch: {
    darkmode(value) {
      this.$nuxt.$vuetify.theme.dark = this.darkmode;
      localStorage.setItem("dark", value);
    },
  },
  methods: {
    async validateLogin() {
      var timestamp = parseInt(localStorage.getItem("expires_in"));
      if (timestamp < Math.floor(Date.now() / 1000)) {
        await this.refreshToken();
      } else {
        this.refreshTokenReady = true;
        this.loginStatus = true;
      }
    },
    async refreshToken() {
      await this.$axios
        .$post(
          this.apiURL +
            `refresh?refresh_token=${localStorage.getItem("refreshToken")}`
        )
        .then((data) => {
          localStorage.setItem("token", data.access_token);
          localStorage.setItem(
            "expires_in",
            Math.floor(Date.now() / 1000) + parseInt(data.expires_in)
          );
          localStorage.setItem("refreshToken", data.refresh_token);
          this.refreshTokenReady = true;
          this.loginStatus = true;
        })
        .catch(() => {
          this.setLogout();
        });
    },
    async getLogin() {
      await this.$axios
        .$post(this.apiURL + `login?code=${this.$route.query.code}`)
        .then((data) => {
          localStorage.setItem("token", data.access_token);
          localStorage.setItem(
            "expires_in",
            Math.floor(Date.now() / 1000) + parseInt(data.expires_in)
          );
          localStorage.setItem("refreshToken", data.refresh_token);
          this.loginStatus = true;
          this.snackBarLogin = true;
          this.refreshTokenReady = true;
          this.snackBarLoginText = "Successfully logged in.";
          this.$router.push("/");
        })
        .catch(() => {
          this.snackBarLogin = true;
          this.snackBarLoginText = "Error logging in, please try again.";
        });
    },
    setLogout() {
      localStorage.removeItem("token");
      localStorage.removeItem("expires_in");
      localStorage.removeItem("refreshToken");
      this.loginStatus = false;
      this.snackBarLogin = true;
      this.snackBarLoginText = "Logged out successfully.";
      this.$bus.$emit("logoutEvent", this.snackBarLogin);
    },
  },
};
</script>
