using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Common.Config;
using FrontEndWeb.Config;
using Microsoft.Extensions.Logging;
using Shared.Server.Extensions;

namespace Shared.CdnStore
{
    public class AwsCdnStoreService : ICdnStoreService
    {
        private readonly ILogger<AwsCdnStoreService> _logger;
        private readonly ChangeableSettings<FrontEndAppSettings> _appSettings;

        private readonly AmazonS3Client _s3Client;
        private readonly AmazonCloudFrontClient _cloudFrontClient;

        public AwsCdnStoreService(
            ILogger<AwsCdnStoreService> logger,
            ChangeableSettings<FrontEndAppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;

            _s3Client = new AmazonS3Client(new BasicAWSCredentials(_appSettings.Value.AwsAccessKey,
                _appSettings.Value.AwsSecretKey), RegionEndpoint.GetBySystemName(_appSettings.Value.AwsS3RegionName));

            _cloudFrontClient = new AmazonCloudFrontClient(_appSettings.Value.AwsAccessKey,
                _appSettings.Value.AwsSecretKey,
                RegionEndpoint.GetBySystemName(_appSettings.Value.AwsCloudFrontRegionName));
        }

        public async Task<string> UploadFileAsync(string path, byte[] bytes)
        {
            try
            {
                path += $"_{Guid.NewGuid()}";

                await using var stream = new MemoryStream(bytes);
                var request = new PutObjectRequest()
                {
                    InputStream = stream,
                    BucketName = _appSettings.Value.AwsS3BucketName,
                    Key = path,
                    CannedACL = S3CannedACL.PublicRead,
                };

                var response = await _s3Client.PutObjectAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning($"AWS S3 upload failed. ({response.HttpStatusCode})");
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "AWS S3 upload failed.");
                return null;
            }

            return $"{_appSettings.Value.CdnUrl}/{path}";
        }

        public async Task RemoveFileAsync(string path)
        {
            try
            {
                var request = new DeleteObjectRequest()
                {
                    BucketName = _appSettings.Value.AwsS3BucketName,
                    Key = path,
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK &&
                    response.HttpStatusCode != HttpStatusCode.NoContent)
                {
                    _logger.LogWarning($"AWS S3 delete failed. ({response.HttpStatusCode})");
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// CloudFront 캐시 무효화
        /// 매달 추가 비용 없이 초기 1,000개의 경로에 대한 무효화 요청을 할 수 있습니다. 이후로 무효화 요청 경로당 0.005 USD가 청구됩니다.
        /// https://aws.amazon.com/ko/cloudfront/pricing/
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<bool> InvalidationPathAsync(string path)
        {
            try
            {
                if (path.StartsWith('/') == false)
                    path = "/" + path;

                var oRequest = new CreateInvalidationRequest
                {
                    DistributionId = _appSettings.Value.AwsCloudFrontDistributionId,
                    InvalidationBatch = new InvalidationBatch
                    {
                        CallerReference = DateTime.Now.Ticks.ToString(),
                        Paths = new Paths
                        {
                            Items = path.Yield().ToList(),
                            Quantity = 1,
                        }
                    }
                };

                var oResponse = await _cloudFrontClient.CreateInvalidationAsync(oRequest);
                if (oResponse.HttpStatusCode != HttpStatusCode.OK &&
                    oResponse.HttpStatusCode != HttpStatusCode.Created)
                {
                    _logger.LogWarning($"AWS CloudFront invalidation failed. ({oResponse.HttpStatusCode})");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "AWS CloudFront invalidation failed.");
                return false;
            }

            return true;
        }
    }
}
