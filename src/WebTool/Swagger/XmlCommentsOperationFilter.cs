using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Shared.Packet;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Xml.XPath;
using Shared.Packet.Utility;

namespace Bluegames.Swagger
{
    public class XmlCommentsOperationFilter : IOperationFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsOperationFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var headerData = context.ApiDescription.GetProperty<PacketHeaderData>();
            
            var reqType = headerData.DataType;
            var resType = PacketHeaderTable.GetResType(headerData.Header);
            
            ApplyOpertionTags(operation, reqType, resType);
        }

        private void ApplyOpertionTags(OpenApiOperation operation, Type reqType ,Type resType)
        {
            var reqTypeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(reqType);
            var resTypeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(resType);

            var reqTypeNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{reqTypeMemberName}']");
            var resTypeNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{resTypeMemberName}']");

            if (reqTypeNode == null) return;

            var summaryNode = reqTypeNode.SelectSingleNode("summary");
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = reqTypeNode.SelectSingleNode("remarks");
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);

            if (resTypeNode == null) return;
            
            var responseNodes = resTypeNode.Select("response");
            ApplyResponseTags(operation, responseNodes);
        }

        private void ApplyResponseTags(OpenApiOperation operation, XPathNodeIterator responseNodes)
        {
            while (responseNodes.MoveNext())
            {
                var code = responseNodes.Current.GetAttribute("code", "");
                var response = operation.Responses.ContainsKey(code)
                    ? operation.Responses[code]
                    : operation.Responses[code] = new OpenApiResponse();

                response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
            }
        }
    }
}
