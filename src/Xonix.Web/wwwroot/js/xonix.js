// Browser side of the game: canvas, keyboard, Web Audio and localStorage.
// All game logic and sound synthesis live in C#; this module only moves pixels, keys and sounds across the boundary.

const highScoreKey = "xonix.highScore";
const handledKeys = new Set([
    "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight",
    "Enter", "NumpadEnter", "Space", "KeyP"
]);

// Browsers keep the context suspended until a user gesture; decoding works meanwhile.
const audio = new AudioContext();
const sounds = new Map();

let context2d;
let image;
let muted = false;

export async function start() {
    const canvas = document.getElementById("screen");
    if (!canvas) {
        throw new Error("Canvas #screen not found");
    }

    context2d = canvas.getContext("2d");
    image = context2d.createImageData(canvas.width, canvas.height);

    const { getAssemblyExports } = await globalThis.getDotnetRuntime(0);
    const host = (await getAssemblyExports("Xonix.Web.dll")).Xonix.Web.GameHost;

    window.addEventListener("keydown", event => {
        if (audio.state === "suspended") {
            audio.resume();
        }
        if (event.code === "KeyM") {
            muted = !muted;
            return;
        }
        if (!handledKeys.has(event.code)) {
            return;
        }
        event.preventDefault();
        if (!event.repeat) {
            host.KeyDown(event.code);
        }
    });
    window.addEventListener("keyup", event => {
        if (handledKeys.has(event.code)) {
            event.preventDefault();
            host.KeyUp(event.code);
        }
    });

    let last = performance.now();
    const loop = now => {
        host.Frame(now - last);
        last = now;
        requestAnimationFrame(loop);
    };
    requestAnimationFrame(loop);
}

// The memory view is only valid during the call, so the bytes are copied before decoding.
export function loadSound(name, wav) {
    sounds.set(name, audio.decodeAudioData(wav.slice().buffer));
}

export function present(pixels) {
    image.data.set(pixels.slice());
    context2d.putImageData(image, 0, 0);
}

export function playSound(name) {
    if (muted || audio.state !== "running") {
        return;
    }
    const decoding = sounds.get(name);
    if (!decoding) {
        throw new Error(`Unknown sound ${name}`);
    }
    decoding.then(buffer => {
        const source = audio.createBufferSource();
        source.buffer = buffer;
        source.connect(audio.destination);
        source.start();
    });
}

export function loadHighScore() {
    try {
        return Number.parseInt(localStorage.getItem(highScoreKey) ?? "0", 10) || 0;
    } catch (error) {
        console.warn("High score unavailable", error);
        return 0;
    }
}

export function saveHighScore(score) {
    try {
        localStorage.setItem(highScoreKey, String(score));
    } catch (error) {
        console.warn("High score not saved", error);
    }
}
