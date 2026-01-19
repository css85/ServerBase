using System;
using Shared.CdnStore;
using Shared.Server.Define;
using Shared.ServerApp.Config;

namespace FrontEndWeb.Config
{
    public class FrontEndAppSettings : AppSettings
    {
        public int ExternalWebPort { get; set; }
        public int ExternalFrontendPort { get; set; }        
        public CdnStoreType CdnStoreType { get; set; }
        public string CdnUrl { get; set; }
        public string AwsAccessKey { get; set; }
        public string AwsSecretKey { get; set; }
        public string AwsS3BucketName { get; set; }
        public string AwsS3RegionName { get; set; }
        public string AwsCloudFrontRegionName { get; set; }
        public string AwsCloudFrontDistributionId { get; set; }
        public string IapPackageName { get; set; }
        public string IapGoogleEmailAddress { get; set; }
        public string IapGoogleKeyFile { get; set; }
        public string IOS_PRODUCT_URL { get; set; }
        public string IOS_SENDBOX_URL { get; set; }

        
    }
}
