# SNIC API - Role-Based Authentication System

## Overview
This API implements a complete JWT-based authentication system with role-based authorization supporting Admin and Customer roles.

## Features

### 🔐 Authentication
- User registration with email, username, and password
- Secure login with JWT token generation
- Password hashing using BCrypt
- Token blacklist system for secure logout
- Token expiration (24 hours)

### 👥 Role-Based Authorization
- **Customer Role (0)**: Default role for regular users
- **Admin Role (1)**: Elevated privileges for administrative users

### 🛡️ Security Features
- JWT tokens with unique identifiers (jti claim)
- Token blacklist middleware
- Automatic cleanup of expired blacklisted tokens
- Role-based endpoint protection

## API Endpoints

### Authentication Endpoints

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "username",
  "password": "password123",
  "role": 0  // 0 = Customer, 1 = Admin
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "username",
  "password": "password123"
}
```

#### Logout (Requires Authentication)
```http
POST /api/auth/logout
Authorization: Bearer YOUR_JWT_TOKEN
```

#### Get User Profile (Requires Authentication)
```http
GET /api/auth/profile
Authorization: Bearer YOUR_JWT_TOKEN
```

#### Check Token Status (Requires Authentication)
```http
GET /api/auth/token-status
Authorization: Bearer YOUR_JWT_TOKEN
```

### Protected Endpoints

#### General Protected Data (Any Authenticated User)
```http
GET /api/protected/data
Authorization: Bearer YOUR_JWT_TOKEN
```

#### Admin-Only Data (Admin Role Required)
```http
GET /api/protected/admin
Authorization: Bearer ADMIN_JWT_TOKEN
```

#### Customer-Only Data (Customer Role Required)
```http
GET /api/protected/customer
Authorization: Bearer CUSTOMER_JWT_TOKEN
```

## Database Models

### User Model
```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

### UserRole Enum
```csharp
public enum UserRole
{
    Customer = 0,
    Admin = 1
}
```

### BlacklistedToken Model
```csharp
public class BlacklistedToken
{
    public int Id { get; set; }
    public string TokenId { get; set; }
    public string Token { get; set; }
    public int UserId { get; set; }
    public DateTime BlacklistedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Reason { get; set; }
}
```

## JWT Token Claims

Each JWT token includes the following claims:
- `NameIdentifier`: User ID
- `Name`: Username
- `Email`: User email
- `Role`: User role (Admin/Customer)
- `jti`: Unique token identifier
- `iat`: Issued at timestamp

## Setup Instructions

### 1. Database Setup
```bash
# Apply migrations to create database tables
dotnet ef database update
```

### 2. Configuration
Update `appsettings.json` with your JWT settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your_Connection_String_Here"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyHereThatIsAtLeast32CharactersLong",
    "Issuer": "SnicApi",
    "Audience": "SnicApiUsers"
  }
}
```

### 3. Run the Application
```bash
dotnet run
```

## Testing the API

### Using the HTTP Test File
Use the provided `api-test-examples.http` file to test all endpoints:

1. Register users with different roles
2. Login to get JWT tokens
3. Test role-based access to protected endpoints
4. Test logout functionality

### Expected Responses

#### Successful Registration/Login
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "username": "testuser",
    "email": "test@example.com",
    "role": "Customer",
    "expiresAt": "2024-01-01T12:00:00Z"
  }
}
```

#### Role-Based Access Denied
```json
{
  "success": false,
  "message": "Access denied"
}
```
Status Code: 403 Forbidden

#### Token Blacklisted
```json
{
  "success": false,
  "message": "Token has been invalidated"
}
```
Status Code: 401 Unauthorized

## Architecture Components

### Services
- **JwtService**: Handles JWT token generation and validation
- **TokenBlacklistService**: Manages token blacklist operations
- **TokenCleanupService**: Background service for cleaning expired tokens

### Middleware
- **JwtBlacklistMiddleware**: Checks for blacklisted tokens on each request

### Controllers
- **AuthController**: Handles authentication operations
- **ProtectedController**: Demonstrates role-based authorization

## Security Considerations

1. **Password Security**: Passwords are hashed using BCrypt
2. **Token Security**: JWT tokens include unique identifiers for tracking
3. **Logout Security**: Tokens are properly invalidated using blacklist
4. **Role Security**: Endpoints are protected with role-based authorization
5. **Automatic Cleanup**: Expired blacklisted tokens are automatically removed

## Development Notes

- Default role is Customer (0) for new registrations
- Admin role (1) must be explicitly assigned during registration
- Tokens expire after 24 hours
- Background service cleans up expired tokens every hour
- All authentication endpoints return consistent API response format

## Error Handling

The API returns structured error responses:
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

Common HTTP status codes:
- 200: Success
- 400: Bad Request (validation errors)
- 401: Unauthorized (invalid/expired token)
- 403: Forbidden (insufficient permissions)
- 500: Internal Server Error
