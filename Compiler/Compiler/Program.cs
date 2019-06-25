using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	class Program
	{
		static void Main(string[] args)
		{
			string code = "int a = 5 b = 3 func(xd)";
			new Compiler(code);
		}
	}
	class Compiler
	{
		enum Identifier { variableName, funcName, variableType, value, operators }

		string[] varTypes = new string[]
		{
			"int","string","float","char"
		};

		string[] operatorTypes = new string[]
		{
			"+","-","=","*","/","%","=="
		};

		public Compiler(string code)
		{
			this.code = code;
		}

		string code;
		List<Piece> compiledCode = new List<Piece>();
		List<Statement> statements = new List<Statement>();

		string currStr = "";

		public void Compile()
		{
			int openFuncNum = 0;
			for (int i = 0; i < code.Length; i++)
			{
				// if currentStr is a variableType
				if(code[i] == ' ')
				{
					if (currStr == null) i++;
					else
					{
						SmallCompile();
					}
				}
				if(code[i] == '(')
				{
					openFuncNum++;
					i++;
					AddPiece(Identifier.funcName);
				}
				if(code[i] == ')')
				{
					if (openFuncNum < 1) ThrowException("Unexpected ')'");
					else
					{
						AddPiece(Identifier.variableName);
					}
				}
				currStr += code[i];
			}
		}

		private void ThrowException(string exception)
		{
			Console.BackgroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("Error: " + exception);
			Environment.Exit(1);
		}

		private void SmallCompile()
		{
			// if we found a varType
			if (varTypes.Contains(currStr))
			{
				AddPiece(Identifier.variableType);
			}
			// if we found an operator
			if (operatorTypes.Contains(currStr))
			{
				AddPiece(Identifier.variableType);
			}
		}

		private void Print()
		{
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			foreach(Piece p in compiledCode)
			{
				Console.WriteLine(p.name + " : " + p.type);
			}
		}

		private void AddPiece(Identifier identifier)
		{
			if (currStr != null)
			{
				compiledCode.Add(new Piece(currStr, identifier));
				currStr = "";
			}
		}

		private void AddStatement()
		{
			Statement newStatement = new Statement(compiledCode);
			compiledCode = new List<Piece>();
			statements.Add(newStatement);
		}

		struct Piece
		{
			public readonly string name;
			public readonly Identifier type;

			public Piece(string name, Identifier type)
			{
				this.name = name;
				this.type = type;
			}
		}
		struct Statement
		{
			public readonly Piece[] pieces;
			public Statement(List<Piece> pieces)
			{
				this.pieces = pieces.ToArray();
			}
		}
	}
}
