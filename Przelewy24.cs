using Przelewy24TransferPayments.Interfaces;
using Przelewy24TransferPayments.Logging;
using Przelewy24TransferPayments.Models;
using Przelewy24TransferPayments.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments
{
    public partial class Przelewy24 : ServiceBase
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ToString();

        private Timer _timer;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(160);

        private readonly IPrzelewy24Service _przelewy24Service;
        private readonly IDatabaseService _databaseService;
        public Przelewy24()
        {
            InitializeComponent();
            _przelewy24Service = new Przelewy24Service();
            _databaseService = new DatabaseService(_connectionString);
        }

        protected override void OnStart(string[] args)
        {
            LogConfig.Configure();
            Log.Information("Service started");

            _timer = new Timer(
                async _ => await TimerTickAsync(),
                null,
                TimeSpan.Zero,
                _interval
            );
        }

        protected override void OnStop()
        {
            LogConfig.Configure();
            Log.Information("Service stopped");
        }

        private async Task TimerTickAsync()
        {
            try
            {
                //var today = DateTime.Today.ToString("yyyyMMdd");
                //var transactions = await _przelewy24Service.GetTransacions(today, today, "transaction");

                var transactions = await _przelewy24Service.GetTransacions("20250501", "20250528", "transaction");
                Log.Information("Poprawnie pobrano transakcje. Pobrano {Count} rekordów", transactions?.Count ?? 0);

                var dispatchTransactionRequest = new List<DispachTransactionRequestDetails>();

                foreach (var transaction in transactions)
                {
                    // Transkacje które nie są jeszcze przeksięgowane i są opłacone
                    if (await _databaseService.IsTransactionTransfered(transaction.Details.OrderId) && transaction.Details.Status == 0)
                        continue;

                    dispatchTransactionRequest.Add(new DispachTransactionRequestDetails
                    {
                        Amount = transaction.Amount,
                        OrderId = transaction.Details.OrderId,
                        SessionId = transaction.Details.SessionId
                    });
                }

                if (dispatchTransactionRequest.Count > 0)
                {
                    var dispatchTransactionResult = await _przelewy24Service.DispatchTransaction(dispatchTransactionRequest);

                    if (dispatchTransactionResult?.Result != null)
                    {
                        foreach (var result in dispatchTransactionResult.Result)
                        {
                            if (!string.IsNullOrEmpty(result.Error))
                            {
                                Log.Error($"Wystąpił błąd przy próbie przeksięgowania płatności '{Math.Round(result.Amount / 100.0, 2)} PLN' " +
                                    $"z zamówienia o ID '{result.OrderId}' na merchanta.{Environment.NewLine}\tError: {result.Error}");

                                await DatabaseLogger.LogTransaction(result.OrderId, result.SessionId, result.Amount, result.Error);
                            }
                            else
                            {
                                Log.Information($"Przeksięgowano płatność '{Math.Round(result.Amount / 100.0, 2)} PLN' " +
                                    $"z zamówienia '{result.OrderId}' na merchanta");

                                await DatabaseLogger.LogTransaction(result.OrderId, result.SessionId, result.Amount);
                            }
                        }
                    }
                    else
                    {
                        Log.Error("Nie udało się przetworzyć transakcji - brak wyników lub błąd w odpowiedzi z API");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in Service");
            }
        }
    }
}
