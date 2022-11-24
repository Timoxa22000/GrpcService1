using Grpc.Core;
using GrpcService1;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System;
using System.Text.RegularExpressions;

namespace GrpcService1.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        const string FILENAME = "game.json";
        //const string PATHJSONFILE = Path.Combine("C:\\Projects\\GrpcService1", FILENAME);

        private IMemoryCache _cache;
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        const string x = "abc";
        const string y = "123";
        

        public static class CacheKeys
        {
            public static string CurrentUser => "_currentUser";
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            bool currentUser = false;
            if (_cache.TryGetValue(CacheKeys.CurrentUser, out currentUser))
            {
                _cache.Set(CacheKeys.CurrentUser, currentUser);
            }

            var isValid = IsValidMessage(request.Name, currentUser);
            if (!string.IsNullOrEmpty(isValid))
            {
                return Task.FromResult(new HelloReply
                {
                    Message = isValid,
                });
            }

            CreateStep (request.Name, currentUser);

            var table = GetTable();

            ToggleCurrentUser(currentUser);

            return Task.FromResult(new HelloReply
            {
                Message = table,
            });
        }

        private bool CreateStep(string step, bool? user)
        {
            if (Char.IsNumber(step[0]))
            {
                step = step.Reverse().ToString();
            }
            var dataGame = GetDataFromCach();
            if (dataGame != null && dataGame.ContainsKey(step)) return false;

            using (FileStream fs = new FileStream(Path.Combine("C:\\Projects\\GrpcService1", FILENAME), FileMode.OpenOrCreate))
            {
                dataGame.Add(step, user);
                JsonSerializer.Serialize(fs, dataGame);
                return true;
            }
        }
        private Dictionary<string, bool?>? GetDataFromCach()
        {            
            if (!File.Exists(Path.Combine("C:\\Projects\\GrpcService1", FILENAME)))
            {
                using FileStream fst = new(Path.Combine("C:\\Projects\\GrpcService1", FILENAME), FileMode.CreateNew);
                JsonSerializer.Serialize<Dictionary<string, bool?>?>(fst, new Dictionary<string, bool?>());
            }

            using FileStream fs = new(Path.Combine("C:\\Projects\\GrpcService1", FILENAME), FileMode.OpenOrCreate);
            Dictionary<string, bool?>? game = JsonSerializer.Deserialize<Dictionary<string, bool?>?>(fs);
            return game;
        }
        private string GetTable()
        {
            var result = $"\tA\tB\tC\r\n";
            var dataGame = GetDataFromCach();
            
            for (int i = 0; i < x.Length; i++)
            {
                result += i + 1;
                for (int j = 0; j < y.Length; j++)
                {
                    var hod = $"{x[j]}{y[i]}";
                    if (dataGame != null && dataGame.ContainsKey(hod))
                    {
                        result += string.Format("\t{0}", dataGame[hod].Value ? "X" : "0");
                    }
                    else
                    {
                        result += "\t-";
                    }
                }
                result += "\r\n";
            }
            if (IsFinish())
            {
                result += "Игра окончена!";
                File.Delete(Path.Combine(Path.Combine("C:\\Projects\\GrpcService1", FILENAME)));
            }
            return result;
        }
        
        private string IsValidMessage(string hod, bool currentUser)
        {
            Regex regex = new Regex(@"^[A-C1-3]{1}[1-3A-C]{1}", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(hod))
            {
                return "!Неверный формат ввода";
            }
            if (Char.IsNumber(hod[0])) 
            {
                hod = hod.Reverse().ToString();
            }
            var dataGame = GetDataFromCach();            
            if (dataGame != null && dataGame.ContainsKey(hod.ToLower()))
            {
                return "!Такой ход уже сделан";
            }
            return string.Empty;
        }

        private void ToggleCurrentUser(bool currentValue)
        {
            _cache.Set(CacheKeys.CurrentUser, currentValue == false);            
        }
        private bool IsFinish()
        {
            var dataGame = GetDataFromCach();
            for (int i = 0; i < x.Length; i++)
            {
                var line = dataGame.Where(c => c.Value.HasValue && c.Key[0] == x[i] && (c.Value.Value || !c.Value.Value)).Count();
                if (line == 3)
                {
                    return true;
                }
            }
            for (int i = 0; i < y.Length; i++)
            {
                var line = dataGame.Where(c => c.Value.HasValue && c.Key[1] == y[i] && (c.Value.Value || !c.Value.Value)).Count();
                if (line == 3)
                {
                    return true;
                }
            }
            for (int i = 0; i < x.Length; i++)
            {
                var line = dataGame.Where(c => c.Value.HasValue 
                    && (c.Key == "a1"
                    && c.Key == "b2"
                    && c.Key == "c3") 
                    || (c.Key == "a3"
                    && c.Key == "b2"
                    && c.Key == "c1")).Count();
                if (line == 3)
                {
                    return true;
                }
            }
            return false;
        }
    }
}