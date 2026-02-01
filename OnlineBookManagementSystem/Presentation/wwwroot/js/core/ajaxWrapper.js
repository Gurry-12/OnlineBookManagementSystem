/**
 * Global AJAX Wrapper
 * Purpose: Centralized AJAX handling with automatic loading states, error handling, and partial view injection
 * Usage: ajaxWrapper.load(), ajaxWrapper.submit(), ajaxWrapper.delete()
 */

const ajaxWrapper = (function () {
    'use strict';

    // Configuration
    const config = {
        defaultTimeout: 30000,
        retryAttempts: 3,
        retryDelay: 1000,
        loadingClass: 'loading',
        errorClass: 'has-error',
        successClass: 'success'
    };

    // State management
    const state = {
        activeRequests: new Map(),
        requestCounter: 0
    };

    /**
     * Show loading state on container
     * @param {HTMLElement|string} container - Container element or selector
     * @param {string} message - Optional loading message
     */
    function showLoading(container, message = 'Loading...') {
        const element = typeof container === 'string' ? document.querySelector(container) : container;
        if (!element) return;

        element.classList.add(config.loadingClass);
        element.setAttribute('aria-busy', 'true');

        // Inject loading component
        const loadingHtml = `
            <div class="loading-state loading-state--overlay" data-loading-overlay>
                <div class="loading-state__content">
                    <div class="loading-state__spinner">
                        <div class="spinner-border" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                    </div>
                    <p class="loading-state__text">${message}</p>
                </div>
            </div>
        `;

        // Remove existing loading overlay
        const existingOverlay = element.querySelector('[data-loading-overlay]');
        if (existingOverlay) {
            existingOverlay.remove();
        }

        element.insertAdjacentHTML('beforeend', loadingHtml);
    }

    /**
     * Hide loading state from container
     * @param {HTMLElement|string} container - Container element or selector
     */
    function hideLoading(container) {
        const element = typeof container === 'string' ? document.querySelector(container) : container;
        if (!element) return;

        element.classList.remove(config.loadingClass);
        element.removeAttribute('aria-busy');

        const overlay = element.querySelector('[data-loading-overlay]');
        if (overlay) {
            overlay.remove();
        }
    }

    /**
     * Show error in container
     * @param {HTMLElement|string} container - Container element or selector
     * @param {string} message - Error message
     * @param {Function} retryCallback - Optional retry callback
     */
    function showError(container, message, retryCallback = null) {
        const element = typeof container === 'string' ? document.querySelector(container) : container;
        if (!element) return;

        element.classList.add(config.errorClass);

        const retryAction = retryCallback ? `ajaxWrapper.retry('${retryCallback}')` : null;
        const errorHtml = `
            <div class="error-alert" data-error-alert>
                <div class="error-alert__content">
                    <div class="error-alert__icon">
                        <i class="bi bi-exclamation-triangle-fill"></i>
                    </div>
                    <div class="error-alert__body">
                        <h6 class="error-alert__title">Error</h6>
                        <p class="error-alert__message">${message}</p>
                    </div>
                    <button type="button" class="error-alert__close" onclick="this.closest('[data-error-alert]').remove()">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
                ${retryAction ? `
                    <div class="error-alert__actions">
                        <button type="button" class="btn btn--sm btn--primary" onclick="${retryAction}">
                            <i class="bi bi-arrow-clockwise me-1"></i>Retry
                        </button>
                    </div>
                ` : ''}
            </div>
        `;

        // Remove existing error
        const existingError = element.querySelector('[data-error-alert]');
        if (existingError) {
            existingError.remove();
        }

        element.insertAdjacentHTML('afterbegin', errorHtml);

        // Use notifications if available
        if (typeof notifications !== 'undefined') {
            notifications.error(message);
        }
    }

    /**
     * Load partial view via AJAX
     * @param {Object} options - Configuration options
     * @returns {Promise}
     */
    async function load(options) {
        const {
            url,
            container,
            method = 'GET',
            data = null,
            loadingMessage = 'Loading...',
            replaceContent = true,
            onSuccess = null,
            onError = null,
            validateForm = false
        } = options;

        const requestId = ++state.requestCounter;
        const containerElement = typeof container === 'string' ? document.querySelector(container) : container;

        if (!containerElement) {
            console.error('Container not found:', container);
            return Promise.reject(new Error('Container not found'));
        }

        try {
            // Show loading state
            showLoading(containerElement, loadingMessage);

            // Prepare request
            const fetchOptions = {
                method: method,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'text/html, application/json'
                },
                credentials: 'same-origin'
            };

            // Add CSRF token
            const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
            if (csrfToken) {
                fetchOptions.headers['RequestVerificationToken'] = csrfToken.value;
            }

            // Add data for POST/PUT
            if (data && (method === 'POST' || method === 'PUT')) {
                if (data instanceof FormData) {
                    fetchOptions.body = data;
                } else {
                    fetchOptions.headers['Content-Type'] = 'application/json';
                    fetchOptions.body = JSON.stringify(data);
                }
            }

            // Build URL with query params for GET
            let requestUrl = url;
            if (data && method === 'GET') {
                const params = new URLSearchParams(data);
                requestUrl = `${url}?${params.toString()}`;
            }

            // Store active request
            const controller = new AbortController();
            fetchOptions.signal = controller.signal;
            state.activeRequests.set(requestId, controller);

            // Make request
            const response = await fetch(requestUrl, fetchOptions);

            // Remove from active requests
            state.activeRequests.delete(requestId);

            // Handle response
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const contentType = response.headers.get('content-type');
            let result;

            if (contentType && contentType.includes('application/json')) {
                result = await response.json();

                if (result.html) {
                    // Server returned JSON with HTML
                    if (replaceContent) {
                        containerElement.innerHTML = result.html;
                    } else {
                        containerElement.insertAdjacentHTML('beforeend', result.html);
                    }
                }
            } else {
                // Server returned HTML directly
                result = await response.text();

                if (replaceContent) {
                    containerElement.innerHTML = result;
                } else {
                    containerElement.insertAdjacentHTML('beforeend', result);
                }
            }

            // Hide loading state
            hideLoading(containerElement);

            // Re-initialize jQuery validation if needed
            if (validateForm && typeof $.validator !== 'undefined') {
                const form = containerElement.querySelector('form');
                if (form) {
                    $.validator.unobtrusive.parse(form);
                }
            }

            // Success callback
            if (onSuccess) {
                onSuccess(result, containerElement);
            }

            // Dispatch custom event
            containerElement.dispatchEvent(new CustomEvent('ajax:success', {
                detail: { result, url, method }
            }));

            return result;

        } catch (error) {
            // Remove from active requests
            state.activeRequests.delete(requestId);

            // Hide loading state
            hideLoading(containerElement);

            // Show error
            const errorMessage = error.message || 'An error occurred while loading content.';
            showError(containerElement, errorMessage);

            // Error callback
            if (onError) {
                onError(error, containerElement);
            }

            // Dispatch custom event
            containerElement.dispatchEvent(new CustomEvent('ajax:error', {
                detail: { error, url, method }
            }));

            throw error;
        }
    }

    /**
     * Submit form via AJAX
     * @param {Object} options - Configuration options
     * @returns {Promise}
     */
    async function submit(options) {
        const {
            form,
            url,
            container,
            successMessage = 'Saved successfully!',
            onSuccess = null,
            onError = null
        } = options;

        const formElement = typeof form === 'string' ? document.querySelector(form) : form;

        if (!formElement) {
            console.error('Form not found:', form);
            return Promise.reject(new Error('Form not found'));
        }

        // Validate form if jQuery validation is available
        if (typeof $.validator !== 'undefined') {
            const validator = $(formElement).validate();
            if (!validator.form()) {
                return Promise.reject(new Error('Form validation failed'));
            }
        }

        const formData = new FormData(formElement);
        const submitUrl = url || formElement.action;
        const targetContainer = container || formElement.closest('[data-ajax-container]') || formElement;

        return load({
            url: submitUrl,
            container: targetContainer,
            method: 'POST',
            data: formData,
            loadingMessage: 'Saving...',
            validateForm: true,
            onSuccess: (result, containerElement) => {
                // Show success message
                if (typeof notifications !== 'undefined') {
                    notifications.success(successMessage);
                }

                if (onSuccess) {
                    onSuccess(result, containerElement);
                }
            },
            onError: onError
        });
    }

    /**
     * Delete resource via AJAX
     * @param {Object} options - Configuration options
     * @returns {Promise}
     */
    async function deleteResource(options) {
        const {
            url,
            container,
            confirmMessage = 'Are you sure you want to delete this item?',
            successMessage = 'Deleted successfully!',
            onSuccess = null,
            onError = null
        } = options;

        // Confirm deletion
        if (confirmMessage && !confirm(confirmMessage)) {
            return Promise.reject(new Error('Deletion cancelled'));
        }

        return load({
            url: url,
            container: container,
            method: 'DELETE',
            loadingMessage: 'Deleting...',
            onSuccess: (result, containerElement) => {
                // Show success message
                if (typeof notifications !== 'undefined') {
                    notifications.success(successMessage);
                }

                if (onSuccess) {
                    onSuccess(result, containerElement);
                }
            },
            onError: onError
        });
    }

    /**
     * Cancel all active requests
     */
    function cancelAll() {
        state.activeRequests.forEach((controller) => {
            controller.abort();
        });
        state.activeRequests.clear();
    }

    /**
     * Cancel specific request
     * @param {number} requestId - Request ID to cancel
     */
    function cancel(requestId) {
        const controller = state.activeRequests.get(requestId);
        if (controller) {
            controller.abort();
            state.activeRequests.delete(requestId);
        }
    }

    // Public API
    return {
        load,
        submit,
        delete: deleteResource,
        showLoading,
        hideLoading,
        showError,
        cancelAll,
        cancel,
        config
    };
})();

// Make globally available
window.ajaxWrapper = ajaxWrapper;
