/**
 * AJAX Book Management System
 * Handles all book-related operations without page reloads
 */

class BookManager {
    constructor() {
        this.currentFilters = {};
        this.searchTimeout = null;
        this.isLoading = false;
        this.init();
    }

    /**
     * Initialize book manager
     */
    init() {
        this.bindEvents();
        this.loadInitialState();
        this.setupUrlStateListener();
    }

    /**
     * Load initial state from URL
     */
    loadInitialState() {
        try {
            this.currentFilters = {
                page: this.parseIntSafe(urlState.get('page', '1'), 1),
                search: this.sanitizeString(urlState.get('search', '')),
                categoryId: this.sanitizeString(urlState.get('categoryId', '')),
                sortBy: this.sanitizeString(urlState.get('sortBy', '')),
                inStock: this.sanitizeString(urlState.get('inStock', ''))
            };

            // Validate page number
            if (this.currentFilters.page < 1) {
                this.currentFilters.page = 1;
            }

            this.updateFormFields();
        } catch (error) {
            console.error('Failed to load initial state:', error);
            notifications.error('Failed to load page state. Using defaults.');

            // Reset to safe defaults
            this.currentFilters = {
                page: 1,
                search: '',
                categoryId: '',
                sortBy: '',
                inStock: ''
            };

            try {
                this.updateFormFields();
            } catch (updateError) {
                console.error('Failed to update form fields with defaults:', updateError);
            }
        }
    }

    /**
     * Safely parse integer with fallback
     */
    parseIntSafe(value, defaultValue = 0) {
        try {
            const parsed = parseInt(value);
            return isNaN(parsed) ? defaultValue : parsed;
        } catch (error) {
            console.warn('Failed to parse integer:', value, error);
            return defaultValue;
        }
    }

    /**
     * Sanitize string input
     */
    sanitizeString(value) {
        try {
            if (typeof value !== 'string') {
                return '';
            }
            // Basic sanitization - remove potentially harmful characters
            return value.replace(/[<>\"']/g, '').trim();
        } catch (error) {
            console.warn('Failed to sanitize string:', value, error);
            return '';
        }
    }

    /**
     * Setup URL state change listener
     */
    setupUrlStateListener() {
        try {
            urlState.addListener('popstate', (state) => {
                try {
                    this.currentFilters = {
                        page: this.parseIntSafe(state.page, 1),
                        search: this.sanitizeString(state.search || ''),
                        categoryId: this.sanitizeString(state.categoryId || ''),
                        sortBy: this.sanitizeString(state.sortBy || ''),
                        inStock: this.sanitizeString(state.inStock || '')
                    };

                    this.updateFormFields();
                    this.loadBooks();
                } catch (error) {
                    console.error('Error handling URL state change:', error);
                    notifications.error('Failed to update page state.');
                }
            });
        } catch (error) {
            console.error('Failed to setup URL state listener:', error);
        }
    }
            this.updateFormFields();
this.loadBooks();
        });
    }

/**
 * Bind event handlers
 */
bindEvents() {
    // Search input with debounce
    const searchInput = document.querySelector('input[name="search"]');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            clearTimeout(this.searchTimeout);
            this.searchTimeout = setTimeout(() => {
                this.updateFilter('search', e.target.value);
            }, 500);
        });
    }

    // Filter dropdowns
    const filterSelects = document.querySelectorAll('select[name="categoryId"], select[name="sortBy"], select[name="inStock"]');
    filterSelects.forEach(select => {
        select.addEventListener('change', (e) => {
            this.updateFilter(e.target.name, e.target.value);
        });
    });

    // Pagination links
    document.addEventListener('click', (e) => {
        if (e.target.matches('.pagination a[data-page]')) {
            e.preventDefault();
            const page = parseInt(e.target.dataset.page);
            this.updateFilter('page', page);
        }
    });

    // Delete book buttons
    document.addEventListener('click', (e) => {
        if (e.target.matches('.delete-book-btn') || e.target.closest('.delete-book-btn')) {
            e.preventDefault();
            const btn = e.target.matches('.delete-book-btn') ? e.target : e.target.closest('.delete-book-btn');
            const bookId = btn.dataset.bookId;
            this.deleteBook(bookId, btn);
        }
    });

    // Create/Edit book modals
    document.addEventListener('click', (e) => {
        if (e.target.matches('.create-book-btn')) {
            e.preventDefault();
            this.showCreateBookModal();
        }

        if (e.target.matches('.edit-book-btn') || e.target.closest('.edit-book-btn')) {
            e.preventDefault();
            const btn = e.target.matches('.edit-book-btn') ? e.target : e.target.closest('.edit-book-btn');
            const bookId = btn.dataset.bookId;
            this.showEditBookModal(bookId);
        }
    });
}

/**
 * Update filter and reload books
 */
updateFilter(key, value) {
    if (this.isLoading) return;

    // Reset to page 1 when changing filters (except pagination)
    if (key !== 'page') {
        this.currentFilters.page = 1;
    }

    this.currentFilters[key] = value;

    // Update URL state
    urlState.updateUrl(this.currentFilters);

    // Load books with new filters
    this.loadBooks();
}

/**
 * Update form fields with current filter values
 */
updateFormFields() {
    const searchInput = document.querySelector('input[name="search"]');
    if (searchInput) searchInput.value = this.currentFilters.search || '';

    const categorySelect = document.querySelector('select[name="categoryId"]');
    if (categorySelect) categorySelect.value = this.currentFilters.categoryId || '';

    const sortSelect = document.querySelector('select[name="sortBy"]');
    if (sortSelect) sortSelect.value = this.currentFilters.sortBy || '';

    const stockSelect = document.querySelector('select[name="inStock"]');
    if (stockSelect) stockSelect.value = this.currentFilters.inStock || '';
}

    /**
     * Load books via AJAX
     */
    async loadBooks() {
    if (this.isLoading) return;

    this.isLoading = true;
    this.showLoadingState();

    try {
        const params = new URLSearchParams();
        Object.entries(this.currentFilters).forEach(([key, value]) => {
            if (value !== null && value !== undefined && value !== '') {
                params.set(key, value.toString());
            }
        });

        const response = await apiClient.get(`/Admin/Books?${params.toString()}`);

        if (typeof response === 'string') {
            // Response is HTML, update the books container
            this.updateBooksContainer(response);
        } else if (response.success) {
            // Response is JSON with data
            this.updateBooksContainer(response.data);
        } else {
            throw new Error(response.message || 'Failed to load books');
        }

    } catch (error) {
        console.error('Error loading books:', error);
        notifications.error('Failed to load books. Please try again.');
    } finally {
        this.isLoading = false;
        this.hideLoadingState();
    }
}

/**
 * Show loading state
 */
showLoadingState() {
    const booksContainer = document.querySelector('.books-grid, .books-container');
    if (booksContainer) {
        booksContainer.style.opacity = '0.6';
        booksContainer.style.pointerEvents = 'none';
    }

    // Show loading overlay
    const loadingOverlay = document.createElement('div');
    loadingOverlay.id = 'books-loading-overlay';
    loadingOverlay.className = 'position-absolute top-50 start-50 translate-middle';
    loadingOverlay.innerHTML = `
            <div class="d-flex flex-column align-items-center">
                <div class="spinner-border text-primary mb-2" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <small class="text-muted">Loading books...</small>
            </div>
        `;

    const container = document.querySelector('.wp-card-body');
    if (container) {
        container.style.position = 'relative';
        container.appendChild(loadingOverlay);
    }
}

/**
 * Hide loading state
 */
hideLoadingState() {
    const booksContainer = document.querySelector('.books-grid, .books-container');
    if (booksContainer) {
        booksContainer.style.opacity = '1';
        booksContainer.style.pointerEvents = 'auto';
    }

    const loadingOverlay = document.getElementById('books-loading-overlay');
    if (loadingOverlay) {
        loadingOverlay.remove();
    }
}

/**
 * Update books container with new content
 */
updateBooksContainer(htmlContent) {
    const booksContent = document.getElementById('books-content');

    if (typeof htmlContent === 'string') {
        // Update the entire books content area
        booksContent.innerHTML = htmlContent;
    } else if (htmlContent && htmlContent.html) {
        // Handle JSON response with HTML content
        booksContent.innerHTML = htmlContent.html;
    }

    // Trigger custom event for other components
    document.dispatchEvent(new CustomEvent('booksUpdated', {
        detail: { filters: this.currentFilters }
    }));
}

    /**
     * Delete book with confirmation
     */
    async deleteBook(bookId, buttonElement) {
    const confirmed = await notifications.confirm(
        'Are you sure you want to delete this book? This action cannot be undone.',
        'Delete Book',
        { type: 'error', confirmText: 'Delete', cancelText: 'Cancel' }
    );

    if (!confirmed) return;

    try {
        apiClient.showLoading(buttonElement.id || 'delete-btn', 'Deleting...');

        const response = await apiClient.post('/Admin/DeleteBook', { id: bookId });

        if (response.success) {
            notifications.success(response.message || 'Book deleted successfully!');

            // Remove book card with animation
            const bookCard = buttonElement.closest('.col-xl-3, .col-lg-4, .col-md-6, .book-card');
            if (bookCard) {
                bookCard.style.transition = 'all 0.3s ease';
                bookCard.style.transform = 'scale(0.8)';
                bookCard.style.opacity = '0';

                setTimeout(() => {
                    bookCard.remove();
                    // Reload books to update pagination and counts
                    this.loadBooks();
                }, 300);
            }
        } else {
            throw new Error(response.message || 'Failed to delete book');
        }

    } catch (error) {
        console.error('Error deleting book:', error);
        notifications.error('Failed to delete book. Please try again.');
    } finally {
        apiClient.hideLoading(buttonElement.id || 'delete-btn');
    }
}

    /**
     * Show create book modal
     */
    async showCreateBookModal() {
    try {
        const response = await apiClient.get('/Admin/CreateBook');
        this.showBookModal('Create Book', response, 'create');
    } catch (error) {
        console.error('Error loading create book form:', error);
        notifications.error('Failed to load create book form.');
    }
}

    /**
     * Show edit book modal
     */
    async showEditBookModal(bookId) {
    try {
        const response = await apiClient.get(`/Admin/EditBook/${bookId}`);
        this.showBookModal('Edit Book', response, 'edit', bookId);
    } catch (error) {
        console.error('Error loading edit book form:', error);
        notifications.error('Failed to load edit book form.');
    }
}

/**
 * Show book modal (create/edit)
 */
showBookModal(title, htmlContent, mode, bookId = null) {
    // Create modal if it doesn't exist
    let modal = document.getElementById('bookModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'bookModal';
        modal.className = 'modal fade';
        modal.innerHTML = `
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title"></h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body"></div>
                    </div>
                </div>
            `;
        document.body.appendChild(modal);
    }

    // Update modal content
    modal.querySelector('.modal-title').textContent = title;
    modal.querySelector('.modal-body').innerHTML = htmlContent;

    // Setup form submission
    const form = modal.querySelector('form');
    if (form) {
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            this.submitBookForm(form, mode, bookId);
        });
    }

    // Show modal
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

    /**
     * Submit book form (create/edit)
     */
    async submitBookForm(form, mode, bookId = null) {
    try {
        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');

        apiClient.showLoading(submitBtn.id || 'submit-btn', 'Saving...');

        const url = mode === 'create' ? '/Admin/CreateBook' : `/Admin/EditBook/${bookId}`;
        const response = await apiClient.uploadFile(url, formData);

        if (response.success) {
            notifications.success(response.message || `Book ${mode}d successfully!`);

            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('bookModal'));
            modal.hide();

            // Reload books
            this.loadBooks();
        } else {
            // Handle validation errors
            this.displayFormErrors(form, response.errors || {});
            notifications.error(response.message || `Failed to ${mode} book.`);
        }

    } catch (error) {
        console.error(`Error ${mode}ing book:`, error);
        notifications.error(`Failed to ${mode} book. Please try again.`);
    } finally {
        const submitBtn = form.querySelector('button[type="submit"]');
        apiClient.hideLoading(submitBtn.id || 'submit-btn');
    }
}

/**
 * Display form validation errors
 */
displayFormErrors(form, errors) {
    // Clear existing errors
    form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
    form.querySelectorAll('.invalid-feedback').forEach(el => el.remove());

    // Display new errors
    Object.entries(errors).forEach(([field, messages]) => {
        const input = form.querySelector(`[name="${field}"]`);
        if (input) {
            input.classList.add('is-invalid');

            const feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            feedback.textContent = Array.isArray(messages) ? messages[0] : messages;

            input.parentNode.appendChild(feedback);
        }
    });
}

/**
 * Refresh books list
 */
refresh() {
    this.loadBooks();
}
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('.books-management-page, [data-page="books"]')) {
        window.bookManager = new BookManager();
    }
});