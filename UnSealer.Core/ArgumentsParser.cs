
#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace UnSealer.Core
{
    public class ArgumentsParser
    {
        private string[] _args;
        private IList<Protection> _protections;
        public ArgumentsParser(IList<Protection> protections, string[] args)
        {
            if (args.Length < 2)
                throw new Exception($"Arguments Must Be More Than {args.Length}.");
            _args = args;
            _protections = new List<Protection>();
            for (int i = 1; i < _args.Length; i++)
            {
                Protection pwithid = protections.FirstOrDefault(x => x.Id == _args[i].Replace("-", ""));
                if (pwithid != null)
                {
                    _protections.Add(pwithid);
                }
            }
        }
        public ArgumentsParserResult Result => new ArgumentsParserResult()
        {
            Path = _args[0],
            Protections = _protections
        };
    }
    public struct ArgumentsParserResult
    {
        public string Path { set; get; }
        public IList<Protection> Protections { set; get; }
    }
}