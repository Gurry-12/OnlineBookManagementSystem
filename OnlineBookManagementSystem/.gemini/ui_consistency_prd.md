# Product Requirements Document: UI Consistency & Aesthetic Standardization

**Project:** Whispering Pages (Online Book Management System)
**Document Status:** Approved & Active
**Date:** February 22, 2026

---

## 1. Executive Summary
The Whispering Pages application currently suffers from significant User Interface (UI) inconsistencies. While a "pastel aesthetic" and custom design system (e.g., `wp-card`, `wp-btn`) have been introduced and applied to segments of the User Module, the vast majority of the admin portals (Admin & SuperAdmin) and public views still rely on uncustomized, default Bootstrap 5 classes (e.g., `.card`, `.btn-primary`). This creates a disjointed user experience and dilutes the premium, cozy, and vibrant aesthetic goal of the platform. The objective of this PRD is to outline the exact inconsistencies and provide a concrete execution plan to standardize the entire UI.

## 2. Goals & Objectives
*   **Aesthetic Cohesion:** Ensure every module (Public, User, Admin, SuperAdmin) reflects the vibrant, pastel, minimalist, and cozy design language.
*   **Component Standardization:** Fully replace default Bootstrap components (`.card`, `.btn`, badges) with our custom design system components (`wp-card`, `wp-btn`, `wp-badge`).
*   **Maintainability:** Centralize and reuse Razor partials (like `_BookCardPartial`) rather than duplicating markup across different modules.

## 3. Current Inconsistencies Identified
1.  **Card Components:** 
    *   **User Module** leverages the `.wp-card` component for lists and cards.
    *   **Admin/SuperAdmin Modules** heavily use standard `.card`, `.card-header`, and `.card-body` with manual border/shadow overrides instead of using the custom design system.
2.  **Button Components:**
    *   **User Module** relies on `.wp-btn` components.
    *   **Admin/SuperAdmin Modules** rely on standard `.btn btn-primary`, `.btn-outline-primary`, causing visual discrepancies.
3.  **Utility Classes (Text & Colors):**
    *   The platform mixes custom semantic colors (`user-text-primary`, `user-text-muted`) with standard Bootstrap utilities (`text-primary`, `bg-warning`).
4.  **Layout Wrappers:**
    *   Inconsistent usage of layout wrappers like `.view-[role]-dashboard` and grid structures (`.dashboard-grid`).

## 4. Implementation Roadmap

### Phase 1: Dashboard Standardization (Admin & SuperAdmin)
*   **Target:** `Presentation/Views/Admin/Dashboard.cshtml` and `Presentation/Views/SuperAdmin/Dashboard.cshtml`.
*   **Actions:**
    *   Replace all instances of `.card` with `.wp-card`.
    *   Replace `.card-header` with `.wp-card-header` and `.card-body` with `.wp-card-body`.
    *   Update buttons from `.btn` wrapper formats to the standard `.wp-btn` counterparts.
    *   Normalize dashboard grid wrapper classes.

### Phase 2: Form & Table Interfaces (Management Views)
*   **Target:** `Presentation/Views/Users/`, `Presentation/Views/Books/`, `Presentation/Views/Orders/`, `Presentation/Views/Logs/`.
*   **Actions:**
    *   Update wrapping containers for DataTables/Lists to use `wp-card`.
    *   Update action buttons (e.g., Edit, Delete, View) in tables to use `wp-btn wp-btn-sm`.
    *   Standardize alert and badge elements to `wp-badge`.

### Phase 3: Public & Shared Views
*   **Target:** `Presentation/Views/Public/`, `Presentation/Views/Auth/`, `Presentation/Views/Cart/`.
*   **Actions:**
    *   Ensure login/registration cards use `wp-card`.
    *   Standardize the Book Detail cards and public lists.
    *   Integrate `_BookCardPartial` where standalone book UI elements are repeated.

## 5. Success Metrics
*   **Component Usage:** 100% replacement of raw `.card` usage across the `Presentation/Views` directory with `.wp-card`.
*   **Visual Regression:** The application feels like a single, unified platform regardless of the user role logged in.
*   **Code Reduction:** Reduction in duplicated CSS classes and manual inline style overrides in Razor views.

---
*End of PRD*
