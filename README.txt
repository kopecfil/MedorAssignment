
# Bitcoin Exchange Rate Viewer

## 📘 Project Description
This project is a simple two-page ASP.NET Web Forms application designed to view, save, and edit comments on Bitcoin exchange rate data.  
It periodically downloads BTC/EUR exchange rates from the CoinDesk API, converts them to CZK using the Czech National Bank’s exchange rate API, and allows users to save the collected data to a local SQL Server database.  

The application demonstrates separation of backend (REST Web API) and frontend (ASP.NET Web Forms), use of Entity Framework for persistence, and compliance with SOLID principles and maintainable code practices.

---

## 🚀 How to Run the Application

### Setup Steps
1. Modify the connection string in WebApp/Web.config according to a database that's available for this purpose.
2. Run CreateDatabase.sql on the target database eg. from SSMS.
3. Run the solution.
4. Navigate to .../LiveData on localhost with the appropriate port.

# URLs for testing purposes
GET https://localhost:44300/api/exchangeRates/coindesk/btceur
POST https://localhost:44300/api/exchangeRates/snapshots/bulk
GET https://localhost:44300/api/exchangeRates/snapshots
GET https://localhost:44300/api/ping
