# API Documentation - Whispering Pages

## Overview
This document outlines the RESTful API endpoints available in the Online Book Management System. All API responses are returned in JSON format.

## Authentication
Most endpoints require authentication.
- **Header:** `Authorization: Bearer <token>`
- **Login Endpoint:** `/Auth/Login`

## Books API
### Get All Books
- **Endpoint:** `GET /User/UserBookList`
- **Description:** Returns a list of all available books with filtering options.
- **Parameters:** `search`, `categoryId`, `sortBy`.

### Get Book Details
- **Endpoint:** `GET /User/BookDetails/{id}`
- **Description:** Returns full details for a specific book.

### Get New Arrivals
- **Endpoint:** `GET /User/GetNewArrivals`
- **Description:** Returns the most recently added books.

## Orders API
### Place Order
- **Endpoint:** `POST /User/Checkout`
- **Description:** Creates a new order from the current shopping cart.

### Get Order History
- **Endpoint:** `GET /User/OrderHistory`
- **Description:** Returns a list of orders for the authenticated user.

## User Management (Admin Only)
### List Users
- **Endpoint:** `GET /SuperAdmin/UserManagement`
- **Description:** Returns all registered users (Super Admin only).

### Approve User
- **Endpoint:** `POST /SuperAdmin/ApproveUser/{id}`
- **Description:** Approves a pending account request.

## Search API
### Global Search
- **Endpoint:** `GET /Public/Search`
- **Description:** Publicly accessible search for books.

## Error Handling
The API uses standard HTTP status codes:
- `200 OK`: Success.
- `400 Bad Request`: Validation error.
- `401 Unauthorized`: Authentication required.
- `403 Forbidden`: Insufficient permissions.
- `404 Not Found`: Resource does not exist.
- `500 Internal Server Error`: Unexpected server error.
