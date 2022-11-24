namespace GrpcService1.Model
{
    public class Game
    {
        public Dictionary<string, bool?> GameHistory { get; set; }
        public bool RunStep(string step, bool? user)
        {
            if(GameHistory[step] != null)
            {
                return false;
            }
            GameHistory[step] = user;
            return true;
        }        
    }
}
