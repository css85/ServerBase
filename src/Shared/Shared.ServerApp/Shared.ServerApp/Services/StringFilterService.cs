using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Shared.Server.Define;
namespace Shared.ServerApp.Services
{
    public class StringFilterService
    {
        public enum FilterType
        {
            None,

            Name,
            Chat,
            BoardMessage,
            BoardMessageCannotNewLine,
            Title,
            ChatMacro,
        }

        private enum ResultCode
        {
            Success = 0,

            EmptyError,
            CommonError,
            EmojiError,
            SlangError,
            LengthError,
            NewLineError,
        }
        public static bool UseSlangFilter { get; set; } = true;
        const char SlangReplaceSpecialCharacter = '♡';
        //const char SlangReplaceOtherCharacter = 'X';

        // 유효한 값 필터
        //static readonly Regex Regex_String = new Regex(@"[^a-zA-Z0-9가-힣]");
        //static readonly Regex Regex_Number = new Regex(@"[^0-9]");
        //static readonly Regex Regex_English = new Regex(@"[^a-zA-Z]");
        //static readonly Regex Regex_Korean = new Regex(@"[^가-힣]");

        // 유효하지 않은 값 필터
        //public static readonly Regex CheckRegex_ExceptString = new Regex(@"[^ㄱ-ㅎㅏ-ㅣ]");   // 체크
        static readonly Regex Regex_ExceptKorean = new Regex(@"[^ㄱ-ㅎㅏ-ㅣ]");     // 자음, 모음 사용 체크

        //static readonly Dictionary<byte, string> ErrorLanguageKeyGroup = new Dictionary<byte, string>()
        //{
        //    { (byte)ResultCode.CommonError, "공백 및 특수 문자는 입력할 수 없습니다." },        // 언어 키값 넣어야됨
        //    { (byte)ResultCode.EmojiError, "사용할 수 없는 기호, 문자가 있습니다." },
        //    { (byte)ResultCode.SlangError, "공백, 특수 문자, 비속어가 포함되어 있습니다." },
        //};


        private readonly ILogger<StringFilterService> _logger;        
        private readonly CsvStoreContext _csvContext;        

        public StringFilterService(
            ILogger<StringFilterService> logger,
            IServiceProvider serviceProvider,            
            CsvStoreContext csvContext
        ) 
        {
            _logger = logger;            
            _csvContext = csvContext;
        }

        #region Base Check Method
        /// <summary>
        /// 모든 문자가 해당 패턴에 포함하는지 체크
        /// True : 패턴에 해당함, False : 예외문자가 들어감
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool CheckStringPattern(Regex regex, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            // IsMatch True : 패턴 외 다른값이 있는 경우, false : 패턴에 포함됨
            return (regex.IsMatch(text) == false);
        }

        /// <summary>
        /// 패턴에 포함하는 문자가 하나라도 들어갔는지 체크
        /// True : 예외문자가 들어감, False : 예외문자가 안들어감
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool CheckExceptStringPattern(Regex regex, string text)
        {
            var replace = regex.Replace(text, "");
            if (string.IsNullOrWhiteSpace(replace)) return false;

            // IsMatch True : 패턴 외 다른값이 있는 경우, false : 패턴에 포함됨
            return (regex.IsMatch(replace) == false);
        }

        /// <summary>
        /// 공백을 제외한 모든 특수문자들을 체크한다.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool CheckSpecialCharacter(string text, bool isSkipWhiteSpace = true)
        {
            if (string.IsNullOrWhiteSpace(text) == true) return false;

            for (int i = 0; i < text.Length; ++i)
            {
                // 공백은 스페셜문자로 취급 안함
                if (isSkipWhiteSpace && char.IsWhiteSpace(text, i)) continue;

                // 여기서 아래타입들 다 걸러짐(만약을 위해 아래 추가 확인)
                if (char.IsLetterOrDigit(text, i) == false) return true;

                // 위에서 다 걸러지는 타입들이지만 혹시나 들어온값이 아래 타입에 해당한다면...
                if (char.IsControl(text, i)) return true;       // 제어문자 체크
                if (char.IsPunctuation(text, i)) return true;   // 문장 부호 체크
                if (char.IsSeparator(text, i)) return true;     // 구분 문자 체크 (공백 포함)
                if (char.IsSurrogate(text, i)) return true;     // Emoji 같은 서로게이트 코드 체크
                if (char.IsSymbol(text, i)) return true;        // 기호 문자 체크
            }

            return false;
        }

        /// <summary>
        /// Emoji 등 서로게이트 코드 체크
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool CheckEmoji(string text)
        {            
            for (int i = 0; i < text.Length; ++i)
            {
                if (char.IsSurrogate(text[i])) 
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 공백 체크
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool CheckSpace(string text)
        {
            return text.Contains(" ");
        }

      
        #endregion

        /// <summary>
        /// 해당 필터에 유효한지 확인
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="text"></param>
        /// <param name="useErrorMsg"></param>
        /// <returns></returns>
        public bool IsValidString(FilterType filter, string text, bool useErrorMsg = true, bool disableReplaceText = false, bool checkLength = false, int limitLength = 0)
        {
            return IsValidString(filter, text, changeText: out string changeText, useErrorMsg: useErrorMsg, disableReplaceText: disableReplaceText, checkLength: checkLength, limitLength: limitLength);
        }

        /// <summary>
        /// 해당 필터에 유효한지 확인 및 채팅에서 비속어 필터 변환된 값
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool IsValidString(FilterType filter, string text, out string changeText, bool useErrorMsg = true, bool disableReplaceText = false, bool checkLength = false, int limitLength = 0)
        {
            if (string.IsNullOrEmpty(text)) text = "";

            changeText = text;
            //if (string.IsNullOrEmpty(text)) return false;

            ResultCode resultCode = ResultCode.Success;
            switch (filter)
            {
                case FilterType.Name: resultCode = IsValidName(text, out changeText, disableReplaceText); break;
                case FilterType.Chat: resultCode = IsValidChat(text, out changeText, disableReplaceText); break;
                case FilterType.BoardMessage: resultCode = IsValidBoardMessage(text, out changeText, disableReplaceText); break;
                case FilterType.BoardMessageCannotNewLine: resultCode = IsValidBoardMessageCannotNewLine(text, out changeText, disableReplaceText); break;
                case FilterType.Title: resultCode = IsValidTitle(text, out changeText, disableReplaceText); break;
                case FilterType.ChatMacro: resultCode = IsValidChatMacro(text, out changeText, disableReplaceText); break;
            }

            // 길이 체크(바뀌기 전 텍스트 기준으로)
            if (checkLength && resultCode == ResultCode.Success)
            {
                if (text.Length > limitLength) resultCode = ResultCode.LengthError;
            }

            return (resultCode == ResultCode.Success);
        }

        /// <summary>
        /// 문자열이 이름에 유효한지 확인
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private ResultCode IsValidName(string text, out string changeText, bool disableReplaceText)
        {
            changeText = text;
            // 내용 없으면 안됨
            if (string.IsNullOrWhiteSpace(text)) return ResultCode.EmptyError;

            // 공백 체크
            if (CheckSpace(text)) return ResultCode.CommonError;

            // Emoji 체크
            //if (CheckEmoji(text)) return false;

            // 특수문자 체크(Emoji 포함)
            if (CheckSpecialCharacter(text, false)) return ResultCode.CommonError;

            // 한글 자음 or 모음 체크
            if (CheckExceptStringPattern(Regex_ExceptKorean, text)) return ResultCode.CommonError;

        
            return ResultCode.Success;
        }

        /// <summary>
        /// 편지, 게시판 등 제목 체크
        /// </summary>
        /// <param name="text"></param>
        /// <param name="changeText"></param>
        /// <param name="disableReplaceText"></param>
        /// <returns></returns>
        private ResultCode IsValidTitle(string text, out string changeText, bool disableReplaceText)
        {
            changeText = text;
            // 내용 없으면 안됨
            if (string.IsNullOrWhiteSpace(text)) return ResultCode.EmptyError;

            // Emoji 체크
            if (CheckEmoji(text)) return ResultCode.EmojiError;

         
            return ResultCode.Success;

        }

        /// <summary>
        /// 문자열이 채팅에 유효한지 확인
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private ResultCode IsValidChat(string text, out string changeText, bool disableReplaceText)
        {
            changeText = text;
            // 내용 없으면 안됨
            if (string.IsNullOrWhiteSpace(text)) return ResultCode.EmptyError;

            // Emoji 체크
            if (CheckEmoji(text)) return ResultCode.EmojiError;

            return ResultCode.Success;
        }

        /// <summary>
        /// 문자열이 게시판 타입에 유효한지 확인(개행 불가능)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private ResultCode IsValidBoardMessageCannotNewLine(string text, out string changeText, bool disableReplaceText)
        {
            changeText = text;
            if (text.Contains(System.Environment.NewLine)) return ResultCode.NewLineError;

            return IsValidBoardMessage(text, out changeText, disableReplaceText);
        }

        /// <summary>
        /// 문자열이 게시판 타입에 유효한지 확인
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private ResultCode IsValidBoardMessage(string text, out string changeText, bool disableReplaceText)
        {
            changeText = text;
            // 내용 없어도 허용
            if (string.IsNullOrWhiteSpace(text)) return ResultCode.Success;

            // Emoji 체크
            if (CheckEmoji(text)) return ResultCode.EmojiError;

            return ResultCode.Success;
        }

        /// <summary>
        /// 문자열이 채팅 매크로에 유효한지 확인
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private ResultCode IsValidChatMacro(string text, out string changeText, bool disableReplaceText)
        {
            changeText = text;
            // 내용 없으면 안됨
            if (string.IsNullOrWhiteSpace(text)) return ResultCode.EmptyError;

            // Emoji 체크
            if (CheckEmoji(text)) return ResultCode.EmojiError;

            return ResultCode.Success;
        }

      
    }
}