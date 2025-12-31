// ~/js/auth/auth.js
$(document).ready(function () {
    // Configure toastr options
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

    // Initialize validation
    initializeValidation();

    // Update UI based on roles
    updateUIBasedOnRoles();
});

// Utility: Display/clear errors
function displayError(elementId, message) {
    $(`#${elementId}Error`).text(message).show();
}

function clearError(elementId) {
    $(`#${elementId}Error`).text('').hide();
}

function clearAllErrors() {
    $('.text-danger').text('').hide();
}

// Real-time validation (client-side)
function validateInput(selector, minLength, pattern = null, errorMsg = '') {
    $(selector).on('input blur', function () {
        const val = $(this).val().trim();
        const elementId = $(this).attr('id');

        if (val.length === 0) {
            clearError(elementId);
            return;
        }

        let valid = val.length >= minLength;
        if (pattern && !pattern.test(val)) valid = false;

        if (!valid) {
            displayError(elementId, errorMsg);
        } else {
            clearError(elementId);
        }
    });
}

// Initialize validation
function initializeValidation() {
    validateInput('#Name', 2, null, 'Name must be at least 2 characters.');
    validateInput('#Email', 1, /^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Please enter a valid email address.');
    validateInput('#Password', 6, null, 'Password must be at least 6 characters.');
    validateInput('#NewPassword', 6, null, 'New password must be at least 6 characters.');
    validateInput('#ConfirmPassword', 6, null, 'Confirm password must match.');
    validateInput('#ForgotEmail', 1, /^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Please enter a valid email address.');
}

// Update UI for roles (e.g., show admin links)
function updateUIBasedOnRoles() {
    const roles = JSON.parse(localStorage.getItem('userRoles') || '[]');
    if (!roles.includes('Admin') && !roles.includes('SuperAdmin')) {
        $('#adminLinks, #superAdminLinks').hide();
    }
    if (roles.includes('SuperAdmin')) {
        $('#superAdminLinks').show();
    }
}

// Login handler - Fixed and improved
$(document).on('click', '#LoginData', async function (e) {
    e.preventDefault();

    // Clear previous errors
    clearAllErrors();

    // Get form data
    const email = $('#Email').val().trim();
    const password = $('#Password').val().trim();

    // Basic validation
    if (!email) {
        displayError('Email', 'Email is required.');
        $('#Email').focus();
        return;
    }

    if (!password) {
        displayError('Password', 'Password is required.');
        $('#Password').focus();
        return;
    }

    // Email format validation
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
        displayError('Email', 'Please enter a valid email address.');
        $('#Email').focus();
        return;
    }

    // Show loading state
    const $button = $(this);
    const originalText = $button.html();
    $button.prop('disabled', true).html('<i class="bi bi-hourglass-split me-1"></i>Logging in...');

    const data = {
        Email: email,
        Password: password
    };

    try {
        const response = await fetch('/Auth/LoginData', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            // Store user data
            if (result.refreshToken) {
                localStorage.setItem('refreshToken', result.refreshToken);
            }
            if (result.roles) {
                localStorage.setItem('userRoles', JSON.stringify(result.roles));
            }
            if (result.userName) {
                localStorage.setItem('userName', result.userName);
            }

            // Show success message
            toastr.success(result.message || 'Welcome back!');

            // Update UI
            updateUIBasedOnRoles();

            // Start token refresh if needed
            if (result.refreshToken) {
                startTokenRefresh();
            }

            // Redirect after short delay
            setTimeout(() => {
                window.location.href = result.redirectUrl || '/User/Dashboard';
            }, 1500);

        } else {
            // Show error message
            toastr.error(result.message || 'Login failed. Please check your credentials.');

            // Focus on email field for retry
            $('#Email').focus();
        }

    } catch (error) {
        console.error('Login error:', error);
        toastr.error('Login failed. Please check your connection and try again.');
    } finally {
        // Restore button state
        $button.prop('disabled', false).html(originalText);
    }
});

// Registration handler
$(document).on('click', '#SubmitForm', async function (e) {
    e.preventDefault();

    clearAllErrors();

    const name = $('#Name').val().trim();
    const email = $('#Email').val().trim();
    const password = $('#Password').val().trim();

    // Validation
    let isValid = true;

    if (name.length < 2) {
        displayError('Name', 'Name must be at least 2 characters.');
        isValid = false;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        displayError('Email', 'Please enter a valid email address.');
        isValid = false;
    }

    if (password.length < 6) {
        displayError('Password', 'Password must be at least 6 characters.');
        isValid = false;
    }

    if (!isValid) return;

    const $button = $(this);
    const originalText = $button.html();
    $button.prop('disabled', true).html('<i class="bi bi-hourglass-split me-1"></i>Creating Account...');

    const data = { Name: name, Email: email, Password: password };

    try {
        const response = await fetch('/Auth/SaveData', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            toastr.success(result.message);
            setTimeout(() => {
                window.location.href = result.redirectUrl || '/Auth/Login';
            }, 2000);
        } else {
            toastr.error(result.message);
        }
    } catch (error) {
        toastr.error('Registration failed. Please try again.');
        console.error(error);
    } finally {
        $button.prop('disabled', false).html(originalText);
    }
});

// Forgot password handler - Fixed ID reference
$(document).on('click', '#SendResetLinkBtn', async function (e) {
    e.preventDefault();

    const email = $('#ForgotEmail').val().trim();

    if (!email) {
        toastr.error('Please enter your email address.');
        $('#ForgotEmail').focus();
        return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        toastr.error('Please enter a valid email address.');
        $('#ForgotEmail').focus();
        return;
    }

    const $button = $(this);
    const originalText = $button.html();
    $button.prop('disabled', true).html('<i class="bi bi-hourglass-split me-1"></i>Sending...');

    try {
        const response = await fetch('/Auth/ForgotPassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: email })
        });

        const result = await response.json();

        if (result.success) {
            toastr.success(result.message);
            $('#forgotModal').modal('hide');
            $('#ForgotEmail').val('');
        } else {
            toastr.error(result.message);
        }
    } catch (error) {
        toastr.error('Failed to send reset link. Please try again.');
        console.error(error);
    } finally {
        $button.prop('disabled', false).html(originalText);
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

        if (result.success && result.accessToken) {
            // Server sets new cookie
            console.log('Token refreshed successfully');
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
    // Refresh token every 50 minutes (tokens expire in 60 minutes)
    setInterval(refreshAccessToken, 50 * 60 * 1000);
    // Initial refresh check
    setTimeout(refreshAccessToken, 5000);
}

// Logout handler
$(document).on('click', '#LogoutBtn', async function (e) {
    e.preventDefault();

    try {
        const response = await fetch('/Auth/Logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        // Clear local storage regardless of response
        localStorage.clear();
        toastr.info('Logged out successfully.');

        // Redirect to login
        setTimeout(() => {
            window.location.href = '/Auth/Login';
        }, 1000);

    } catch (error) {
        console.error('Logout error:', error);
        // Still clear local storage and redirect
        localStorage.clear();
        window.location.href = '/Auth/Login';
    }
});

// Reset password handler
$(document).on('click', '#ResetPasswordBtn', async function (e) {
    e.preventDefault();

    const newPassword = $('#NewPassword').val().trim();
    const confirmPassword = $('#ConfirmPassword').val().trim();
    const token = new URLSearchParams(window.location.search).get('token');

    if (!newPassword || !confirmPassword) {
        toastr.error('Please fill in both password fields.');
        return;
    }

    if (newPassword !== confirmPassword) {
        toastr.error('Passwords don\'t match.');
        return;
    }

    if (newPassword.length < 6) {
        toastr.error('Password must be at least 6 characters.');
        return;
    }

    const $button = $(this);
    const originalText = $button.html();
    $button.prop('disabled', true).html('<i class="bi bi-hourglass-split me-1"></i>Resetting...');

    const data = {
        Token: token,
        NewPassword: newPassword,
        ConfirmPassword: confirmPassword
    };

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
        toastr.error('Password reset failed. Please try again.');
        console.error(error);
    } finally {
        $button.prop('disabled', false).html(originalText);
    }
});

// Profile update handler
$(document).on('click', '#UpdateProfileBtn', async function (e) {
    e.preventDefault();

    const name = $('#Name').val().trim();
    const email = $('#Email').val().trim();
    const userId = $('#UserId').val();

    if (!name || !email) {
        toastr.error('Please fill in all fields.');
        return;
    }

    const $button = $(this);
    const originalText = $button.html();
    $button.prop('disabled', true).html('<i class="bi bi-hourglass-split me-1"></i>Updating...');

    const data = {
        Id: userId,
        NewName: name,
        NewEmail: email
    };

    try {
        const response = await fetch('/Auth/UpdateUserDetails', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            toastr.success('Profile updated successfully.');
            localStorage.setItem('userName', name);
            updateUIBasedOnRoles();
        } else {
            toastr.error(result.message);
        }
    } catch (error) {
        toastr.error('Profile update failed. Please try again.');
        console.error(error);
    } finally {
        $button.prop('disabled', false).html(originalText);
    }
});

// Utility functions
function getAccessToken() {
    // Parse cookie for access token (if not HttpOnly)
    return document.cookie.split('; ').find(row => row.startsWith('accessToken='))?.split('=')[1] || '';
}

function logout() {
    localStorage.clear();
    window.location.href = '/Auth/Login';
}

// Handle form submission with Enter key
$(document).on('keypress', '#loginForm input', function (e) {
    if (e.which === 13) { // Enter key
        e.preventDefault();
        $('#LoginData').click();
    }
});

// Clear error messages when user starts typing
$(document).on('input', 'input', function () {
    const elementId = $(this).attr('id');
    if (elementId) {
        clearError(elementId);
    }
});