namespace Przelewy24TransferPayments.Service.Helpers
{
    public static class TransactionHelpers
    {
        public static string GetTransactionLabel(int count, bool dispatched = true)
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
