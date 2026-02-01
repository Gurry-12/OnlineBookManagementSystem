/**
 * UNIFIED HTTP CLIENT
 * Consolidates apiClient.js, ajaxWrapper.js, and unified-interactions.js
 * Provides both fetch and jQuery AJAX support with consistent error handling
 * Eliminates code duplication across multiple JS files
 */

class UnifiedHttpClient {
    constructor() {
        this.baseUrl = '';
        this.defaultTimeout = 30000;
        this.retryAttempts = 3;
        this.retryDelay = 1000;
        this.loadingStates = new Map();
        this.requestCounter = 0;

        // Initialize on DOM ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initialize());
        } else {
            this.initialize();
        }
    }

    /**
     * Initialize client and global event handlers
     */
    initialize() {
        this.initializeToastr();
        this.initializeGlobalHandlers();
        this.initializeCsrfToken();
    }

    /**
     * Configure toastr notifications
     */
    initializeToastr() {
        if (typeof toastr !== 'undefined') {
            toastr.options = {
                closeButton: true,
                debug: false,
                newestOnTop: true,
                progressBar: true,
                positionClass: "toast-top-right",
                preventDuplicates: true,
                showDuration: "300",
                hideDuration: "1000",
                timeOut: "4000",
                extendedTimeOut: "1000",
                showEasing: "swing",
                hideEasing: "linear",
                showMethod: "fadeIn",
                hideMethod: "fadeOut"
            };
        }
    }

    /**
     * Initialize global event handlers
     */
    initializeGlobalHandlers() {
        // Global form submission handler
        document.addEventListener('submit', (e) => {
            if (e.target.classList.contains('wp-form-ajax')) {
                e.preventDefault();
                this.submitForm(e.target);
            }
        });

        // Global button click handler
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('wp-btn-ajax')) {
                e.preventDefault();
                this.handleButtonClick(e.target);
            }
        });

        // Global AJAX error handler
        $(document).ajaxError((event, xhr, settings, thrownError) => {
            this.handleGlobalError(xhr, settings, thrownError);
        });
    }

    /**
     * Initialize CSRF token
     */
    initializeCsrfToken() {
        this.csrfToken = this.getCsrfToken();

        // Set up CSRF for jQuery AJAX
        if (typeof $ !== 'undefined') {
            $.ajaxSetup({
                beforeSend: (xhr, settings) => {
                    if (!/^(GET|HEAD|OPTIONS|TRACE)$/i.test(settings.type) && !this.crossDomain) {
                        xhr.setRequestHeader("X-CSRF-TOKEN", this.csrfToken);
                    }
                }
            });
        }
    }

    /**
     * Get CSRF token from various sources
     */
    getCsrfToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value
            || document.querySelector('meta[name="csrf-token"]')?.content
            || $('input[name="__RequestVerificationToken"]').val()
            || '';
    }

    /**
     * Build headers for requests
     */
    buildHeaders(customHeaders = {}) {
        const headers = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };

        if (this.csrfToken) {
            headers['X-CSRF-TOKEN'] = this.csrfToken;
        }

        const authToken = localStorage.getItem('token');
        if (authToken) {
            headers['Authorization'] = `Bearer ${authToken}`;
        }

        return { ...headers, ...customHeaders };
    }

    /**
     * FETCH-BASED METHODS
     */

    /**
     * Generic fetch request with error handling and retries
     */
    async fetchRequest(url, options = {}) {
        const requestId = ++this.requestCounter;

        const config = {
            method: 'GET',
            headers: this.buildHeaders(options.headers),
            ...options
        };

        // Show loading if element specified
        if (options.loadingElement) {
            this.showLoading(options.loadingElement, options.loadingMessage);
        }

        try {
            const response = await this.fetchWithRetry(url, config);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await this.parseResponse(response);

            if (options.loadingElement) {
                this.hideLoading(options.loadingElement);
            }

            return { success: true, data, response };

        } catch (error) {
            if (options.loadingElement) {
                this.hideLoading(options.loadingElement);
            }

            this.handleError(error, options);
            return { success: false, error: error.message };
        }
    }

    /**
     * Fetch with retry logic
     */
    async fetchWithRetry(url, config, attempt = 1) {
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), this.defaultTimeout);

            const response = await fetch(url, {
                ...config,
                signal: controller.signal
            });

            clearTimeout(timeoutId);
            return response;

        } catch (error) {
            if (attempt < this.retryAttempts && this.shouldRetry(error)) {
                await this.delay(this.retryDelay * attempt);
                return this.fetchWithRetry(url, config, attempt + 1);
            }
            throw error;
        }
    }

    /**
     * Parse response based on content type
     */
    async parseResponse(response) {
        const contentType = response.headers.get('content-type');

        if (contentType?.includes('application/json')) {
            return await response.json();
        } else if (contentType?.includes('text/html')) {
            return await response.text();
        } else {
            return await response.text();
        }
    }

    /**
     * GET request
     */
    async get(url, options = {}) {
        return this.fetchRequest(url, { ...options, method: 'GET' });
    }

    /**
     * POST request
     */
    async post(url, data = null, options = {}) {
        return this.fetchRequest(url, {
            ...options,
            method: 'POST',
            body: data ? JSON.stringify(data) : null
        });
    }

    /**
     * PUT request
     */
    async put(url, data = null, options = {}) {
        return this.fetchRequest(url, {
            ...options,
            method: 'PUT',
            body: data ? JSON.stringify(data) : null
        });
    }

    /**
     * DELETE request
     */
    async delete(url, options = {}) {
        return this.fetchRequest(url, { ...options, method: 'DELETE' });
    }

    /**
     * JQUERY AJAX METHODS (for backward compatibility)
     */

    /**
     * jQuery AJAX wrapper with consistent error handling
     */
    ajax(options) {
        const defaults = {
            timeout: this.defaultTimeout,
            headers: {
                'X-CSRF-TOKEN': this.csrfToken,
                'X-Requested-With': 'XMLHttpRequest'
            },
            beforeSend: (xhr, settings) => {
                if (options.loadingElement) {
                    this.showLoading(options.loadingElement, options.loadingMessage);
                }
            },
            complete: (xhr, status) => {
                if (options.loadingElement) {
                    this.hideLoading(options.loadingElement);
                }
            },
            error: (xhr, status, error) => {
                this.handleAjaxError(xhr, status, error, options);
            }
        };

        return $.ajax({ ...defaults, ...options });
    }

    /**
     * Load content into container via AJAX
     */
    load(url, container, options = {}) {
        const $container = typeof container === 'string' ? $(container) : container;

        if (!$container.length) {
            console.error('Container not found:', container);
            return Promise.reject('Container not found');
        }

        this.showLoading($container[0], options.loadingMessage);

        return this.ajax({
            url: url,
            method: 'GET',
            success: (data) => {
                $container.html(data);
                this.hideLoading($container[0]);

                // Trigger custom event
                $container.trigger('contentLoaded', [data]);

                if (options.onSuccess) {
                    options.onSuccess(data);
                }
            },
            error: (xhr, status, error) => {
                this.hideLoading($container[0]);
                this.showError($container[0], 'Failed to load content');

                if (options.onError) {
                    options.onError(xhr, status, error);
                }
            }
        });
    }

    /**
     * Submit form via AJAX
     */
    submitForm(form, options = {}) {
        const $form = $(form);
        const formData = new FormData(form);
        const url = form.action || options.url;
        const method = form.method || options.method || 'POST';

        // Convert FormData to JSON if needed
        const data = options.json ? this.formDataToJson(formData) : formData;

        return this.ajax({
            url: url,
            method: method,
            data: data,
            processData: !options.json,
            contentType: options.json ? 'application/json' : false,
            loadingElement: options.loadingElement || form,
            success: (response) => {
                if (response.success) {
                    this.showSuccess(response.message || 'Form submitted successfully');

                    if (response.redirect) {
                        window.location.href = response.redirect;
                    }
                } else {
                    this.showError(response.message || 'Form submission failed');
                }

                if (options.onSuccess) {
                    options.onSuccess(response);
                }
            },
            error: (xhr, status, error) => {
                this.showError('Form submission failed');

                if (options.onError) {
                    options.onError(xhr, status, error);
                }
            }
        });
    }

    /**
     * LOADING STATE MANAGEMENT
     */

    /**
     * Show loading state
     */
    showLoading(element, message = 'Loading...') {
        const $element = $(element);

        if (!$element.length) return;

        $element.addClass('loading').attr('aria-busy', 'true');

        // Store original content
        if (!$element.data('original-content')) {
            $element.data('original-content', $element.html());
        }

        // Remove existing loading overlay
        $element.find('[data-loading-overlay]').remove();

        // Add loading overlay
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

        $element.append(loadingHtml);
        this.loadingStates.set(element, true);
    }

    /**
     * Hide loading state
     */
    hideLoading(element) {
        const $element = $(element);

        if (!$element.length) return;

        $element.removeClass('loading').removeAttr('aria-busy');
        $element.find('[data-loading-overlay]').remove();

        this.loadingStates.delete(element);
    }

    /**
     * Show error in container
     */
    showError(element, message = 'An error occurred') {
        const $element = $(element);

        if (!$element.length) return;

        const errorHtml = `
            <div class="wp-alert wp-alert-error" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
            </div>
        `;

        $element.prepend(errorHtml);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            $element.find('.wp-alert-error').fadeOut();
        }, 5000);
    }

    /**
     * NOTIFICATION METHODS
     */

    showSuccess(message, title = 'Success') {
        if (typeof toastr !== 'undefined') {
            toastr.success(message, title);
        } else {
            console.log(`Success: ${title} - ${message}`);
        }
    }

    showError(message, title = 'Error') {
        if (typeof toastr !== 'undefined') {
            toastr.error(message, title);
        } else {
            console.error(`Error: ${title} - ${message}`);
        }
    }

    showWarning(message, title = 'Warning') {
        if (typeof toastr !== 'undefined') {
            toastr.warning(message, title);
        } else {
            console.warn(`Warning: ${title} - ${message}`);
        }
    }

    showInfo(message, title = 'Info') {
        if (typeof toastr !== 'undefined') {
            toastr.info(message, title);
        } else {
            console.info(`Info: ${title} - ${message}`);
        }
    }

    /**
     * ERROR HANDLING
     */

    handleError(error, options = {}) {
        console.error('HTTP Client Error:', error);

        if (!options.silent) {
            this.showError(error.message || 'An unexpected error occurred');
        }

        if (options.onError) {
            options.onError(error);
        }
    }

    handleAjaxError(xhr, status, error, options = {}) {
        let message = 'An error occurred';

        if (xhr.status === 401) {
            message = 'Authentication required';
            // Redirect to login if needed
            if (options.redirectOnAuth !== false) {
                window.location.href = '/Auth/Login';
                return;
            }
        } else if (xhr.status === 403) {
            message = 'Access denied';
        } else if (xhr.status === 404) {
            message = 'Resource not found';
        } else if (xhr.status === 422) {
            message = 'Validation failed';
        } else if (xhr.status === 429) {
            message = 'Too many requests';
        } else if (xhr.status >= 500) {
            message = 'Server error';
        }

        if (!options.silent) {
            this.showError(message);
        }
    }

    handleGlobalError(xhr, settings, thrownError) {
        console.error('Global AJAX Error:', {
            url: settings.url,
            status: xhr.status,
            error: thrownError
        });
    }

    /**
     * UTILITY METHODS
     */

    shouldRetry(error) {
        return error.name === 'AbortError' ||
            error.message.includes('network') ||
            error.message.includes('timeout');
    }

    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    formDataToJson(formData) {
        const object = {};
        formData.forEach((value, key) => {
            object[key] = value;
        });
        return JSON.stringify(object);
    }

    handleButtonClick(button) {
        const $button = $(button);
        const url = $button.data('url') || $button.attr('href');
        const method = $button.data('method') || 'GET';
        const confirm = $button.data('confirm');

        if (confirm && !window.confirm(confirm)) {
            return;
        }

        if (method.toLowerCase() === 'get') {
            window.location.href = url;
        } else {
            this.ajax({
                url: url,
                method: method,
                loadingElement: button,
                success: (response) => {
                    if (response.success) {
                        this.showSuccess(response.message || 'Operation completed');

                        if (response.redirect) {
                            window.location.href = response.redirect;
                        } else if (response.reload) {
                            window.location.reload();
                        }
                    }
                }
            });
        }
    }
}

// Initialize global instance
const httpClient = new UnifiedHttpClient();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UnifiedHttpClient;
}

// Global functions for backward compatibility
window.showSuccess = (message, title) => httpClient.showSuccess(message, title);
window.showError = (message, title) => httpClient.showError(message, title);
window.showWarning = (message, title) => httpClient.showWarning(message, title);
window.showInfo = (message, title) => httpClient.showInfo(message, title);

// Book-specific functions
window.initializeBookDetails = function (bookId, roleContext) {
    // Quantity controls
    $('#increaseQty').on('click', function () {
        const $qty = $('#quantity');
        const max = parseInt($qty.attr('max'));
        const current = parseInt($qty.val());
        if (current < max) {
            $qty.val(current + 1);
        }
    });

    $('#decreaseQty').on('click', function () {
        const $qty = $('#quantity');
        const min = parseInt($qty.attr('min'));
        const current = parseInt($qty.val());
        if (current > min) {
            $qty.val(current - 1);
        }
    });

    // Add to cart
    $('#addToCartBtn').on('click', function () {
        const quantity = parseInt($('#quantity').val());
        httpClient.post('/Cart/AddToCart', {
            bookId: bookId,
            quantity: quantity
        }, {
            loadingElement: this,
            onSuccess: (response) => {
                if (response.success) {
                    httpClient.showSuccess('Book added to cart successfully');
                }
            }
        });
    });

    // Toggle favorite
    $('#favoriteBtn').on('click', function () {
        httpClient.post(`/UniversalBooks/ToggleFavorite/${bookId}`, null, {
            loadingElement: this,
            onSuccess: (response) => {
                if (response.success) {
                    const $btn = $(this);
                    const isFavorite = $btn.hasClass('btn-danger');

                    if (isFavorite) {
                        $btn.removeClass('btn-danger').addClass('btn-outline-danger');
                        $btn.html('<i class="fas fa-heart me-2"></i>Add to Favorites');
                        httpClient.showSuccess('Removed from favorites');
                    } else {
                        $btn.removeClass('btn-outline-danger').addClass('btn-danger');
                        $btn.html('<i class="fas fa-heart me-2"></i>Remove from Favorites');
                        httpClient.showSuccess('Added to favorites');
                    }
                }
            }
        });
    });

    // Delete book (admin only)
    $('#confirmDeleteBtn').on('click', function () {
        httpClient.delete(`/UniversalBooks/Delete/${bookId}`, {
            loadingElement: this,
            onSuccess: (response) => {
                if (response.success) {
                    httpClient.showSuccess('Book deleted successfully');
                    setTimeout(() => {
                        window.location.href = '/UniversalBooks/List';
                    }, 1500);
                }
            }
        });
    });
};

window.loadBookReviews = function (bookId) {
    httpClient.load(`/Review/GetBookReviews/${bookId}`, '#reviewsList');
};