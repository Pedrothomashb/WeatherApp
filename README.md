# ⛅ WeatherApp

Aplicação full-stack para registro e consulta de temperaturas por cidade ou coordenadas geográficas.

**Stack:** .NET 8 (C#) · Vue 3 + TypeScript · PostgreSQL · Docker

---

## Funcionalidades

- Registrar temperatura por **nome de cidade**
- Registrar temperatura por **latitude/longitude**
- Consultar **histórico dos últimos 30 dias** em lista e gráfico
- Provider de clima real (**OpenWeatherMap**) ou simulado (**Fake**, padrão)
- **Feature flag** para trocar o provider via variável de ambiente
- Swagger UI para explorar a API
- Health check em `/health`

---

## Início rápido (Docker)

### Pré-requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e rodando

### Subir tudo com um comando

```bash
git clone https://github.com/seu-usuario/weatherapp.git
cd weatherapp
docker compose up --build
```

Após o build (~2 min na primeira vez):

| Serviço     | URL                                    |
|-------------|----------------------------------------|
| Frontend    | http://localhost:3000                  |
| API         | http://localhost:5000                  |
| Swagger     | http://localhost:5000/swagger          |
| Health      | http://localhost:5000/health           |

---

## Usando com OpenWeatherMap (API real)

1. Crie uma conta gratuita em https://openweathermap.org/api
2. Obtenha sua API key
3. Edite o `docker-compose.yml`:

```yaml
environment:
  WeatherProviders__UseProvider: "OpenWeatherMap"
  WeatherProviders__OpenWeatherMap__ApiKey: "SUA_CHAVE_AQUI"
```

4. Suba novamente: `docker compose up --build`

---

## Desenvolvimento local (sem Docker)

### Backend

```bash
# Pré-requisito: .NET 8 SDK + PostgreSQL rodando localmente

cd backend/WeatherApp.Api
dotnet restore
dotnet run
```

A string de conexão para dev está em `appsettings.Development.json`.

### Migrations

```bash
cd backend/WeatherApp.Api
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# Acesse http://localhost:3000
```

### Testes

```bash
cd backend
dotnet test
```

---

## Estrutura do projeto

```
weatherapp/
├── backend/
│   ├── WeatherApp.Api/
│   │   ├── Controllers/       # WeatherController
│   │   ├── Services/          # WeatherService (orquestração)
│   │   ├── Repositories/      # WeatherRepository (EF Core)
│   │   ├── Providers/         # OpenWeatherMapProvider, FakeWeatherProvider
│   │   ├── Models/            # City, TemperatureRecord
│   │   ├── DTOs/              # Request/Response contracts
│   │   ├── Data/              # WeatherDbContext
│   │   ├── Middleware/        # ExceptionMiddleware
│   │   └── Program.cs
│   ├── WeatherApp.Tests/
│   │   ├── Unit/              # Testes de WeatherService, FakeProvider
│   │   └── Integration/       # Testes da API com WebApplicationFactory
│   └── Dockerfile
├── frontend/
│   ├── src/
│   │   ├── components/        # TemperatureChart.vue
│   │   ├── services/          # weatherApi.ts (axios)
│   │   ├── stores/            # weather.ts (Pinia)
│   │   ├── types/             # weather.ts (TypeScript interfaces)
│   │   └── App.vue
│   ├── Dockerfile
│   └── nginx.conf
├── .github/workflows/ci.yml   # GitHub Actions CI
├── docker-compose.yml
└── README.md
```

---

## Endpoints da API

| Método | Rota                    | Descrição                                  |
|--------|-------------------------|--------------------------------------------|
| POST   | `/api/weather/city`     | Registra temperatura por nome de cidade     |
| POST   | `/api/weather/coordinates` | Registra por latitude e longitude        |
| GET    | `/api/weather/history`  | Histórico dos últimos 30 dias              |
| GET    | `/health`               | Health check                               |
| GET    | `/swagger`              | Documentação interativa                    |

### Exemplos

```bash
# Registrar por cidade
curl -X POST http://localhost:5000/api/weather/city \
  -H "Content-Type: application/json" \
  -d '{"cityName": "Curitiba"}'

# Registrar por coordenadas
curl -X POST http://localhost:5000/api/weather/coordinates \
  -H "Content-Type: application/json" \
  -d '{"latitude": -25.4284, "longitude": -49.2733}'

# Histórico por cidade
curl "http://localhost:5000/api/weather/history?city=Curitiba"

# Histórico por coordenadas
curl "http://localhost:5000/api/weather/history?lat=-25.4284&lon=-49.2733"
```

---

## Decisões técnicas

- **Feature flag de provider** — controlada via `WeatherProviders:UseProvider` (env var ou appsettings). Sem reinício de código, só configuração.
- **FakeProvider** habilitado por padrão — sem necessidade de API key para rodar.
- **EF Core com migrations automáticas** — o banco é criado/atualizado no startup.
- **Upsert de cidade** — evita duplicatas ao buscar a mesma cidade várias vezes.
- **SOLID aplicado** — `IWeatherProvider`, `IWeatherRepository`, `IWeatherService` permitem substituição fácil por mocks nos testes.
- **CORS aberto** — adequado para dev; em produção, restringir para o domínio do frontend.
