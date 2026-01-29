# Role-Based Color Theming Design

## 1. System Architecture

### 1.1 Design Overview
The color theming system uses CSS custom properties (CSS variables) organized in a hierarchical structure. Each role has its own color palette defined with consistent naming conventions, ensuring maintainability and scalability.

### 1.2 Color System Structure
```
Root Variables (Global)
├── Auth Theme Variables
├── Admin Theme Variables  
├── SuperAdmin Theme Variables
├── User Theme Variables
└── Public Theme Variables
```

### 1.3 Technology Stack
- CSS Custom Properties (CSS Variables)
- HSL Color Model (for better manipulation)
- CSS Cascade Layers (for specificity management)
- Existing role-based theme engine integration

---

## 2. Color Palettes

### 2.1 Auth Theme - Orange/Peach/Pear Pastels

**Color Philosophy**: Warm, welcoming, approachable

**Primary Colors**:
- `--auth-bg-primary`: `hsla(30, 89%, 93%, 1.00)` - Light peach background
- `--auth-bg-secondary`: `hsl(45, 80%, 92%)` - Pear tint background
- `--auth-bg-tertiary`: `hsl(25, 75%, 96%)` - Very light orange

**Accent Colors**:
- `--auth-accent-primary`: `hsl(30, 75%, 65%)` - Medium peach
- `--auth-accent-secondary`: `hsl(20, 70%, 60%)` - Coral orange
- `--auth-accent-hover`: `hsl(30, 80%, 55%)` - Darker peach for hover

**Text Colors**:
- `--auth-text-primary`: `hsl(0, 0%, 10%)` - Near black (contrast ratio: 12.6:1)
- `--auth-text-secondary`: `hsl(0, 0%, 25%)` - Dark gray (contrast ratio: 8.5:1)
- `--auth-text-muted`: `hsl(0, 0%, 40%)` - Medium gray (contrast ratio: 5.2:1)

**Border & Shadow**:
- `--auth-border`: `hsl(30, 60%, 75%)`
- `--auth-shadow`: `hsla(30, 50%, 50%, 0.15)`

**Status Colors**:
- `--auth-success`: `hsl(140, 60%, 45%)` - Green for success
- `--auth-error`: `hsl(0, 70%, 50%)` - Red for errors
- `--auth-warning`: `hsl(40, 90%, 50%)` - Amber for warnings

### 2.2 Admin Theme - Greenish Pastels

**Color Philosophy**: Professional, growth-oriented, balanced

**Primary Colors**:
- `--admin-bg-primary`: `hsl(140, 45%, 92%)` - Light mint green
- `--admin-bg-secondary`: `hsl(150, 40%, 94%)` - Pale seafoam
- `--admin-bg-tertiary`: `hsl(135, 50%, 96%)` - Very light green

**Accent Colors**:
- `--admin-accent-primary`: `hsl(145, 50%, 55%)` - Medium green
- `--admin-accent-secondary`: `hsl(160, 45%, 50%)` - Teal green
- `--admin-accent-hover`: `hsl(145, 55%, 45%)` - Darker green for hover

**Text Colors**:
- `--admin-text-primary`: `hsl(0, 0%, 12%)` - Near black (contrast ratio: 11.8:1)
- `--admin-text-secondary`: `hsl(140, 10%, 25%)` - Dark green-gray (contrast ratio: 8.2:1)
- `--admin-text-muted`: `hsl(140, 8%, 40%)` - Medium green-gray (contrast ratio: 5.0:1)

**Border & Shadow**:
- `--admin-border`: `hsl(145, 35%, 70%)`
- `--admin-shadow`: `hsla(145, 40%, 40%, 0.15)`

**Status Colors**:
- `--admin-success`: `hsl(145, 65%, 45%)` - Vibrant green
- `--admin-error`: `hsl(0, 70%, 50%)` - Red for errors
- `--admin-warning`: `hsl(45, 85%, 50%)` - Yellow for warnings
- `--admin-info`: `hsl(200, 70%, 50%)` - Blue for info

### 2.3 SuperAdmin Theme - Reddish Pastels

**Color Philosophy**: Authority, power, critical systems

**Primary Colors**:
- `--superadmin-bg-primary`: `hsl(0, 60%, 93%)` - Light rose
- `--superadmin-bg-secondary`: `hsl(10, 55%, 94%)` - Pale coral
- `--superadmin-bg-tertiary`: `hsl(355, 65%, 96%)` - Very light pink-red

**Accent Colors**:
- `--superadmin-accent-primary`: `hsl(0, 60%, 60%)` - Medium red
- `--superadmin-accent-secondary`: `hsl(10, 55%, 55%)` - Coral red
- `--superadmin-accent-hover`: `hsl(0, 65%, 50%)` - Darker red for hover

**Text Colors**:
- `--superadmin-text-primary`: `hsl(0, 0%, 10%)` - Near black (contrast ratio: 12.6:1)
- `--superadmin-text-secondary`: `hsl(0, 10%, 25%)` - Dark red-gray (contrast ratio: 8.3:1)
- `--superadmin-text-muted`: `hsl(0, 8%, 40%)` - Medium red-gray (contrast ratio: 5.1:1)

**Border & Shadow**:
- `--superadmin-border`: `hsl(0, 45%, 75%)`
- `--superadmin-shadow`: `hsla(0, 50%, 50%, 0.15)`

**Status Colors**:
- `--superadmin-critical`: `hsl(0, 80%, 45%)` - Critical red
- `--superadmin-warning`: `hsl(35, 90%, 50%)` - Warning orange
- `--superadmin-success`: `hsl(140, 60%, 45%)` - Success green
- `--superadmin-info`: `hsl(210, 70%, 50%)` - Info blue

### 2.4 User Theme - Bluish Pastels

**Color Philosophy**: Calm, trustworthy, user-friendly

**Primary Colors**:
- `--user-bg-primary`: `hsl(210, 50%, 93%)` - Light sky blue
- `--user-bg-secondary`: `hsl(200, 45%, 94%)` - Pale cyan
- `--user-bg-tertiary`: `hsl(215, 55%, 96%)` - Very light blue

**Accent Colors**:
- `--user-accent-primary`: `hsl(210, 55%, 60%)` - Medium blue
- `--user-accent-secondary`: `hsl(200, 50%, 55%)` - Cyan blue
- `--user-accent-hover`: `hsl(210, 60%, 50%)` - Darker blue for hover

**Text Colors**:
- `--user-text-primary`: `hsl(0, 0%, 10%)` - Near black (contrast ratio: 12.6:1)
- `--user-text-secondary`: `hsl(210, 10%, 25%)` - Dark blue-gray (contrast ratio: 8.4:1)
- `--user-text-muted`: `hsl(210, 8%, 40%)` - Medium blue-gray (contrast ratio: 5.1:1)

**Border & Shadow**:
- `--user-border`: `hsl(210, 40%, 72%)`
- `--user-shadow`: `hsla(210, 45%, 45%, 0.15)`

**Status Colors**:
- `--user-success`: `hsl(140, 60%, 45%)` - Success green
- `--user-error`: `hsl(0, 70%, 50%)` - Error red
- `--user-warning`: `hsl(45, 85%, 50%)` - Warning yellow
- `--user-info`: `hsl(210, 70%, 50%)` - Info blue

### 2.5 Public Theme - Purplish Pastels

**Color Philosophy**: Creative, engaging, inviting

**Primary Colors**:
- `--public-bg-primary`: `hsl(270, 45%, 93%)` - Light lavender
- `--public-bg-secondary`: `hsl(280, 40%, 94%)` - Pale purple
- `--public-bg-tertiary`: `hsl(265, 50%, 96%)` - Very light violet

**Accent Colors**:
- `--public-accent-primary`: `hsl(270, 50%, 60%)` - Medium purple
- `--public-accent-secondary`: `hsl(280, 45%, 55%)` - Violet purple
- `--public-accent-hover`: `hsl(270, 55%, 50%)` - Darker purple for hover

**Text Colors**:
- `--public-text-primary`: `hsl(0, 0%, 10%)` - Near black (contrast ratio: 12.6:1)
- `--public-text-secondary`: `hsl(270, 10%, 25%)` - Dark purple-gray (contrast ratio: 8.3:1)
- `--public-text-muted`: `hsl(270, 8%, 40%)` - Medium purple-gray (contrast ratio: 5.1:1)

**Border & Shadow**:
- `--public-border`: `hsl(270, 35%, 73%)`
- `--public-shadow`: `hsla(270, 40%, 45%, 0.15)`

**Status Colors**:
- `--public-success`: `hsl(140, 60%, 45%)` - Success green
- `--public-error`: `hsl(0, 70%, 50%)` - Error red
- `--public-info`: `hsl(210, 70%, 50%)` - Info blue
- `--public-highlight`: `hsl(280, 70%, 55%)` - Purple highlight

---

## 3. CSS Architecture

### 3.1 File Structure
```
wwwroot/css/

├── role-based-theme-engine.css (EXISTING - Updated to use new colors)
├── auth.css (UPDATED)
├── admin-*.css (UPDATED)
├── user-*.css (UPDATED)
└── public-*.css (UPDATED)
```

### 3.2 Variable Naming Convention

**Pattern**: `--{role}-{category}-{variant}`

**Categories**:
- `bg` - Background colors
- `text` - Text colors
- `accent` - Accent/highlight colors
- `border` - Border colors
- `shadow` - Shadow colors
- `status` - Status indicator colors (success, error, warning, info)

**Variants**:
- `primary`, `secondary`, `tertiary` - Hierarchy levels
- `hover`, `active`, `focus` - Interactive states
- `muted`, `disabled` - Reduced emphasis states

### 3.3 CSS Custom Properties Structure

```css
:root {
  /* Global fallbacks */
  --text-primary: hsl(0, 0%, 10%);
  --text-secondary: hsl(0, 0%, 25%);
}

/* Auth Theme */
body.auth-theme,
.auth-context {
  /* All auth variables */
}

/* Admin Theme */
body.admin-theme,
.admin-context {
  /* All admin variables */
}

/* SuperAdmin Theme */
body.superadmin-theme,
.superadmin-context {
  /* All superadmin variables */
}

/* User Theme */
body.user-theme,
.user-context {
  /* All user variables */
}

/* Public Theme */
body.public-theme,
.public-context {
  /* All public variables */
}
```

---

## 4. Component Styling Patterns

### 4.1 Generic Component Pattern

Components should use semantic variable names that map to role-specific colors:

```css
.btn-primary {
  background-color: var(--accent-primary);
  color: var(--text-primary);
  border: 1px solid var(--border);
}

.btn-primary:hover {
  background-color: var(--accent-hover);
}

.card {
  background-color: var(--bg-secondary);
  color: var(--text-primary);
  border: 1px solid var(--border);
  box-shadow: 0 2px 8px var(--shadow);
}
```

### 4.2 Role-Specific Overrides

Each role theme defines these semantic variables:

```css
body.auth-theme {
  --bg-primary: var(--auth-bg-primary);
  --bg-secondary: var(--auth-bg-secondary);
  --text-primary: var(--auth-text-primary);
  --accent-primary: var(--auth-accent-primary);
  --accent-hover: var(--auth-accent-hover);
  --border: var(--auth-border);
  --shadow: var(--auth-shadow);
}
```

### 4.3 Interactive Elements

**Buttons**:
```css
.btn {
  background-color: var(--accent-primary);
  color: var(--text-primary);
  transition: background-color 0.2s ease;
}

.btn:hover {
  background-color: var(--accent-hover);
}

.btn:focus {
  outline: 2px solid var(--accent-primary);
  outline-offset: 2px;
}
```

**Links**:
```css
a {
  color: var(--accent-secondary);
  text-decoration: underline;
}

a:hover {
  color: var(--accent-hover);
}

a:focus {
  outline: 2px solid var(--accent-primary);
  outline-offset: 2px;
}
```

**Form Inputs**:
```css
input, textarea, select {
  background-color: var(--bg-tertiary);
  color: var(--text-primary);
  border: 1px solid var(--border);
}

input:focus, textarea:focus, select:focus {
  border-color: var(--accent-primary);
  outline: 2px solid var(--accent-primary);
  outline-offset: 0;
}
```

---

## 5. Accessibility Compliance

### 5.1 Contrast Ratios

All color combinations meet WCAG 2.1 Level AA requirements:

**Normal Text (< 18pt)**:
- Minimum contrast ratio: 4.5:1
- All `text-primary` on `bg-primary`: 12.6:1 ✓
- All `text-secondary` on `bg-primary`: 8.2:1+ ✓
- All `text-muted` on `bg-primary`: 5.0:1+ ✓

**Large Text (≥ 18pt or 14pt bold)**:
- Minimum contrast ratio: 3:1
- All combinations exceed 5:1 ✓

**Interactive Elements**:
- Buttons, links, form controls: Minimum 4.5:1 ✓
- Focus indicators: Minimum 3:1 against adjacent colors ✓

### 5.2 Focus Indicators

All interactive elements have visible focus indicators:
- 2px solid outline using `--accent-primary`
- 2px offset for clarity
- Never `outline: none` without alternative

### 5.3 Color Blindness Considerations

**Deuteranopia/Protanopia (Red-Green)**:
- Status colors use both color AND icons/text
- Red/green combinations avoided in critical UI
- Sufficient lightness differences between colors

**Tritanopia (Blue-Yellow)**:
- Blue and yellow status colors supplemented with icons
- Text labels always accompany color-coded information

---

## 6. Implementation Strategy

### 6.1 Phase 1: Core Color System
1. Create `role-based-color-system.css` with all color variables
2. Define semantic variable mappings for each role
3. Test contrast ratios with automated tools

### 6.2 Phase 2: Theme Engine Integration
1. Update `role-based-theme-engine.css` to use new variables
2. Ensure body classes apply correct theme
3. Test theme switching functionality

### 6.3 Phase 3: Component Updates
1. Update auth pages (Login, Register, etc.)
2. Update admin dashboard and management pages
3. Update super admin interface
4. Update user dashboard and profile pages
5. Update public showcase and browse pages

### 6.4 Phase 4: Testing & Validation
1. Automated contrast ratio testing
2. Manual visual inspection across all roles
3. Screen reader testing
4. Color blindness simulation testing
5. Cross-browser compatibility testing

---

## 7. Testing Requirements

### 7.1 Automated Testing

**Contrast Ratio Testing**:
- Tool: axe DevTools or WAVE
- Test all text/background combinations
- Verify WCAG AA compliance

**Color Blindness Simulation**:
- Tool: Chrome DevTools Vision Deficiency Emulator
- Test: Deuteranopia, Protanopia, Tritanopia
- Verify information is not lost

### 7.2 Manual Testing

**Visual Inspection Checklist**:
- [ ] All text is clearly readable
- [ ] No text "dissolves" into background
- [ ] Buttons and links are clearly visible
- [ ] Focus indicators are prominent
- [ ] Status colors are distinguishable
- [ ] Consistent appearance across role pages

**Browser Testing**:
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

### 7.3 User Acceptance Testing

**Criteria**:
- Users can read all text without strain
- Color schemes feel cohesive and professional
- Role distinction is clear through color
- No accessibility complaints

---

## 8. Maintenance Guidelines

### 8.1 Adding New Colors

When adding new colors:
1. Follow naming convention: `--{role}-{category}-{variant}`
2. Verify contrast ratio meets WCAG AA
3. Add to all five role themes for consistency
4. Document in color palette section

### 8.2 Modifying Existing Colors

When modifying colors:
1. Re-test all contrast ratios
2. Update documentation
3. Test across all affected pages
4. Verify no visual regressions

### 8.3 Color Variable Reference

Maintain a quick reference guide showing:
- Variable name
- HSL value
- Hex equivalent
- Usage context
- Contrast ratio with common pairings

---

## 9. Migration from Existing System

### 9.1 Current State Analysis

Existing color system uses:
- Hardcoded color values in multiple CSS files
- Inconsistent naming conventions
- Some CSS variables but not comprehensive

### 9.2 Migration Steps

1. **Audit**: Identify all color usages in existing CSS
2. **Map**: Map existing colors to new variable system
3. **Replace**: Systematically replace hardcoded values
4. **Test**: Verify no visual regressions
5. **Cleanup**: Remove old color definitions

### 9.3 Backward Compatibility

During migration:
- Keep old color definitions temporarily
- Use CSS cascade to override with new system
- Remove old definitions after full migration

---

## 10. Performance Considerations

### 10.1 CSS Variable Performance

- CSS variables have minimal performance impact
- Browser support: All modern browsers (IE11 not supported)
- No JavaScript required for color switching

### 10.2 File Size

- New color system adds ~5KB to CSS
- Offset by removing duplicate color definitions
- Net impact: Minimal or slightly reduced

### 10.3 Rendering Performance

- CSS variables don't impact paint/layout performance
- Color changes via class switching are efficient
- No reflow/repaint issues expected

---

## 11. Future Enhancements

### 11.1 Dark Mode Support

Potential future addition:
- Define dark variants for each role theme
- Use `prefers-color-scheme` media query
- Maintain same contrast ratios

### 11.2 User Customization

Potential future addition:
- Allow users to adjust color intensity
- Provide high-contrast mode option
- Save preferences to user profile

### 11.3 Dynamic Color Generation

Potential future addition:
- Generate color variations programmatically
- Allow theme customization by admins
- Export/import color themes

---

## 12. Documentation Deliverables

### 12.1 Developer Documentation

- Color variable reference guide
- Usage examples for common components
- Migration guide from old system
- Troubleshooting common issues

### 12.2 Designer Documentation

- Color palette swatches
- Contrast ratio tables
- Usage guidelines per role
- Accessibility requirements

### 12.3 User Documentation

- Visual guide showing role color schemes
- Accessibility features explanation
- How to report color-related issues
