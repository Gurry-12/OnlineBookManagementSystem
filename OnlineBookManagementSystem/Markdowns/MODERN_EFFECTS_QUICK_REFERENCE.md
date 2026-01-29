# Modern Effects Quick Reference Guide

## 🎨 CSS Classes Cheat Sheet

### Layout Effects
```html
<!-- Bento Grid Layout -->
<div class="bento-grid bento-grid--dashboard">
    <div class="bento-item bento-item--stats">Stats</div>
    <div class="bento-item bento-item--chart1">Chart 1</div>
    <div class="bento-item bento-item--chart2">Chart 2</div>
    <div class="bento-item bento-item--activity">Activity</div>
    <div class="bento-item bento-item--recent">Recent</div>
</div>

<!-- Sticky Header -->
<header class="sticky-header">
    <!-- Automatically adds 'scrolled' class on scroll -->
</header>

<!-- Scroll Fade Container -->
<div class="scroll-fade-container">
    <!-- Long content with fade at bottom -->
</div>

<!-- Parallax Card Stack -->
<div class="card-stack">
    <div class="card-stack__item">Card 1</div>
    <div class="card-stack__item">Card 2</div>
    <div class="card-stack__item">Card 3</div>
</div>
```

### Surface Effects
```html
<!-- Glassmorphism -->
<div class="glass">Frosted glass effect</div>
<div class="glass glass--light">Light variant</div>
<div class="glass glass--dark">Dark variant</div>

<!-- Claymorphism -->
<button class="clay">3D Clay Button</button>

<!-- Inner Glow -->
<div class="glow-inner glow-inner--accent">
    Backlit effect
</div>

<!-- Skeuomorphic Borders -->
<div class="carved">Carved appearance</div>
```

### Background Effects
```html
<!-- Aurora Background -->
<div class="aurora-bg">
    Animated gradient blobs
</div>

<!-- Noise Overlay -->
<div class="noise-overlay">
    Subtle texture
</div>

<!-- Spotlight Card -->
<div class="spotlight-card">
    Mouse-following highlight
</div>

<!-- Moving Grid -->
<div class="grid-bg">
    Animated grid lines
</div>
```

### Interaction Effects
```html
<!-- Skeleton Loading -->
<div class="skeleton skeleton--text"></div>
<div class="skeleton skeleton--title"></div>
<div class="skeleton skeleton--card"></div>

<!-- Magnetic Button -->
<button class="magnetic-btn">
    Cursor-attracted button
</button>

<!-- Staggered Fade In -->
<div data-fade-in class="stagger-fade-in">Item 1</div>
<div data-fade-in class="stagger-fade-in">Item 2</div>
<div data-fade-in class="stagger-fade-in">Item 3</div>

<!-- Holographic Shimmer -->
<button class="holographic">
    Diagonal light streak on hover
</button>

<!-- Ripple Effect -->
<button class="ripple-effect">
    Click for ripple
</button>
```

### Utility Classes
```html
<!-- Smooth Transitions -->
<div class="transition-smooth">Standard easing</div>
<div class="transition-bounce">Bouncy easing</div>

<!-- Hover Effects -->
<div class="hover-lift">Lifts on hover</div>
<div class="hover-glow">Glows on hover</div>
<div class="hover-scale">Scales on hover</div>
```

---

## 🎯 Common Combinations

### Premium Card
```html
<div class="spotlight-card glass hover-lift">
    <h3>Premium Card</h3>
    <p>With multiple effects</p>
    <button class="magnetic-btn holographic">Action</button>
</div>
```

### Auth Page
```html
<div class="aurora-bg noise-overlay">
    <div class="auth-container">
        <div class="auth-card glass">
            <h2>Login</h2>
            <button class="magnetic-btn holographic">
                Sign In
            </button>
        </div>
    </div>
</div>
```

### Dashboard Stats
```html
<div class="bento-grid bento-grid--dashboard">
    <div class="bento-item glass hover-lift">
        <div class="spotlight-card">
            <h3>Total Books</h3>
            <p class="stats-number">1,234</p>
            <button class="magnetic-btn">View</button>
        </div>
    </div>
</div>
```

### Loading State
```html
<!-- Before loading -->
<div id="content">
    <div class="skeleton skeleton--title"></div>
    <div class="skeleton skeleton--text"></div>
    <div class="skeleton skeleton--text"></div>
    <div class="skeleton skeleton--card"></div>
</div>

<!-- After loading -->
<div id="content">
    <h2>Actual Title</h2>
    <p>Actual content...</p>
    <div class="spotlight-card">...</div>
</div>
```

---

## 💻 JavaScript API

### Loading Manager
```javascript
const loader = new ModernEffects.LoadingStateManager();

// Show loading
loader.show('#my-element');

// Hide loading
loader.hide('#my-element');
```

### Toast Notifications
```javascript
const toast = new ModernEffects.ToastManager();

// Types: 'success', 'error', 'warning', 'info'
toast.show('Message here', 'success', 3000);
```

### Manual Initialization
```javascript
// Reinitialize after dynamic content
ModernEffects.init();
```

---

## 🎨 Color Customization

Override CSS variables in your stylesheet:

```css
:root {
    /* Accent Colors */
    --accent-rgb: 59, 130, 246;
    
    /* Surface Colors */
    --surface-color: #1a1a1a;
    --bg-color: #0a0a0a;
    
    /* Role-based (if using theme engine) */
    --role-primary: #3b82f6;
    --role-accent: #f3f4f6;
}
```

---

## 📱 Responsive Behavior

All effects automatically adapt:
- **Desktop:** Full effects
- **Tablet:** Optimized layouts
- **Mobile:** Simplified animations
- **Reduced Motion:** Respects user preferences

---

## ⚡ Performance Tips

1. **Use CSS-only effects when possible** - Better performance
2. **Limit spotlight cards** - Can be GPU intensive
3. **Lazy load effects** - Only on visible elements
4. **Combine classes wisely** - Don't overdo it
5. **Test on mobile** - Ensure smooth performance

---

## 🐛 Troubleshooting

### Effect not working?
1. Check if `modern-effects.css` is loaded
2. Check if `modern-effects.js` is loaded
3. Verify class names are correct
4. Check browser console for errors

### Spotlight not following mouse?
- Ensure element has `.spotlight-card` class
- Check if JavaScript is initialized
- Verify no CSS conflicts

### Magnetic buttons not working?
- Add `.magnetic-btn` class
- Ensure JavaScript is loaded
- Check for CSS transform conflicts

---

## 📚 Where Effects Are Used

### Admin Dashboard
- ✅ Bento grid layout
- ✅ Spotlight cards (stats)
- ✅ Glass effects
- ✅ Magnetic buttons
- ✅ Staggered fade-in (activities)

### Book Management
- ✅ Spotlight cards (book items)
- ✅ Magnetic buttons (actions)
- ✅ Hover lift effects

### Auth Pages
- ✅ Aurora background
- ✅ Noise overlay
- ✅ Glass cards
- ✅ Holographic buttons
- ✅ Magnetic social buttons

### All Layouts
- ✅ Sticky headers
- ✅ Smooth scroll
- ✅ Ripple effects
- ✅ Toast notifications

---

**Last Updated:** January 29, 2026  
**Version:** 1.0.0
