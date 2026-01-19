using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Services;
using Shared.Repository.Services;
using WebTool.Connection.Services;
using WebTool.Base.DataTables;


namespace WebTool.Controllers
{
    [Route("api/mail")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly ILogger<MailController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ServerSessionService _serverSessionService;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly SequenceService _seqService;

        public MailController(
            ILogger<MailController> logger,
            IServiceScopeFactory scopeFactory,
            ServerSessionService serverSessionService,
            DatabaseRepositoryService dbRepo,
            SequenceService seqService
        )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _serverSessionService = serverSessionService;
            _dbRepo = dbRepo;
            _seqService = seqService;
        }
        

        public static DataTablesColumnInfo[] SendMailListInfo =
        {
            new DataTablesColumnInfo("Seq", ""),
            new DataTablesColumnInfo("GmId", ""),
            new DataTablesColumnInfo("유저타입", ""),
            new DataTablesColumnInfo("유저", ""),
            new DataTablesColumnInfo("발송여부", ""),
            new DataTablesColumnInfo("상태", ""),
            new DataTablesColumnInfo("메일타입", ""),
            new DataTablesColumnInfo("메일타입값", ""),
            new DataTablesColumnInfo("제목", ""),
            new DataTablesColumnInfo("내용", ""),
            new DataTablesColumnInfo("보상", ""),
            new DataTablesColumnInfo("예약시간", ""),
            new DataTablesColumnInfo("만료시간", ""),
            new DataTablesColumnInfo("발송시간", ""),
            new DataTablesColumnInfo("생성시간", ""),
            new DataTablesColumnInfo("취소", ""),
        };

    }
}
