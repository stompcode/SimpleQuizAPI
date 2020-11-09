using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuizAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Required]
        [StringLength(15, ErrorMessage = "Length must be more than {1}", MinimumLength = 8)]
        public String Username { get; set; }
        [Required]
        [StringLength(15, ErrorMessage = "Length must be more than {1}", MinimumLength = 5)]
        public String Email { get; set; }
        [Required]
        public String PasswordHash { get; set; }
        public List<String> QuizIdList { get; set; }
        public List<String> FavoritesQuizId { get; set; }
        [DefaultValue(false)]
        public bool Confirmed { get; set; }
        public string ConfirmationCode { get; set; }
    }
}
