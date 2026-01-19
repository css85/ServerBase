using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Shared;
using Shared.Server.Packet.Internal;
using Shared.ServerApp.Connection;
using Shared.TcpNetwork.Transport;
using static Shared.Session.Extensions.ReplyExtensions;
using Shared.ServerApp.Services;
using WebTool.Services;
using Shared.Clock;

namespace WebTool.Controllers
{
    [Route("api/operation/internal-command")]
    [ApiController]
    public class InternalCommandController:ControllerBase
    {
        private readonly AppServerSessionServiceBase _appServerSessionServiceBase;
        private readonly ITcpSetting _tcpSettings;
        private readonly SelectItemService _selectItemService;
        private readonly CsvStoreContext _csvContext;

        public InternalCommandController(
            AppServerSessionServiceBase appServerSessionServiceBase,
            SelectItemService selectItemService,
            CsvStoreContext csvContext,
            TcpConnectionSettingBase tcpSettings
        )
        {
            _tcpSettings = tcpSettings;
            _appServerSessionServiceBase = appServerSessionServiceBase;        
            _selectItemService = selectItemService;
            _csvContext = csvContext;
        }



        [HttpPost("send")]
        public async Task<IActionResult> SendCommandAsync()
        {
            if(Enum.TryParse(Request.Form["serviceType"].ToString(), out NetServiceType serviceType)==false)
                return Ok(new { ret_code = 3 });
            
            var name = Request.Form["commandName"].ToString();
            var messages = Request.Form["messageText"].ToString();
            
            var response 
                = await _appServerSessionServiceBase.SendAsync<InternalSendCommandRes>(
                    serviceType,
                    MakeReqReply(new InternalSendCommandReq
                    {
                        Command = name,
                        Message = messages
                    }));
            
            if (response == null)
                return Ok(new { ret_code = 1 });
            
            if(ResultCode.Success!= response.Result)
                return Ok(new { ret_code = 2 });

            return Ok(new {ret_code =0});
        }

        

        #region Simple Long Switch Function
        private void RefreshCsvDatas(string[] fileNames)
        {
            _selectItemService.Init();
        }


        private Task<int> ValidateCsvFileToUpload(string[] fileNames, string[] dataStrings)
        {
            var csvData = _csvContext.GetData();

            var result = -1;
            for (var i = 0; i < fileNames.Length; i++)
            {
                var isLoaded = false;
                var dataString = dataStrings[i];
                var fileName = fileNames[i];

                if (csvData.ValidateFuncMap.TryGetValue(fileName, out var validateTask))
                {
                    isLoaded = validateTask(dataString);
                }

                if (!isLoaded)
                {
                    result = i;
                    break;
                }
            }

            return Task.FromResult(result);
        }
        #endregion
    }
}