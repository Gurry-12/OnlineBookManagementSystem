-- Fix for RefreshToken concurrency issue
-- This script removes manually inserted refresh tokens that are causing concurrency conflicts

-- Delete the manually inserted refresh token that's causing the issue
DELETE FROM RefreshTokens WHERE Id = 4;

-- Optional: Clean up all existing refresh tokens to start fresh
-- DELETE FROM RefreshTokens;

-- Note: After running this, the application will generate new refresh tokens properly
-- without concurrency conflicts.