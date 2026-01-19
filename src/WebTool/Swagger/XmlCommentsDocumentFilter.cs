using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using System;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Reflection;
using Shared.Packet.Utility;

namespace Bluegames.Swagger
{
    public class PacketHeaderActionDescriptor : ActionDescriptor
    {
        public virtual string ActionName { get; set; }
        public string ControllerName { get; set; }
        public PacketHeaderData HeaderData { get; set; }
        public TypeInfo PacketHeaderTypeInfo { get; set; }
        public override string DisplayName { get; set; }
        public MethodInfo MethodInfo { get; set; }
    }

    public class XmlCommentsDocumentFilter : IDocumentFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Collect (unique) controller names and types in a dictionary
            //major -> minors
            //var namesAndTypes = context.ApiDescriptions
            //    .Select(apiDesc=> apiDesc.ActionDescriptor as PacketHeaderActionDescriptor)
            //    .SkipWhile(actionDesc => actionDesc == null)
            //    .GroupBy(actionDesc => actionDesc.DisplayName.Split('.')[0])
            //    .Select(group => new KeyValuePair<string, Type>(group.Key, group.First().PacketHeaderTypeInfo.AsType()));
            //foreach (var nameAndType in namesAndTypes)
            //{
            //    var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(nameAndType.Value);
            //    var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

            //    if (typeNode != null)
            //    {
            //        var summaryNode = typeNode.SelectSingleNode(SummaryTag);
            //        if (summaryNode != null)
            //        {
            //            if (swaggerDoc.Tags == null)
            //                swaggerDoc.Tags = new List<OpenApiTag>();

            //            swaggerDoc.Tags.Add(new OpenApiTag
            //            {
            //                Name = nameAndType.Key,
            //                Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml)
            //            });
            //        }
            //    }
            //}
        }
    }
}
