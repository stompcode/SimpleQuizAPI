using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuizAPI.Models
{
    public class Quiz
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = "Length must be between {2} and {1}", MinimumLength = 3)]
        public String QuizName { get; set; }
        public List<String> Questions { get; set; }
        public String CreatedById { get; set; }
        public int Favorited { get; set; }
    }
}
