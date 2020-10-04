using MongoDB.Driver;
using QuizAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizAPI.Services
{
    public class QuizService
    {
        private readonly IMongoCollection<Quiz> _quiz;

        public QuizService(IQuizStoreDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _quiz = database.GetCollection<Quiz>(settings.QuizCollectionName);
        }

        public List<Quiz> Get() =>
            _quiz.Find(quiz => true).ToList();

        public Quiz Get(string id) =>
            _quiz.Find<Quiz>(quiz => quiz.Id == id).FirstOrDefault();

        public Quiz Create(Quiz quiz)
        {
            _quiz.InsertOne(quiz);
            return quiz;
        }

        public void Update(string id, Quiz quizIn) =>
            _quiz.ReplaceOne(quiz => quiz.Id == id, quizIn);

        public void Remove(Quiz quizIn) =>
            _quiz.DeleteOne(quiz => quiz.Id == quizIn.Id);

        public void Remove(string id) =>
            _quiz.DeleteOne(quiz => quiz.Id == id);
    }
}
