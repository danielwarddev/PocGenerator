let _dotNetRef = null;

function onKeyDown(e) {
    if (e.code === 'Space' && _dotNetRef) {
        e.preventDefault();
        _dotNetRef.invokeMethodAsync('OnKeyDown');
    }
}

function onKeyUp(e) {
    if (e.code === 'Space' && _dotNetRef) {
        _dotNetRef.invokeMethodAsync('OnKeyUp');
    }
}

export function registerKeyListeners(dotNetRef) {
    _dotNetRef = dotNetRef;
    window.addEventListener('keydown', onKeyDown);
    window.addEventListener('keyup', onKeyUp);
}

export function unregisterKeyListeners() {
    window.removeEventListener('keydown', onKeyDown);
    window.removeEventListener('keyup', onKeyUp);
    _dotNetRef = null;
}
