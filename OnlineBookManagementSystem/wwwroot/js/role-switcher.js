// Role Switcher JavaScript for SuperAdmin
document.addEventListener('DOMContentLoaded', function () {
    console.log('Role switcher script loaded');

    // Check if Bootstrap is loaded
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap is not loaded!');
        // Fallback: manual dropdown implementation
        initManualDropdown();
        return;
    }

    const roleSwitcher = document.getElementById('roleSwitcher');

    if (roleSwitcher) {
        console.log('Role switcher element found');

        // Initialize Bootstrap dropdown
        try {
            const dropdown = new bootstrap.Dropdown(roleSwitcher);
            console.log('Bootstrap dropdown initialized successfully');

            // Add event listeners
            roleSwitcher.addEventListener('show.bs.dropdown', function () {
                console.log('Dropdown is about to show');
            });

            roleSwitcher.addEventListener('shown.bs.dropdown', function () {
                console.log('Dropdown is now visible');
            });

        } catch (error) {
            console.error('Error initializing Bootstrap dropdown:', error);
            initManualDropdown();
        }

        // Add confirmation for role switching
        const roleLinks = document.querySelectorAll('.dropdown-menu a[href*="SwitchToRole"]');

        roleLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                const roleText = this.textContent.trim();
                const role = roleText.replace(' View', '').replace(/^\s*\S+\s*/, ''); // Remove icon

                console.log('Role switch clicked:', role);

                const confirmed = confirm(`Are you sure you want to switch to ${role}? You can return to SuperAdmin view at any time.`);

                if (!confirmed) {
                    e.preventDefault();
                    return false;
                }

                // Show loading state
                this.innerHTML = '<i class="bi bi-arrow-repeat me-2"></i>Switching...';
                this.classList.add('disabled');
            });
        });

        // Add tooltip to role switcher
        if (typeof bootstrap.Tooltip !== 'undefined') {
            try {
                new bootstrap.Tooltip(roleSwitcher, {
                    title: 'Switch between different role views to test user experience',
                    placement: 'bottom'
                });
            } catch (error) {
                console.log('Tooltip initialization failed:', error);
            }
        }
    } else {
        console.error('Role switcher element not found!');
    }

    // Handle return to SuperAdmin confirmation
    const returnButtons = document.querySelectorAll('a[href*="ReturnToSuperAdmin"]');
    returnButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const confirmed = confirm('Return to SuperAdmin dashboard?');
            if (!confirmed) {
                e.preventDefault();
            }
        });
    });

    // Add visual feedback for role switching
    const currentViewAlert = document.querySelector('.superadmin-return-alert');
    if (currentViewAlert) {
        // Add a subtle animation to draw attention
        setTimeout(() => {
            currentViewAlert.style.animation = 'pulse 2s infinite';
        }, 1000);
    }
});

// Manual dropdown implementation as fallback
function initManualDropdown() {
    console.log('Initializing manual dropdown fallback');

    const roleSwitcher = document.getElementById('roleSwitcher');
    const dropdownMenu = document.querySelector('#roleSwitcher + .dropdown-menu');

    if (!roleSwitcher || !dropdownMenu) {
        console.error('Manual dropdown elements not found');
        return;
    }

    // Toggle dropdown on click
    roleSwitcher.addEventListener('click', function (e) {
        e.preventDefault();
        e.stopPropagation();

        console.log('Manual dropdown toggle clicked');

        const isOpen = dropdownMenu.classList.contains('show');

        // Close all other dropdowns
        document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
            menu.classList.remove('show');
        });

        // Toggle current dropdown
        if (!isOpen) {
            dropdownMenu.classList.add('show');
            roleSwitcher.setAttribute('aria-expanded', 'true');
        } else {
            dropdownMenu.classList.remove('show');
            roleSwitcher.setAttribute('aria-expanded', 'false');
        }
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function (e) {
        if (!roleSwitcher.contains(e.target) && !dropdownMenu.contains(e.target)) {
            dropdownMenu.classList.remove('show');
            roleSwitcher.setAttribute('aria-expanded', 'false');
        }
    });

    // Close dropdown when pressing Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            dropdownMenu.classList.remove('show');
            roleSwitcher.setAttribute('aria-expanded', 'false');
        }
    });
}

// Function to show role switch notification
function showRoleSwitchNotification(fromRole, toRole) {
    if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
        const toastHtml = `
            <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header">
                    <i class="bi bi-person-gear me-2"></i>
                    <strong class="me-auto">Role Switched</strong>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    Successfully switched from ${fromRole} to ${toRole} view.
                </div>
            </div>
        `;

        const toastContainer = document.querySelector('.toast-container');
        if (toastContainer) {
            toastContainer.insertAdjacentHTML('beforeend', toastHtml);
            const toast = new bootstrap.Toast(toastContainer.lastElementChild);
            toast.show();
        }
    } else {
        // Fallback notification
        alert(`Successfully switched from ${fromRole} to ${toRole} view.`);
    }
}