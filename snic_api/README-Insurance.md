# SNIC API - Medical Insurance Management System

## Overview
This system extends the authentication API with comprehensive medical insurance management capabilities, including products, features, and policies with full CRUD operations and role-based access control.

## Database Models & Relationships

### 🏥 Product (Medical Insurance Product/Service)
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }           // Product name
    public decimal Price { get; set; }         // Base price
    public bool IsActive { get; set; }         // Active status
    public int CreatedByUserId { get; set; }   // Creator (FK to User)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public User CreatedByUser { get; set; }                // Creator user
    public ICollection<Feature> Features { get; set; }     // Product features
    public ICollection<Policy> Policies { get; set; }      // Associated policies
}
```

### ✨ Feature (Product Features)
```csharp
public class Feature
{
    public int Id { get; set; }
    public string Title { get; set; }          // Feature title
    public string Detail { get; set; }         // Feature description
    public int ProductId { get; set; }         // Parent product (FK)
    public DateTime CreatedAt { get; set; }
    
    // Navigation Property
    public Product Product { get; set; }       // Parent product
}
```

### 📋 Policy (Insurance Policy)
```csharp
public class Policy
{
    public int Id { get; set; }
    public string PolicyNumber { get; set; }   // Unique policy number
    public string HolderName { get; set; }     // Policy holder name
    public DateTime StartDate { get; set; }    // Policy start date
    public DateTime EndDate { get; set; }      // Policy end date
    public decimal Premium { get; set; }       // Policy premium
    public int ProductId { get; set; }         // Associated product (FK)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Property
    public Product Product { get; set; }       // Associated product
}
```

## Relationships Configuration

- **User → Products**: One-to-Many (User can create multiple products)
- **Product → Features**: One-to-Many with Cascade Delete (Delete product deletes features)
- **Product → Policies**: One-to-Many with Restrict Delete (Cannot delete product with policies)

## API Endpoints

### 🏥 Products Controller (`/api/products`)

#### Get All Products
```http
GET /api/products
Authorization: Bearer JWT_TOKEN
```
**Response**: List of products with features and creator information

#### Get Product by ID
```http
GET /api/products/{id}
Authorization: Bearer JWT_TOKEN
```
**Response**: Single product with features

#### Create Product with Features
```http
POST /api/products
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "name": "Premium Health Insurance",
  "price": 1500.00,
  "isActive": true,
  "createdByUserId": 1,
  "features": [
    {
      "title": "Comprehensive Coverage",
      "detail": "Covers hospitalization, surgery, and emergency care"
    },
    {
      "title": "Prescription Drugs",
      "detail": "80% coverage for prescription medications"
    }
  ]
}
```

#### Update Product
```http
PUT /api/products/{id}
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "name": "Updated Product Name",
  "price": 1600.00,
  "isActive": true
}
```

#### Delete Product (Admin Only)
```http
DELETE /api/products/{id}
Authorization: Bearer ADMIN_JWT_TOKEN
```
**Note**: Cannot delete products that have associated policies

### 📋 Policies Controller (`/api/policies`)

#### Get All Policies
```http
GET /api/policies
Authorization: Bearer JWT_TOKEN
```
**Response**: List of policies with product information

#### Get Policy by ID
```http
GET /api/policies/{id}
Authorization: Bearer JWT_TOKEN
```

#### Get Policies by Product
```http
GET /api/policies/by-product/{productId}
Authorization: Bearer JWT_TOKEN
```

#### Get Active Policies
```http
GET /api/policies/active
Authorization: Bearer JWT_TOKEN
```
**Response**: Policies that are currently active (current date between start and end dates)

#### Create Policy
```http
POST /api/policies
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "policyNumber": "POL-2024-001",
  "holderName": "John Smith",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "premium": 1500.00,
  "productId": 1
}
```

#### Update Policy
```http
PUT /api/policies/{id}
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "holderName": "John Smith Jr.",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "premium": 1550.00,
  "productId": 1
}
```

#### Delete Policy (Admin Only)
```http
DELETE /api/policies/{id}
Authorization: Bearer ADMIN_JWT_TOKEN
```

## Authorization & Security

### Authentication Requirements
- All endpoints require JWT authentication
- Use `Authorization: Bearer JWT_TOKEN` header

### Role-Based Access Control
- **Any Authenticated User**: Can view and create products/policies
- **Admin Only**: Can delete products and policies
- **Product Deletion**: Blocked if product has associated policies

### Data Validation
- Product prices must be > 0
- Policy end date must be after start date
- Policy numbers must be unique
- Required fields validation on all models

## Database Setup

### Apply Migrations
```bash
# Apply all migrations including the new insurance models
dotnet ef database update
```

### Migration History
1. `InitialCreate` - User authentication system
2. `AddBlacklistedTokens` - Token blacklist for logout
3. `AddUserRoles` - Role-based authorization
4. `AddInsuranceModels` - Products, Features, and Policies

## Response Format

All endpoints return standardized API responses:

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

## Sample Data Flow

### 1. Create a Product
```json
POST /api/products
{
  "name": "Family Health Plan",
  "price": 2000.00,
  "isActive": true,
  "createdByUserId": 1,
  "features": [
    {
      "title": "Family Coverage",
      "detail": "Covers spouse and up to 4 children"
    }
  ]
}
```

### 2. Create Policies for the Product
```json
POST /api/policies
{
  "policyNumber": "FAM-001",
  "holderName": "Johnson Family",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "premium": 2000.00,
  "productId": 1
}
```

### 3. View Product with Policies
```http
GET /api/products/1  // Shows product with features
GET /api/policies/by-product/1  // Shows all policies for this product
```

## Business Rules

1. **Product Management**
   - Products can have multiple features
   - Features are automatically deleted when product is deleted
   - Products cannot be deleted if they have associated policies

2. **Policy Management**
   - Each policy must be linked to an existing product
   - Policy numbers must be unique across the system
   - Policy end date must be after start date

3. **User Permissions**
   - Any authenticated user can create products and policies
   - Only admins can delete products and policies
   - Users can view all products and policies (business requirement)

## Testing

Use the provided `insurance-api-examples.http` file for comprehensive testing:

1. **CRUD Operations**: Test all create, read, update, delete operations
2. **Relationship Testing**: Create products with features, then policies
3. **Validation Testing**: Test invalid data scenarios
4. **Authorization Testing**: Test role-based access control
5. **Error Handling**: Test various error scenarios

## Performance Considerations

- Database indexes on frequently queried fields:
  - `Product.Name`
  - `Policy.PolicyNumber` (unique)
- Proper use of `Include()` for related data loading
- Pagination can be added for large datasets if needed

## Future Enhancements

Potential future features:
- Claims management system
- Payment tracking
- Policy renewal automation
- Reporting and analytics
- Document management
- Customer portal integration
