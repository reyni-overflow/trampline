import { writable } from 'svelte/store';

interface ForgotPasswordModalState {
    open: boolean;
}

const INITIAL: ForgotPasswordModalState = {
    open: false
};

function createForgotPasswordModalStore() {
    const { subscribe, set } = writable<ForgotPasswordModalState>(INITIAL);

    return {
        subscribe,

        open() {
            set({ open: true });
        },

        close() {
            set(INITIAL);
        }
    };
}

export const forgotPasswordModal = createForgotPasswordModalStore();
