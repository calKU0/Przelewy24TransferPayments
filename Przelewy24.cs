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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments
{
    public partial class Przelewy24 : ServiceBase
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ToString();

        private readonly TimeSpan _interval = TimeSpan.FromSeconds(Convert.ToInt32(ConfigurationManager.AppSettings["CheckTransactionInvervalSeconds"]));

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

            Timer timer = new Timer(
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
                string nip;
                var today = DateTime.Today.ToString("yyyyMMdd");
                var transactions = await _przelewy24Service.GetTransacions(today, today, "transaction");
                int count = transactions?.Count ?? 0;

                if (count == 0)
                {
                    Log.Information("Nie znalezniono żadnych transakacji z dnia {Date}.", DateTime.Today.ToString("dd.MM.yyyy"));
                    return;
                }

                Log.Information("Pobrano {Count} {Label} z dnia {Date}.", count, GetTransactionLabel(count), DateTime.Today.ToString("dd.MM.yyyy"));

                var dispatchTransactionRequest = new List<DispachTransactionRequestDetails>();

                foreach (var transaction in transactions)
                {
                    // Transkacje które nie są jeszcze przeksięgowane i są opłacone
                    if (await _databaseService.IsTransactionTransfered(transaction.Details.OrderId) || transaction.Details.Status == 0)
                        continue;

                    string pattern = @"nip:\s*(\d{10})";
                    Match match = Regex.Match(transaction.Details.Description, pattern, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        nip = match.Groups[1].Value;
                    }
                    else
                    {
                        Log.Error("Nie udało się pobrać merchanta po NIP-ie z opisu transakcji.");
                        await DatabaseLogger.LogTransaction(transaction.Details.OrderId, transaction.Details.SessionId, 1, transaction.Details.SettledAmount, "Nie udało się pobrać merchanta po NIP-ie z opisu transakcji.");
                        continue;
                    }

                    var merchant = await _przelewy24Service.GetMerchant(new MerchantExistsRequest
                    {
                        IdentificationType = "nip",
                        IdentificationNumber = nip
                    });

                    dispatchTransactionRequest.Add(new DispachTransactionRequestDetails
                    {
                        Amount = transaction.Amount,
                        OrderId = transaction.Details.OrderId,
                        SessionId = transaction.Details.SessionId,
                        SellerId = Convert.ToInt32(merchant.Data.First())
                    });
                }

                if (dispatchTransactionRequest.Count > 0)
                {
                    Log.Information("Znaleziono {Count} {Label}. Przystępuję do przeksięgowania.", dispatchTransactionRequest.Count, GetTransactionLabel(dispatchTransactionRequest.Count, false));
                    var dispatchTransactionResult = await _przelewy24Service.DispatchTransaction(dispatchTransactionRequest);

                    if (dispatchTransactionResult?.Result != null)
                    {
                        foreach (var result in dispatchTransactionResult.Result)
                        {
                            if (!string.IsNullOrEmpty(result.Error))
                            {
                                Log.Error($"Wystąpił błąd przy próbie przeksięgowania płatności '{Math.Round(result.Amount / 100.0, 2)} PLN' " +
                                    $"z zamówienia o ID '{result.OrderId}' na merchanta o ID '{result.SellerId}'.{Environment.NewLine}\tError: {result.Error}");

                                await DatabaseLogger.LogTransaction(result.OrderId, result.SessionId, result.SellerId, result.Amount, result.Error);
                            }
                            else
                            {
                                Log.Information($"Przeksięgowano płatność '{Math.Round(result.Amount / 100.0, 2)} PLN' " +
                                    $"z zamówienia '{result.OrderId}' na merchanta o ID '{result.SellerId}'");

                                await DatabaseLogger.LogTransaction(result.OrderId, result.SessionId, result.SellerId, result.Amount);
                            }
                        }
                    }
                    else
                    {
                        Log.Error("Nie udało się przetworzyć transakcji - brak wyników lub błąd w odpowiedzi z API");
                    }
                }
                else
                {
                    Log.Information("Nie znaleziono nieprzeksięgowanych płatności.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in Service");
            }
        }
        private string GetTransactionLabel(int count, bool dispatched = true)
        {
            if (dispatched)
            {
                if (count == 1)
                    return "transakcję";
                else if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20))
                    return "transakcje";
                else
                    return "transakcji";
            }
            else
            {
                if (count == 1)
                    return "nierozliczoną transakcję";
                else if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20))
                    return "nierozliczone transakcje";
                else
                    return "nierozliczonych transakcji";
            }
        }
    }
}
