using NeoNetsphere.Database.Auth;

namespace NeoNetsphere
{
    internal class Account
    {
        public Account(AccountDto dto)
        {
            AccountDto = dto;
            Id = (ulong) dto.Id;
            Username = dto.Username;
            Nickname = dto.Nickname;
            SecurityLevel = (SecurityLevel) dto.SecurityLevel;
        }

        public AccountDto AccountDto { get; }
        public ulong Id { get; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public SecurityLevel SecurityLevel { get; set; }
    }
}
