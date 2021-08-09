namespace OnlineBanking.DAL
{
    public class AccountViewModel
    {
        public AccountViewModel()
        {
        }

        public AccountViewModel(Accounts account)
        {
            AccountId = account.AccountId;
            Name = account.Name;
            Email = account.Email;
            Password = account.Password;
            Phone = account.Phone;
            Birthday = account.Birthday?.ToString("dd-MM-yyyy");
            Status = account.Status;
            StatusName = ((AccountStatus) account.Status).ToString();
            RoleName = account.Role.Name;
            RoleId = account.RoleId;
            Address = account.Address;
            NumberId = account.NumberID;
            CreatedAt = account.CreatedAt?.ToString("dd-MM-yyyy");
            UpdatedAt = account.UpdatedAt?.ToString("dd-MM-yyyy");
        }

        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public string NumberId { get; set; }
        public int? Status { get; set; }
        public string StatusName { get; set; } // active, delete, lock
        public int? RoleId { get; set; } // Quyền
        public string RoleName { get; set; } // Quyền
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}