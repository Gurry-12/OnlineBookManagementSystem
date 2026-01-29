/**
 * URL State Manager
 * Handles browser history and URL state for AJAX operations
 */

class UrlStateManager {
    constructor() {
        this.currentState = this.parseUrl();
        this.listeners = new Map();
        this.init();
    }

    /**
     * Initialize URL state manager
     */
    init() {
        // Listen for browser back/forward buttons
        window.addEventListener('popstate', (event) => {
            if (event.state) {
                this.currentState = event.state;
                this.notifyListeners('popstate', event.state);
            }
        });
    }

    /**
     * Parse current URL parameters
     */
    parseUrl() {
        const urlParams = new URLSearchParams(window.location.search);
        const state = {};

        for (const [key, value] of urlParams.entries()) {
            // Handle numeric values
            if (!isNaN(value) && value !== '') {
                state[key] = parseInt(value);
            } else {
                state[key] = value;
            }
        }

        return state;
    }

    /**
     * Update URL without page reload
     */
    updateUrl(newState, title = null, replaceState = false) {
        // Merge with current state
        const updatedState = { ...this.currentState, ...newState };

        // Remove null/undefined values
        Object.keys(updatedState).forEach(key => {
            if (updatedState[key] === null || updatedState[key] === undefined || updatedState[key] === '') {
                delete updatedState[key];
            }
        });

        // Build new URL
        const params = new URLSearchParams();
        Object.entries(updatedState).forEach(([key, value]) => {
            if (value !== null && value !== undefined && value !== '') {
                params.set(key, value.toString());
            }
        });

        const newUrl = `${window.location.pathname}${params.toString() ? '?' + params.toString() : ''}`;

        // Update browser history
        if (replaceState) {
            history.replaceState(updatedState, title || document.title, newUrl);
        } else {
            history.pushState(updatedState, title || document.title, newUrl);
        }

        this.currentState = updatedState;
        this.notifyListeners('statechange', updatedState);
    }

    /**
     * Get current state
     */
    getState() {
        return { ...this.currentState };
    }

    /**
     * Get specific state value
     */
    get(key, defaultValue = null) {
        return this.currentState[key] || defaultValue;
    }

    /**
     * Set state value
     */
    set(key, value, updateUrl = true) {
        const newState = { [key]: value };

        if (updateUrl) {
            this.updateUrl(newState);
        } else {
            this.currentState[key] = value;
        }
    }

    /**
     * Remove state value
     */
    remove(key, updateUrl = true) {
        if (updateUrl) {
            this.updateUrl({ [key]: null });
        } else {
            delete this.currentState[key];
        }
    }

    /**
     * Clear all state
     */
    clear(updateUrl = true) {
        if (updateUrl) {
            history.replaceState({}, document.title, window.location.pathname);
        }
        this.currentState = {};
        this.notifyListeners('statechange', {});
    }

    /**
     * Add state change listener
     */
    addListener(event, callback) {
        if (!this.listeners.has(event)) {
            this.listeners.set(event, []);
        }
        this.listeners.get(event).push(callback);
    }

    /**
     * Remove state change listener
     */
    removeListener(event, callback) {
        if (this.listeners.has(event)) {
            const callbacks = this.listeners.get(event);
            const index = callbacks.indexOf(callback);
            if (index > -1) {
                callbacks.splice(index, 1);
            }
        }
    }

    /**
     * Notify listeners of state changes
     */
    notifyListeners(event, state) {
        if (this.listeners.has(event)) {
            this.listeners.get(event).forEach(callback => {
                try {
                    callback(state);
                } catch (error) {
                    console.error('Error in state change listener:', error);
                }
            });
        }
    }

    /**
     * Build query string from object
     */
    buildQueryString(params) {
        const urlParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
            if (value !== null && value !== undefined && value !== '') {
                urlParams.set(key, value.toString());
            }
        });
        return urlParams.toString();
    }

    /**
     * Navigate to new URL with state
     */
    navigate(url, state = {}) {
        const params = this.buildQueryString(state);
        const fullUrl = `${url}${params ? '?' + params : ''}`;

        history.pushState(state, document.title, fullUrl);
        this.currentState = state;
        this.notifyListeners('navigate', state);
    }

    /**
     * Refresh current page with updated state
     */
    refresh(newState = {}) {
        this.updateUrl(newState, null, true);
        this.notifyListeners('refresh', this.currentState);
    }
}

// Create global instance
window.urlState = new UrlStateManager();