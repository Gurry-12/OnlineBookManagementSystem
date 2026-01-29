/**
 * Unified Notification System
 * Provides consistent toast notifications across the application
 */

class NotificationManager {
    constructor() {
        this.container = null;
        this.init();
    }

    /**
     * Initialize notification container
     */
    init() {
        // Create toast container if it doesn't exist
        this.container = document.querySelector('.toast-container');
        if (!this.container) {
            this.container = document.createElement('div');
            this.container.className = 'toast-container position-fixed top-0 end-0 p-3';
            this.container.style.zIndex = '1055';
            document.body.appendChild(this.container);
        }
    }

    /**
     * Show notification
     */
    show(message, type = 'info', options = {}) {
        const {
            title = '',
            duration = 5000,
            closable = true,
            icon = this.getIcon(type)
        } = options;

        const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const bgClass = this.getBgClass(type);

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        ${icon ? `<i class="${icon} me-2"></i>` : ''}
                        ${title ? `<strong>${title}</strong><br>` : ''}
                        ${message}
                    </div>
                    ${closable ? `
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                    ` : ''}
                </div>
            </div>
        `;

        this.container.insertAdjacentHTML('beforeend', toastHtml);

        const toastElement = document.getElementById(toastId);
        const bsToast = new bootstrap.Toast(toastElement, {
            delay: duration,
            autohide: duration > 0
        });

        // Clean up after toast is hidden
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });

        bsToast.show();
        return toastId;
    }

    /**
     * Show success notification
     */
    success(message, options = {}) {
        return this.show(message, 'success', options);
    }

    /**
     * Show error notification
     */
    error(message, options = {}) {
        return this.show(message, 'error', { duration: 8000, ...options });
    }

    /**
     * Show warning notification
     */
    warning(message, options = {}) {
        return this.show(message, 'warning', { duration: 6000, ...options });
    }

    /**
     * Show info notification
     */
    info(message, options = {}) {
        return this.show(message, 'info', options);
    }

    /**
     * Show loading notification
     */
    loading(message = 'Loading...', options = {}) {
        return this.show(
            `<div class="d-flex align-items-center">
                <div class="spinner-border spinner-border-sm me-2" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                ${message}
            </div>`,
            'info',
            { duration: 0, closable: false, ...options }
        );
    }

    /**
     * Hide specific notification
     */
    hide(toastId) {
        const toastElement = document.getElementById(toastId);
        if (toastElement) {
            const bsToast = bootstrap.Toast.getInstance(toastElement);
            if (bsToast) {
                bsToast.hide();
            }
        }
    }

    /**
     * Hide all notifications
     */
    hideAll() {
        const toasts = this.container.querySelectorAll('.toast');
        toasts.forEach(toast => {
            const bsToast = bootstrap.Toast.getInstance(toast);
            if (bsToast) {
                bsToast.hide();
            }
        });
    }

    /**
     * Get Bootstrap background class for notification type
     */
    getBgClass(type) {
        const classes = {
            success: 'bg-success',
            error: 'bg-danger',
            warning: 'bg-warning',
            info: 'bg-info',
            primary: 'bg-primary',
            secondary: 'bg-secondary'
        };
        return classes[type] || classes.info;
    }

    /**
     * Get icon class for notification type
     */
    getIcon(type) {
        const icons = {
            success: 'bi bi-check-circle-fill',
            error: 'bi bi-exclamation-triangle-fill',
            warning: 'bi bi-exclamation-triangle-fill',
            info: 'bi bi-info-circle-fill',
            primary: 'bi bi-info-circle-fill',
            secondary: 'bi bi-info-circle-fill'
        };
        return icons[type] || icons.info;
    }

    /**
     * Show confirmation dialog
     */
    confirm(message, title = 'Confirm Action', options = {}) {
        return new Promise((resolve) => {
            const {
                confirmText = 'Confirm',
                cancelText = 'Cancel',
                type = 'warning'
            } = options;

            const modalId = `confirm-modal-${Date.now()}`;
            const modalHtml = `
                <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">
                                    <i class="${this.getIcon(type)} me-2"></i>
                                    ${title}
                                </h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">
                                ${message}
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
                                <button type="button" class="btn btn-${type === 'error' ? 'danger' : 'primary'}" id="${modalId}-confirm">${confirmText}</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            document.body.insertAdjacentHTML('beforeend', modalHtml);

            const modalElement = document.getElementById(modalId);
            const modal = new bootstrap.Modal(modalElement);

            const confirmBtn = document.getElementById(`${modalId}-confirm`);

            confirmBtn.addEventListener('click', () => {
                modal.hide();
                resolve(true);
            });

            modalElement.addEventListener('hidden.bs.modal', () => {
                modalElement.remove();
                resolve(false);
            });

            modal.show();
        });
    }
}

// Create global instance
window.notifications = new NotificationManager();