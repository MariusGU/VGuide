using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Entities.Dtos.AccountDtos
{
    public class AccountDtoCreator
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string ppicture { get; set; }
        public string[] followers { get; set; }
        public string[] followed { get; set; }

        public AccountDtoCreator() { }
        public AccountDtoCreator(string ID, string fName, string lNmae, string pPicture, string[] Followers, string[] Followed) 
        {
            _id = ID;
            firstname = fName;
            lastname = lNmae;
            ppicture = pPicture;
            followers = Followers;
            followed = Followed;
        }

        // Maps AccountDtoCreator with Accounts
        public AccountDtoCreator(Accounts account)
        {
            _id = account._id;
            firstname = account.firstname;
            lastname = account.lastname;
            ppicture = account.ppicture;
            followers = account.followers;
            followed = account.followed;
        }
    }
}
