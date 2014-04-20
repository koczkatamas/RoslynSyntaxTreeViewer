using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynSyntaxTreeViewer
{
    public class AppModel: INotifyPropertyChanged
    {
        public static AppModel Instance { get; protected set; }

        public Node RootNode { get; set; }
        public Node SelectedNode { get; set; }

        public AppModel()
        {
            Instance = this;

            LoadSource(@"
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = File.ReadAllText(@""Program.cs"");
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(data);
        }
    }
}");
        }

        public void LoadSource(string sourceText)
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
            RootNode = new Node(syntaxTree.GetRoot());
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Node: INotifyPropertyChanged
    {
        public SyntaxNode SyntaxNode { get; protected set; }

        public Node(SyntaxNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
        }

        private static string MaxLength(string str, int maxLen)
        {
            return str.Length > maxLen ? str.Substring(0, maxLen) : str;
        }

        private static int FirstNonWsIdx(string str)
        {
            var firstNonWs = 0;
            foreach (var c in str)
                if (c != '\t' && c != ' ')
                    break;
                else
                    firstNonWs++;
            return firstNonWs;
        }

        public string Name { get { return String.Format("[{0}] {1}", SyntaxNode.GetType().Name.Replace("Syntax", ""), Regex.Replace(MaxLength(Text, 200), @"\s+", " ")); } }

        public string Text
        {
            get
            {
                var text = SyntaxNode.ToString();
                var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var linesToCalc = lines.Skip(1).Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
                if (linesToCalc.Length < 2)
                    return text;
                var min = linesToCalc.Select(FirstNonWsIdx).Min();
                return String.Join("\r\n", new[]{ lines[0] }.Concat(lines.Skip(1).Select(x => x.Length > min ? x.Substring(min) : x)));
            }
        }

        public IEnumerable<Node> Children 
        {
            get { return SyntaxNode.ChildNodes().Select(x => new Node(x)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
