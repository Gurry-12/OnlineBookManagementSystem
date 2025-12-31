// Pure CSS Role Switcher JavaScript for SuperAdmin
document.addEventListener('DOMContentLoaded', function () {
    console.log('Role switcher script loaded - Pure CSS implementation');

    const roleSwitcher = document.getElementById('roleSwitcher');
    const roleSwitcherMenu = document.getElementById('roleSwitcherMenu');
    const roleSwitcherContainer = document.querySelector('.wp-role-switcher');

    if (roleSwitcher && roleSwitcherMenu && roleSwitcherContainer) {
        console.log('Role switcher elements found');

        // Toggle dropdown on button click
        roleSwitcher.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            console.log('Role switcher clicked');

            // Close other dropdowns first
            document.querySelectorAll('.wp-role-switcher.active').forEach(dropdown => {
                if (dropdown !== roleSwitcherContainer) {
                    dropdown.classList.remove('active');
                }
            });

            // Toggle current dropdown
            const isActive = roleSwitcherContainer.classList.contains('active');

            if (isActive) {
                closeDropdown();
            } else {
                openDropdown();
            }
        });

        // Handle keyboard navigation
        roleSwitcher.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                roleSwitcher.click();
            } else if (e.key === 'ArrowDown') {
                e.preventDefault();
                openDropdown();
                focusFirstMenuItem();
            } else if (e.key === 'Escape') {
                closeDropdown();
            }
        });

        // Handle menu item keyboard navigation
        const menuItems = roleSwitcherMenu.querySelectorAll('.wp-role-switcher-item');
        menuItems.forEach((item, index) => {
            item.addEventListener('keydown', function (e) {
                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    const nextIndex = (index + 1) % menuItems.length;
                    menuItems[nextIndex].focus();
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    const prevIndex = (index - 1 + menuItems.length) % menuItems.length;
                    menuItems[prevIndex].focus();
                } else if (e.key === 'Escape') {
                    closeDropdown();
                    roleSwitcher.focus();
                } else if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    item.click();
                }
            });
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            if (!roleSwitcherContainer.contains(e.target)) {
                closeDropdown();
            }
        });

        // Close dropdown on escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeDropdown();
            }
        });

        // Add confirmation for role switching
        const roleLinks = roleSwitcherMenu.querySelectorAll('.wp-role-switcher-item[href*="SwitchToRole"]');

        roleLinks.forEach(link => {
            link.addEventListener('click', handleRoleSwitch);
        });

    } else {
        console.error('Role switcher elements not found!');
    }

    // Handle return to SuperAdmin confirmation
    const returnButtons = document.querySelectorAll('a[href*="ReturnToSuperAdmin"]');
    returnButtons.forEach(button => {
        button.addEventListener('click', handleReturnToSuperAdmin);
    });

    // Add visual feedback for role switching
    const currentViewAlert = document.querySelector('.superadmin-return-alert');
    if (currentViewAlert) {
        setTimeout(() => {
            currentViewAlert.style.animation = 'pulse 2s infinite';
        }, 1000);
    }

    // Helper functions
    function openDropdown() {
        roleSwitcherContainer.classList.add('active');
        roleSwitcher.setAttribute('aria-expanded', 'true');

        // Reset menu item animations
        const menuItems = roleSwitcherMenu.querySelectorAll('.wp-role-switcher-item');
        menuItems.forEach((item, index) => {
            item.style.animation = 'none';
            setTimeout(() => {
                item.style.animation = `fadeInUp 0.3s ease forwards`;
                item.style.animationDelay = `${0.1 + (index * 0.05)}s`;
            }, 10);
        });

        console.log('Dropdown opened');
    }

    function closeDropdown() {
        roleSwitcherContainer.classList.remove('active');
        roleSwitcher.setAttribute('aria-expanded', 'false');
        console.log('Dropdown closed');
    }

    function focusFirstMenuItem() {
        const firstMenuItem = roleSwitcherMenu.querySelector('.wp-role-switcher-item');
        if (firstMenuItem) {
            firstMenuItem.focus();
        }
    }
});

// Event handler functions
function handleRoleSwitch(e) {
    const roleText = this.textContent.trim();
    const role = roleText.replace(' View', '').replace(/^\s*\S+\s*/, ''); // Remove icon

    console.log('Role switch clicked:', role);

    const confirmed = confirm(`Are you sure you want to switch to ${role}? You can return to SuperAdmin view at any time.`);

    if (!confirmed) {
        e.preventDefault();
        return false;
    }

    // Show loading state
    this.classList.add('loading');
    this.innerHTML = '<i class="bi bi-arrow-repeat me-2"></i>Switching...';

    // Allow the navigation to proceed
    return true;
}

function handleReturnToSuperAdmin(e) {
    const confirmed = confirm('Return to SuperAdmin dashboard?');
    if (!confirmed) {
        e.preventDefault();
    }
}

// Global confirmation function for onclick handlers
function confirmRoleSwitch(role) {
    return confirm(`Are you sure you want to switch to ${role} view? You can return to SuperAdmin view at any time.`);
}

// Function to show role switch notification (if needed)
function showRoleSwitchNotification(fromRole, toRole) {
    // Simple notification without Bootstrap dependency
    const notification = document.createElement('div');
    notification.className = 'wp-notification';
    notification.innerHTML = `
        <div class="wp-notification-content">
            <i class="bi bi-person-gear me-2"></i>
            <strong>Role Switched</strong>
            <p>Successfully switched from ${fromRole} to ${toRole} view.</p>
        </div>
    `;

    // Add notification styles
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: white;
        border: 1px solid #e5e7eb;
        border-radius: 8px;
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
        padding: 16px;
        z-index: 9999;
        animation: slideInRight 0.3s ease;
    `;

    document.body.appendChild(notification);

    // Auto remove after 3 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

// Add notification animations to CSS if not already present
if (!document.querySelector('#wp-notification-styles')) {
    const style = document.createElement('style');
    style.id = 'wp-notification-styles';
    style.textContent = `
        @keyframes slideInRight {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        
        @keyframes slideOutRight {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(100%);
                opacity: 0;
            }
        }
        
        .wp-notification-content {
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .wp-notification-content p {
            margin: 0;
            font-size: 14px;
        }
    `;
    document.head.appendChild(style);
}