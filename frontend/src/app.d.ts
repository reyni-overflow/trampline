declare global {
    namespace App {}
}

declare module '$env/static/public' {
    export const PUBLIC_API_URL: string;
    export const PUBLIC_CONTACT_EMAIL: string;
    export const PUBLIC_LINK_VK: string;
    export const PUBLIC_LINK_MAX: string;
    export const PUBLIC_LINK_DZEN: string;
    export const PUBLIC_LINK_TELEGRAM: string;
    export const PUBLIC_BOT_VK: string;
    export const PUBLIC_BOT_MAX: string;
    export const PUBLIC_BOT_TELEGRAM: string;
    export const PUBLIC_MOBILE_APP_LINK: string;
}

export {};
