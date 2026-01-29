/**
 * Reusable AJAX API Client for Online Book Management System
 * Handles fetch requests, CSRF tokens, loading states, and error handling
 */

class ApiClient {
    constructor() {
        this.baseUrl = '';
        this.defaultHeaders = {
            'Content-Type': 'application/json'
        };
        this.loadingStates = new Map();
    }

    /**
     * Get CSRF token from various sources
     */
    getCsrfToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value
            || document.querySelector('meta[name="csrf-token"]')?.content
            || '';
    }

    /**
     * Get authorization token if available
     */
    getAuthToken() {
        return localStorage.getItem('token') || '';
    }

    /**
     * Build headers for request
     */
    buildHeaders(customHeaders = {}) {
        const headers = { ...this.defaultHeaders };

        const csrfToken = this.getCsrfToken();
        if (csrfToken) {
            headers['X-CSRF-TOKEN'] = csrfToken;
        }

        const authToken = this.getAuthToken();
        if (authToken) {
            headers['Authorization'] = `Bearer ${authToken}`;
        }

        return { ...headers, ...customHeaders };
    }

    /**
     * Show loading state for a specific element or operation
     */
    showLoading(elementId, message = 'Loading...') {
        const element = document.getElementById(elementId);
        if (element) {
            element.classList.add('loading');
            element.setAttribute('data-original-content', element.innerHTML);
            element.innerHTML = `
                <div class="d-flex align-items-center justify-content-center">
                    <div class="spinner-border spinner-border-sm me-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    ${message}
                </div>
            `;
            element.disabled = true;
        }
        this.loadingStates.set(elementId, true);
    }

    /**
     * Hide loading state
     */
    hideLoading(elementId) {
        const element = document.getElementById(elementId);
        if (element && this.loadingStates.get(elementId)) {
            element.classList.remove('loading');
            const originalContent = element.getAttribute('data-original-content');
            if (originalContent) {
                element.innerHTML = originalContent;
                element.removeAttribute('data-original-content');
            }
            element.disabled = false;
        }
        this.loadingStates.delete(elementId);
    }

    /**
     * Generic fetch wrapper with comprehensive error handling
     */
    async request(url, options = {}) {
        const config = {
            method: 'GET',
            headers: this.buildHeaders(options.headers),
            ...options
        };

        try {
            // Validate URL
            if (!url || typeof url !== 'string') {
                throw new Error('Invalid URL provided');
            }

            const response = await fetch(url, config);

            // Handle different HTTP status codes
            if (!response.ok) {
                let errorMessage = `HTTP ${response.status}: ${response.statusText}`;

                try {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        const errorData = await response.json();
                        errorMessage = errorData.message || errorData.error || errorMessage;
                    } else {
                        const errorText = await response.text();
                        if (errorText) {
                            errorMessage = errorText.substring(0, 200); // Limit error message length
                        }
                    }
                } catch (parseError) {
                    console.warn('Failed to parse error response:', parseError);
                }

                // Handle specific status codes
                switch (response.status) {
                    case 401:
                        // Unauthorized - redirect to login
                        if (window.location.pathname !== '/Auth/Login') {
                            window.location.href = '/Auth/Login';
                        }
                        throw new Error('Authentication required');
                    case 403:
                        throw new Error('Access denied');
                    case 404:
                        throw new Error('Resource not found');
                    case 429:
                        throw new Error('Too many requests. Please try again later.');
                    case 500:
                        throw new Error('Server error. Please try again later.');
                    default:
                        throw new Error(errorMessage);
                }
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }

            return await response.text();
        } catch (error) {
            // Network errors, timeout, etc.
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                console.error('Network error:', error);
                throw new Error('Network connection failed. Please check your internet connection.');
            }

            if (error.name === 'AbortError') {
                console.error('Request aborted:', error);
                throw new Error('Request was cancelled');
            }

            console.error('API Request failed:', error);
            throw error;
        }
    }

    /**
     * GET request
     */
    async get(url, options = {}) {
        return this.request(url, { ...options, method: 'GET' });
    }

    /**
     * POST request with enhanced error handling
     */
    async post(url, data = null, options = {}) {
        const config = { ...options, method: 'POST' };

        try {
            if (data) {
                if (data instanceof FormData) {
                    // Remove Content-Type header for FormData (browser sets it with boundary)
                    const headers = { ...config.headers };
                    delete headers['Content-Type'];
                    config.headers = headers;
                    config.body = data;
                } else if (typeof data === 'object') {
                    try {
                        config.body = JSON.stringify(data);
                    } catch (jsonError) {
                        console.error('Failed to serialize request data:', jsonError);
                        throw new Error('Invalid request data format');
                    }
                } else {
                    config.body = data;
                }
            }

            return await this.request(url, config);
        } catch (error) {
            console.error('POST request failed:', error);
            throw error;
        }
    }

    /**
     * PUT request
     */
    async put(url, data = null, options = {}) {
        return this.post(url, data, { ...options, method: 'PUT' });
    }

    /**
     * DELETE request
     */
    async delete(url, options = {}) {
        return this.request(url, { ...options, method: 'DELETE' });
    }

    /**
     * Upload file with progress tracking
     */
    async uploadFile(url, formData, onProgress = null) {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();

            if (onProgress) {
                xhr.upload.addEventListener('progress', (e) => {
                    if (e.lengthComputable) {
                        const percentComplete = (e.loaded / e.total) * 100;
                        onProgress(percentComplete);
                    }
                });
            }

            xhr.addEventListener('load', () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        resolve(response);
                    } catch (e) {
                        resolve(xhr.responseText);
                    }
                } else {
                    reject(new Error(`HTTP ${xhr.status}: ${xhr.statusText}`));
                }
            });

            xhr.addEventListener('error', () => {
                reject(new Error('Network error occurred'));
            });

            xhr.open('POST', url);

            // Add CSRF token header
            const csrfToken = this.getCsrfToken();
            if (csrfToken) {
                xhr.setRequestHeader('X-CSRF-TOKEN', csrfToken);
            }

            const authToken = this.getAuthToken();
            if (authToken) {
                xhr.setRequestHeader('Authorization', `Bearer ${authToken}`);
            }

            xhr.send(formData);
        });
    }
}

// Create global instance
window.apiClient = new ApiClient();