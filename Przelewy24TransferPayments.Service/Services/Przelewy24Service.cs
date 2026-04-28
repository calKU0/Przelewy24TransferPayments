using Przelewy24TransferPayments.Contracts.Clients;
using Przelewy24TransferPayments.Contracts.DTOs;
using Przelewy24TransferPayments.Contracts.Repositories;
using Przelewy24TransferPayments.Service.Helpers;
using System.Text.RegularExpressions;

namespace Przelewy24TransferPayments.Service.Services
{
    public class Przelewy24Service
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IPrzelewy24ApiClient _client;
        private readonly ILogger<Przelewy24Service> _logger;
        public Przelewy24Service(ITransactionRepository transactionRepo, IPrzelewy24ApiClient client, ILogger<Przelewy24Service> logger)
        {
            _transactionRepo = transactionRepo;
            _client = client;
            _logger = logger;
        }

        public async Task TransferTransactions(CancellationToken cancellationToken)
        {
            try
            {
                string nip;
                var dispatchTransactionRequestDetails = new List<DispachTransactionRequestDetails>();
                var today = DateTime.Today.ToString("yyyyMMdd");

                var getTransactionsRequest = new GetTransactionsRequest
                {
                    DateFrom = today,
                    DateTo = today,
                    Type = "transaction"
                };
                var transactions = await _client.GetTransacions(getTransactionsRequest);

                if (transactions.Count == 0)
                {
                    _logger.LogInformation("Not found any transactions from date {Date}.", DateTime.Today.ToString("dd.MM.yyyy"));
                    return;
                }

                _logger.LogInformation("Found {Count} transactions from date {Date}.", transactions.Count, DateTime.Today.ToString("dd.MM.yyyy"));

                foreach (var transaction in transactions)
                {
                    try
                    {
                        // Transkacje które nie są jeszcze przeksięgowane i są opłacone
                        if (await _transactionRepo.IsTransactionTransfered(transaction.Details.OrderId) || transaction.Details.Status == 0)
                            continue;

                        string pattern = @"nip:\s*(\d{10})";
                        Match match = Regex.Match(transaction.Details.Description, pattern, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            nip = match.Groups[1].Value;
                            _logger.LogInformation("Extracted NIP {Nip} from transaction description for order ID {OrderId}.", nip, transaction.Details.OrderId);
                        }
                        else
                        {
                            _logger.LogError("Nie znaleziono NIP'u merchanta w opisie transakcji.");
                            await _transactionRepo.AddTransaction(transaction.Details.OrderId, transaction.Details.SessionId, 1, transaction.Amount, "Nie udało się pobrać merchanta po NIP-ie z opisu transakcji.");
                            continue;
                        }

                        _logger.LogInformation("Checking if merchant with NIP {Nip} exists for order ID {OrderId}.", nip, transaction.Details.OrderId);
                        var metchantExistRequest = new MerchantExistsRequest
                        {
                            IdentificationType = "nip",
                            IdentificationNumber = nip
                        };
                        var merchant = await _client.GetMerchant(metchantExistRequest);
                        if (merchant == null || merchant.Data == null || !merchant.Data.Any())
                        {
                            _logger.LogError("Merchant with NIP {Nip} not found for order ID {OrderId}.", nip, transaction.Details.OrderId);
                            await _transactionRepo.AddTransaction(transaction.Details.OrderId, transaction.Details.SessionId, 1, transaction.Amount, "Merchant not found.");
                            continue;
                        }
                        _logger.LogInformation("Merchant with NIP {Nip} found for order ID {OrderId}. Seller ID: {SellerId}.", nip, transaction.Details.OrderId, merchant.Data.First());

                        dispatchTransactionRequestDetails.Add(new DispachTransactionRequestDetails
                        {
                            Amount = transaction.Amount,
                            OrderId = transaction.Details.OrderId,
                            SessionId = transaction.Details.SessionId,
                            SellerId = Convert.ToInt32(merchant.Data.First())
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing transaction with order ID {OrderId}.", transaction.Details.OrderId);
                        await _transactionRepo.AddTransaction(transaction.Details.OrderId, transaction.Details.SessionId, 1, transaction.Amount, $"Error processing transaction: {ex.Message}");
                    }
                }

                if (dispatchTransactionRequestDetails.Count == 0)
                {
                    _logger.LogInformation("No unprocessed payments found.");
                    return;
                }

                try
                {
                    _logger.LogInformation("Found {Count} {Label}. Proceeding to transfer.", dispatchTransactionRequestDetails.Count, TransactionHelpers.GetTransactionLabel(dispatchTransactionRequestDetails.Count, false));
                    var dispatchTransactionRequest = new DispachTransactionRequest
                    {
                        BatchId = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue),
                        Details = dispatchTransactionRequestDetails
                    };
                    var dispatchTransactionResult = await _client.DispatchTransaction(dispatchTransactionRequest);

                    if (dispatchTransactionResult?.Result != null)
                    {
                        foreach (var result in dispatchTransactionResult.Result)
                        {
                            var amountPln = Math.Round(result.Amount / 100.0, 2);

                            if (!string.IsNullOrEmpty(result.Error))
                            {
                                _logger.LogError(
                                    "Error occurred when transferring {Amount} PLN from order ID {OrderId} to merchant ID {SellerId}. Error: {Error}",
                                    amountPln,
                                    result.OrderId,
                                    result.SellerId,
                                    result.Error
                                );

                                await _transactionRepo.AddTransaction(result.OrderId, result.SessionId, result.SellerId, result.Amount, result.Error);
                            }
                            else
                            {
                                _logger.LogInformation(
                                    "Payment transferred {Amount} PLN from order ID {OrderId} to merchant ID {SellerId}",
                                    amountPln,
                                    result.OrderId,
                                    result.SellerId
                                );

                                await _transactionRepo.AddTransaction(result.OrderId, result.SessionId, result.SellerId, result.Amount);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to process transactions - no results or error in API response");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while dispatching transactions.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while transferring transactions.");
            }
        }
    }
}