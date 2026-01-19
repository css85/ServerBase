using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.ServerModel;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Services;

using FrontEndWeb.Config;
using Shared.ServerApp.Services;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Common.Config;
using Shared.Services.Redis;
using SampleGame.Shared.Common;
using Shared.Repository.Services;
using Amazon.S3.Model;
using Shared;
using Microsoft.Toolkit.HighPerformance;
using System.Linq;

namespace FrontEndWeb.Services
{   
    public class IAPService
    {        
        private readonly ILogger<IAPService> _logger;

        private readonly CsvStoreContext _csvContext;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;
        private AndroidPublisherService _androidService;
//        private string _packageName;

        private readonly ChangeableSettings<FrontEndAppSettings> _appSettings;

        public IAPService(
            CsvStoreContext csvContext,            
            RedisRepositoryService redisRepo,
            ILogger<IAPService> logger,
            DatabaseRepositoryService dbRepo,
            ChangeableSettings<FrontEndAppSettings> appSettings
            )
        {
            _logger = logger;
            _csvContext = csvContext;
            _appSettings = appSettings;            
            _dbRepo = dbRepo;
            _redisRepo = redisRepo;
            InitConfig();

        }

        public void InitConfig()
        {

            var certificate = new X509Certificate2(@_appSettings.Value.IapGoogleKeyFile, "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(_appSettings.Value.IapGoogleEmailAddress)
                {
                    Scopes = new[] { AndroidPublisherService.Scope.Androidpublisher }
                }.FromCertificate(certificate));
            _androidService = new AndroidPublisherService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _appSettings.Value.IapPackageName,
                });
        }

        public async Task<IapVerificationResult> VerifyGooglePurchaseAsync(string receipt)
        {
            if( receipt == null )
                return null;
            try
            {
//                _logger.LogInformation($"receipt :{receipt}");
                var receiptInfo = JsonTextSerializer.Deserialize<DataGoogleReceipt>(receipt);

//                _logger.LogInformation($"productId :{receiptInfo.productId}");
//                _logger.LogInformation($"purchaseToken :{receiptInfo.purchaseToken}");

                var request = _androidService.Purchases.Products.Get(_appSettings.Value.IapPackageName, receiptInfo.productId, receiptInfo.purchaseToken);
                var productPurchase = await request.ExecuteAsync();

//                _logger.LogInformation($"ProductPurchase :{productPurchase.PurchaseState}");

                if (productPurchase.PurchaseState != 0)
                    return IapVerificationResult.Fail(false);

 //               _logger.LogInformation($"Expected ProductId :{storeProductPrice.ProductIdAndroid}, Actual: {productPurchase.ProductId}");

                return IapVerificationResult.Success(productPurchase.OrderId, receiptInfo.productId);
            }
            catch(Exception ec)
            {
                _logger.LogWarning(ec.Message);
                return IapVerificationResult.Fail(false);
            }
        }

        private async Task<string> VerifyIOSReceiptInternalAsync(string receiptData, bool isProduction)
        {
            using var httpClient = new HttpClient();
            HttpContent content = new StringContent(receiptData);

            try
            {
                var uri = isProduction ? _appSettings.Value.IOS_PRODUCT_URL : _appSettings.Value.IOS_SENDBOX_URL;

                //if (isProduction)
                //{
                //    _logger.LogInformation("iOS IAP Production Verify: " + receiptData);
                //}
                //else
                //{
                //    _logger.LogInformation("iOS IAP Sandbox Verify: " + receiptData);
                //}

                var response = await httpClient.PostAsync(uri, content);
                var responseJsonString = await response.Content.ReadAsStringAsync();

                //if (isProduction)
                //{
                //    _logger.LogInformation("iOS IAP Production Verify Result: " + responseJsonString);
                //}
                //else
                //{
                //    _logger.LogInformation("iOS IAP Sandbox Verify Result: " + responseJsonString);
                //}

                return responseJsonString;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Error verifying receipt: " + e.Message);
            }

            return null;
        }

        public async Task<IapVerificationResult> VerifyIosPurchaseAsync(string transactionId, string receipt)
        {
            var isRetryable = false;
            try
            {
                var receiptData = new JObject(new JProperty("receipt-data", receipt)).ToString();
                var resultJson = await VerifyIOSReceiptInternalAsync(receiptData, true);
                var result = JObject.Parse(resultJson);

                if (result["status"].ToString() == "21007")
                {
                    resultJson = await VerifyIOSReceiptInternalAsync(receiptData, false);
                    result = JObject.Parse(resultJson);
                }

                if (result.ContainsKey("is-retryable"))
                {
                    isRetryable = bool.Parse(result["is-retryable"].ToString());
                }

                if (result["status"].ToString() != "0")
                    return IapVerificationResult.Fail(isRetryable);


                // product id 검사

                string targetProductId = null;
                var inApps = result["receipt"]["in_app"];
                if (inApps.HasValues)
                {
                    foreach (var inApp in inApps)
                    {
                        if (inApp["transaction_id"].ToString() == transactionId)
                        {
                            targetProductId = inApp["product_id"].ToString();
                            break;
                        }
                    }
                }

                if (targetProductId == null)
                    return IapVerificationResult.Fail(isRetryable);

                return IapVerificationResult.Success(transactionId, targetProductId);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error verifying receipt");
                return IapVerificationResult.Fail(isRetryable);
            }
        }

        
    }
}
