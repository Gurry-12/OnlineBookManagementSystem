/**
 * Enhanced Cart Utilities - Updated to use new AJAX infrastructure
 * Provides backward compatibility while leveraging new cart manager
 */

class CartUtils {
    static async addToCart(bookId, quantity = 1) {
        if (!this.requireAuth()) return { success: false, message: 'Authentication required' };

        if (window.cartManager) {
            // Use new cart manager if available
            return await cartManager.addToCart(bookId, quantity, document.createElement('button'));
        }

        // Fallback to original implementation
        try {
            const response = await apiClient.post('/User/AddToCart', {
                bookId: bookId,
                quantity: quantity
            });

            if (response.success) {
                notifications.success(response.message);
                this.updateCartCount(response.cartCount);
                $(document).trigger('cartUpdated');
                return { success: true, cartCount: response.cartCount };
            } else {
                notifications.error(response.message);
                return { success: false, message: response.message };
            }
        } catch (error) {
            notifications.error('Error adding item to cart');
            return { success: false, message: 'Network error' };
        }
    }

    static async updateQuantity(bookId, quantity) {
        if (window.cartManager) {
            // Use new cart manager if available
            return await cartManager.updateQuantity(bookId, quantity);
        }

        // Fallback to original implementation
        try {
            const response = await apiClient.post('/Cart/UpdateQuantity', {
                bookId: bookId,
                quantity: quantity
            });

            if (response.success) {
                $(document).trigger('cartUpdated');
                return { success: true };
            } else {
                notifications.error('Failed to update quantity');
                return { success: false };
            }
        } catch (error) {
            notifications.error('Error updating cart');
            return { success: false };
        }
    }

    static async removeFromCart(bookId) {
        if (window.cartManager) {
            // Use new cart manager if available
            return await cartManager.removeItem(bookId, 'Item');
        }

        // Fallback to original implementation
        try {
            const response = await apiClient.post('/Cart/RemoveItem', { bookId: bookId });

            if (response.success) {
                notifications.success('Item removed from cart');
                $(document).trigger('cartUpdated');
                return { success: true };
            } else {
                notifications.error('Failed to remove item');
                return { success: false };
            }
        } catch (error) {
            notifications.error('Error removing item');
            return { success: false };
        }
    }

    static async getCartCount() {
        try {
            const response = await apiClient.get('/User/GetCartCount');
            return response.count || 0;
        } catch (error) {
            return 0;
        }
    }

    static updateCartCount(count) {
        const cartCountElements = document.querySelectorAll('#cart-count, .cart-count');
        cartCountElements.forEach(element => {
            if (element) {
                element.textContent = count;
                if (count > 0) {
                    element.style.display = 'inline';
                } else {
                    element.style.display = 'none';
                }
            }
        });
    }

    static async refreshCartCount() {
        const count = await this.getCartCount();
        this.updateCartCount(count);
    }

    static requireAuth() {
        const isAuthenticated = document.body.dataset.authenticated === 'true' ||
            document.querySelector('[data-authenticated="true"]') !== null;
        if (!isAuthenticated) {
            window.location.href = '/Auth/Login';
            return false;
        }
        return true;
    }
}

// Global functions for backward compatibility
window.addToCart = function (bookId, quantity = 1) {
    if (!CartUtils.requireAuth()) return;
    return CartUtils.addToCart(bookId, quantity);
};

window.updateCartQuantity = function (bookId, quantity) {
    return CartUtils.updateQuantity(bookId, quantity);
};

window.removeFromCart = function (bookId) {
    return CartUtils.removeFromCart(bookId);
};

// Initialize cart count on page load
document.addEventListener('DOMContentLoaded', function () {
    if (document.querySelector('#cart-count, .cart-count')) {
        CartUtils.refreshCartCount();
    }
});