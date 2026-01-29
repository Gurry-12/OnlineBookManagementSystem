# Role-Based Color Theming - Implementation Tasks

## Phase 1: Core Color System Setup

### 1. Complete Core Color System File (Validates: Requirements 1.1-5.5, 7.1-7.5)
- [x] 1.1 Add Admin theme color variables to `role-based-color-system.css` (greenish pastels per design)
- [x] 1.2 Add SuperAdmin theme color variables to `role-based-color-system.css` (reddish pastels per design)
- [x] 1.3 Add User theme color variables to `role-based-color-system.css` (bluish pastels per design)
- [x] 1.4 Add Public theme color variables to `role-based-color-system.css` (purplish pastels per design)
- [x] 1.5 Create semantic variable mappings for all themes (--bg-primary, --text-primary, --accent-primary, etc.)

### 2. Update Body Classes in Layouts (Validates: Requirements 1.5, 2.5, 3.5, 4.5, 5.5)
- [x] 2.1 Change `body` class in _LayoutAuth.cshtml from `auth-body` to `auth-theme`
- [x] 2.2 Change `body` class in _LayoutAdmin.cshtml from `admin-layout` to `admin-layout admin-theme`
- [x] 2.3 Change `body` class in _LayoutSuperAdmin.cshtml from `superadmin-layout` to `superadmin-layout superadmin-theme`
- [x] 2.4 Change `body` class in _LayoutUser.cshtml from `user-layout` to `user-layout user-theme`
- [x] 2.5 Change `body` class in _LayoutPublic.cshtml from `public-layout` to `public-layout public-theme`

### 3. Link Color System to Application (Validates: Requirements 1.5, 2.5, 3.5, 4.5, 5.5)
- [x] 3.1 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutAuth.cshtml (before role-based-theme-engine.css)
- [x] 3.2 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutAdmin.cshtml (before role-based-theme-engine.css)
- [x] 3.3 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutSuperAdmin.cshtml (before role-based-theme-engine.css)
- [x] 3.4 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutUser.cshtml (before role-based-theme-engine.css)
- [x] 3.5 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutPublic.cshtml (before role-based-theme-engine.css)
- [ ] 3.2 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutAdmin.cshtml (before role-based-theme-engine.css)
- [ ] 3.3 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutSuperAdmin.cshtml (before role-based-theme-engine.css)
- [ ] 3.4 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutUser.cshtml (before role-based-theme-engine.css)
- [ ] 3.5 Add `<link href="~/css/role-based-color-system.css" rel="stylesheet" asp-append-version="true" />` to _LayoutPublic.cshtml (before role-based-theme-engine.css)

---

## Phase 2: Testing & Validation (Validates: Requirements 6.1-6.5)

### 4. Visual Testing
- [x] 4.1 Test Auth pages display orange/peach/pear pastels with black text
- [ ] 4.2 Test Admin pages display greenish pastels with high contrast text
- [ ] 4.3 Test SuperAdmin pages display reddish pastels with clear text
- [ ] 4.4 Test User pages display bluish pastels with readable text
- [x] 4.5 Test Public pages display purplish pastels with excellent contrast

### 5. Accessibility Testing (Validates: Requirements 6.1-6.5)
- [ ] 5.1 Run axe DevTools or WAVE on all role pages
- [ ] 5.2 Verify all text meets WCAG 2.1 Level AA contrast (4.5:1 minimum)
- [ ] 5.3 Test keyboard navigation and focus indicators on all pages
- [ ] 5.4 Test with Chrome DevTools Vision Deficiency Emulator (Deuteranopia, Protanopia, Tritanopia)
- [ ] 5.5 Verify no information is conveyed by color alone

### 6. Cross-Browser Testing
- [ ] 6.1 Test in Chrome (latest) - verify colors display correctly
- [ ] 6.2 Test in Firefox (latest) - verify colors display correctly
- [ ] 6.3 Test in Edge (latest) - verify colors display correctly
- [ ] 6.4 Test responsive design at mobile (375px), tablet (768px), desktop (1920px)

---

## Phase 3: Documentation

### 7. Update Documentation (Validates: Requirements 7.1-7.5)
- [ ] 7.1 Update `Markdowns/COLOR_PALETTE_QUICK_REFERENCE.md` with new color system
- [ ] 7.2 Add color swatches and HSL values for each role
- [ ] 7.3 Document contrast ratios for all text/background combinations
- [ ] 7.4 Add usage examples for developers
- [ ] 7.5 Document semantic variable naming conventions

---

## Notes

### Current State Analysis
- Auth theme colors are partially defined in `role-based-color-system.css`
- Body classes in layouts use `-layout` suffix but need `-theme` class added
- `role-based-color-system.css` is not yet linked in any layout files
- `role-color-palette-fix.css` exists and provides fallback colors
- `role-based-theme-engine.css` provides the base theming infrastructure

### Implementation Strategy
1. Complete the color definitions in `role-based-color-system.css` for all roles
2. Add semantic variable mappings so components can use generic names
3. Update body classes to include theme classes
4. Link the new CSS file in all layouts
5. Test thoroughly for accessibility and visual correctness

### Task Dependencies
- Phase 1 must be completed before Phase 2
- Phase 2 testing validates Phase 1 implementation
- Phase 3 documentation can be done in parallel with Phase 2

### Estimated Time
- Phase 1: 3-4 hours
- Phase 2: 2-3 hours
- Phase 3: 1-2 hours

**Total Estimated Time: 6-9 hours**

### Success Criteria
- All role themes display distinct pastel colors per design
- All text meets WCAG 2.1 Level AA contrast requirements (4.5:1 minimum)
- No text "dissolves" into backgrounds
- Color system works across all modern browsers
- Documentation is complete and accurate
