# Color Palette Quick Reference Guide

## 🎨 Role-Based Pastel Color System

### Auth Theme - Orange/Peach/Pear Pastels
```
Background: Light Peach (hsla(30, 89%, 93%, 1.00))
Secondary: Pear Tint (hsl(45, 80%, 92%))
Tertiary: Very Light Orange (hsl(25, 75%, 96%))
Primary Accent: Medium Peach (hsl(30, 75%, 65%))
Secondary Accent: Coral Orange (hsl(20, 70%, 60%))
Hover: Darker Peach (hsl(30, 80%, 55%))
Text: Near Black (hsl(0, 0%, 10%)) - Contrast: 12.6:1
Border: Soft Peach (hsl(30, 60%, 75%))
```

### Admin Theme - Greenish Pastels
```
Background: Light Mint Green (hsl(140, 45%, 92%))
Secondary: Pale Seafoam (hsl(150, 40%, 94%))
Tertiary: Very Light Green (hsl(135, 50%, 96%))
Primary Accent: Medium Green (hsl(145, 50%, 55%))
Secondary Accent: Teal Green (hsl(160, 45%, 50%))
Hover: Darker Green (hsl(145, 55%, 45%))
Text: Near Black (hsl(0, 0%, 12%)) - Contrast: 11.8:1
Border: Soft Green (hsl(145, 35%, 70%))
```

### SuperAdmin Theme - Reddish Pastels
```
Background: Light Rose (hsl(0, 60%, 93%))
Secondary: Pale Coral (hsl(10, 55%, 94%))
Tertiary: Very Light Pink-Red (hsl(355, 65%, 96%))
Primary Accent: Medium Red (hsl(0, 60%, 60%))
Secondary Accent: Coral Red (hsl(10, 55%, 55%))
Hover: Darker Red (hsl(0, 65%, 50%))
Text: Near Black (hsl(0, 0%, 10%)) - Contrast: 12.6:1
Border: Soft Red (hsl(0, 45%, 75%))
```

### User Theme - Bluish Pastels
```
Background: Light Sky Blue (hsl(210, 50%, 93%))
Secondary: Pale Cyan (hsl(200, 45%, 94%))
Tertiary: Very Light Blue (hsl(215, 55%, 96%))
Primary Accent: Medium Blue (hsl(210, 55%, 60%))
Secondary Accent: Cyan Blue (hsl(200, 50%, 55%))
Hover: Darker Blue (hsl(210, 60%, 50%))
Text: Near Black (hsl(0, 0%, 10%)) - Contrast: 12.6:1
Border: Soft Blue (hsl(210, 40%, 72%))
```

### Public Theme - Purplish Pastels
```
Background: Light Lavender (hsl(270, 45%, 93%))
Secondary: Pale Purple (hsl(280, 40%, 94%))
Tertiary: Very Light Violet (hsl(265, 50%, 96%))
Primary Accent: Medium Purple (hsl(270, 50%, 60%))
Secondary Accent: Violet Purple (hsl(280, 45%, 55%))
Hover: Darker Purple (hsl(270, 55%, 50%))
Text: Near Black (hsl(0, 0%, 10%)) - Contrast: 12.6:1
Border: Soft Purple (hsl(270, 35%, 73%))
```

## 📋 Component Colors

### Buttons
| Type | Background | Text | Border |
|------|------------|------|--------|
| Primary | Role Primary | White | Role Primary |
| Secondary | Gray (#6B7280) | White | Gray |
| Success | Green (#10B981) | White | Green |
| Warning | Orange (#F59E0B) | Dark | Orange |
| Danger | Red (#EF4444) | White | Red |
| Info | Blue (#3B82F6) | White | Blue |

### Badges
| Type | Background | Text |
|------|------------|------|
| Primary | Role Primary | White |
| Success | Green (#10B981) | White |
| Warning | Orange (#F59E0B) | Dark (#92400E) |
| Danger | Red (#EF4444) | White |
| Info | Blue (#3B82F6) | White |

### Alerts
| Type | Background | Border | Text |
|------|------------|--------|------|
| Success | Light Green (10% opacity) | Green | Dark Green (#065F46) |
| Warning | Light Orange (10% opacity) | Orange | Dark Orange (#92400E) |
| Danger | Light Red (10% opacity) | Red | Dark Red (#991B1B) |
| Info | Light Blue (10% opacity) | Blue | Dark Blue (#1E40AF) |

### Cards
```
Background: White (#FFFFFF)
Header: Role Primary with white text
Body: White with dark text (#111827)
Footer: Light Gray (#F3F4F6) with dark text
Border: Light Gray (#E5E7EB)
```

### Forms
```
Labels: Dark Gray (#1F2937)
Inputs: White background, dark text (#111827)
Placeholders: Medium Gray (#9CA3AF)
Focus Border: Role Primary
Error Border: Red (#EF4444)
```

### Tables
```
Header: Role Primary with white text
Body: White with dark text (#374151)
Striped Rows: Light Gray (#F3F4F6)
Hover: Light Gray (#F9FAFB)
Border: Light Gray (#E5E7EB)
```

## 🔤 Text Colors

### Standard Text
```css
Primary Text: #111827 (Very Dark Gray)
Secondary Text: #4B5563 (Dark Gray)
Muted Text: #6B7280 (Medium Gray)
Light Text: #9CA3AF (Light Gray)
```

### Text on Backgrounds
```css
On White/Light: #111827 (Dark)
On Dark: #FFFFFF (White)
On Primary: #FFFFFF (White)
On Success: #FFFFFF (White)
On Warning: #111827 (Dark)
On Danger: #FFFFFF (White)
On Info: #FFFFFF (White)
```

## 🎯 Semantic Colors

```css
Success: #10B981 (Green) - All themes
Warning: #F59E0B (Orange) - All themes  
Error/Danger: #EF4444 (Red) - All themes
Info: #3B82F6 (Blue) - All themes
Auth Success: hsl(140, 60%, 45%)
Admin Success: hsl(145, 65%, 45%)
SuperAdmin Critical: hsl(0, 80%, 45%)
User Info: hsl(210, 70%, 50%)
Public Highlight: hsl(280, 70%, 55%)
```

## 📐 Contrast Ratios (WCAG Compliance)

All role themes meet WCAG 2.1 Level AA requirements:

| Role | Text on Background | Ratio | Level |
|------|-------------------|-------|-------|
| Auth | Near Black on Light Peach | 12.6:1 | AAA ✅ |
| Admin | Near Black on Light Mint | 11.8:1 | AAA ✅ |
| SuperAdmin | Near Black on Light Rose | 12.6:1 | AAA ✅ |
| User | Near Black on Light Sky Blue | 12.6:1 | AAA ✅ |
| Public | Near Black on Light Lavender | 12.6:1 | AAA ✅ |
| All | Secondary Text on Background | 8.2:1+ | AAA ✅ |
| All | Muted Text on Background | 5.0:1+ | AA ✅ |

## 🛠️ Usage Examples

### Card with Header
```html
<div class="card">
    <div class="card-header bg-primary text-white">
        <h5>Card Title</h5>
    </div>
    <div class="card-body">
        <p class="text-dark">Card content with dark text</p>
    </div>
</div>
```

### Button Group
```html
<div class="btn-group">
    <button class="btn btn-primary">Primary</button>
    <button class="btn btn-success">Success</button>
    <button class="btn btn-warning">Warning</button>
</div>
```

### Form Field
```html
<div class="mb-3">
    <label class="form-label text-dark">Field Label</label>
    <input type="text" class="form-control" placeholder="Enter value">
    <small class="text-muted">Helper text</small>
</div>
```

### Alert
```html
<div class="alert alert-success">
    <i class="bi bi-check-circle"></i>
    Success message with proper contrast
</div>
```

### Badge
```html
<span class="badge bg-primary">Primary</span>
<span class="badge bg-success">Success</span>
<span class="badge bg-warning text-dark">Warning</span>
```

## 🎨 CSS Variables Reference

### Role-Specific Color Variables

#### Auth Theme
```css
--auth-bg-primary: hsla(30, 89%, 93%, 1.00)
--auth-bg-secondary: hsl(45, 80%, 92%)
--auth-bg-tertiary: hsl(25, 75%, 96%)
--auth-accent-primary: hsl(30, 75%, 65%)
--auth-accent-secondary: hsl(20, 70%, 60%)
--auth-accent-hover: hsl(30, 80%, 55%)
--auth-text-primary: hsl(0, 0%, 10%)
--auth-text-secondary: hsl(0, 0%, 25%)
--auth-text-muted: hsl(0, 0%, 40%)
--auth-border: hsl(30, 60%, 75%)
--auth-shadow: hsla(30, 50%, 50%, 0.15)
```

#### Admin Theme
```css
--admin-bg-primary: hsl(140, 45%, 92%)
--admin-bg-secondary: hsl(150, 40%, 94%)
--admin-bg-tertiary: hsl(135, 50%, 96%)
--admin-accent-primary: hsl(145, 50%, 55%)
--admin-accent-secondary: hsl(160, 45%, 50%)
--admin-accent-hover: hsl(145, 55%, 45%)
--admin-text-primary: hsl(0, 0%, 12%)
--admin-text-secondary: hsl(140, 10%, 25%)
--admin-text-muted: hsl(140, 8%, 40%)
--admin-border: hsl(145, 35%, 70%)
--admin-shadow: hsla(145, 40%, 40%, 0.15)
```

#### SuperAdmin Theme
```css
--superadmin-bg-primary: hsl(0, 60%, 93%)
--superadmin-bg-secondary: hsl(10, 55%, 94%)
--superadmin-bg-tertiary: hsl(355, 65%, 96%)
--superadmin-accent-primary: hsl(0, 60%, 60%)
--superadmin-accent-secondary: hsl(10, 55%, 55%)
--superadmin-accent-hover: hsl(0, 65%, 50%)
--superadmin-text-primary: hsl(0, 0%, 10%)
--superadmin-text-secondary: hsl(0, 10%, 25%)
--superadmin-text-muted: hsl(0, 8%, 40%)
--superadmin-border: hsl(0, 45%, 75%)
--superadmin-shadow: hsla(0, 50%, 50%, 0.15)
```

#### User Theme
```css
--user-bg-primary: hsl(210, 50%, 93%)
--user-bg-secondary: hsl(200, 45%, 94%)
--user-bg-tertiary: hsl(215, 55%, 96%)
--user-accent-primary: hsl(210, 55%, 60%)
--user-accent-secondary: hsl(200, 50%, 55%)
--user-accent-hover: hsl(210, 60%, 50%)
--user-text-primary: hsl(0, 0%, 10%)
--user-text-secondary: hsl(210, 10%, 25%)
--user-text-muted: hsl(210, 8%, 40%)
--user-border: hsl(210, 40%, 72%)
--user-shadow: hsla(210, 45%, 45%, 0.15)
```

#### Public Theme
```css
--public-bg-primary: hsl(270, 45%, 93%)
--public-bg-secondary: hsl(280, 40%, 94%)
--public-bg-tertiary: hsl(265, 50%, 96%)
--public-accent-primary: hsl(270, 50%, 60%)
--public-accent-secondary: hsl(280, 45%, 55%)
--public-accent-hover: hsl(270, 55%, 50%)
--public-text-primary: hsl(0, 0%, 10%)
--public-text-secondary: hsl(270, 10%, 25%)
--public-text-muted: hsl(270, 8%, 40%)
--public-border: hsl(270, 35%, 73%)
--public-shadow: hsla(270, 40%, 45%, 0.15)
```

### Semantic Variables (Auto-mapped by role)
```css
--bg-primary: Maps to active role's primary background
--bg-secondary: Maps to active role's secondary background
--bg-tertiary: Maps to active role's tertiary background
--text-primary: Maps to active role's primary text
--text-secondary: Maps to active role's secondary text
--text-muted: Maps to active role's muted text
--accent-primary: Maps to active role's primary accent
--accent-secondary: Maps to active role's secondary accent
--accent-hover: Maps to active role's hover accent
--border: Maps to active role's border color
--shadow: Maps to active role's shadow color
```

### Legacy Variables (Maintained for compatibility)
```css
--wp-brand-primary: #6366F1
--wp-brand-primary-dark: #4F46E5
--wp-brand-secondary: #10B981
--wp-success: #10B981
--wp-warning: #F59E0B
--wp-error: #EF4444
--wp-info: #3B82F6
```

## 🔍 Troubleshooting

### Text Not Visible?
1. Check if background color is set
2. Verify text color class is applied
3. Ensure no conflicting inline styles
4. Check browser console for CSS errors

### Colors Not Matching Design?
1. Clear browser cache
2. Verify CSS file is loaded (check Network tab)
3. Check for CSS specificity conflicts
4. Ensure role-color-palette-fix.css loads after theme engine

### Accessibility Issues?
1. Use browser DevTools to check contrast
2. Test with high contrast mode
3. Verify WCAG AA compliance (4.5:1 minimum)
4. Test with screen readers

## 📱 Responsive Considerations

All colors maintain proper contrast at all screen sizes:
- Mobile: Same color system
- Tablet: Same color system
- Desktop: Same color system

## 🖨️ Print Styles

When printing:
- All backgrounds become white
- All text becomes black
- Borders become black
- Simplified for readability

## ♿ Accessibility Features

- High contrast mode support
- Screen reader compatible
- Keyboard navigation friendly
- Focus states clearly visible
- Color blind friendly (uses patterns + colors)

## 🚀 Performance

- CSS file size: ~15KB (8KB minified)
- No JavaScript required
- Browser cached after first load
- No runtime calculations
- Pure CSS solution

## 📚 Related Files

- `role-based-theme-engine.css` - Base theme system
- `role-color-palette-fix.css` - Color fixes (this system)
- `modern-effects.css` - Animations and effects
- `wp-base-components.css` - Component styles

## 💡 Best Practices

1. **Always use semantic classes**: `btn-primary`, `text-muted`, etc.
2. **Avoid inline styles**: Use CSS classes instead
3. **Test contrast**: Use browser DevTools
4. **Consider accessibility**: WCAG AA minimum
5. **Use role variables**: `var(--role-primary)` in custom CSS
6. **Maintain consistency**: Follow the established patterns

## 🎓 Learning Resources

- [WCAG Contrast Guidelines](https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html)
- [Color Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [CSS Custom Properties](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)
- [Bootstrap Color System](https://getbootstrap.com/docs/5.3/customize/color/)
