using FlowNetFramework.Commons.Enums;

namespace FlowNetFramework.Commons.Exceptions
{
    public class BTException : Exception
    {
        private static readonly Dictionary<ExceptionType, string> ExceptionMessages = new Dictionary<ExceptionType, string>()
        {
            { ExceptionType.HasNoRecord, "Kayıt bulunamadı." },
            { ExceptionType.UnauthorizedError, "Yetkisiz Giriş" },
            { ExceptionType.Duplicate, "Mükerrer Kayıt" },
            { ExceptionType.HasRelatedEntites, "Bu kayda bağlı başka kayıtlar var." },
            { ExceptionType.UnhandledError, "Sistem Hatası" }
        };

        public ExceptionType ExceptionType { get; set; }

        public BTException() { }
        public BTException(string message) : base(message) { }
        public BTException(ExceptionType type) : base(FormatMessage(type))
        {
            ExceptionType = type;
        }

        /// <summary>
        /// To override default messages by the BT Framework. Use this.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public BTException(ExceptionType type, string message) : base(message)
        {
            ExceptionType = type;
        }

        private static string FormatMessage(ExceptionType type)
        {
            var message = "Error";
            ExceptionMessages.TryGetValue(type, out message);
            return message;
        }

        public BTException(string message, Exception inner) : base(message, inner) { }
    }
}
