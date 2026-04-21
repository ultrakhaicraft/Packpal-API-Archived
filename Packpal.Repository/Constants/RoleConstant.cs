using Microsoft.Identity.Client;

namespace Packpal.DAL.Constants
{
    public class RoleConstant
    {
        public const string ADMIN = "ADMIN";
        public const string STAFF = "STAFF";
        public const string KEEPER = "KEEPER";
        public const string RENTER = "RENTER";
        public const string ADMIN_STAFF = "ADMIN,STAFF";
        public const string RENTER_KEEPER = "RENTER,KEEPER";
        public const string KEEPER_STAFF = "KEEPER,STAFF";
    }
}
