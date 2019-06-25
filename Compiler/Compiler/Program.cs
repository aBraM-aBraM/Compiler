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
		// identifier for each object
		enum Identifier { variableName, funcName, variableType, value, operators }

		string[] varTypes = new string[]
		{
			"int","string","float","char"
		}; // variable types

		string[] operatorTypes = new string[]
		{
			"+","-","=","*","/","%","=="
		}; // operator types

		public Compiler(string code)
		{
			this.code = code;
			Compile();
		} // constructor

		//	<code>
		//	a string of the whole code
		//	that will be compiled 
		//	as a raw string 
		//	</code>
		string code;
		// list of identified pieces
		List<Piece> compiledCode = new List<Piece>();
		// list of identified statements (list of arrays of pieces)
		List<Statement> statements = new List<Statement>();

		// the unidentified current string
		string currStr = "";
		// number of open round bracket '('
		int openFuncNum = 0;

		public void Compile()
		{
			// Main function : compiles the code
			Debug(code, ConsoleColor.Magenta);
			Debug("");
			// loops through the whole code
			for (int i = 0; i < code.Length; i++)
			{
				Debug(currStr + "|| " + i);
				// if reached the end of the code with objects and without at the end ';' throw exception
				if (i == code.Length - 1 && code[i] != ';' && currStr != "") ThrowException("Expected ';'"); 
				// if there's a ';' reached end of statement : adding new statement
				if (code[i] == ';')
				{
					AddStatement();
				}
				// main compiling 
				if(code[i] == ' ')
				{
					// skip spaces if there were no objects yet
					if (currStr == "") continue;
					// if the current unidentified string
					// was not empty identify (compile) it
					else
					{
						SmallCompile();
					}
				}
				// if an opening method char appeared '('
				if(code[i] == '(')
				{
					// if there's no method name throw exception
					if (currStr == "") ThrowException("Unexpected '('");
					// else increase the number of open method
					openFuncNum++;
					// if we opened a method the object behind it 
					// must be a method name
					AddPiece(Identifier.funcName);
					// skip to the next index 
					continue;
				}
				// if a closing method char appeared ')'
				if(code[i] == ')')
				{
					// if there are no open methods there's no reason
					// to end one : throw exception
					if (openFuncNum < 1) ThrowException("Unexpected ')'");
					// ending a method
					else
					{
						// the method's input is always a variable
						// the variable can be either a value or a variableName

						if (ulong.TryParse(currStr, out ulong numResult))
						{
							AddPiece(Identifier.value);
						}
						else
						{
							AddPiece(Identifier.variableName);
						}
						// decreasing the number of open methods
						openFuncNum--;
						// skip to the next index
						continue;
					}
					
				}
				// increase the current raw unidentified string
				if (code[i] != ' ')
				{
					currStr += code[i];
				}
				Debug(currStr + "|| " + i);
			}
			// print the compiled code in an organized manner
			Print();
		}

		private void Debug<T>(T text, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(text.ToString());
		} // a neat debugging tool

		private void ThrowException(string exception)
		{
			Console.BackgroundColor = ConsoleColor.Red;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Beep();
			Console.WriteLine("Error: " + exception);
			Environment.Exit(1);
		} // exceptions

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
		} // compiling method

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
		} // print method used after compiling

		private void AddPiece(Identifier identifier)
		{
			if (currStr != "")
			{
				compiledCode.Add(new Piece(currStr, identifier));
				Debug(currStr + " : " + identifier, ConsoleColor.Cyan);
				currStr = "";
			}
		} // adds an identified object to the list of identified objects (pieces)

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
		} // adds an array of identified objects to the list of arrays of identified objects (statements)

		struct Piece
		{
			public readonly string name; // name of the identified object
			public readonly Identifier type; // identifier (variable / method / operator / etc...)

			public Piece(string name, Identifier type)
			{
				this.name = name;
				this.type = type;
			}
		} // identified object
		struct Statement
		{
			public readonly Piece[] pieces;
			public Statement(List<Piece> pieces)
			{
				this.pieces = pieces.ToArray();
			}
		} // array of identified objects 
	}
}
