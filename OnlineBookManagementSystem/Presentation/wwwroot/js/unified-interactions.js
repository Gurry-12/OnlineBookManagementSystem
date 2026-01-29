// unified-interactions.js - Consistent UI interactions across all pages

$(document).ready(function () {
    // Initialize unified components
    initializeUnifiedComponents();

    // Initialize toastr with consistent settings
    initializeNotifications();

    // Initialize form validation
    initializeFormValidation();

    // Initialize sidebar functionality
    initializeSidebar();

    // Initialize time and date display
    initializeTimeDate();
});

// Initialize all unified components
function initializeUnifiedComponents() {
    // Add hover effects to wp-card elements
    $('.wp-card').hover(
        function () {
            $(this).addClass('wp-card-hover');
        },
        function () {
            $(this).removeClass('wp-card-hover');
        }
    );

    // Initialize button loading states
    $('.wp-btn').on('click', function () {
        const $btn = $(this);
        if ($btn.hasClass('wp-btn-loading')) return false;

        if ($btn.attr('type') === 'submit' || $btn.hasClass('wp-btn-submit')) {
            showButtonLoading($btn);
        }
    });

    // Initialize form control focus effects
    $('.wp-form-control').on('focus', function () {
        $(this).closest('.wp-form-group').addClass('wp-form-group-focused');
    }).on('blur', function () {
        $(this).closest('.wp-form-group').removeClass('wp-form-group-focused');
    });
}

// Initialize consistent notifications
function initializeNotifications() {
    // Configure toastr options
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": true,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "4000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut",
        "toastClass": "wp-toast",
        "iconClasses": {
            error: 'wp-toast-error',
            info: 'wp-toast-info',
            success: 'wp-toast-success',
            warning: 'wp-toast-warning'
        }
    };
}

// Unified notification functions
function showSuccess(message, title = 'Success') {
    toastr.success(message, title);
}

function showError(message, title = 'Error') {
    toastr.error(message, title);
}

function showWarning(message, title = 'Warning') {
    toastr.warning(message, title);
}

function showInfo(message, title = 'Info') {
    toastr.info(message, title);
}

// Initialize form validation
function initializeFormValidation() {
    // Real-time validation for wp-form-control elements
    $('.wp-form-control').on('input blur', function () {
        validateField($(this));
    });

    // Form submission validation
    $('form').on('submit', function (e) {
        const $form = $(this);
        let isValid = true;

        $form.find('.wp-form-control[required]').each(function () {
            if (!validateField($(this))) {
                isValid = false;
            }
        });

        if (!isValid) {
            e.preventDefault();
            showError('Please fix the errors in the form before submitting.');
        }
    });
}

// Validate individual field
function validateField($field) {
    const value = $field.val().trim();
    const $errorSpan = $field.siblings('.wp-form-error');
    const fieldName = $field.attr('name') || $field.attr('id') || 'Field';

    // Clear previous errors
    $field.removeClass('is-invalid');
    $errorSpan.text('').hide();

    // Required field validation
    if ($field.attr('required') && !value) {
        showFieldError($field, $errorSpan, `${fieldName} is required.`);
        return false;
    }

    // Email validation
    if ($field.attr('type') === 'email' && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
            showFieldError($field, $errorSpan, 'Please enter a valid email address.');
            return false;
        }
    }

    // Password validation
    if ($field.attr('type') === 'password' && value) {
        if (value.length < 6) {
            showFieldError($field, $errorSpan, 'Password must be at least 6 characters long.');
            return false;
        }
    }

    // Number validation
    if ($field.attr('type') === 'number' && value) {
        const min = parseFloat($field.attr('min'));
        const max = parseFloat($field.attr('max'));
        const numValue = parseFloat(value);

        if (isNaN(numValue)) {
            showFieldError($field, $errorSpan, 'Please enter a valid number.');
            return false;
        }

        if (!isNaN(min) && numValue < min) {
            showFieldError($field, $errorSpan, `Value must be at least ${min}.`);
            return false;
        }

        if (!isNaN(max) && numValue > max) {
            showFieldError($field, $errorSpan, `Value must not exceed ${max}.`);
            return false;
        }
    }

    return true;
}

// Show field error
function showFieldError($field, $errorSpan, message) {
    $field.addClass('is-invalid');
    $errorSpan.text(message).show();
}

// Clear field error
function clearFieldError($field) {
    const $errorSpan = $field.siblings('.wp-form-error');
    $field.removeClass('is-invalid');
    $errorSpan.text('').hide();
}

// Initialize sidebar functionality
function initializeSidebar() {
    $('#toggleSidebar').on('click', function () {
        const $sidebar = $('.wp-sidebar');
        $sidebar.toggleClass('collapsed');

        // Save state to localStorage
        localStorage.setItem('sidebarCollapsed', $sidebar.hasClass('collapsed'));
    });

    // Restore sidebar state
    const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
    if (sidebarCollapsed) {
        $('.wp-sidebar').addClass('collapsed');
    }

    // Mobile sidebar toggle
    $(document).on('click', '.mobile-sidebar-toggle', function () {
        $('.wp-sidebar').toggleClass('mobile-open');
    });

    // Close mobile sidebar when clicking outside
    $(document).on('click', function (e) {
        if ($(window).width() <= 768) {
            if (!$(e.target).closest('.wp-sidebar, .mobile-sidebar-toggle').length) {
                $('.wp-sidebar').removeClass('mobile-open');
            }
        }
    });
}

// Initialize time and date display
function initializeTimeDate() {
    function updateDateTime() {
        const now = new Date();
        const timeString = now.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        });
        const dateString = now.toLocaleDateString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric'
        });

        $('.currenttime').text(timeString);
        $('.currentdate').text(dateString);
    }

    // Update immediately and then every minute
    updateDateTime();
    setInterval(updateDateTime, 60000);
}

// Button loading state
function showButtonLoading($btn) {
    const originalText = $btn.html();
    $btn.data('original-text', originalText);
    $btn.addClass('wp-btn-loading');
    $btn.prop('disabled', true);
    $btn.html('<i class="bi bi-arrow-repeat spin me-2"></i>Loading...');
}

function hideButtonLoading($btn) {
    const originalText = $btn.data('original-text');
    $btn.removeClass('wp-btn-loading');
    $btn.prop('disabled', false);
    $btn.html(originalText);
}

// Unified AJAX helper
function makeAjaxRequest(options) {
    const defaults = {
        type: 'POST',
        dataType: 'json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        beforeSend: function () {
            if (options.$button) {
                showButtonLoading(options.$button);
            }
        },
        complete: function () {
            if (options.$button) {
                hideButtonLoading(options.$button);
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error:', error);
            showError('An error occurred. Please try again.');
        }
    };

    return $.ajax($.extend({}, defaults, options));
}

// Password toggle functionality
$(document).on('click', '.toggle-password', function () {
    const $input = $(this).prev('input');
    const $icon = $(this).find('[toggle-password-icon]');

    if ($input.attr('type') === 'password') {
        $input.attr('type', 'text');
        $icon.removeClass('bi-eye-slash').addClass('bi-eye');
    } else {
        $input.attr('type', 'password');
        $icon.removeClass('bi-eye').addClass('bi-eye-slash');
    }
});

// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

function formatDate(date) {
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Add CSS for loading animation
const style = document.createElement('style');
style.textContent = `
    .spin {
        animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
    
    .wp-card-hover {
        transform: translateY(-2px);
        box-shadow: var(--wp-shadow-lg);
    }
    
    .wp-form-group-focused .wp-form-label {
        color: var(--current-primary, var(--wp-primary));
    }
    
    .wp-btn-loading {
        opacity: 0.7;
        cursor: not-allowed;
    }
`;
document.head.appendChild(style);