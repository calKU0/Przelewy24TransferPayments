
# Przelewy24TransferPayments

**Przelewy24TransferPayments** is a Windows Service application built using .NET Framework 4.7.2. It automates the process of retrieving payments from Przelewy24 and transferring them to respective merchants.

## Features

- 🕒 **Scheduled Execution**: Runs every one minute to check for new payments.
- 🔗 **Przelewy24 API Integration**: Connects to the Przelewy24 payment API to fetch transaction data.
- 💸 **Merchant Transfers**: Automatically transfers received funds to merchants.
- 📝 **Logging**:
  - Saves response JSON from Przelewy24 to a designated folder.
  - Logs operational details to a database table.
  - Writes general logs to a log file for traceability.

## Technologies Used

- **.NET Framework 4.7.2**
- **Windows Services**
- **Przelewy24 API**
- **SQL Server** (via ADO.NET or EF)

## Configuration

Settings are defined in `App.config`. Below is a sample configuration:

```xml
<appSettings>
  <!-- Przelewy24 API -->
  <add key="ApiUser" value="user" />
  <add key="ApiCRCKey" value="crckey " />
  <add key="ApiSecret" value="secret" />
  <add key="ApiBaseUrl" value="https://secure.przelewy24.pl/api/v1/" />
</appSettings>

<connectionStrings>
  <add name="ConnectionString" connectionString="Server='server';Database='database';User Id='user';Password='password';Connection Timeout=5;TrustServerCertificate=True"/>
</connectionStrings>
```

## Usage

1. Build and install the service using `sc` or Visual Studio's installer projects.
2. Configure app settings and database access.
3. Ensure proper permissions for writing logs and accessing the Przelewy24 API.

## License

This project is proprietary and confidential. See the [LICENSE](LICENSE) file for more information.
