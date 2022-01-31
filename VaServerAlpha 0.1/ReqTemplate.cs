using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaServerAlpha_0._1
{
    public class ReqTemplate
    {
        public string ReqType { get; set; } = "";
        public string Comand { get; set; } = "";
        public string ReqTarget { get; set; } = "";
        public string Value { get; set; } = "";
        public bool dataIsredy { get; set; } = false;

        private string[] ValuesU = new[] { "ГРОМЧЕ", "ПРИБАВЬ", "ДОБАВЬ", "УВЕЛИЧЬ" };
        private string[] ValuesL = new[] { "ТИШЕ", "УБАВЬ", "УМЕНЬШИ" };

        public byte[] Serialize()
        {
            var serialzied = "ReqType " + ReqType.Replace(" ", string.Empty) + " ReqTarget " + ReqTarget + " Value " + Value;
            return Encoding.UTF8.GetBytes(serialzied);
        }

        public void ParceData(string req)
        {
            var words = req.Split(' ');
            int lenght = words.Length / 2;
            for (int i = 0; i < lenght; i++)
            {
                if (words[i + lenght] != "O")
                    if (words[i + lenght].Contains("B-COM"))
                        Comand = words[i];
                    else if (words[i + lenght].Contains("I-COM"))
                        Comand += words[i];
                    else if (words[i + lenght].Contains("B-TAR"))
                        ReqTarget += words[i];
                    else if (words[i + lenght].Contains("I-TAR"))
                        ReqTarget += words[i];
                    else if (words[i + lenght].Contains("B-VAL"))
                        Value += words[i];
                    else if (words[i + lenght].Contains("I-VAL"))
                        Value += words[i];

            }

            if (ValuesU.Contains(Value))
                Value = "u";
            else if (ValuesL.Contains(Value))
            {
                Value = "l";
            }
            DB.getKeyWords();
            var kb = DB.KB;
            List<KeywordReqType> kwF = new List<KeywordReqType>();
            kwF.AddRange(kb.Where(c => Comand.Contains(c.Word)));
            kwF.AddRange(kb.Where(c => ReqTarget.Contains(c.Word)));
            var result = kwF.GroupBy(v => v.Type)
                .Select(g => new { g.Key, Count = g.Count() })
                .OrderByDescending(e => e.Count)
                .ToList();
            ReqType = result.Max(c => c.Key);
            dataIsredy = true;
        }
    }
}
