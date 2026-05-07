const voiceStorageKey = "rocketreps-review-voice-uri";

function supportsSpeechSynthesis() {
    return "speechSynthesis" in window && "SpeechSynthesisUtterance" in window;
}

function loadVoices() {
    if (!supportsSpeechSynthesis()) {
        return Promise.resolve([]);
    }

    const voices = window.speechSynthesis.getVoices();
    if (voices.length > 0) {
        return Promise.resolve(voices);
    }

    return new Promise((resolve) => {
        const timeoutId = window.setTimeout(() => resolve(window.speechSynthesis.getVoices()), 1000);
        window.speechSynthesis.onvoiceschanged = () => {
            window.clearTimeout(timeoutId);
            resolve(window.speechSynthesis.getVoices());
        };
    });
}

export async function getVoices() {
    const voices = await loadVoices();
    return voices
        .map((voice) => ({
            voiceUri: voice.voiceURI,
            name: voice.name,
            lang: voice.lang,
            isDefault: voice.default,
            localService: voice.localService,
        }))
        .sort((left, right) => {
            const leftEnglish = left.lang?.toLowerCase().startsWith("en") ? 0 : 1;
            const rightEnglish = right.lang?.toLowerCase().startsWith("en") ? 0 : 1;
            return leftEnglish - rightEnglish || left.lang.localeCompare(right.lang) || left.name.localeCompare(right.name);
        });
}

export function getSavedVoiceUri() {
    try {
        return window.localStorage.getItem(voiceStorageKey) || "";
    } catch {
        return "";
    }
}

export function saveVoiceUri(voiceUri) {
    try {
        if (voiceUri) {
            window.localStorage.setItem(voiceStorageKey, voiceUri);
        } else {
            window.localStorage.removeItem(voiceStorageKey);
        }
    } catch {
    }
}

export async function speak(text, lang = "en-US", voiceUri = "") {
    if (!text || !("speechSynthesis" in window) || !("SpeechSynthesisUtterance" in window)) {
        return false;
    }

    window.speechSynthesis.cancel();

    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = lang;
    utterance.rate = 0.9;
    utterance.pitch = 1;
    utterance.volume = 1;

    if (voiceUri) {
        const voices = await loadVoices();
        const selectedVoice = voices.find((voice) => voice.voiceURI === voiceUri);
        if (selectedVoice) {
            utterance.voice = selectedVoice;
            utterance.lang = selectedVoice.lang;
        }
    }

    return new Promise((resolve) => {
        let isSettled = false;
        const settle = (result) => {
            if (isSettled) {
                return;
            }

            isSettled = true;
            resolve(result);
        };

        utterance.onend = () => settle(true);
        utterance.onerror = () => settle(false);
        window.setTimeout(() => settle(true), 8000);
        window.speechSynthesis.speak(utterance);
    });
}
