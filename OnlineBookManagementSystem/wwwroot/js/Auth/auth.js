// ~/js/auth/auth.js
toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": false,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": true,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "3000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};

// Utility: Display/clear errors
function displayError(elementId, message) {
    $(`#${elementId}Error`).text(message).show();
}

function clearError(elementId) {
    $(`#${elementId}Error`).text('').hide();
}

// Real-time validation (client-side)
function validateInput(selector, minLength, pattern = null, errorMsg = '') {
    $(selector).on('input', function () {
        const val = $(this).val();
        let valid = val.length >= minLength;
        if (pattern && !pattern.test(val)) valid = false;
        if (!valid) displayError($(this).attr('id'), errorMsg);
        else clearError($(this).attr('id'));
    });
}

// Init validation
$(document).ready(function () {
    validateInput('#Name', 2, null, 'Name must be at least 2 characters.');
    validateInput('#Email', 1, /^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Invalid email.');
    validateInput('#Password', 8, null, 'Password must be at least 8 characters.');
    validateInput('#NewPassword', 8, null, 'New password must be at least 8 characters.');
    validateInput('#ConfirmPassword', 8, null, 'Confirm password must match.');

    // Role-based UI (hide/show based on sessionStorage roles)
    updateUIBasedOnRoles();
});

// Update UI for roles (e.g., show admin links)
function updateUIBasedOnRoles() {
    const roles = JSON.parse(localStorage.getItem('userRoles') || '[]');
    if (!roles.includes('Admin') && !roles.includes('SuperAdmin')) {
        $('#adminLinks, #superAdminLinks').hide();  // Assume IDs in layout
    }
    if (roles.includes('SuperAdmin')) {
        $('#superAdminLinks').show();
    }
}

// Registration handler
$('#SubmitForm').on('click', async function (e) {
    e.preventDefault();

    const data = {
        Name: $('#Name').val().trim(),
        Email: $('#Email').val().trim(),
        Password: $('#Password').val().trim()
    };

    // Final validation
    let isValid = true;
    if (data.Name.length < 2) { displayError('Name', 'Name too short.'); isValid = false; }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.Email)) { displayError('Email', 'Invalid email.'); isValid = false; }
    if (data.Password.length < 8) { displayError('Password', 'Password too short.'); isValid = false; }
    if (!isValid) return;

    try {
        const response = await fetch('/Auth/SaveData', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const result = await response.json();

        if (result.success) {
            toastr.success(result.message);
            // Redirect to confirmation
            setTimeout(() => {
                window.location.href = result.redirectUrl;
            }, 2000);
        } else {
            toastr.error(result.message);
        }
    } catch (error) {
        toastr.error('Registration failed. Try again.');
        console.error(error);
    }
});

// Login handler
$('#LoginData').on('click', async function (e) {
    e.preventDefault();

    debugger;
    const data = {
        Email: $('#Email').val().trim(),
        Password: $('#Password').val().trim()
    };

    if (!data.Email || !data.Password) {
        toastr.error('Enter email and password.');
        return;
    }

    try {
        const response = await fetch('/Auth/LoginData', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const result = await response.json();
        debugger;
        if (result.success) {
            // Store refresh token securely
            localStorage.setItem('refreshToken', result.refreshToken);
            localStorage.setItem('jwtToken', result.accessToken); // Store for authFetch
            localStorage.setItem('userRoles', JSON.stringify(result.roles));
            localStorage.setItem('userName', result.userName);
            debugger;
            // Access token set as HttpOnly cookie by server
            toastr.success('Welcome back!');
            updateUIBasedOnRoles();

            // Auto-refresh access token (every 10 min)
            startTokenRefresh();

            setTimeout(() => {
                window.location.href = result.redirectUrl;
            }, 1500);
        } else {
            toastr.error(result.message || 'Login failed.');
        }
    } catch (error) {
        toastr.error('Login error. Check connection.');
        console.error(error);
    }
});

// Token refresh function
async function refreshAccessToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) return;

    try {
        const response = await fetch('/Auth/RefreshToken', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken })
        });
        const result = await response.json();

        if (result.accessToken) {
            // Server sets new cookie
            localStorage.setItem('jwtToken', result.accessToken); // Update local storage
            toastr.info('Session refreshed.');
        } else {
            // Invalid refresh: Logout
            logout();
        }
    } catch (error) {
        console.error('Refresh failed:', error);
        logout();
    }
}

function startTokenRefresh() {
    setInterval(refreshAccessToken, 10 * 60 * 1000);  // 10 min
    refreshAccessToken();  // Initial if near expiry
}

// Logout
$('#LogoutBtn').on('click', async function (e) {
    e.preventDefault();
    try {
        const response = await fetch('/Auth/Logout', { method: 'POST' });
        const result = await response.json();
        localStorage.clear();  // Clear all
        toastr.info('Logged out.');
        window.location.href = '/Auth/Login';
    } catch (error) {
        console.error('Logout error:', error);
    }
});

// Password reset handlers
$('#ForgotPasswordBtn').on('click', async function (e) {
    e.preventDefault();
    const email = $('#ForgotEmail').val().trim();
    if (!email) return toastr.error('Enter email.');

    try {
        const response = await fetch('/Auth/ForgotPassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email })
        });
        const result = await response.json();
        toastr.success(result.message);
    } catch (error) {
        toastr.error('Reset request failed.');
    }
});

$('#ResetPasswordBtn').on('click', async function (e) {
    e.preventDefault();
    const data = {
        Token: new URLSearchParams(window.location.search).get('token'),
        NewPassword: $('#NewPassword').val().trim(),
        ConfirmPassword: $('#ConfirmPassword').val().trim()
    };

    if (data.NewPassword !== data.ConfirmPassword) {
        toastr.error('Passwords don\'t match.');
        return;
    }

    try {
        const response = await fetch('/Auth/ResetPassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const result = await response.json();

        if (result.success) {
            toastr.success(result.message);
            setTimeout(() => window.location.href = '/Auth/Login', 2000);
        } else {
            toastr.error(result.message);
        }
    } catch (error) {
        toastr.error('Reset failed.');
    }
});

// Profile update (for EditProfile view)
$('#UpdateProfileBtn').on('click', async function (e) {
    e.preventDefault();
    const data = {
        Id: $('#UserId').val(),
        NewName: $('#Name').val().trim(),
        NewEmail: $('#Email').val().trim()
    };

    try {
        const response = await fetch('/Auth/UpdateUserDetails', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${getAccessToken()}` },  // If not cookie
            body: JSON.stringify(data)
        });
        const result = await response.json();

        if (result.success) {
            toastr.success('Profile updated.');
            updateUIBasedOnRoles();  // Refresh if roles changed
        } else {
            toastr.error(result.message);
        }
    } catch (error) {
        toastr.error('Update failed.');
    }
});

// Utility: Get access token (from cookie if HttpOnly)
function getAccessToken() {
    // Parse cookie; for HttpOnly, server handles auth
    return document.cookie.split('; ').find(row => row.startsWith('accessToken='))?.split('=')[1] || '';
}

function logout() {
    localStorage.clear();
    window.location.href = '/Auth/Login';
}