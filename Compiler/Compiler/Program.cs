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
			//Console.WriteLine(ulong.TryParse("3",out ulong ___Ulong ));
			new Compiler("string b = 3; ; int a = 5; func(value) = 22"); 
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
			Compile();
		}

		string code;
		List<Piece> compiledCode = new List<Piece>();
		List<Statement> statements = new List<Statement>();

		string currStr = "";
		int openFuncNum = 0;

		public void Compile()
		{
			Debug(code, ConsoleColor.Magenta);
			Debug("");
			for (int i = 0; i < code.Length; i++)
			{
				Debug(currStr + "|| " + i);
				if (i == code.Length - 1 && code[i] != ';' && currStr != "") ThrowException("Expected ';'"); 
				if (code[i] == ';')
				{
					AddStatement();
				}
				// if currentStr is a variableType
				if(code[i] == ' ')
				{
					if (currStr == "") continue;
					if (currStr == ";") AddStatement();
					else
					{
						SmallCompile();
					}
				}
				// if opening func
				if(code[i] == '(')
				{
					if (currStr == "") ThrowException("Unexpected '('");
					openFuncNum++;
					AddPiece(Identifier.funcName);
					continue;
				}
				// if closing func
				if(code[i] == ')')
				{
					if (openFuncNum < 1) ThrowException("Unexpected ')'");
					else
					{
						if (ulong.TryParse(currStr, out ulong numResult))
						{
							AddPiece(Identifier.value);
						}
						else
						{
							AddPiece(Identifier.variableName);
						}
						openFuncNum--;
						continue;
					}
					
				}
				if (code[i] != ' ')
				{
					currStr += code[i];
				}
				Debug(currStr + "|| " + i);
			}
			Print();
		}

		private void Debug<T>(T text, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(text.ToString());
		}

		private void ThrowException(string exception)
		{
			Console.BackgroundColor = ConsoleColor.Red;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Beep();
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
				AddPiece(Identifier.operators);
			}
			// if we found a fresh variable (3 , 'c' , "st") etc..
			if(ulong.TryParse(currStr,out ulong numResult))
			{
				AddPiece(Identifier.value);
			}
			// if we found a variable name
			if(compiledCode.Count > 0 && compiledCode[compiledCode.Count - 1].type == Identifier.variableType)
			{
				AddPiece(Identifier.variableName);
			}
		}

		private void Print()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("===================");
			Console.WriteLine("COMPILED STATEMENTS");
			Console.WriteLine("===================");
			Console.ForegroundColor = ConsoleColor.Green;
			foreach(Statement s in statements)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("Statement Number: " + statements.IndexOf(s));
				Console.ForegroundColor = ConsoleColor.White;
				foreach(Piece p in s.pieces)
				{
					Console.WriteLine(p.name + " : " + p.type);
				}
			}
		}

		private void AddPiece(Identifier identifier)
		{
			if (currStr != "")
			{
				compiledCode.Add(new Piece(currStr, identifier));
				Debug(currStr + " : " + identifier, ConsoleColor.Cyan);
				currStr = "";
			}
		}

		private void AddStatement()
		{
			SmallCompile();
			if (compiledCode.Count > 0)
			{
				Statement newStatement = new Statement(compiledCode);
				Debug("Added Statement (" + compiledCode.Count + ')', ConsoleColor.Magenta);
				compiledCode = new List<Piece>();
				statements.Add(newStatement);
			}
			currStr = "";
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
