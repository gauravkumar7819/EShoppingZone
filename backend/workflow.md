# 🛍️ ShoppingKaro (formerly EShoppingZone) Architecture & Workflow Deep-Dive

Welcome to the definitive architectural and engineering guide for **ShoppingKaro**, a premium, state-of-the-art e-commerce platform tailored for the Indian retail market. ShoppingKaro is engineered on a resilient, high-performance, and scalable distributed microservices architecture on the backend, complemented by a highly reactive, responsive, and visually stunning React SPA on the frontend.

This document provides a highly detailed architectural dissection, technical breakdowns, database models, and interactive flow diagrams mapping out critical business workflows across the application ecosystem.

---

## 🏗️ 1. High-Level Architecture Overview

ShoppingKaro utilizes a **Database-per-Service microservices pattern** coupled with a reverse proxy gateway using **Microsoft YARP (Yet Another Reverse Proxy)**. This design provides maximum loose coupling, fault isolation, and independent scalability for individual domains.

```mermaid
graph TD
    %% Styling
    classDef client fill:#3b82f6,stroke:#1d4ed8,stroke-width:2px,color:#fff;
    classDef gateway fill:#10b981,stroke:#047857,stroke-width:2px,color:#fff;
    classDef service fill:#f59e0b,stroke:#b45309,stroke-width:2px,color:#fff;
    classDef storage fill:#ec4899,stroke:#be185d,stroke-width:2px,color:#fff;
    classDef ext fill:#8b5cf6,stroke:#6d28d9,stroke-width:2px,color:#fff;

    %% Elements
    Client["🎨 React Frontend Client<br/>(Vite, Tailwind, React Query)"]:::client
    Gateway["🛡️ YARP API Gateway<br/>(Port: 5000)"]:::gateway
    
    ProfileSvc["👤 Profile & Auth Service<br/>(Port: 5001)"]:::service
    ProductSvc["📦 Product Catalog Service<br/>(Port: 5002)"]:::service
    CartSvc["🛒 Cart & Redis Cache Service<br/>(Port: 5003)"]:::service
    OrderSvc["📝 Order Orchestrator Service<br/>(Port: 5004)"]:::service
    WalletSvc["💳 Wallet & Razorpay Service<br/>(Port: 5005)"]:::service
    
    RedisCache[("⚡ Redis Cache<br/>(Cart Cache)")]:::storage
    AzureSql[("🛢️ Azure SQL Server<br/>(Isolated DbContexts)")]:::storage
    AzureBlob[("🖼️ Azure Blob Storage<br/>(Product Images)")]:::storage
    
    GoogleAuth["🔑 Google Identity Provider<br/>(OIDC Login)"]:::ext
    RazorpayPay["💸 Razorpay Payment Gateway<br/>(Recharges)"]:::ext

    %% Connections
    Client ==>|HTTP / JWT Bearer| Gateway
    
    Gateway ==>|/api/auth & /api/profile| ProfileSvc
    Gateway ==>|/api/product| ProductSvc
    Gateway ==>|/api/cart| CartSvc
    Gateway ==>|/api/order| OrderSvc
    Gateway ==>|/api/wallet| WalletSvc
    
    %% Service DBs
    ProfileSvc -->|EF Core - ProfileDbContext| AzureSql
    ProductSvc -->|EF Core - ProductDbContext| AzureSql
    CartSvc -->|EF Core - CartDbContext| AzureSql
    CartSvc <==>|Cache Read/Write| RedisCache
    OrderSvc -->|EF Core - OrderDbContext| AzureSql
    WalletSvc -->|EF Core - WalletDbContext| AzureSql
    
    %% Integrations
    ProfileSvc -.->|Oauth Callback Exchange| GoogleAuth
    ProductSvc -.->|Store & Serve| AzureBlob
    WalletSvc -.->|Initiate & Verify| RazorpayPay
    
    %% Inter-service communication
    OrderSvc ==>|Internal HTTP| CartSvc
    OrderSvc ==>|Internal HTTP| WalletSvc
    OrderSvc ==>|Internal HTTP| ProfileSvc
```

### Key Architectural Characteristics:
1. **API Gateway Routing**: The YARP API Gateway (`eshoppingzone-api-gateway`) acts as a single secure entry point. It handles path-based transformations and routes traffic down to the corresponding microservices inside an isolated Docker bridge network.
2. **State Isolation (Database-per-Service)**: Each backend microservice runs its own instance of Entity Framework Core (`DbContext`) and owns its database tables. Cross-service database queries are forbidden, preventing schema coupling.
3. **Internal Orchestration**: The `Order.Service` acts as an orchestrator, hitting other microservice endpoints internally over HTTP (`Cart.Service`, `Wallet.Service`, `Profile.Service`) to place and process orders.
4. **Caching Strategy**: The `Cart.Service` employs a high-performance Write-Through / Cache-Aside caching architecture utilizing a `Redis` instance for sub-millisecond cart retrieval times.

---

## 🛠️ 2. Comprehensive Tech Stack

### Frontend Application
- **Core Engine**: React 18 SPA built with [Vite](file:///d:/EShoppingZone_Frontend/EShoppingZone_frontend/frontend/vite.config.js) (Hot Module Replacement, ultra-fast builds).
- **Styling System**: [Tailwind CSS](file:///d:/EShoppingZone_Frontend/EShoppingZone_frontend/frontend/tailwind.config.js) for an Indian-branded premium, responsive user interface.
- **State Management & Server Cache**: `@tanstack/react-query` (React Query) for smart caching, background fetching, automatic retries, and optimistic UI mutations.
- **Client Routing**: `react-router-dom` with customized [ProtectedRoute](file:///d:/EShoppingZone_Frontend/EShoppingZone_frontend/frontend/src/App.jsx#L8) supporting role-based authorization (Customer, Merchant, Admin).
- **Network Requests**: `axios` with interceptors injecting Authorization Bearer Tokens and providing global response error handling (e.g., auto-logout on 401 Session Expired).
- **Notifications**: `react-hot-toast` for fluid, beautifully styled micro-animations.

### Backend Infrastructure
- **Framework**: ASP.NET Core 8.0 Web API, compiling down to cross-platform high-performance binaries.
- **API Gateway**: Microsoft YARP (Yet Another Reverse Proxy) for dynamic configuration, route transforms, and resilient load balancing.
- **Object Relational Mapper (ORM)**: EF Core (Entity Framework) with code-first migrations.
- **Databases**:
  - **Azure SQL Database**: Hosts application tables for persistence.
  - **Redis Cache**: Used for lightning-fast cart operations.
- **Security & Authorization**: JWT (JSON Web Tokens) with asymmetrical/symmetrical verification, token extraction, role validation, and OAuth 2.0 (Google OIDC).
- **Third-Party Integrations**:
  - **Razorpay**: API client integration for secure credit/debit card, UPI, and net banking deposits to the customer's digital wallet.
  - **Azure Blob Storage**: Storage containers for high-performance, cached product images.
- **Development Lifecycle**: Multi-stage `Dockerfiles` for each service, orchestration via `docker-compose.yml`, SonarQube integration, and Cypress E2E testing.

---

## 📦 3. Microservices Inventory

Let's drill down into the responsibilities, code architectures, and API endpoints of each core microservice on the backend.

### 👤 A. Profile & Auth Service (`Profile.Service`)
Manages identities, role definitions, standard profiles, addresses, and third-party authentication.

- **Technology**: ASP.NET Core API + EF Core + SQL Server.
- **Controllers**:
  - [AuthController.cs](file:///d:/EShoppingZone/EShoppingZone/backend/Services/Profile.Service/EShoppingZone.Profile.API/Controllers/AuthController.cs): Manages registrations (`/register/customer`, `/register/merchant`), credentials verification `/login`, and Google OAuth `/google-callback` exchanges.
  - `ProfileController.cs`: Customer/Merchant profile updates and addresses CRUD.
  - `AdminController.cs`: Enforces high-level administrative functions such as searching profiles and viewing dashboard metadata.
- **Google OAuth Integration**:
  1. Frontend redirects the user to Google OAuth endpoint.
  2. User consents, Google redirects back to `/google-callback?code=XXX`.
  3. Frontend sends authorization code to the backend.
  4. Backend exchanges authorization code with Google APIs for an access token, fetches the user info (`email`, `name`, `id`), checks if the profile exists in the DB, registers a new Customer profile if not, and responds to the frontend with a secure JWT.

---

### 📦 B. Product Service (`Product.Service`)
Maintains the inventory, catalogs, search queries, and merchant-owned product lines.

- **Technology**: ASP.NET Core API + EF Core + SQL Server + Azure Blob Storage.
- **Controllers**:
  - `ProductController.cs`: Public-facing catalog searches, filters, categories, and specific product information retrieval.
  - `MerchantProductController.cs`: Role-authorized endpoints for merchants to create, edit, delete, and restock products.
  - `AdminProductController.cs`: Admin auditing endpoints to manage products and override inventories.
- **Product Media Flow**:
  - Merchants upload images directly through the product dashboard.
  - `Product.Service` uploads the image binary asynchronously to Azure Blob Storage and stores the generated public URL in the SQL database.

---

### 🛒 C. Cart Service (`Cart.Service`)
Provides temporary caching and shopping cart management, ensuring high responsiveness under high user traffic.

- **Technology**: ASP.NET Core API + EF Core + Redis Cache (`IDistributedCache`).
- **Controllers**:
  - `CartController.cs`: Core CRUD for cart items, clearing carts, and calculating breakdowns.
- **Write-Through Caching Logic**:
  - **Reads**: To avoid choking the relational database, `Cart.Service` first checks Redis. On a cache hit, the JSON string is immediately deserialized and returned. On a cache miss, it reads from SQL Server, caches it, and returns the DTO.
  - **Writes**: Operations like "Add to Cart" or "Change Quantity" commit the updates directly to the SQL database to ensure hard consistency and immediately overwrite the cached value in Redis with the latest state.
  - **Clears/Deletes**: Completely invalidates the Redis cache key (`cart_{id}`) to prevent stale reads.

---

### 💳 D. Wallet Service (`Wallet.Service`)
Operates as the financial heart of the platform, managing digital cash transactions, statements, and payment gateway interactions.

- **Technology**: ASP.NET Core API + EF Core + Razorpay .NET SDK.
- **Controllers**:
  - `WalletController.cs`: Wallet balances, credit deposits, statements history, and internal merchant payouts/order payments.
- **Razorpay Payment Verification Workflow**:
  - Standard cryptography is used to prevent transaction tampering.
  - During deposits, a signature is generated via Razorpay's API and validated on the backend using HMAC-SHA256 signature verification matching with the local secret.

---

### 📝 E. Order Service (`Order.Service`)
Orchestrates order creation, coordinates multi-service payment checking, manages tracking status histories, and aggregates global analytics.

- **Technology**: ASP.NET Core API + EF Core + Dynamic HTTP Service Clients.
- **Controllers**:
  - [OrderController.cs](file:///d:/EShoppingZone/EShoppingZone/backend/Services/Order.Service/EShoppingZone.Order.API/Controllers/OrderController.cs): Places orders (`/place`), customer histories (`/customer/{id}`), cancellations (`/{id}/cancel`), tracking timelines, and admin overrides.
  - `AdminAnalyticsController.cs`: Computes total earnings, order frequencies, top-selling categories, and metrics.
- **Dynamic Integrations**:
  - Coordinates inter-service HTTP calls using [CartServiceClient](file:///d:/EShoppingZone/EShoppingZone/backend/Services/Order.Service/EShoppingZone.Order.API/Integrations/CartServiceClient.cs), [WalletServiceClient](file:///d:/EShoppingZone/EShoppingZone/backend/Services/Order.Service/EShoppingZone.Order.API/Integrations/WalletServiceClient.cs), and `ProfileServiceClient`.

---

## 🔄 4. Detailed Architectural Workflows & Diagrams

### 🛒 Workflow 1: Cart Caching Strategy (Write-Through Cache-Aside)

This diagram details the exact read-write workflow implemented in [CartService.cs](file:///d:/EShoppingZone/EShoppingZone/backend/Services/Cart.Service/EShoppingZone.Cart.API/Services/CartService.cs) to achieve maximum performance and low database load:

```mermaid
sequenceDiagram
    autonumber
    actor Customer as 👤 Customer Client
    participant API as 🛡️ API Gateway
    participant CartSvc as 🛒 Cart Service
    participant Redis as ⚡ Redis Cache
    participant SQL as 🛢️ SQL Database

    %% READ SCENARIO - CACHE HIT
    Note over Customer, Redis: Scenario A: Reading Cart (Cache Hit)
    Customer->>API: GET /api/cart/{id}
    API->>CartSvc: Forward GetCartById(id)
    CartSvc->>Redis: Check cache key "cart_{id}"
    Redis-->>CartSvc: Return Cached JSON
    CartSvc-->>API: Return Cart DTO (Instantly)
    API-->>Customer: Return 200 OK

    %% READ SCENARIO - CACHE MISS
    Note over Customer, SQL: Scenario B: Reading Cart (Cache Miss)
    Customer->>API: GET /api/cart/{id}
    API->>CartSvc: Forward GetCartById(id)
    CartSvc->>Redis: Check cache key "cart_{id}"
    Redis-->>CartSvc: Return NULL (Cache Miss)
    CartSvc->>SQL: Query cart & items from DB
    SQL-->>CartSvc: Return Cart Records
    CartSvc->>Redis: Set cache key "cart_{id}" with JSON
    CartSvc-->>API: Return Cart DTO
    API-->>Customer: Return 200 OK

    %% WRITE SCENARIO
    Note over Customer, SQL: Scenario C: Modifying Cart (Write-Through)
    Customer->>API: POST /api/cart/add (Item, Qty)
    API->>CartSvc: Forward AddToCart()
    CartSvc->>SQL: Insert/Update CartItem in DB
    SQL-->>CartSvc: Save OK
    CartSvc->>Redis: Overwrite cache key "cart_{id}" with updated DTO JSON
    CartSvc-->>API: Return Updated Cart DTO
    API-->>Customer: Return 200 OK
```

---

### 💳 Workflow 2: Wallet Recharge & Verification (Razorpay)

Recharging the wallet is protected against fraudulent injection using strict signature checks.

```mermaid
sequenceDiagram
    autonumber
    actor Customer as 👤 Customer Client
    participant UI as 🎨 React Frontend
    participant Gateway as 🛡️ YARP Gateway
    participant WalletSvc as 💳 Wallet Service
    participant RazorpayAPI as 💳 Razorpay Gateway API
    participant SQL as 🛢️ SQL Database

    Customer->>UI: Input Amount (e.g. ₹1000) & Click Recharge
    UI->>Gateway: POST /api/wallet/deposit/initiate
    Gateway->>WalletSvc: Forward initiate request
    WalletSvc->>RazorpayAPI: Create Razorpay Order (Amt, Receipt)
    RazorpayAPI-->>WalletSvc: Return Razorpay Order ID (order_xxxx)
    WalletSvc-->>Gateway: Return Order ID & Key ID
    Gateway-->>UI: Return Order ID & Key ID
    
    Note over UI, Customer: Load Razorpay Checkout Script
    UI->>Customer: Render Secure Razorpay Payment Dialog
    Customer->>UI: Enter payment details (UPI/Card) & click PAY
    UI->>RazorpayAPI: Process payment transaction
    RazorpayAPI-->>UI: Return verification payload (PaymentId, Signature)
    
    UI->>Gateway: POST /api/wallet/deposit/verify (PaymentId, OrderId, Signature, WalletId, Amount)
    Gateway->>WalletSvc: Forward verification request
    
    Note over WalletSvc: Validate payment via Cryptography:<br/>Compute HMAC-SHA256 of (OrderId + "|" + PaymentId)<br/>Match with Signature
    
    alt Signature is Valid
        WalletSvc->>SQL: Begin Database Transaction
        WalletSvc->>SQL: Increment Wallet.CurrentBalance += Amount
        WalletSvc->>SQL: Insert Statement (Type: CREDIT, RazorpayPaymentId)
        WalletSvc->>SQL: Commit Transaction
        WalletSvc-->>Gateway: Return Success & New Balance
        Gateway-->>UI: Show Toast "Money added successfully!"
        UI->>Customer: Render updated dashboard balance
    else Signature is Invalid / Fraud Attempt
        WalletSvc-->>Gateway: Return 400 Bad Request
        Gateway-->>UI: Show Error toast
        UI->>Customer: Render Payment Verification Failed
    end
```

---

### 📝 Workflow 3: E2E Order Placement Orchestration & Transaction Rollback

This is the most critical distributed transaction workflow. The order placement is managed inside a robust, resilient flow with rollback capabilities in case of insufficient funds or service crashes.

```mermaid
sequenceDiagram
    autonumber
    actor Customer as 👤 Customer Client
    participant UI as 🎨 React Frontend
    participant Gateway as 🛡️ YARP Gateway
    participant OrderSvc as 📝 Order Service
    participant CartSvc as 🛒 Cart Service
    participant ProfileSvc as 👤 Profile Service
    participant WalletSvc as 💳 Wallet Service
    participant SQL as 🛢️ SQL Database

    Customer->>UI: Click "Place Order" (Wallet Mode)
    UI->>Gateway: POST /api/order/place (CartId, AddressId, PaymentMode)
    Gateway->>OrderSvc: Forward PlaceOrder()
    
    Note over OrderSvc: Step 1: Internal API Gather
    OrderSvc->>CartSvc: GET internal /api/Cart/{CartId}
    CartSvc-->>OrderSvc: Return active Cart items & Prices
    
    OrderSvc->>ProfileSvc: GET internal /api/Profile/address/{AddressId}
    ProfileSvc-->>OrderSvc: Return formatted shipping address
    
    Note over OrderSvc: Step 2: Create Initial Order
    OrderSvc->>SQL: Insert Order record (Status: Placed, TotalAmount = CartTotal)
    SQL-->>OrderSvc: Saved (Assigns real OrderId, e.g. #901)
    
    alt Mode of Payment = Wallet
        Note over OrderSvc: Step 3: Financial Settlement
        OrderSvc->>WalletSvc: GET internal /api/Wallet/customer/{CustId}
        WalletSvc-->>OrderSvc: Return Wallet Details (WalletId, Balance)
        
        OrderSvc->>WalletSvc: POST internal /api/Wallet/process-payment (WalletId, TotalAmount, OrderId)
        
        alt Wallet has Sufficient Funds
            WalletSvc->>SQL: Begin Transaction
            WalletSvc->>SQL: Deduct Balance (Wallet.Balance -= TotalAmount)
            WalletSvc->>SQL: Insert Statement (Type: DEBIT, OrderId: 901)
            WalletSvc->>SQL: Commit Transaction
            WalletSvc-->>OrderSvc: Return Success & TransactionId (StatementId)
            
            Note over OrderSvc: Step 4: Finalize Order Creation
            OrderSvc->>SQL: Update Order #901 with TransactionId & PaymentDate
            OrderSvc->>SQL: Insert OrderItems (linked to Order #901)
            OrderSvc->>SQL: Insert StatusHistory (Status: Placed)
            
            OrderSvc->>CartSvc: DELETE internal /api/Cart/clear/{CartId}
            CartSvc-->>OrderSvc: Cart Cleared successfully
            
            OrderSvc-->>Gateway: Return Success Response
            Gateway-->>UI: Redirect to Success Page!
            UI->>Customer: Display "Order #901 Placed successfully"
            
        else Insufficient Funds / Wallet Inactive
            WalletSvc-->>OrderSvc: Return Failure DTO ("Insufficient funds")
            
            Note over OrderSvc: Distributed Rollback Action!
            OrderSvc->>SQL: Delete Order record #901 (Rollback)
            
            OrderSvc-->>Gateway: Return 400 Bad Request ("Insufficient Wallet Balance")
            Gateway-->>UI: Show Error Toast
            UI->>Customer: Display "Payment Failed: Please recharge your wallet"
        end
        
    else Mode of Payment = Cash on Delivery (COD)
        Note over OrderSvc: COD Bypass Flow
        OrderSvc->>SQL: Generate TransactionId "COD_xxxx" & Save OrderItems
        OrderSvc->>SQL: Insert StatusHistory (Status: Placed)
        OrderSvc->>CartSvc: DELETE internal /api/Cart/clear/{CartId}
        CartSvc-->>OrderSvc: Cart Cleared
        OrderSvc-->>Gateway: Return Success Response
        Gateway-->>UI: Redirect to Success Page
        UI->>Customer: Display "Order Placed successfully (COD)"
    end
```

---

## 🔒 5. Key Design Patterns Implemented

1. **API Gateway Transform Pattern**: Inside [appsettings.json](file:///d:/EShoppingZone/EShoppingZone/backend/EShoppingZone.ApiGateway/appsettings.json), YARP utilizes Path Transformations to strip prefixes and cleanly match routing structures without altering the underlying service routing controllers.
2. **Write-Through / Cache-Aside Caching**: Cart Service ensures high-performance shopping cart updates by checking Redis first (Cache-Aside) and writing to both databases on changes (Write-Through).
3. **Database-per-Service**: Promotes structural integrity. Services communicate through synchronous HTTP endpoints, guaranteeing schema isolation.
4. **Saga-like Rollback Pattern**: Handled synchronously in `OrderService`. When an order is initially created in the database, the corresponding payment deduction is triggered. If the payment service returns a failure, the Order Service executes a clean database delete of the pending order to restore consistency.
5. **Secure Cryptographic Verifications**: Recharges use HMAC-SHA256 signatures validated against Razorpay keys to prevent client-side payment forgery.

---

## 🐳 6. Containerization and Deployment Topology

### Local Multi-Container Development Setup
The platform is fully orchestrated locally using Docker. The [docker-compose.yml](file:///d:/EShoppingZone/EShoppingZone/backend/docker-compose.yml) orchestrates 7 independent containers:
1. `eshoppingzone-profile-service`: Profile & Auth Service API.
2. `eshoppingzone-product-service`: Product Catalog Service API.
3. `eshoppingzone-redis`: Redis Alpine container for Cart Caching.
4. `eshoppingzone-cart-service`: Cart caching API.
5. `eshoppingzone-order-service`: Order orchestrator API.
6. `eshoppingzone-wallet-service`: Wallet transaction API.
7. `eshoppingzone-api-gateway`: YARP reverse proxy gateway.

All containers are linked using a customized bridge network (`eshoppingzone-network`), allowing secure service discovery via internal service names.

### Production Cloud Topology (Azure Container Apps)
When running in production, environment variables are loaded to point directly to cloud resources:
- **Compute**: Deployed on **Azure Container Apps** (serverless microservices that scale to zero based on HTTP traffic or CPU usage).
- **Relational Storage**: Connects to a high-capacity, geo-replicated **Azure SQL Database**.
- **Caching**: Scaled via **Azure Cache for Redis**.
- **Media Assets**: Served using CDN-backed **Azure Blob Storage**.
