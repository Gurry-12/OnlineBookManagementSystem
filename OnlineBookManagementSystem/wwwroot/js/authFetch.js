// wwwroot/js/authFetch.js

// Function to get the JWT token from localStorage
function getJwtToken() {
    return localStorage.getItem('jwtToken');
}

/**
 * A fetch wrapper that injects the Authorization header.
 * @param {string} url - The URL to fetch.
 * @param {object} options - Fetch options (method, headers, body, etc.).
 * @returns {Promise<Response>}
 */
export async function authFetch(url, options = {}) {
    const token = getJwtToken();

    // Ensure headers object exists
    options.headers = options.headers || {};

    // Inject Authorization header if token exists
    if (token) {
        options.headers['Authorization'] = `Bearer ${token}`;
    }

    // Default to JSON content type if sending body and not form data
    if (options.body && !(options.body instanceof FormData) && !options.headers['Content-Type']) {
        options.headers['Content-Type'] = 'application/json';
    }

    const response = await fetch(url, options);

    // Handle 401 Unauthorized globally if needed (e.g., redirect to login)
    if (response.status === 401) {
        console.warn('Unauthorized access. Redirecting to login...');
        window.location.href = '/Auth/Login';
    }

    return response;
}
