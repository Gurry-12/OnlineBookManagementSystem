/**
 * User Book Manager - AJAX functionality for user book browsing
 */

class UserBookManager {
    constructor() {
        this.currentFilters = {};
        this.searchTimeout = null;
        this.isLoading = false;
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadInitialState();
        this.setupUrlStateListener();
    }

    loadInitialState() {
        this.currentFilters = {
            page: urlState.get('page', 1),
            search: urlState.get('search', ''),
            categoryId: urlState.get('categoryId', ''),
            sortBy: urlState.get('sortBy', ''),
            minPrice: urlState.get('minPrice', ''),
            maxPrice: urlState.get('maxPrice', '')
        };
        this.updateFormFields();
    }

    setupUrlStateListener() {
        urlState.addListener('popstate', (state) => {
            this.currentFilters = {
                page: state.page || 1,
                search: state.search || '',
                categoryId: state.categoryId || '',
                sortBy: state.sortBy || '',
                minPrice: state.minPrice || '',
                maxPrice: state.maxPrice || ''
            };
            this.updateFormFields();
            this.loadBooks();
        });
    }

    bindEvents() {
        // Search with debounce
        const searchInput = document.querySelector('input[name="search"]');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                clearTimeout(this.searchTimeout);
                this.searchTimeout = setTimeout(() => {
                    this.updateFilter('search', e.target.value);
                }, 500);
            });
        }

        // Filter controls
        this.bindFilterEvents();
        this.bindPaginationEvents();
        this.bindBookActionEvents();
    }

    bindFilterEvents() {
        const filterSelects = document.querySelectorAll('select[name="categoryId"], select[name="sortBy"]');
        filterSelects.forEach(select => {
            select.addEventListener('change', (e) => {
                this.updateFilter(e.target.name, e.target.value);
            });
        });

        const priceInputs = document.querySelectorAll('input[name="minPrice"], input[name="maxPrice"]');
        priceInputs.forEach(input => {
            input.addEventListener('change', (e) => {
                this.updateFilter(e.target.name, e.target.value);
            });
        });
    }

    bindPaginationEvents() {
        document.addEventListener('click', (e) => {
            if (e.target.matches('.pagination a[data-page]')) {
                e.preventDefault();
                const page = parseInt(e.target.dataset.page);
                this.updateFilter('page', page);
            }
        });
    }

    bindBookActionEvents() {
        document.addEventListener('click', (e) => {
            // Add to cart
            if (e.target.matches('.add-to-cart-btn') || e.target.closest('.add-to-cart-btn')) {
                e.preventDefault();
                const btn = e.target.matches('.add-to-cart-btn') ? e.target : e.target.closest('.add-to-cart-btn');
                const bookId = btn.dataset.bookId;
                const quantity = btn.dataset.quantity || 1;
                this.addToCart(bookId, parseInt(quantity), btn);
            }

            // Toggle favorite
            if (e.target.matches('.toggle-favorite-btn') || e.target.closest('.toggle-favorite-btn')) {
                e.preventDefault();
                const btn = e.target.matches('.toggle-favorite-btn') ? e.target : e.target.closest('.toggle-favorite-btn');
                const bookId = btn.dataset.bookId;
                this.toggleFavorite(bookId, btn);
            }
        });
    }

    updateFilter(key, value) {
        if (this.isLoading) return;

        if (key !== 'page') {
            this.currentFilters.page = 1;
        }

        this.currentFilters[key] = value;
        urlState.updateUrl(this.currentFilters);
        this.loadBooks();
    }

    updateFormFields() {
        const fields = [
            { name: 'search', type: 'input' },
            { name: 'categoryId', type: 'select' },
            { name: 'sortBy', type: 'select' },
            { name: 'minPrice', type: 'input' },
            { name: 'maxPrice', type: 'input' }
        ];

        fields.forEach(field => {
            const element = document.querySelector(`${field.type}[name="${field.name}"]`);
            if (element) element.value = this.currentFilters[field.name] || '';
        });
    }

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

            const response = await apiClient.get(`/User/UserBookList?${params.toString()}`);

            if (typeof response === 'string') {
                this.updateBooksContainer(response);
            } else if (response.success) {
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

    showLoadingState() {
        const booksContainer = document.querySelector('#books-list');
        if (booksContainer) {
            booksContainer.style.opacity = '0.6';
            booksContainer.style.pointerEvents = 'none';
        }

        const loadingOverlay = this.createLoadingOverlay();
        const container = document.querySelector('.container');
        if (container) {
            container.style.position = 'relative';
            container.appendChild(loadingOverlay);
        }
    }

    hideLoadingState() {
        const booksContainer = document.querySelector('#books-list');
        if (booksContainer) {
            booksContainer.style.opacity = '1';
            booksContainer.style.pointerEvents = 'auto';
        }

        const loadingOverlay = document.getElementById('books-loading-overlay');
        if (loadingOverlay) loadingOverlay.remove();
    }

    createLoadingOverlay() {
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
        return loadingOverlay;
    }

    updateBooksContainer(htmlContent) {
        const booksContent = document.getElementById('user-books-content');

        if (typeof htmlContent === 'string') {
            booksContent.innerHTML = htmlContent;
        } else if (htmlContent && htmlContent.html) {
            booksContent.innerHTML = htmlContent.html;
        }

        document.dispatchEvent(new CustomEvent('userBooksUpdated', {
            detail: { filters: this.currentFilters }
        }));
    }

    async addToCart(bookId, quantity, buttonElement) {
        try {
            apiClient.showLoading(buttonElement.id || 'add-to-cart-btn', 'Adding...');

            const response = await apiClient.post('/User/AddToCart', {
                bookId: parseInt(bookId),
                quantity: quantity
            });

            if (response.success) {
                notifications.success(response.message || 'Added to cart successfully!');
                this.updateCartCount(response.cartCount);
                this.showButtonSuccess(buttonElement);

                if (typeof window.updateCartCount === 'function') {
                    window.updateCartCount();
                }
            } else {
                throw new Error(response.message || 'Failed to add to cart');
            }

        } catch (error) {
            console.error('Error adding to cart:', error);
            notifications.error('Failed to add to cart. Please try again.');
        } finally {
            apiClient.hideLoading(buttonElement.id || 'add-to-cart-btn');
        }
    }

    async toggleFavorite(bookId, buttonElement) {
        try {
            apiClient.showLoading(buttonElement.id || 'favorite-btn', 'Updating...');

            const response = await apiClient.post('/User/ToggleFavorite', {
                bookId: parseInt(bookId)
            });

            if (response.success) {
                notifications.success(response.message || 'Favorite updated!');
                this.updateFavoriteButton(buttonElement, response.isFavorite);
            } else {
                throw new Error(response.message || 'Failed to update favorite');
            }

        } catch (error) {
            console.error('Error toggling favorite:', error);
            notifications.error('Failed to update favorite. Please try again.');
        } finally {
            apiClient.hideLoading(buttonElement.id || 'favorite-btn');
        }
    }

    showButtonSuccess(buttonElement) {
        const originalContent = buttonElement.innerHTML;
        const originalClasses = buttonElement.className;

        buttonElement.innerHTML = '<i class="bi bi-check-circle me-1"></i>Added';
        buttonElement.classList.remove('btn-primary');
        buttonElement.classList.add('btn-success');

        setTimeout(() => {
            buttonElement.innerHTML = originalContent;
            buttonElement.className = originalClasses;
        }, 2000);
    }

    updateFavoriteButton(buttonElement, isFavorite) {
        const icon = buttonElement.querySelector('i');
        if (isFavorite) {
            icon.className = 'bi bi-heart-fill text-danger';
            buttonElement.setAttribute('title', 'Remove from favorites');
        } else {
            icon.className = 'bi bi-heart';
            buttonElement.setAttribute('title', 'Add to favorites');
        }
    }

    updateCartCount(count) {
        const cartCountSelectors = [
            '#cartItemCount', '#cart-count', '.cart-count', '[data-cart-count]'
        ];

        cartCountSelectors.forEach(selector => {
            const elements = document.querySelectorAll(selector);
            elements.forEach(element => {
                if (element) {
                    element.textContent = count || 0;
                    element.style.display = count && count > 0 ? 'inline' : 'none';
                    element.classList.toggle('d-none', !count || count === 0);
                }
            });
        });

        if (typeof window.updateCartCount === 'function') {
            window.updateCartCount();
        }
    }

    refresh() {
        this.loadBooks();
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('.user-books-page, [data-page="user-books"]')) {
        window.userBookManager = new UserBookManager();
    }
});