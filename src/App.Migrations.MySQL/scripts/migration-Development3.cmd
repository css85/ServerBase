dotnet-ef migrations list -c ExchangeCtx -- --environment Development3 --index 0;
dotnet-ef migrations list -c GateCtx -- --environment Development3 --index 0;
dotnet-ef migrations list -c AccountCtx -- --environment Development3 --index 0;
dotnet-ef migrations list -c UserCtx -- --environment Development3 --index 0;
dotnet-ef migrations list -c ChatCtx -- --environment Development3 --index 0;
dotnet-ef migrations list -c MailCtx -- --environment Development3 --index 0;
dotnet-ef migrations list -c webtoolCtx -- --environment Development3 --index 0;

dotnet-ef database update app_modify_store_02 -c ExchangeCtx -- --environment Development3 --index 0;
dotnet-ef database update edit_app_group_01 -c GateCtx -- --environment Development3 --index 0;
dotnet-ef database update account_add_user_access_grade -c AccountCtx -- --environment Development3 --index 0;
dotnet-ef database update user_modify_store_07 -c UserCtx -- --environment Development3 --index 0;
dotnet-ef database update chat_add_chat_user_chat_room_type -c ChatCtx -- --environment Development3 --index 0;
dotnet-ef database update add_UserMailIconType -c MailCtx -- --environment Development3 --index 0;
dotnet-ef database update add_user_kick_table -c WebtoolCtx -- --environment Development3 --index 0;

dotnet-ef migrations add rename_tabel_colname_01 -c ExchangeCtx -- --environment Development3 --index 0;
dotnet-ef migrations add rename_tabel_colname_01 -c GateCtx -- --environment Development3 --index 0;
dotnet-ef migrations add rename_tabel_colname_01 -c AccountCtx -- --environment Development3 --index 0;
dotnet-ef migrations add rename_tabel_colname_01 -c UserCtx -- --environment Development3 --index 0;
dotnet-ef migrations add rename_tabel_colname_01 -c ChatCtx -- --environment Development3 --index 0;
dotnet-ef migrations add rename_tabel_colname_01 -c MailCtx -- --environment Development3 --index 0;

dotnet-ef database update -c ExchangeCtx -- --environment Development3 --index 0;
dotnet-ef database update -c GateCtx -- --environment Development3 --index 0;
dotnet-ef database update -c AccountCtx -- --environment Development3 --index 0;
dotnet-ef database update -c UserCtx -- --environment Development3 --index 0;
dotnet-ef database update -c ChatCtx -- --environment Development3 --index 0;
dotnet-ef database update -c MailCtx -- --environment Development3 --index 0;
dotnet-ef database update -c BbsCtx -- --environment Development3 --index 0;
dotnet-ef database update -c WebtoolCtx -- --environment Development3 --index 0;


dotnet-ef database update -c WebtoolCtx -- --environment Development3 --index 0;