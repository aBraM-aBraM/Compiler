﻿using System;
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
			new Compiler("int choko;"); 
		}
	}
	class Compiler
	{
		// identifier for each object
		enum Identifier { variableName, funcName, variableType, value, operators , assigner, declareVariable}

		string[] varTypes = new string[]
		{
			"int","string","float","char"
		}; // variable types

		string[] operatorTypes = new string[]
		{
			"+","-","*","/","%","=="
		}; // operator types

		string[] assignments = new string[]
		{
			"=","+=","-=","*=","/="
		};

		List<string> variables = new List<string>();

		// dictionary used to customize the language
		Dictionary<string, char> dict = new Dictionary<string, char>()
		{
			{"openMethod",'(' },
			{"closeMethod",')' },
			{"openDesc",'{' },
			{"closeDec",'}' },
			{"closeStatement",';' },
			{"string",'"' },
			{"char","'"[0] }
		};

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
		// number of open round brackets '('
		int openMethods = 0;
		// number of open curly brackets '{'
		int openDesc = 0;
		// boolean : currentStatement has assignment
		bool hasAssigned = false;
		// boolean : opened a string
		bool openStr = false;
		// boolean : opened a char
		bool openChar = false;

		private Statement[] Compile()
		{
			// Main function : compiles the code
			Debug(code, ConsoleColor.Magenta);
			Debug("");
			// loops through the whole code
			for (int i = 0; i < code.Length; i++)
			{
				Debug(currStr + "|| " + i);
				// if reached the end of the code with objects and without at the end ';' throw exception
				if (i == code.Length - 1 && code[i] != dict["closeStatement"] && currStr != "") ThrowException("Expected '" + dict["closeStatement"] + "'"); 
				// if there's a closingStatementChar reached end of statement : adding new statement
				if (code[i] == dict["closeStatement"])
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
				// if an opening method char has appeared '('
				if(code[i] == dict["openMethod"])
				{
					// if there's no method name throw exception
					if (currStr == "") ThrowException("Unexpected '" + dict["openMethod"] + "'");
					// else increase the number of open method
					openMethods++;
					// if we opened a method the object behind it 
					// must be a method name
					AddPiece(Identifier.funcName);
					// skip to the next index 
					continue;
				}
				// if a closing method char has appeared ')'
				if(code[i] == dict["closeMethod"])
				{
					// if there are no open methods there's no reason
					// to end one : throw exception
					if (openMethods < 1) ThrowException("Unexpected '" + dict["closeMethod"] + "'");
					// ending a method
					else
					{
						// the method's input is always a variable
						// the variable can be either a value or a variableName

						if (ulong.TryParse(currStr, out ulong numResult))
						{
							AddPiece(Identifier.value);
						}
						else if (variables.Contains(currStr))
						{
							AddPiece(Identifier.variableName);
						}
						else ThrowException("Unidentified ", currStr, " in the current scope", 1);
						// decreasing the number of open methods
						openMethods--;
						// skip to the next index
						continue;
					}
					
				}
				// if a string (opener/closer) has appeared
				if (code[i] == dict["string"])
				{
					if (openStr)
					{
						// the currStr is a string 
						AddPiece(Identifier.value);
						openStr = false;
						continue;
					}
					else
					{
						openStr = true;
						continue;
					}
				}
				// if a char (opener/closer) has appeared
				if (code[i] == dict["char"])
				{
					if (openChar)
					{
						if (currStr.Length != 1) ThrowException("Too many characters in character literal");
						else
						{
							// the currStr is a char (character)
							AddPiece(Identifier.value);
							openChar = false;
							continue;
						}
					}
					else
					{
						openChar = true;
						continue;
					}
				}
				// increase the current raw unidentified string
				if (code[i] != ' ' && code[i] != dict["closeStatement"])
				{
					currStr += code[i];
				}
				Debug(currStr + "|| " + i);
			}

			// print the compiled code in an organized manner
			Print();

			return statements.ToArray();
		}

		private void Debug<T>(T text, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			//Console.ForegroundColor = color;
			//Console.WriteLine(text.ToString());
		} // a neat debugging tool

		private void ThrowException(string exception)
		{
			Console.ResetColor();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Beep();
			Console.WriteLine("Error: " + exception);
			Environment.Exit(1);
		} // exception

		private void ThrowException(string first,string second = "",string third = "",int colorIndex = -1)
		{
			ConsoleColor color = ConsoleColor.Cyan;
			Console.ResetColor();
			Console.ForegroundColor = ConsoleColor.Red;
			if (colorIndex == 0) Console.ForegroundColor = color;
			else Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(first);
			if (colorIndex == 1) Console.ForegroundColor = color;
			else Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(second);
			if (colorIndex == 2) Console.ForegroundColor = color;
			else Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(third);

			Environment.Exit(1);
		} // formatted exception

		private void SmallCompile()
		{
			// if we found an existing variable;
			if (variables.Contains(currStr) && compiledCode[compiledCode.Count - 1].type != Identifier.variableType)
			{
				AddPiece(Identifier.variableName);
			}
			else if (assignments.Contains(currStr))
			{
				if (hasAssigned) ThrowException("Statements contain only one assignment");
				else
				{
					hasAssigned = true;
					AddPiece(Identifier.assigner);
				}
			}
			// if we found a varType
			else if (varTypes.Contains(currStr))
			{
				AddPiece(Identifier.variableType);
			}
			// if we found an operator
			else if (operatorTypes.Contains(currStr))
			{
				AddPiece(Identifier.operators);
			}
			// if we found a fresh variable (3 , 'c' , "st") etc..
			else if(ulong.TryParse(currStr,out ulong numResult))
			{
				AddPiece(Identifier.value);
			}
			// if we found a variable name
			else if(compiledCode.Count > 0 && compiledCode[compiledCode.Count - 1].type == Identifier.variableType)
			{
				if (variables.Contains(currStr)) ThrowException("Cant declare ",currStr," as it's declared in another scope",1);
				else
				{
					variables.Add(currStr);
					AddPiece(Identifier.declareVariable);
				}
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
				string formattedStatement = "";
				foreach(Piece p in s.pieces)
				{
					Console.WriteLine(p.name + " : " + p.type);
					formattedStatement += " " + p.name;
				}
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine(formattedStatement);
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
			// check if any object were identified (compiled)
			if (compiledCode.Count > 0)
			{
				bool viableStatement = false;
				foreach(Piece p in compiledCode)
				{
					if (p.type == Identifier.operators || p.type == Identifier.funcName || p.type == Identifier.assigner || p.type == Identifier.declareVariable) viableStatement = true;
				}
				if (!viableStatement) ThrowException("Only assignment, increment, decrement, call, await," +
					"and new object expressions can be used as a statement");
				if (openMethods != 0) ThrowException("Expected '", ")", "'", 1);
				if (openDesc != 0) ThrowException("Expected '", "'", "}", 1);
				if (openStr) ThrowException("Expected '", '"'.ToString(), "'", 1);
				if (openChar) ThrowException("Expected '", "'", "'", 1);

				Statement newStatement = new Statement(compiledCode);
				Debug("Added Statement (" + compiledCode.Count + ')', ConsoleColor.Magenta);
				compiledCode = new List<Piece>();
				statements.Add(newStatement);
			}
			else
			{
				ThrowException("Only assignment, increment, decrement, call, await," +
					"and new object expressions can be used as a statement");
			}
			currStr = "";
			openDesc = 0;
			openMethods = 0;
			hasAssigned = false;
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
