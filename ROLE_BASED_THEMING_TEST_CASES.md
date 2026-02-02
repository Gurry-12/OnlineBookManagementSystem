# Role-Based Theming System - Test Cases

## 🧪 **Test Suite Overview**

This document contains comprehensive test cases for validating the role-based theming system implementation.

**Test Environment**: `http://localhost:5076`  
**Test Date**: February 2, 2026  
**System**: Online Book Management System

---

## 📋 **Test Case Categories**

### **Category 1: Theme Application Tests**
### **Category 2: Visual Consistency Tests**
### **Category 3: Component Integration Tests**
### **Category 4: Role Transition Tests**
### **Category 5: Architecture Compliance Tests**

---

## 🌐 **CATEGORY 1: THEME APPLICATION TESTS**

### **TC-001: Public Theme Application**
**Objective**: Verify public theme is correctly applied for unauthenticated users

**Pre-conditions**: 
- Application is running at localhost:5076
- User is not logged in

**Test Steps**:
1. Navigate to `http://localhost:5076`
2. Open browser DevTools (F12)
3. Inspect the `<body>` element
4. Check CSS computed styles

**Expected Results**:
- [ ] Body element has class `theme-public`
- [ ] Body element has attribute `data-authenticated="false"`
- [ ] CSS variables show public theme colors:
  - `--color-primary: #0ea5e9` (sky blue)
  - `--color-secondary: #06b6d4` (cyan)
  - `--color-accent: #10b981` (emerald)
- [ ] Background is light/white
- [ ] Text is dark for good contrast

**Test Data**: N/A  
**Priority**: High  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-002: User Theme Application**
**Objective**: Verify user theme is correctly applied for authenticated regular users

**Pre-conditions**: 
- Application is running
- User credentials: `user@gmail.com` / `User123!`

**Test Steps**:
1. Navigate to `http://localhost:5076/Auth/Login`
2. Login with user credentials
3. Verify redirect to user dashboard
4. Open DevTools and inspect `<body>` element
5. Check CSS computed styles

**Expected Results**:
- [ ] Body element has class `theme-user`
- [ ] Body element has attribute `data-authenticated="true"`
- [ ] CSS variables show user theme colors:
  - `--color-primary: #3b82f6` (blue)
  - `--color-secondary: #6366f1` (indigo)
  - `--color-accent: #8b5cf6` (violet)
- [ ] Clean, productive appearance
- [ ] User dashboard loads correctly

**Test Data**: 
- Email: `user@gmail.com`
- Password: `User123!`

**Priority**: High  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-003: Admin Theme Application**
**Objective**: Verify admin theme is correctly applied for admin users

**Pre-conditions**: 
- Application is running
- Admin credentials: `admin@gmail.com` / `Admin123!`

**Test Steps**:
1. Navigate to `http://localhost:5076/Auth/Login`
2. Login with admin credentials
3. Verify redirect to admin dashboard
4. Open DevTools and inspect `<body>` element
5. Check CSS computed styles

**Expected Results**:
- [ ] Body element has class `theme-admin`
- [ ] CSS variables show admin theme colors:
  - `--color-bg: #0f172a` (dark slate)
  - `--color-surface: #1e293b` (slate)
  - `--color-primary: #7c3aed` (violet)
  - `--color-accent: #22c55e` (green)
- [ ] Dark professional appearance
- [ ] Admin dashboard loads correctly
- [ ] Text is light colored for dark background

**Test Data**: 
- Email: `admin@gmail.com`
- Password: `Admin123!`

**Priority**: High  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-004: SuperAdmin Theme Application**
**Objective**: Verify superadmin theme is correctly applied for superadmin users

**Pre-conditions**: 
- Application is running
- SuperAdmin credentials: `superadmin@gmail.com` / `SuperAdmin123!`

**Test Steps**:
1. Navigate to `http://localhost:5076/Auth/Login`
2. Login with superadmin credentials
3. Verify redirect to superadmin dashboard
4. Open DevTools and inspect `<body>` element
5. Check CSS computed styles

**Expected Results**:
- [ ] Body element has class `theme-superadmin`
- [ ] CSS variables show superadmin theme colors:
  - `--color-bg: #000000` (black)
  - `--color-surface: #18181b` (zinc)
  - `--color-primary: #dc2626` (red)
  - `--color-accent: #facc15` (amber)
- [ ] High-contrast appearance
- [ ] SuperAdmin dashboard loads correctly
- [ ] Maximum contrast for critical operations

**Test Data**: 
- Email: `superadmin@gmail.com`
- Password: `SuperAdmin123!`

**Priority**: High  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

## 🎨 **CATEGORY 2: VISUAL CONSISTENCY TESTS**

### **TC-005: Button Component Theming**
**Objective**: Verify buttons adapt to each theme correctly

**Pre-conditions**: Access to all role accounts

**Test Steps**:
1. Test each role (Public, User, Admin, SuperAdmin)
2. Navigate to pages with buttons
3. Inspect button colors and hover states
4. Test primary, secondary, success, danger button variants

**Expected Results**:
- [ ] **Public**: Buttons use sky blue (#0ea5e9) primary color
- [ ] **User**: Buttons use blue (#3b82f6) primary color  
- [ ] **Admin**: Buttons use violet (#7c3aed) primary color on dark background
- [ ] **SuperAdmin**: Buttons use red (#dc2626) primary color on black background
- [ ] Hover states work correctly for each theme
- [ ] Button text remains readable in all themes

**Priority**: High  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-006: Form Component Theming**
**Objective**: Verify form elements adapt to each theme

**Test Steps**:
1. Test login form in each theme context
2. Test book creation form (admin)
3. Test search forms
4. Check input fields, labels, validation messages

**Expected Results**:
- [ ] Input fields have appropriate background colors for each theme
- [ ] Labels are readable in all themes
- [ ] Focus states use theme-appropriate colors
- [ ] Validation messages are visible
- [ ] Form backgrounds adapt to theme

**Priority**: Medium  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-007: Table Component Theming**
**Objective**: Verify tables adapt to each theme

**Test Steps**:
1. View book lists in each role
2. Check admin user management tables
3. View order history tables
4. Test table hover states

**Expected Results**:
- [ ] Table backgrounds appropriate for each theme
- [ ] Row striping visible in all themes
- [ ] Header styling matches theme
- [ ] Hover states work correctly
- [ ] Text remains readable

**Priority**: Medium  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-008: Dashboard Cards Theming**
**Objective**: Verify dashboard statistics cards adapt to themes

**Test Steps**:
1. View user dashboard
2. View admin dashboard  
3. View superadmin dashboard
4. Check stats card colors and variants

**Expected Results**:
- [ ] Cards use theme-appropriate backgrounds
- [ ] Icon colors match theme palette
- [ ] Numbers and text are readable
- [ ] Card hover effects work
- [ ] Success/warning/danger variants visible

**Priority**: Medium  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

## 🔄 **CATEGORY 3: COMPONENT INTEGRATION TESTS**

### **TC-009: Navigation Theming**
**Objective**: Verify navigation elements adapt to themes

**Test Steps**:
1. Check public navigation bar
2. Check user sidebar navigation
3. Check admin sidebar navigation
4. Check superadmin navigation

**Expected Results**:
- [ ] Navigation backgrounds match theme
- [ ] Active/hover states use theme colors
- [ ] Navigation text is readable
- [ ] Icons are visible and themed
- [ ] Dropdowns match theme

**Priority**: High  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-010: Toast Notifications Theming**
**Objective**: Verify toast notifications use theme colors

**Test Steps**:
1. Trigger success notifications in each theme
2. Trigger error notifications
3. Trigger warning notifications
4. Check toast appearance and readability

**Expected Results**:
- [ ] Toast backgrounds use theme tokens
- [ ] Success toasts use `--color-success`
- [ ] Error toasts use `--color-danger`
- [ ] Warning toasts use `--color-warning`
- [ ] Text remains readable in all themes

**Priority**: Low  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

## 🔀 **CATEGORY 4: ROLE TRANSITION TESTS**

### **TC-011: Theme Switching on Login**
**Objective**: Verify theme changes when user logs in

**Test Steps**:
1. Start on public homepage (theme-public)
2. Login as regular user
3. Verify theme changes to theme-user
4. Logout and verify return to theme-public

**Expected Results**:
- [ ] Theme changes immediately upon login
- [ ] No visual glitches during transition
- [ ] All components update to new theme
- [ ] Theme reverts on logout

**Priority**: High  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-012: Cross-Role Theme Consistency**
**Objective**: Verify theme remains consistent across different pages for same role

**Test Steps**:
1. Login as admin user
2. Navigate to: Dashboard → Books → Users → Analytics
3. Verify theme consistency across all pages
4. Repeat for other roles

**Expected Results**:
- [ ] Theme class remains consistent across navigation
- [ ] Colors remain consistent on all pages
- [ ] No theme "bleeding" between roles
- [ ] Layout components maintain theme

**Priority**: Medium  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

## 🏗️ **CATEGORY 5: ARCHITECTURE COMPLIANCE TESTS**

### **TC-013: No Role-Based Selectors**
**Objective**: Verify CSS contains no role-based selectors

**Test Steps**:
1. Search all CSS files for `.admin`, `.user`, `.public`, `.superadmin`
2. Verify only theme classes exist (`.theme-admin`, etc.)
3. Check that components don't reference roles directly

**Expected Results**:
- [ ] No `.admin` selectors found
- [ ] No `.user` selectors found  
- [ ] No `.public` selectors found
- [ ] No `.superadmin` selectors found
- [ ] Only `.theme-*` selectors exist
- [ ] Components use only design tokens

**Priority**: Critical  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-014: Design Token Usage**
**Objective**: Verify components use design tokens, not hardcoded colors

**Test Steps**:
1. Inspect component CSS files
2. Search for hardcoded hex colors (#ffffff, etc.)
3. Verify all colors use var(--color-*) syntax
4. Check that tokens are properly defined

**Expected Results**:
- [ ] No hardcoded colors in component files
- [ ] All colors use `var(--color-*)` syntax
- [ ] Design tokens are properly defined
- [ ] Theme files only override tokens
- [ ] Components remain role-agnostic

**Priority**: Critical  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

### **TC-015: Layout Boundary Theme Application**
**Objective**: Verify themes are applied only at layout boundary

**Test Steps**:
1. Inspect layout files (_LayoutPublic, _LayoutUser, etc.)
2. Verify theme class is applied to `<body>` element
3. Check that individual views don't set theme classes
4. Verify ViewData["ThemeClass"] usage

**Expected Results**:
- [ ] Theme class applied only in layout files
- [ ] Body element gets theme class from ViewData
- [ ] Individual views don't set theme classes
- [ ] LayoutService determines theme correctly
- [ ] No theme logic in Razor views

**Priority**: Critical  
**Status**: [ ] Pass [ ] Fail [ ] Not Tested

---

## 📊 **TEST EXECUTION SUMMARY**

### **Test Results Overview**
- **Total Test Cases**: 15
- **Passed**: ___
- **Failed**: ___
- **Not Tested**: ___
- **Blocked**: ___

### **Critical Issues Found**
1. _[List any critical issues]_
2. _[List any critical issues]_

### **Recommendations**
1. _[List recommendations]_
2. _[List recommendations]_

### **Sign-off**
- **Tester**: ________________
- **Date**: ________________
- **Status**: [ ] Approved [ ] Needs Fixes [ ] Rejected

---

## 🚀 **Quick Test Execution Guide**

### **5-Minute Smoke Test**
1. **TC-001**: Check public theme on homepage
2. **TC-002**: Login as user, verify theme change
3. **TC-003**: Login as admin, verify dark theme
4. **TC-013**: Quick CSS scan for role selectors
5. **TC-015**: Verify body element theme classes

### **Complete Test Execution**
Execute all 15 test cases in order, documenting results for each.

---

**End of Test Cases Document**