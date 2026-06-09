# ⛅ WeatherApp

Sistema Web com integração a uma API falsa para resgate aleatório de dados meteorológicos.

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

## Rodando localmente (passo a passo)

### 1. Pré-requisitos

Instale as ferramentas abaixo antes de começar:

| Ferramenta | Link | Para que serve |
|---|---|---|
| .NET 8 SDK | https://dotnet.microsoft.com/download/dotnet/8 | Compilar e rodar o backend |
| Node.js 20+ | https://nodejs.org/ | Rodar o frontend Vue |
| Docker Desktop | https://www.docker.com/products/docker-desktop/ | Rodar o banco PostgreSQL |
| VS Code | https://code.visualstudio.com/ | Editor de código |

**Extensões recomendadas no VS Code:**
- `C# Dev Kit` (Microsoft)
- `Vue - Official` (Volar)
- `Docker` (Microsoft)

---

### 2. Clonar o repositório

```bash
git clone https://github.com/seu-usuario/weatherapp.git
cd weatherapp
```

Ou extraia o arquivo `.zip` e abra a pasta no VS Code:
`File > Open Folder` → selecione a pasta `weatherapp`

---

### 3. Subir o banco de dados

Abra o **Docker Desktop** e aguarde ele inicializar (ícone fica verde na bandeja do sistema).

No terminal do VS Code (`Ctrl + '`):

```bash
docker compose up db -d
```

Confirme que está rodando:

```bash
docker ps
```

Deve aparecer `weatherapp-db` com status `Up`.

---

### 4. Instalar a ferramenta de migrations

```bash
dotnet tool install --global dotnet-ef
```

> Após instalar, **feche e reabra o terminal** para o comando ser reconhecido.

---

### 5. Criar as tabelas no banco

```bash
cd backend/WeatherApp.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Deve aparecer `Done.` ao final.

---

### 6. Rodar o backend

No mesmo terminal, ainda dentro de `backend/WeatherApp.Api`:

```bash
dotnet run
```

A API estará disponível em:
- **API:** http://localhost:5000
- **Swagger:** http://localhost:5000/swagger
- **Health check:** http://localhost:5000/health

---

### 7. Rodar o frontend

Abra um **segundo terminal** no VS Code (clique em `+` no painel de terminais):

```bash
cd frontend
npm install
npm run dev
```

O frontend estará disponível em:
- **App:** http://localhost:3000

---

### 8. Usar o sistema

Acesse **http://localhost:3000**, digite o nome de uma cidade (ex: `Curitiba`, `São Paulo`, `Manaus`) e clique em **Registrar**. O histórico e o gráfico aparecerão logo abaixo.

---

### Resumo dos terminais

```
Terminal 1:  docker compose up db -d          → banco na porta 5432
Terminal 2:  cd backend/WeatherApp.Api
             dotnet run                        → API em localhost:5000
Terminal 3:  cd frontend
             npm run dev                       → app em localhost:3000
```

---

### Problemas comuns

**"Este host não é conhecido" ao rodar o backend**
O backend está tentando conectar em `Host=db` (configuração do Docker). Certifique-se de que o arquivo `backend/WeatherApp.Api/appsettings.Development.json` tem `Host=localhost` e que a variável de ambiente está correta:

```bash
# PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run

# CMD
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

**"Porta 5000 em uso" ao rodar o backend**
Verifique qual processo está usando a porta e encerre:

```bash
netstat -ano | findstr :5000
taskkill /PID <numero_do_pid> /F
```

Se receber "Acesso negado", abra o terminal como **Administrador** (Windows + X → Terminal Administrador).

**Banco não conecta (porta 5432 recusada)**
O Docker Desktop não está rodando. Abra-o pelo menu iniciar e aguarde inicializar antes de rodar `docker compose up db -d`.

---

## Subindo tudo com Docker (alternativa)

Se preferir rodar tudo via Docker sem instalar .NET ou Node localmente:

```bash
docker compose up --build
```

| Serviço | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| Health | http://localhost:5000/health |

---

## Usando com OpenWeatherMap (API real)

Por padrão o sistema usa dados simulados. Para usar a API real:

1. Crie uma conta gratuita em https://openweathermap.org/api e obtenha sua chave
2. Edite o `docker-compose.yml`:

```yaml
environment:
  WeatherProviders__UseProvider: "OpenWeatherMap"
  WeatherProviders__OpenWeatherMap__ApiKey: "SUA_CHAVE_AQUI"
```

3. Suba novamente: `docker compose up --build`

---

## Rodando os testes

```bash
cd backend
dotnet test
```

Resultado esperado: **21 testes**, todos passando (13 unitários + 8 de integração).

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

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/weather/city` | Registra temperatura por nome de cidade |
| POST | `/api/weather/coordinates` | Registra por latitude e longitude |
| GET | `/api/weather/history` | Histórico dos últimos 30 dias |
| GET | `/health` | Health check |
| GET | `/swagger` | Documentação interativa |

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
```
