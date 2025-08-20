## Subster Backend

### Overview
Subster is a subscription management platform designed for personal trainers and their clients.  
The backend provides secure authentication, subscription handling, and integration with external services for invoicing and electronic identification.  

Built with modern best practices, Subster ensures **security, scalability, and maintainability** in production environments.

---

### Architecture
The backend is built on **.NET 9.0** with **Entity Framework Core** for database access and runs inside a **Docker** environment with a **PostgreSQL** database.  

The system follows a layered architecture to maintain **Separation of Concerns**:
- **Controllers** – REST API endpoints  
- **Services** – Business logic  
- **Repositories** – Database access  
- **Models** – Data structures  

This structure keeps responsibilities well-isolated and simplifies testing and future development.

---

### Security
Security was a core design principle from the beginning:
- All sensitive data (API keys, secrets) is **encrypted with AES-256**  
- Encrypted data is only decrypted in memory during use  
- JWT authentication is used for user sessions  
- External identity verification (via Taktikal eID)  

When interacting with **Payday** (the invoicing system), the backend:
1. Validates stored ClientId & ClientSecret for each trainer  
2. Looks up cached JWT tokens in memory (IMemoryCache)  
3. Refreshes tokens securely when expired  
4. Stores all tokens encrypted with proper expiration  

This design minimizes security risks and reduces unnecessary external API requests.

---

### Deployment
- **Local Development**: Run using Docker Compose with `.env` file for configuration  
- **Production**: Hosted on Render with automated deployments from GitHub  

Swagger is available for API testing:  
- Local: [http://localhost:10000/swagger](http://localhost:10000/swagger)  
- Production: *(no longer active, previously Render deployment)*  

---

### Setup

#### Prerequisites
- Docker & Docker Compose  
- .NET SDK 9.0  
- PostgreSQL (via Docker container)  
- Node.js (if working with frontend too)  

#### Steps
```
# Clone the repository
git clone git@github.com:Kacper2003/subster-api.git
cd subster-api

# Build the backend
dotnet build Subster.API/Subster.API.csproj

# Run with Docker
docker compose up --build
```

The backend will run on `http://localhost:10000`.

---

### API Documentation
The backend follows the **OpenAPI standard** for documenting endpoints.

- **Swagger UI** provides an interactive view of all REST endpoints, parameters, and responses  
- Documentation updates automatically as the backend evolves  

Swagger UI was available at deployment time on Render (now disabled).  
For local development, Swagger can still be accessed after running the backend at:  
[http://localhost:10000/swagger](http://localhost:10000/swagger)

---

### Example Endpoint Categories

- **Auth**  
  - `POST /api/auth/login/trainer` – Trainer login with eID  
  - `POST /api/auth/login/client` – Client login with eID  
  - `GET /api/auth/me` – Fetch current user claims  

- **Client Dashboard**  
  - `GET /api/client/subscriptions` – Get all client subscriptions  
  - `POST /api/client/subscriptions` – Create new subscription  

- **Trainer Dashboard**  
  - `GET /api/trainer/clients` – Get all trainer’s clients  
  - `POST /api/trainer/clients` – Register new client  

---

## License
This project was developed as a **Bachelor’s Final Project at Reykjavík University** (Spring 2025).  
