/**
 * Simple test script to verify AJAX functionality
 * This can be removed after testing
 */

document.addEventListener('DOMContentLoaded', () => {
    console.log('AJAX Test Script Loaded');

    // Test API Client
    if (window.apiClient) {
        console.log('✅ API Client loaded successfully');

        // Test CSRF token
        const csrfToken = apiClient.getCsrfToken();
        console.log('CSRF Token:', csrfToken ? '✅ Found' : '❌ Not found');
    } else {
        console.error('❌ API Client not loaded');
    }

    // Test Notifications
    if (window.notifications) {
        console.log('✅ Notifications loaded successfully');

        // Test notification (uncomment to test)
        // setTimeout(() => {
        //     notifications.info('AJAX system is working!', { title: 'Test Notification' });
        // }, 2000);
    } else {
        console.error('❌ Notifications not loaded');
    }

    // Test URL State Manager
    if (window.urlState) {
        console.log('✅ URL State Manager loaded successfully');
        console.log('Current URL state:', urlState.getState());
    } else {
        console.error('❌ URL State Manager not loaded');
    }

    // Test Book Manager (only on books page)
    if (document.querySelector('[data-page="books"]')) {
        setTimeout(() => {
            if (window.bookManager) {
                console.log('✅ Book Manager loaded successfully');
            } else {
                console.error('❌ Book Manager not loaded');
            }
        }, 1000);
    }
});