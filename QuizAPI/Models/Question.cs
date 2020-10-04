using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuizAPI.Models
{
    public class Question
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Length must be between {2} and {1}", MinimumLength = 3)]
        public String TheQuestion { get; set; }
        [Required]
        public List<String> FalseAnswers { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Length must be between {2} and {1}", MinimumLength = 3)]
        public String CorrectAnswer { get; set; }
        public String QuizId { get; set; }
    }
}
