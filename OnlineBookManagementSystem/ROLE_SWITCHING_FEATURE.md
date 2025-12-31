# SuperAdmin Role Switching Feature

## Overview
The SuperAdmin Role Switching feature allows SuperAdmins to switch between different role views (Admin, User, Public) to test and experience the application from different user perspectives without logging out and logging in as different users.

## Features

### 1. Multiple Roles for SuperAdmin
- SuperAdmin users are automatically assigned all roles: SuperAdmin, Admin, User, and Public
- This allows them to access all functionality across the application
- The primary role remains SuperAdmin for authorization purposes

### 2. Role Switching Dropdown
- Located in the SuperAdmin dashboard top bar
- Provides options to switch to:
  - Admin View
  - User View  
  - Public View
- Includes visual indicators and icons for each role

### 3. Session-Based Role Management
- Uses session storage to track the current view role
- Maintains the original SuperAdmin role for security
- Middleware handles role switching logic automatically

### 4. Return to SuperAdmin
- Prominent "Return to SuperAdmin" button appears in all other role views
- Animated alert with visual feedback
- One-click return to SuperAdmin dashboard

### 5. Security Features
- Only SuperAdmins can access role switching functionality
- Regular users cannot switch roles or access this feature
- Session data is cleared for non-SuperAdmin users
- All role switches are logged for audit purposes

## Technical Implementation

### Database Seeding
- **Roles Created**: SuperAdmin, Admin, User, Guest, Public
- **SuperAdmin User**: Gets all roles assigned automatically
- **Other Users**: Get single role assignments as before
- **Public User**: New default public user for testing

### Authorization Policies
- `SuperAdminOnly`: SuperAdmin role only
- `AdminOrHigher`: SuperAdmin, Admin roles
- `UserOrHigher`: SuperAdmin, Admin, User roles
- `PublicOrHigher`: SuperAdmin, Admin, User, Public roles
- `AuthenticatedUsers`: All roles including Guest

### Middleware
- `RoleSwitchingMiddleware`: Handles role switching logic
- Adds ViewRole claim for switched roles
- Clears unauthorized role switching attempts

### Session Management
- `OriginalRole`: Stores the user's actual role (SuperAdmin)
- `CurrentViewRole`: Stores the currently active view role
- Automatic cleanup for security

## User Experience

### SuperAdmin Dashboard
1. SuperAdmin logs in normally
2. Sees role switcher dropdown in top bar
3. Can select any role view from dropdown
4. Confirmation dialog appears before switching

### Switched Role Views
1. User is redirected to appropriate dashboard/page
2. Animated return button appears prominently
3. All functionality works as if user has that role
4. Can return to SuperAdmin at any time

### Visual Feedback
- Role switcher has gradient styling and hover effects
- Return button has pulsing animation to draw attention
- Toast notifications for successful role switches
- Clear visual indicators of current view mode

## Configuration

### appsettings.json
```json
{
  "SuperAdmin": {
    "Email": "superadmin@gmail.com",
    "Password": "SuperP@ssw0rd123!"
  },
  "Public": {
    "Email": "public@whisperingpages.com", 
    "Password": "Public123!"
  }
}
```

### CSS Styling
- Custom CSS classes for role switcher styling
- Animated return button with pulse effect
- Responsive design for all screen sizes

## Usage Instructions

### For SuperAdmins
1. Log in with SuperAdmin credentials
2. Navigate to SuperAdmin dashboard
3. Click "Switch View" dropdown in top bar
4. Select desired role view
5. Confirm the switch in dialog
6. Experience the application as that role
7. Click "Return to SuperAdmin" when done

### For Developers
1. Role switching is automatic for SuperAdmins
2. No additional configuration needed
3. All existing authorization continues to work
4. Audit logs capture all role switches

## Benefits

1. **Testing**: Easy testing of different user experiences
2. **Support**: Better understanding of user issues
3. **Development**: Faster development and debugging
4. **Training**: Training staff on different role capabilities
5. **Audit**: Complete audit trail of administrative actions

## Security Considerations

- Only SuperAdmins can switch roles
- Original role is preserved in session
- All switches are logged for audit
- Session data is secured and validated
- Automatic cleanup prevents unauthorized access

This feature enhances the SuperAdmin experience while maintaining security and providing comprehensive testing capabilities across all user roles.