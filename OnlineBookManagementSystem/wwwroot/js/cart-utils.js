// Cart utility functions
class CartUtils {
    static async addToCart(bookId, quantity = 1) {
        try {
            const response = await $.post('/User/AddToCart', {
                bookId: bookId,
                quantity: quantity
            });

            if (response.success) {
                toastr.success(response.message);
                this.updateCartCount(response.cartCount);
                $(document).trigger('cartUpdated');
                return { success: true, cartCount: response.cartCount };
            } else {
                toastr.error(response.message);
                return { success: false, message: response.message };
            }
        } catch (error) {
            toastr.error('Error adding item to cart');
            return { success: false, message: 'Network error' };
        }
    }

    static async updateQuantity(bookId, quantity) {
        try {
            const response = await $.ajax({
                url: '/Cart/UpdateQuantity',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify({ bookId: bookId, quantity: quantity })
            });

            if (response.success) {
                $(document).trigger('cartUpdated');
                return { success: true };
            } else {
                toastr.error('Failed to update quantity');
                return { success: false };
            }
        } catch (error) {
            toastr.error('Error updating cart');
            return { success: false };
        }
    }

    static async removeFromCart(bookId) {
        try {
            const response = await $.ajax({
                url: '/Cart/RemoveItem',
                method: 'DELETE',
                data: { bookId: bookId }
            });

            if (response.success) {
                toastr.success('Item removed from cart');
                $(document).trigger('cartUpdated');
                return { success: true };
            } else {
                toastr.error('Failed to remove item');
                return { success: false };
            }
        } catch (error) {
            toastr.error('Error removing item');
            return { success: false };
        }
    }

    static async getCartCount() {
        try {
            const response = await $.get('/User/GetCartCount');
            return response.count || 0;
        } catch (error) {
            return 0;
        }
    }

    static updateCartCount(count) {
        const cartCountElement = $('#cart-count');
        if (cartCountElement.length) {
            cartCountElement.text(count);
            if (count > 0) {
                cartCountElement.show();
            } else {
                cartCountElement.hide();
            }
        }
    }

    static async refreshCartCount() {
        const count = await this.getCartCount();
        this.updateCartCount(count);
    }

    static requireAuth() {
        const isAuthenticated = $('body').data('authenticated') === 'true';
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
$(document).ready(function () {
    if ($('#cart-count').length) {
        CartUtils.refreshCartCount();
    }
});