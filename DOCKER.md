# MyFit - Docker Setup Guide

## 🐳 Prerequisites
- Docker Desktop for Windows
- Docker Compose (included with Docker Desktop)
- At least 4GB RAM allocated to Docker

## 🚀 Quick Start with Docker

### Option 1: One Command Start (Recommended)
```powershell
# From the project root directory
cd c:\Users\hp\Desktop\myfitness
docker-compose up --build
```

That's it! The entire stack (PostgreSQL, API, and Blazor Client) will start automatically.

### Option 2: Background Mode
```powershell
# Start all services in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f api
docker-compose logs -f client
docker-compose logs -f postgres
```

## 🌐 Access the Application

Once all services are running:

- **Blazor Client (Frontend)**: http://localhost:5001
- **API (Backend)**: http://localhost:7001
- **Swagger UI**: http://localhost:7001/swagger
- **PostgreSQL**: localhost:5432

### Test the Application
1. Navigate to http://localhost:5001
2. Click "Sign Up" to create a new account
3. Complete the onboarding wizard
4. View your dashboard with nutrition charts!

## 📦 Docker Services

### Service Overview
```yaml
myfit-postgres:  # PostgreSQL 16 database
  - Port: 5432
  - Database: myfit_db
  - User: myfit_user
  - Password: MyFit@2025!

myfit-api:       # ASP.NET Core Web API
  - Port: 7001 (maps to internal 80)
  - Auto-applies migrations on startup
  - Seeds 20 exercises automatically

myfit-client:    # Blazor WASM with Nginx
  - Port: 5001 (maps to internal 80)
  - Optimized static file serving
  - Gzip compression enabled
```

## 🔧 Docker Commands

### Start Services
```powershell
# Build and start
docker-compose up --build

# Start without rebuilding
docker-compose up

# Start in background
docker-compose up -d
```

### Stop Services
```powershell
# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ deletes database data)
docker-compose down -v
```

### View Logs
```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f postgres
docker-compose logs -f client

# Last 100 lines
docker-compose logs --tail=100 api
```

### Rebuild After Code Changes
```powershell
# Rebuild specific service
docker-compose up -d --build api
docker-compose up -d --build client

# Rebuild all services
docker-compose up -d --build
```

### Database Management
```powershell
# Connect to PostgreSQL container
docker exec -it myfit-postgres psql -U myfit_user -d myfit_db

# Run SQL commands
docker exec -it myfit-postgres psql -U myfit_user -d myfit_db -c "SELECT COUNT(*) FROM \"Exercises\";"

# Backup database
docker exec myfit-postgres pg_dump -U myfit_user myfit_db > backup.sql

# Restore database
docker exec -i myfit-postgres psql -U myfit_user -d myfit_db < backup.sql
```

### Container Management
```powershell
# List running containers
docker ps

# List all containers (including stopped)
docker ps -a

# Stop a specific container
docker stop myfit-api

# Remove a container
docker rm myfit-api

# View container resource usage
docker stats

# Execute commands in container
docker exec -it myfit-api bash
```

### Clean Up
```powershell
# Remove all stopped containers
docker container prune

# Remove all unused images
docker image prune -a

# Remove all unused volumes
docker volume prune

# Complete cleanup (⚠️ removes everything)
docker system prune -a --volumes
```

## 🔍 Troubleshooting

### Port Already in Use
```powershell
# Check what's using port 5432 (PostgreSQL)
netstat -ano | findstr :5432

# Check what's using port 7001 (API)
netstat -ano | findstr :7001

# Kill process by PID (from above command)
taskkill /PID <PID> /F

# Or change ports in docker-compose.yml
```

### Database Connection Issues
```powershell
# Check if PostgreSQL is healthy
docker-compose ps

# View PostgreSQL logs
docker-compose logs postgres

# Restart PostgreSQL
docker-compose restart postgres
```

### API Won't Start
```powershell
# Check API logs
docker-compose logs api

# Rebuild API
docker-compose up -d --build api

# Check if migrations ran
docker exec -it myfit-postgres psql -U myfit_user -d myfit_db -c "\dt"
```

### Client Can't Connect to API
The client uses `http://localhost:7001` for the API. If running in Docker, ensure:
```powershell
# Test API from host
curl http://localhost:7001/api/workouts/exercises

# Check API CORS settings in logs
docker-compose logs api | findstr CORS
```

### Rebuild Everything from Scratch
```powershell
# Stop and remove everything
docker-compose down -v

# Remove built images
docker rmi myfit-api myfit-client

# Rebuild and start
docker-compose up --build
```

## 🔐 Environment Variables

### Default Credentials
- **Database User**: myfit_user
- **Database Password**: MyFit@2025!
- **Database Name**: myfit_db

### Change Credentials
Edit `docker-compose.yml`:
```yaml
environment:
  POSTGRES_USER: your_user
  POSTGRES_PASSWORD: your_secure_password
  POSTGRES_DB: your_db_name
```

Also update the API connection string:
```yaml
ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=your_db_name;Username=your_user;Password=your_secure_password"
```

## 📊 Database Seeding

The API automatically:
1. Creates the database schema (migrations)
2. Seeds 20 exercises from `exercises.json`
3. Ready to use immediately

Verify seeding:
```powershell
docker exec -it myfit-postgres psql -U myfit_user -d myfit_db -c "SELECT name FROM \"Exercises\" LIMIT 5;"
```

## 🚀 Production Deployment

### Using Docker Compose
1. Update `docker-compose.yml` for production:
   - Use environment-specific secrets
   - Add restart policies: `restart: always`
   - Use production PostgreSQL volume
   - Enable HTTPS
   - Set `ASPNETCORE_ENVIRONMENT=Production`

2. Deploy:
```powershell
docker-compose -f docker-compose.prod.yml up -d
```

### Using Docker Swarm
```powershell
docker stack deploy -c docker-compose.yml myfit
```

### Using Kubernetes
Generate Kubernetes manifests:
```powershell
# Install kompose
choco install kubernetes-kompose

# Convert docker-compose to k8s
kompose convert -f docker-compose.yml
```

## 🎯 Development Workflow

### Local Development with Hot Reload
```powershell
# Run only PostgreSQL in Docker
docker-compose up -d postgres

# Run API and Client locally (for hot reload)
cd src/MyFit.API
dotnet run

# In another terminal
cd src/MyFit.Client
dotnet watch run
```

### Docker Development
```powershell
# Make code changes
# Rebuild affected service
docker-compose up -d --build api

# Or rebuild all
docker-compose up -d --build
```

## 📝 Docker Compose Services Explained

### PostgreSQL Service
- Uses official PostgreSQL 16 Alpine image (smaller size)
- Persistent volume for data
- Health check ensures API waits for DB
- Exposed on host port 5432 for local tools (pgAdmin, DBeaver)

### API Service
- Multi-stage build for optimized image size
- Includes EF Core tools for migrations
- Auto-runs migrations on startup
- Copies seed data files
- Health depends on PostgreSQL

### Client Service
- Built as static files (Blazor WASM)
- Served by lightweight Nginx
- Optimized caching and compression
- Single-page application routing

## 🔗 Useful Links

- [Docker Desktop Download](https://www.docker.com/products/docker-desktop)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)

## 💡 Tips

1. **Use Docker Desktop Dashboard** for visual management
2. **Check resource usage** with `docker stats`
3. **Use named volumes** for data persistence
4. **Set up health checks** for better orchestration
5. **Use .dockerignore** to reduce build context size
6. **Multi-stage builds** reduce final image size
7. **Layer caching** speeds up rebuilds

## 🆘 Getting Help

If you encounter issues:
```powershell
# Collect diagnostic info
docker-compose config        # Validate compose file
docker-compose ps           # Service status
docker-compose logs         # All logs
docker system df            # Disk usage
docker inspect myfit-api    # Container details
```

---

**Next Steps**: Start the application with `docker-compose up --build` and visit http://localhost:5001! 🎉
