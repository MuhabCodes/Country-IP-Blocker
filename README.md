# Country IP Blocker

## Overview
The Country IP Blocker API is a .NET Core Web API service that allows management of blocked countries and validation of IP addresses using third-party geolocation services. This application uses in-memory data storage rather than a database to maintain lists of blocked countries and logs of blocking attempts.
## Getting Started

### Installation
1. Clone the repository
   ```
   git clone https://github.com/MuhabCodes/country-ip-blocker.git
   cd country-ip-blocker
   ```

2. Configure the API key
   - Open `appsettings.json`
   - Add your third-party geolocation API key:
     ```json
     {
      "IPGeolocation": {
         "ApiKey": "[Redacted]",
         "BaseUrl": "https://api.ipgeolocation.io/ipgeo/"
      }
     }
     ```

3. Build and run the application
   ```
   dotnet build
   dotnet run
   ```

4. Access the API documentation
   - Navigate to `https://localhost:5294` in your browser

## API Endpoints

### Country Management
- `POST /api/countries/block` - Block a country by country code
- `DELETE /api/countries/block/{countryCode}` - Unblock a country
- `GET /api/countries/blocked` - Get list of all blocked countries (with pagination and filtering)
- `POST /api/countries/temporal-block` - Temporarily block a country for specified duration

### IP Validation
- `GET /api/ip/lookup?ipAddress={ip}` - Look up country information for an IP address
- `GET /api/ip/check-block` - Check if the current IP address is from a blocked country

### Logging
- `GET /api/logs/blocked-attempts` - Retrieve logs of blocked access attempts (with pagination)

## Configuration Options

### Geolocation API
The application supports multiple third-party geolocation providers. Configure your preferred provider in `appsettings.json`.

### Background Services
The application includes a background service that runs every 5 minutes to remove expired temporary blocks.

## Development

### Project Structure
- `Controllers/` - API endpoint controllers
- `Services/` - Service implementations for business logic and third-party API integration
- `Models/` - Request/response models and data transfer objects
- `Repositories/` - In-memory data stores
- `Background/` - Background services for maintenance tasks