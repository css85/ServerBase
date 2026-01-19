
namespace Shared.ServerModel
{
    public class DataGoogleReceipt
    {
        public string orderId { get; set; }
        public string packageName { get; set; }
        public string productId { get; set; }
        public long purchaseTime { get; set; }
        public int purchaseState { get; set; }
        //	public GoogleDeveloperPayload developerPayload = new GoogleDeveloperPayload();
        public string purchaseToken { get; set; }
    }
}
