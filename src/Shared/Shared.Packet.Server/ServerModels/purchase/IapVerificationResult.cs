
namespace Shared.ServerModel
{
    public class IapVerificationResult
    {
        public bool IsSuccess;
        public bool IsRetryable;
        public string TransactionId;
        public string ProductId;

        public IapVerificationResult(bool isSuccess, bool isRetryable, string transactionId, string productId)
        {
            IsSuccess = isSuccess;
            IsRetryable = isRetryable;
            TransactionId = transactionId;
            ProductId = productId;
        }

        public static IapVerificationResult Success(string transactionId, string productId)
        {
            return new IapVerificationResult(true, false, transactionId,productId);
        }

        public static IapVerificationResult Fail(bool isRetryable)
        {
            return new IapVerificationResult(false, isRetryable, "", "");
        }
    }
}
