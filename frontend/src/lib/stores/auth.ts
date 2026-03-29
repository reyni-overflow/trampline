import { writable, derived } from 'svelte/store';
import { authApi, type UserResponse } from '$lib/api/auth';

export const authReady = writable(false);

function createAuthStore() {
    const { subscribe, set } = writable<UserResponse | null>(null);

    return {
        subscribe,

        async fetchUser() {
            try {
                const user = await authApi.me();
                set(user);
                authReady.set(true);
                return user;
            } catch {
                set(null);
                authReady.set(true);
                return null;
            }
        },

        setUser(user: UserResponse | null) {
            set(user);
            authReady.set(true);
        },

        clear() {
            set(null);
        }
    };
}

export const user = createAuthStore();
export const isAuthenticated = derived(user, ($user) => $user !== null);
