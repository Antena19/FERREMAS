using Transbank.Common;

namespace Transbank.Common
{
    public class IntegrationType : IIntegrationType
    {
        public static readonly IntegrationType Test = new IntegrationType("TEST", "https://webpay3gint.transbank.cl/");

        public string Value { get; }
        public string Key => Value;
        public string ApiBase { get; }

        private IntegrationType(string value, string apiBase)
        {
            Value = value;
            ApiBase = apiBase;
        }

        public override string ToString() => Value;
    }
} 