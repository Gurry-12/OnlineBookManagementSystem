/**
 * Enhanced Cart Manager - AJAX cart operations
 */

class CartManager {
    constructor() {
        this.isUpdating = false;
        this.init();
    }

    init() {
        this.bindEvents();
        this.updateCartCount();
    }

    bindEvents() {
        document.addEventListener('click', this.handleClick.bind(this));
        document.addEventListener('change', this.handleChange.bind(this));
    }

    handleClick(e) {
        // Quantity buttons
        if (e.target.matches('.quantity-btn')) {
            e.preventDefault();
            const { bookId, action, currentQty } = e.target.dataset;
            const newQty = action === 'increase' ? parseInt(currentQty) + 1 : parseInt(currentQty) - 1;

            if (newQty > 0) {
                this.updateQuantity(bookId, newQty);
            }
        }

        // Remove item buttons
        if (e.target.matches('.remove-item-btn') || e.target.closest('.remove-item-btn')) {
            e.preventDefault();
            const btn = e.target.matches('.remove-item-btn') ? e.target : e.target.closest('.remove-item-btn');
            const { bookId, bookTitle } = btn.dataset;
            this.removeItem(bookId, bookTitle);
        }

        // Add to cart buttons
        if (e.target.matches('.add-to-cart-btn') || e.target.closest('.add-to-cart-btn')) {
            e.preventDefault();
            const btn = e.target.matches('.add-to-cart-btn') ? e.target : e.target.closest('.add-to-cart-btn');
            const { bookId, quantity = 1 } = btn.dataset;
            this.addToCart(bookId, parseInt(quantity), btn);
        }
    }

    handleChange(e) {
        if (e.target.matches('.quantity-input')) {
            const { bookId } = e.target.dataset;
            const newQty = parseInt(e.target.value);

            if (newQty > 0) {
                this.updateQuantity(bookId, newQty);
            }
        }
    }

    async addToCart(bookId, quantity, buttonElement) {
        if (this.isUpdating) return;

        try {
            this.isUpdating = true;
            apiClient.showLoading(buttonElement.id || 'add-to-cart-btn', 'Adding...');

            const response = await apiClient.post('/User/AddToCart', {
                bookId: parseInt(bookId),
                quantity: quantity
            });

            if (response.success) {
                notifications.success(response.message || 'Added to cart successfully!');
                this.updateCartCount(response.cartCount);
                this.showAddedState(buttonElement);
                this.dispatchCartEvent('add', bookId, quantity, response.cartCount);
            } else {
                throw new Error(response.message || 'Failed to add to cart');
            }

        } catch (error) {
            console.error('Error adding to cart:', error);
            notifications.error('Failed to add to cart. Please try again.');
        } finally {
            this.isUpdating = false;
            apiClient.hideLoading(buttonElement.id || 'add-to-cart-btn');
        }
    }

    async updateQuantity(bookId, newQuantity) {
        if (this.isUpdating) return;

        try {
            this.isUpdating = true;
            this.setQuantityLoading(bookId, true);

            const response = await apiClient.post('/Cart/UpdateQuantity', {
                bookId: parseInt(bookId),
                quantity: newQuantity
            });

            if (response.success) {
                this.updateQuantityDisplay(bookId, newQuantity, response);
                notifications.success('Quantity updated successfully!');
                this.dispatchCartEvent('update', bookId, newQuantity, response.cartCount);
            } else {
                throw new Error(response.message || 'Failed to update quantity');
            }

        } catch (error) {
            console.error('Error updating quantity:', error);
            notifications.error('Failed to update quantity. Please try again.');
        } finally {
            this.isUpdating = false;
            this.setQuantityLoading(bookId, false);
        }
    }

    async removeItem(bookId, bookTitle) {
        const confirmed = await notifications.confirm(
            `Are you sure you want to remove "${bookTitle}" from your cart?`,
            'Remove Item',
            { type: 'warning', confirmText: 'Remove', cancelText: 'Cancel' }
        );

        if (!confirmed) return;

        try {
            this.isUpdating = true;

            const response = await apiClient.post('/Cart/RemoveItem', {
                bookId: parseInt(bookId)
            });

            if (response.success) {
                this.animateItemRemoval(bookId);
                this.updateCartSummary(response.cartSummary);
                this.updateCartCount(response.cartCount);
                notifications.success(`"${bookTitle}" removed from cart`);
                this.dispatchCartEvent('remove', bookId, 0, response.cartCount);
            } else {
                throw new Error(response.message || 'Failed to remove item');
            }

        } catch (error) {
            console.error('Error removing item:', error);
            notifications.error('Failed to remove item. Please try again.');
        } finally {
            this.isUpdating = false;
        }
    }

    setQuantityLoading(bookId, isLoading) {
        const quantityContainer = document.querySelector(`[data-book-id="${bookId}"].quantity-container`);
        if (quantityContainer) {
            quantityContainer.style.opacity = isLoading ? '0.6' : '1';
        }
    }

    updateQuantityDisplay(bookId, newQuantity, response) {
        // Update quantity input
        const quantityInput = document.querySelector(`input.quantity-input[data-book-id="${bookId}"]`);
        if (quantityInput) quantityInput.value = newQuantity;

        // Update quantity buttons
        const quantityBtns = document.querySelectorAll(`button.quantity-btn[data-book-id="${bookId}"]`);
        quantityBtns.forEach(btn => btn.dataset.currentQty = newQuantity);

        // Update item total
        const itemTotal = document.querySelector(`[data-book-id="${bookId}"] .item-total`);
        if (itemTotal && response.itemTotal) {
            itemTotal.textContent = `₹${response.itemTotal.toFixed(2)}`;
        }

        // Update cart summary
        if (response.cartSummary) {
            this.updateCartSummary(response.cartSummary);
        }

        this.updateCartCount(response.cartCount);
    }

    animateItemRemoval(bookId) {
        const cartItem = document.querySelector(`[data-cart-item="${bookId}"]`);
        if (cartItem) {
            cartItem.style.transition = 'all 0.3s ease';
            cartItem.style.transform = 'translateX(-100%)';
            cartItem.style.opacity = '0';

            setTimeout(() => {
                cartItem.remove();

                const remainingItems = document.querySelectorAll('[data-cart-item]');
                if (remainingItems.length === 0) {
                    this.showEmptyCart();
                }
            }, 300);
        }
    }

    updateCartCount(count) {
        if (count !== undefined) {
            const cartCountElements = document.querySelectorAll('#cart-count, .cart-count');
            cartCountElements.forEach(element => {
                element.textContent = count;
                element.style.display = count > 0 ? 'inline' : 'none';
            });
        } else {
            this.fetchCartCount();
        }
    }

    async fetchCartCount() {
        try {
            const response = await apiClient.get('/User/GetCartCount');
            this.updateCartCount(response.count || 0);
        } catch (error) {
            console.error('Error fetching cart count:', error);
        }
    }

    updateCartSummary(summary) {
        const summaryContainer = document.querySelector('.cart-summary');
        if (summaryContainer && summary) {
            const subtotalElement = summaryContainer.querySelector('.subtotal');
            const totalElement = summaryContainer.querySelector('.total');

            if (subtotalElement) {
                subtotalElement.textContent = `₹${summary.subtotal.toFixed(2)}`;
            }

            if (totalElement) {
                totalElement.textContent = `₹${summary.total.toFixed(2)}`;
            }
        }
    }

    showAddedState(buttonElement) {
        const originalContent = buttonElement.innerHTML;
        const originalClasses = buttonElement.className;

        buttonElement.innerHTML = '<i class="bi bi-check-circle me-1"></i>Added';
        buttonElement.className = buttonElement.className.replace('btn-primary', 'btn-success');

        setTimeout(() => {
            buttonElement.innerHTML = originalContent;
            buttonElement.className = originalClasses;
        }, 2000);
    }

    showEmptyCart() {
        const cartContainer = document.querySelector('.cart-items-container');
        if (cartContainer) {
            cartContainer.innerHTML = `
                <div class="text-center py-5">
                    <i class="bi bi-cart-x display-1 text-muted mb-3"></i>
                    <h5>Your cart is empty</h5>
                    <p class="text-muted mb-4">Add some books to get started!</p>
                    <a href="/User/UserBookList" class="btn btn-primary">
                        <i class="bi bi-book me-2"></i>Browse Books
                    </a>
                </div>
            `;
        }
    }

    dispatchCartEvent(action, bookId, quantity, cartCount) {
        document.dispatchEvent(new CustomEvent('cartUpdated', {
            detail: { action, bookId, quantity, cartCount }
        }));
    }

    async clearCart() {
        const confirmed = await notifications.confirm(
            'Are you sure you want to clear your entire cart?',
            'Clear Cart',
            { type: 'warning', confirmText: 'Clear All', cancelText: 'Cancel' }
        );

        if (!confirmed) return;

        try {
            const response = await apiClient.post('/Cart/Clear');

            if (response.success) {
                this.showEmptyCart();
                this.updateCartCount(0);
                notifications.success('Cart cleared successfully!');
                this.dispatchCartEvent('clear', null, 0, 0);
            } else {
                throw new Error(response.message || 'Failed to clear cart');
            }

        } catch (error) {
            console.error('Error clearing cart:', error);
            notifications.error('Failed to clear cart. Please try again.');
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.cartManager = new CartManager();
});