using System;

namespace GraphAPILibraries
{
    [Serializable]
    public class AccessToken
    {
        public string DisplayName { get; set; }
        public string Spn { get; set; }
        public string EmailDomain { get; set; }
        public string Token { get; set; }
    }

}