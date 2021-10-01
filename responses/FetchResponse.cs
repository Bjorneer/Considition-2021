using System;

namespace DotNet.Responses
{
    public class FetchResponse
    {
        public string GameId { get; set; }
        public string TeamName { get; set; }
        public int Score { get; set; }
        public string Map { get; set; }
        public DateTime CompletedAtTime { get; set; }

        public string Solution { get; set; }
        
        public FetchResponse()
        {
            
        }
        
        public FetchResponse(string gameId, string teamName, int score, string map, DateTime completedAtTime, 
            string solution
        )
        {
            GameId = gameId;
            TeamName = teamName;
            Score = score;
            Map = map;
            CompletedAtTime = completedAtTime;
            Solution = solution;
        }
    }
}
