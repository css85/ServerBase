using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Shared.Packet.Utility;

namespace Bluegames.Swagger
{
    using Swashbuckle.AspNetCore.SwaggerGen;
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorOptions _options;

        public SwaggerGenerator(
            Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorOptions options,
            ISchemaGenerator schemaGenerator)
        {
            _options = options ?? new Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorOptions();
            _schemaGenerator = schemaGenerator;
        }

        public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
                throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select(d => d.Key));

            var descriptionFromHeaderTable = PacketHeaderTable.GetReqTypeAll()
                .OrderBy(type => PacketHeaderTable.ConvertHeaderToInt(PacketHeaderTable.GetHeader(type)))
                .Select(type =>
                {
                    var headerData = PacketHeaderTable.GetHeaderData(type);
                    
                    var fakeApiDesc = ApiDescriptionFactory.CreateFromPacketType(
                        headerData: headerData,
                        groupName: documentName,
                        httpMethod: "POST"
                        );

                    fakeApiDesc.SetProperty(headerData);

                    return fakeApiDesc;
                });

            var schemaRepository = new SchemaRepository();

            var swaggerDoc = new OpenApiDocument
            {
                Info = info,
                Servers = GenerateServers(host, basePath),
                Paths = GeneratePaths(descriptionFromHeaderTable, schemaRepository),
                Components = new OpenApiComponents
                {
                    Schemas = schemaRepository.Schemas,
                    SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(_options.SecuritySchemes)
                },
                SecurityRequirements = new List<OpenApiSecurityRequirement>(_options.SecurityRequirements)
            };

            var filterContext = new DocumentFilterContext(descriptionFromHeaderTable, _schemaGenerator, schemaRepository);
            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private IList<OpenApiServer> GenerateServers(string host, string basePath)
        {
            if (_options.Servers.Any())
            {
                return new List<OpenApiServer>(_options.Servers);
            }

            return (host == null && basePath == null)
                ? new List<OpenApiServer>()
                : new List<OpenApiServer> { new OpenApiServer { Url = $"{host}{basePath}" } };
        }

        private OpenApiPaths GeneratePaths(IEnumerable<ApiDescription> apiDescriptions, SchemaRepository schemaRepository)
        {
            var apiDescriptionsByPath = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.RelativePath);

            var paths = new OpenApiPaths();
            foreach (var group in apiDescriptionsByPath)
            {
                paths.Add($"{group.Key}",
                    new OpenApiPathItem
                    {
                        Operations = GenerateOperations(group, schemaRepository)
                    });
            };

            return paths;
        }

        private IDictionary<OperationType, OpenApiOperation> GenerateOperations(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository)
        {
            var apiDescriptionsByMethod = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.HttpMethod);

            var operations = new Dictionary<OperationType, OpenApiOperation>();

            foreach (var group in apiDescriptionsByMethod)
            {
                var httpMethod = group.Key;

                if (httpMethod == null)
                    throw new SwaggerGeneratorException(string.Format(
                        "Ambiguous HTTP method for action - {0}. " +
                        "Actions require an explicit HttpMethod binding for Swagger/OpenAPI 3.0",
                        group.First().ActionDescriptor.DisplayName));

                if (group.Count() > 1 && _options.ConflictingActionsResolver == null)
                    throw new SwaggerGeneratorException(string.Format(
                        "Conflicting method/path combination \"{0} {1}\" for actions - {2}. " +
                        "Actions require a unique method/path combination for Swagger/OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                        httpMethod,
                        group.First().RelativePath,
                        string.Join(",", group.Select(apiDesc => apiDesc.ActionDescriptor.DisplayName))));

                var apiDescription = (group.Count() > 1) ? _options.ConflictingActionsResolver(group) : group.Single();

                operations.Add(OperationTypeMap[httpMethod.ToUpper()], GenerateOperation(apiDescription, schemaRepository));
            };

            return operations;
        }

        private OpenApiOperation GenerateOperation(ApiDescription apiDescription, SchemaRepository schemaRepository)
        {
            try
            {
                var operation = new OpenApiOperation
                {
                    Tags = GenerateOperationTags(apiDescription),
                    OperationId = _options.OperationIdSelector(apiDescription),
                    Parameters = GenerateParameters(apiDescription, schemaRepository),
                    RequestBody = GenerateRequestBody(apiDescription, schemaRepository),
                    Responses = GenerateResponses(apiDescription, schemaRepository),
                    Deprecated = apiDescription.CustomAttributes().OfType<ObsoleteAttribute>().Any()
                };

                apiDescription.TryGetMethodInfo(out MethodInfo methodInfo);
                var filterContext = new OperationFilterContext(apiDescription, _schemaGenerator, schemaRepository, methodInfo);
                foreach (var filter in _options.OperationFilters)
                {
                    filter.Apply(operation, filterContext);
                }

                return operation;
            }
            catch (Exception ex)
            {
                throw new SwaggerGeneratorException(
                    message: $"Failed to generate Operation for action - {apiDescription.ActionDescriptor.DisplayName}. See inner exception",
                    innerException: ex);
            }
        }

        private OpenApiRequestBody GenerateRequestBody(ApiDescription apiDescription, SchemaRepository schemaRepository)
        {
            return null;
        }

        private IList<OpenApiTag> GenerateOperationTags(ApiDescription apiDescription)
        {
            return _options.TagsSelector(apiDescription)
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }

        private IList<OpenApiParameter> GenerateParameters(ApiDescription apiDescription, SchemaRepository schemaRespository)
        {
            return null;
        }

        private OpenApiResponses GenerateResponses(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            var supportedResponseTypes = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 0 });

            var responses = new OpenApiResponses();
            foreach (var responseType in supportedResponseTypes)
            {
                var statusCode = responseType.StatusCode.ToString();
                responses.Add(statusCode, GenerateResponse(apiDescription, schemaRepository, statusCode, responseType));
            }

            return responses;
        }

        private OpenApiResponse GenerateResponse(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            string statusCode,
            ApiResponseType apiResponseType)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(statusCode, entry.Key))
                .Value;

            return new OpenApiResponse
            {
                Description = description,
            };
        }

        private static readonly Dictionary<string, OperationType> OperationTypeMap = new Dictionary<string, OperationType>
        {
            { "GET", OperationType.Get },
            { "PUT", OperationType.Put },
            { "POST", OperationType.Post },
            { "DELETE", OperationType.Delete },
            { "OPTIONS", OperationType.Options },
            { "HEAD", OperationType.Head },
            { "PATCH", OperationType.Patch },
            { "TRACE", OperationType.Trace },
        };

        private static readonly Dictionary<string, string> ResponseDescriptionMap = new Dictionary<string, string>
        {
            { "0","성공"},
            {  "-1", "잘못된 토큰값"},
            {  "-2", "예전 토큰임"},
            {  "-3", "토큰 만료됨 (7일)"},
            {  "-4", "토큰 인증 안됨"},
            {  "-5", "연결 안됨"},
            {  "-6", "지원하지 않는 패킷"},

            {  "-101", "잘못된 값"},

            { "-2001"," 채널 못찾음"},
            { "-2002"," 채널 이미 입장함"},
            { "-2003"," 채널 유저수 초과"},
            { "-2004"," 채널 룸수 초과"},

            { "-3001","방 못찾음"},
            { "-3002","방 이미 입장함"},
            { "-3003","방 인원수 초과"},
            { "-3004","액세스코드 틀림"},
            { "-3005","방에서 유저 못찾음"},
            { "-3016","액세스코드 불필요"},
            { "-3006","유저 권한 부족"},
            { "-3007","방 관전 불가능"},
            { "-3008","방 관전 권한 안됨"},
            { "-3009","댄서가 아님"},
            { "-3010","옵저버가 아님"},
            { "-3011","댄서슬롯 꽉참"},
            { "-3012","옵저버슬롯 꽉참"},
            { "-3013","마스터는 불가능"},
            { "-3014","자기 자신은 불가능"},
            { "-3015","팀전이 아님"},
            { "-3017","댄서 혼자있을때는 불가능"},
        };
    }
}
