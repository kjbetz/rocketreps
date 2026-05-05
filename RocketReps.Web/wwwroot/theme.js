(() => {
    const storageKey = "rocketreps-theme";
    const cookieName = "rocketreps_theme";
    const allowedModes = new Set(["light", "dark", "system"]);
    const systemDarkQuery = matchMedia("(prefers-color-scheme: dark)");

    const normalizeMode = (mode) => allowedModes.has(mode) ? mode : "system";

    const getCookieMode = () => {
        const cookie = document.cookie
            .split("; ")
            .find((row) => row.startsWith(`${cookieName}=`));

        return cookie ? decodeURIComponent(cookie.split("=")[1]) : "";
    };

    const persistMode = (mode) => {
        document.cookie = `${cookieName}=${encodeURIComponent(mode)}; Max-Age=31536000; Path=/; SameSite=Lax`;

        try {
            localStorage.setItem(storageKey, mode);
        } catch {
            // Storage can be blocked in strict privacy modes; the cookie still carries the preference.
        }
    };

    const readMode = () => {
        try {
            return normalizeMode(localStorage.getItem(storageKey) || getCookieMode());
        } catch {
            return normalizeMode(getCookieMode());
        }
    };

    const resolveTheme = (mode) => mode === "system" && systemDarkQuery.matches ? "dark" : mode === "dark" ? "dark" : "light";

    const nextMode = (mode) => mode === "system" ? "light" : mode === "light" ? "dark" : "system";

    const modeLabel = (mode) => mode.charAt(0).toUpperCase() + mode.slice(1);

    const syncControls = (mode) => {
        document.querySelectorAll("[data-theme-switcher]").forEach((control) => {
            if (control instanceof HTMLSelectElement && control.value !== mode) {
                control.value = mode;
            }

            if (control instanceof HTMLButtonElement) {
                const label = modeLabel(mode);
                control.dataset.themeMode = mode;
                control.setAttribute("aria-label", `Theme preference: ${label}`);
                control.title = `Theme: ${label}`;
            }
        });
    };

    const applyTheme = (mode, shouldPersist = true) => {
        const normalizedMode = normalizeMode(mode);
        const resolvedTheme = resolveTheme(normalizedMode);

        document.documentElement.dataset.theme = resolvedTheme;
        document.documentElement.dataset.themeMode = normalizedMode;
        document.documentElement.style.colorScheme = resolvedTheme;

        if (shouldPersist) {
            persistMode(normalizedMode);
        }

        syncControls(normalizedMode);
    };

    const currentMode = () => normalizeMode(document.documentElement.dataset.themeMode || readMode());

    document.addEventListener("DOMContentLoaded", () => {
        applyTheme(currentMode(), false);

        document.querySelectorAll("[data-theme-switcher]").forEach((control) => {
            if (control instanceof HTMLSelectElement) {
                control.addEventListener("change", (event) => applyTheme(event.currentTarget.value));
            }

            if (control instanceof HTMLButtonElement) {
                control.addEventListener("click", () => applyTheme(nextMode(currentMode())));
            }
        });
    });

    systemDarkQuery.addEventListener("change", () => {
        if (currentMode() === "system") {
            applyTheme("system", false);
        }
    });
})();
