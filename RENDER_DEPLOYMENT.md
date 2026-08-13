# Nursery Management System - Render Deployment Guide

This guide explains how to deploy the Nursery Management System backend and PostgreSQL database to Render.com.

## Prerequisites

- Render.com account (free tier available)
- GitHub repository (https://github.com/kiro314159265359/NurseryManagementSystem)
- The code must be pushed to the GitHub repository

## Project Structure

The project consists of:
- **Backend API**: ASP.NET Core Web API with Clean Architecture
- **Database**: PostgreSQL 18
- **Configuration**: Render-native deployment using `render.yaml`

## Deployment Steps

### 1. Prepare Your GitHub Repository

Ensure your code is pushed to the GitHub repository:
```bash
git add .
git commit -m "Add Render deployment configuration"
git push origin main
```

### 2. Create Render Account

1. Go to [render.com](https://render.com)
2. Sign up/login with your GitHub account
3. Authorize Render to access your repository

### 3. Deploy to Render

#### Option A: Automatic Deployment via render.yaml (Recommended)

1. In Render dashboard, click "New +"
2. Select "Web Service"
3. Connect your GitHub repository: `kiro314159265359/NurseryManagementSystem`
4. Render will automatically detect the `render.yaml` file
5. Click "Deploy Web Service"

This will automatically create:
- PostgreSQL database service
- Web API service
- Environment variables and connections

#### Option B: Manual Deployment

If automatic deployment doesn't work, follow these steps:

##### Step 1: Deploy PostgreSQL Database

1. In Render dashboard, click "New +"
2. Select "PostgreSQL"
3. Configure:
   - **Name**: `nursery-management-db`
   - **Database**: `nursery_management`
   - **User**: `postgres`
   - **Region**: Oregon (or nearest to you)
   - **Plan**: Free
4. Click "Create Database"

Save the **Internal Database URL** from the database dashboard.

##### Step 2: Deploy Web API

1. In Render dashboard, click "New +"
2. Select "Web Service"
3. Connect your GitHub repository
4. Configure:
   - **Name**: `nursery-management-api`
   - **Region**: Same as database
   - **Branch**: `main`
   - **Runtime**: Docker
   - **Context**: `.`
   - **Dockerfile**: `./Dockerfile`
   - **Plan**: Free
5. Add Environment Variables:
   ```
   ASPNETCORE_ENVIRONMENT = Production
   ASPNETCORE_URLS = http://0.0.0.0:8080
   ConnectionStrings__DefaultConnection = <Your Internal Database URL>
   Jwt__Issuer = NurseryManagementSystem
   Jwt__Audience = NurseryManagementSystem
   Jwt__SecretKey = <Generate a long random secret key>
   Jwt__AccessTokenExpirationMinutes = 60
   Jwt__RefreshTokenExpirationDays = 7
   ```
6. Click "Deploy Web Service"

### 4. Verify Deployment

1. Check the Render dashboard for deployment status
2. Once deployed, you'll get a URL like: `https://nursery-management-api.onrender.com`
3. Test the health endpoint: `https://nursery-management-api.onrender.com/health`
4. Expected response:
   ```json
   {
     "status": "healthy",
     "timestamp": "2024-08-13T12:00:00Z"
   }
   ```

## Configuration Files Explained

### render.yaml
This file defines the entire deployment infrastructure:
- **PostgreSQL Service**: Database configuration
- **Web Service**: API configuration with automatic database connection
- **Environment Variables**: Application settings

### Dockerfile
Multi-stage Docker build:
- **Build Stage**: Compiles the .NET application
- **Publish Stage**: Creates optimized production build
- **Runtime Stage**: Lightweight ASP.NET runtime

### Program.cs Updates
- Added health check endpoint for Render monitoring
- Added CORS support for frontend integration
- Configured to listen on port 8080 (Render standard)

## Environment Variables

### Required Variables
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Jwt__SecretKey`: JWT signing key (minimum 32 characters)

### Optional Variables
- `Jwt__Issuer`: JWT issuer (default: NurseryManagementSystem)
- `Jwt__Audience`: JWT audience (default: NurseryManagementSystem)
- `Jwt__AccessTokenExpirationMinutes`: Token expiration (default: 60)
- `Jwt__RefreshTokenExpirationDays`: Refresh token expiration (default: 7)

## Accessing the API

### Base URL
After deployment, your API will be available at:
```
https://nursery-management-api.onrender.com
```

### Authentication
The API automatically seeds a default super admin user:
- **Username**: `superadmin`
- **Password**: `Admin@12345` (change this immediately after first login)

### Testing Endpoints

#### 1. Login
```bash
POST https://nursery-management-api.onrender.com/api/auth/login
Content-Type: application/json

{
  "userName": "superadmin",
  "password": "Admin@12345"
}
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "..."
}
```

#### 2. Use Token for Authenticated Requests
```bash
GET https://nursery-management-api.onrender.com/api/users
Authorization: Bearer <accessToken>
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/revoke` - Revoke refresh token

### Users (SuperAdmin only)
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `POST /api/users/{id}/assign-role` - Assign role
- `POST /api/users/{id}/change-password` - Change password
- `POST /api/users/{id}/set-active` - Set user active status

### Children
- `GET /api/children` - List all children
- `GET /api/children/{id}` - Get child by ID
- `POST /api/children` - Create child
- `PUT /api/children/{id}` - Update child
- `POST /api/children/{id}/set-active` - Set child active status
- `POST /api/children/{id}/emergency-contacts` - Add emergency contact
- `DELETE /api/children/{id}/emergency-contacts/{contactId}` - Remove emergency contact

### Plans
- `GET /api/plans` - List all plans
- `GET /api/plans/{id}` - Get plan by ID
- `POST /api/plans` - Create plan (SuperAdmin only)
- `PUT /api/plans/{id}` - Update plan (SuperAdmin only)
- `DELETE /api/plans/{id}` - Delete plan (SuperAdmin only)

### Plan Assignments
- `GET /api/planassignments/child/{childId}` - Get child's plan assignments
- `POST /api/planassignments` - Assign plan to child
- `POST /api/planassignments/end` - End plan assignment

### Attendance
- `POST /api/attendance/child/check-in` - Child check-in
- `POST /api/attendance/child/check-out` - Child check-out
- `POST /api/attendance/staff/check-in` - Staff check-in
- `POST /api/attendance/staff/check-out` - Staff check-out
- `GET /api/attendance/child/{childId}` - Get child attendance history
- `GET /api/attendance/staff/{staffId}` - Get staff attendance history

### Schedule
- `GET /api/schedule` - Get schedule
- `POST /api/schedule` - Create schedule slot
- `PUT /api/schedule/{id}` - Update schedule slot
- `DELETE /api/schedule/{id}` - Delete schedule slot

### Billing
- `GET /api/billing` - List invoices
- `GET /api/billing/{id}` - Get invoice by ID
- `POST /api/billing/generate-monthly` - Generate monthly invoices
- `POST /api/billing/{id}/mark-paid` - Mark invoice as paid
- `POST /api/billing/{id}/cancel` - Cancel invoice

### Session Logs (SuperAdmin only)
- `GET /api/sessionlogs` - Get session logs
- `POST /api/sessionlogs` - Create session log

## Troubleshooting

### Deployment Fails
- Check Render logs for specific error messages
- Ensure Dockerfile is in the root directory
- Verify GitHub repository has the latest code

### Database Connection Issues
- Verify `ConnectionStrings__DefaultConnection` is set correctly
- Check that database service is running
- Ensure both services are in the same region

### JWT Authentication Issues
- Verify `Jwt__SecretKey` is set and at least 32 characters
- Check that JWT settings are configured correctly
- Ensure token is being sent in the Authorization header

### CORS Issues
- CORS is configured to allow all origins in the current setup
- For production, you may want to restrict to specific frontend domains

## Monitoring and Logs

- Access logs in Render dashboard
- Monitor database performance
- Check API response times
- Set up alerts for errors

## Cost

- **Free Tier**: Both database and web service can run on Render's free tier
- **Limitations**: Free tier spins down after 15 minutes of inactivity (cold starts)
- **Production**: Consider upgrading to paid tiers for better performance

## Security Recommendations

1. **Change Default Password**: Change the super admin password immediately
2. **Environment Variables**: Never commit secrets to GitHub
3. **JWT Secret**: Use a strong, randomly generated secret key
4. **Database**: Use strong database passwords
5. **HTTPS**: Render automatically provides SSL certificates
6. **CORS**: Restrict CORS to specific frontend domains in production

## Scaling

### Horizontal Scaling
- Render supports horizontal scaling for web services
- Configure scaling based on CPU/memory usage
- Consider load balancing for high traffic

### Database Scaling
- Upgrade to paid PostgreSQL plans for better performance
- Consider read replicas for read-heavy workloads
- Implement connection pooling

## Next Steps

1. Deploy your frontend application
2. Update frontend API base URL to the Render deployment URL
3. Configure CORS to allow your frontend domain
4. Set up monitoring and alerting
5. Implement CI/CD pipeline
6. Configure backup strategy for database

## Support

For issues specific to:
- **Render**: Check [Render Documentation](https://render.com/docs)
- **ASP.NET Core**: Check [Microsoft Documentation](https://docs.microsoft.com/aspnet/core)
- **This Project**: Check the GitHub repository issues

## Notes

- The API automatically applies database migrations on startup
- The API automatically seeds initial data (roles, super admin user)
- Health check endpoint is available at `/health`
- OpenAPI documentation is available in development mode only
- The API uses Clean Architecture with CQRS pattern
