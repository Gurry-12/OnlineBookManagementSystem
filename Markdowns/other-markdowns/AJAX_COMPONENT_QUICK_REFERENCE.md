# AJAX Component Architecture - Quick Reference

## 🚀 Quick Start

### 1. Load Partial View
```javascript
ajaxWrapper.load({
    url: '/Admin/Books',
    container: '#books-content',
    data: { page: 1, search: 'fiction' }
});
```

### 2. Submit Form
```javascript
ajaxWrapper.submit({
    form: '#book-form',
    successMessage: 'Saved!'
});
```

### 3. Delete Resource
```javascript
ajaxWrapper.delete({
    url: `/Books/Delete/${id}`,
    container: '#books-content',
    confirmMessage: 'Delete this book?'
});
```

---

## 📦 Component Usage

### Book Card
```razor
@await Html.PartialAsync("Components/_BookCard", bookDto)
```

### Book Grid
```razor
<div id="books-content">
    @await Html.PartialAsync("Components/_BookGrid", bookListViewModel)
</div>
```

### Loading State
```razor
@await Html.PartialAsync("Components/_LoadingState", new { Message = "Loading..." })
```

### Empty State
```razor
@await Html.PartialAsync("Components/_EmptyState", new { 
    Icon = "book",
    Title = "No Books Found",
    Message = "Try adjusting your filters",
    ActionText = "Add Book",
    ActionClass = "create-book-btn"
})
```

### Error Alert
```razor
@await Html.PartialAsync("Components/_ErrorAlert", new {
    Title = "Error",
    Message = "Something went wrong",
    RetryAction = "retryFunction()"
})
```

---

## 🎨 CSS Classes (BEM)

### Book Card
```html
<div class="book-card">
    <div class="book-card__header">
        <div class="book-card__badge book-card__badge--out-of-stock">
        <div class="book-card__image-container">
            <img class="book-card__image" />
    <div class="book-card__body">
        <h6 class="book-card__title">
        <p class="book-card__author">
        <div class="book-card__price-stock">
            <span class="book-card__price">
            <span class="book-card__stock">
```

### Buttons
```html
<button class="btn btn--primary">Primary</button>
<button class="btn btn--secondary">Secondary</button>
<button class="btn btn--success">Success</button>
<button class="btn btn--danger">Danger</button>
<button class="btn btn--sm">Small</button>
<button class="btn btn--lg">Large</button>
```

### Badges
```html
<span class="badge badge--category">Fiction</span>
<span class="badge badge--success">In Stock</span>
<span class="badge badge--danger">Out of Stock</span>
<span class="badge badge--warning">Low Stock</span>
```

---

## 🎯 CSS Variables

### Colors
```css
var(--color-primary)
var(--color-success)
var(--color-danger)
var(--color-warning)
var(--color-info)
```

### Spacing
```css
var(--spacing-xs)    /* 4px */
var(--spacing-sm)    /* 8px */
var(--spacing-md)    /* 16px */
var(--spacing-lg)    /* 24px */
var(--spacing-xl)    /* 32px */
```

### Typography
```css
var(--font-size-xs)
var(--font-size-sm)
var(--font-size-base)
var(--font-size-lg)
var(--font-size-xl)
```

### Shadows
```css
var(--shadow-sm)
var(--shadow-md)
var(--shadow-lg)
var(--shadow-xl)
```

### Transitions
```css
var(--transition-fast)   /* 150ms */
var(--transition-base)   /* 250ms */
var(--transition-slow)   /* 350ms */
```

---

## 🔧 Controller Pattern

```csharp
[HttpGet]
public async Task<IActionResult> Books(string search, int page = 1)
{
    var model = await _service.GetBooksAsync(search, page);
    
    // AJAX request - return partial
    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
    {
        return PartialView("Components/_BookGrid", model);
    }
    
    // Full page request
    return View(model);
}
```

---

## 📱 Event Handling

### Pagination
```javascript
document.addEventListener('click', (e) => {
    if (e.target.matches('.pagination__link')) {
        e.preventDefault();
        const page = e.target.dataset.page;
        ajaxWrapper.load({
            url: '/Books',
            container: '#books-content',
            data: { page }
        });
    }
});
```

### Card Actions
```javascript
document.addEventListener('click', (e) => {
    const action = e.target.dataset.action;
    const bookId = e.target.dataset.bookId;
    
    if (action === 'view') {
        window.location.href = `/Books/Details/${bookId}`;
    }
    
    if (action === 'edit') {
        ajaxWrapper.load({
            url: `/Books/Edit/${bookId}`,
            container: '#modal-content'
        });
    }
    
    if (action === 'delete') {
        ajaxWrapper.delete({
            url: `/Books/Delete/${bookId}`,
            container: '#books-content',
            onSuccess: () => refreshBooks()
        });
    }
});
```

---

## 🎭 Loading States

### Manual Control
```javascript
// Show loading
ajaxWrapper.showLoading('#books-content', 'Loading books...');

// Hide loading
ajaxWrapper.hideLoading('#books-content');

// Show error
ajaxWrapper.showError('#books-content', 'Failed to load books');
```

### Automatic (via ajaxWrapper.load)
```javascript
// Loading state is automatic!
ajaxWrapper.load({
    url: '/Books',
    container: '#books-content',
    loadingMessage: 'Custom loading message...'
});
```

---

## ✅ Validation Integration

### Form with Validation
```javascript
ajaxWrapper.submit({
    form: '#book-form',
    validateForm: true,  // Re-initializes jQuery validation
    onSuccess: (result) => {
        // Form is valid and submitted
    }
});
```

### Manual Validation Check
```javascript
const form = document.querySelector('#book-form');
const validator = $(form).validate();

if (validator.form()) {
    // Form is valid
    ajaxWrapper.submit({ form: '#book-form' });
}
```

---

## 🎨 Custom Styling

### Override Variables
```css
:root {
    --color-primary: #your-color;
    --spacing-md: 1.5rem;
    --border-radius-lg: 1rem;
}
```

### Extend Components
```css
/* Add custom modifier */
.book-card--featured {
    border: 2px solid var(--color-primary);
    box-shadow: var(--shadow-xl);
}

.book-card--featured .book-card__title {
    color: var(--color-primary);
    font-size: var(--font-size-lg);
}
```

---

## 🐛 Debugging

### Check Active Requests
```javascript
console.log(ajaxWrapper.config);
```

### Cancel All Requests
```javascript
ajaxWrapper.cancelAll();
```

### Listen to Events
```javascript
document.querySelector('#books-content').addEventListener('ajax:success', (e) => {
    console.log('AJAX Success:', e.detail);
});

document.querySelector('#books-content').addEventListener('ajax:error', (e) => {
    console.error('AJAX Error:', e.detail);
});
```

---

## 📋 Common Patterns

### Search with Debounce
```javascript
let searchTimeout;
document.querySelector('[name="search"]').addEventListener('input', (e) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        ajaxWrapper.load({
            url: '/Books',
            container: '#books-content',
            data: { search: e.target.value }
        });
    }, 300);
});
```

### Infinite Scroll
```javascript
window.addEventListener('scroll', () => {
    if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 500) {
        loadMoreBooks();
    }
});

function loadMoreBooks() {
    ajaxWrapper.load({
        url: '/Books',
        container: '#books-content',
        data: { page: currentPage + 1 },
        replaceContent: false  // Append instead of replace
    });
}
```

### Modal Form
```javascript
function openBookModal(bookId) {
    ajaxWrapper.load({
        url: `/Books/Edit/${bookId}`,
        container: '#modal-content',
        validateForm: true,
        onSuccess: () => {
            $('#bookModal').modal('show');
        }
    });
}

function saveBookModal() {
    ajaxWrapper.submit({
        form: '#modal-book-form',
        onSuccess: () => {
            $('#bookModal').modal('hide');
            refreshBooks();
        }
    });
}
```

---

## 🎯 Performance Tips

1. **Use `replaceContent: false` for appending**
2. **Debounce search inputs (300ms)**
3. **Cancel previous requests before new ones**
4. **Use lazy loading for images**
5. **Minimize DOM queries (cache selectors)**

---

## 🔒 Security

### CSRF Token (Automatic)
```javascript
// ajaxWrapper automatically includes CSRF token
// from: <input name="__RequestVerificationToken" />
```

### Manual CSRF
```javascript
const token = document.querySelector('[name="__RequestVerificationToken"]').value;
ajaxWrapper.load({
    url: '/Books',
    headers: { 'RequestVerificationToken': token }
});
```

---

## 📚 File Locations

```
Presentation/
├── Views/Shared/Components/
│   ├── _BookCard.cshtml
│   ├── _BookGrid.cshtml
│   ├── _LoadingState.cshtml
│   ├── _EmptyState.cshtml
│   ├── _Pagination.cshtml
│   └── _ErrorAlert.cshtml
├── wwwroot/
│   ├── css/
│   │   ├── core/variables.css
│   │   ├── components/
│   │   │   ├── book-card.css
│   │   │   ├── empty-state.css
│   │   │   ├── loading-state.css
│   │   │   ├── pagination.css
│   │   │   └── error-alert.css
│   │   └── components.css
│   └── js/
│       └── core/ajaxWrapper.js
```

---

## 🎉 That's It!

You're ready to build AJAX-driven, component-based features!

**Remember:**
- Use components for reusability
- Use CSS variables for consistency
- Use ajaxWrapper for AJAX
- Use BEM for naming
- Test loading & error states
