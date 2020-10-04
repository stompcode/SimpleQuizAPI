using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizAPI.Models
{
    public class QuizStoreDatabaseSettings : IQuizStoreDatabaseSettings
    {
        public string QuizCollectionName { get; set; }
        public string UserCollectionName { get; set; }
        public string QuestionCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IQuizStoreDatabaseSettings
    {
        string QuizCollectionName { get; set; }
        string UserCollectionName { get; set; }
        string QuestionCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
