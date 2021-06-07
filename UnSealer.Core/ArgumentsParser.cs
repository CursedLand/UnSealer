
#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace UnSealer.Core {
	public class ArgumentsParser {

        #region Private Fields.
        private string[] _args;
		private IList<Protection> _protections;
        #endregion

        public ArgumentsParserResult Result {
			get => new() { Path = _args[0], Protections = _protections };
		}
		public ArgumentsParser(IList<Protection> protections, string[] args) {
			if (args.Length < 2) throw new Exception($"Arguments Must Be More Than {args.Length}.");
			_args = args;
			_protections = new List<Protection>();
			for (var i = 1; i < _args.Length; i++) {
				Protection protection = protections.FirstOrDefault(x => x.Id == _args[i].Replace("-", ""));
				if (protection != null)
					_protections.Add(protection);
			}
		}
	}
	public struct ArgumentsParserResult {
		public string Path { get; set; }
		public IList<Protection> Protections { get; set; }
	}
}