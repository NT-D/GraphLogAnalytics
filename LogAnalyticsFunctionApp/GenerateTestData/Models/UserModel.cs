namespace GenerateTestData.Models
{
    public class UserModel
    {
        public string odatacontext { get; set; }
        public Value[] value { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string[] businessPhones { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public object jobTitle { get; set; }
        public string mail { get; set; }
        public object mobilePhone { get; set; }
        public object officeLocation { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
    }

}
