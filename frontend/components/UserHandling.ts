import type { LoginRequest } from "~/utils/schemas";

export abstract class UserHandling {
    public static async validateLogin() {
        if (localStorage.getItem("expires_in") != null) {
            var expiresInValue = localStorage.getItem("expires_in")
            var timestamp = parseInt(String(expiresInValue));
            var refreshToken = false;
            if (timestamp < Math.floor(Date.now() / 1000)) {
                refreshToken = true;
            }
            return { refreshToken }
        }
        return { refreshToken: null }
    }

    public static async refreshToken(apiEdnpoint: string) {
        const data  = await $fetch<LoginRequest>(`${apiEdnpoint}/refresh?refresh_token=${localStorage.getItem("refreshToken")}`, {
            method: "POST"
        })
        if (data) {
            localStorage.setItem("token", data.access_token);
            localStorage.setItem(
                "expires_in",
                String(Math.floor(Date.now() / 1000) + parseInt(data.expires_in))
            );
            localStorage.setItem("refreshToken", data.refresh_token);
            return { loginStatus: true }
        }
        return { loginStatus: false }
    }

    public static async getLogin(apiEndpoint: string, loginCode: string) {
        const data = await $fetch<LoginRequest>(`${apiEndpoint}/login?code=${loginCode}`, {
            method: "POST"
        })
        if (data) {
            localStorage.setItem("token", data.access_token);
            localStorage.setItem(
                "expires_in",
                String(Math.floor(Date.now() / 1000) + parseInt(data.expires_in))
            );
            localStorage.setItem("refreshToken", data.refresh_token);
            return { loginStatus: true }
        }
        return { loginStatus: false }
    }

    public static setLogout() {
        localStorage.removeItem("token");
        localStorage.removeItem("expires_in");
        localStorage.removeItem("refreshToken");
        return { loginStatus: false }
    }
}