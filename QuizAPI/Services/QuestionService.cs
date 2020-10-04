using MongoDB.Driver;
using QuizAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizAPI.Services
{
    public class QuestionService
    {
        private readonly IMongoCollection<Question> _questions;

        public QuestionService(IQuizStoreDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _questions = database.GetCollection<Question>(settings.QuestionCollectionName);
        }

        public List<Question> Get() =>
            _questions.Find(question => true).ToList();

        public Question Get(string id) =>
            _questions.Find<Question>(question => question.Id == id).FirstOrDefault();

        public Question Create(Question question)
        {
            _questions.InsertOne(question);
            return question;
        }

        public void Update(string id, Question questionIn) =>
            _questions.ReplaceOne(question => question.Id == id, questionIn);

        public void Remove(Question questionIn) =>
            _questions.DeleteOne(question => question.Id == questionIn.Id);

        public void Remove(string id) =>
            _questions.DeleteOne(question => question.Id == id);
    }
}
