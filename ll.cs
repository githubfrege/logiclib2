// Online C# Editor for free
// Write, Edit and Run your C# code using C# Online Compiler

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace logiclib
{


    public static class TruthTables
    {
        private static List<bool[]> _operandValueCombinations = new List<bool[]>();
        public static void GenerateTruthTable(Statement statement)
        {
            bool[] arr = new bool[statement.Variables.Count];
            List<Dictionary<string, bool>> allCombinations = new List<Dictionary<string, bool>>();
            generateBoolCombinations(statement.Variables.Count, arr, 0);
            foreach (bool[] combo in _operandValueCombinations)
            {
                int idx = 0;
                Dictionary<string, bool> comboDict = new Dictionary<string, bool>();
                foreach (bool b in combo)
                {
                    comboDict.Add(statement.Variables[idx], b);


                    idx++;

                }
                allCombinations.Add(comboDict);
            }
            foreach (var combo in allCombinations)
            {
                foreach (var kvp in combo)
                {
                    Console.Write(kvp);
                }
                Console.Write($"result: {statement.GetValue(combo)}");
                Console.WriteLine("\n");
            }
        }
        public static void addToCombos(bool[] arr, int n)
        {
            bool[] comb = new bool[n];
            for (int i = 0; i < n; i++)
            {
                comb[i] = arr[i];
            }
            _operandValueCombinations.Add(comb);
        }
        private static void generateBoolCombinations(int n,
                                   bool[] arr, int i)
        {
            if (i == n)
            {
                addToCombos(arr,n);
                return;
            }


            arr[i] = false;
            generateBoolCombinations(n, arr, i + 1);


            arr[i] = true;
            generateBoolCombinations(n, arr, i + 1);
        }

    }
    public class Statement
    {
        private static Dictionary<string, int> _precedence = new Dictionary<string, int>() { ["¬"] = 4, ["&"] = 3, ["|"] = 2, ["->"] = 1, ["<->"] = 1 };
        public List<string> Tokens;
        public List<string> RPN;
        public List<string> Variables => getVariables();
        public bool StatementInvalid = false;
        private List<string> getVariables()
        {
            List<string> variables = new List<string>();
            foreach (string token in RPN)
            {
                if (Regex.IsMatch(token, @"^[a-zA-Z]+$") && !variables.Contains(token))
                {
                    variables.Add(token);
                }
            }
            return variables;
        }
        public Statement(string str)
        {
            List<string> tokens = new List<string>();
            foreach (char c in str)
            {
                if (Char.IsLetter(c) || _precedence.ContainsKey(c.ToString()))
                {
                    tokens.Add(c.ToString());
                }
                else
                {
                    switch (c)
                    {
                        case '!':
                            tokens.Add("¬");
                            break;
                        case '>':
                            tokens.Add("->");
                            break;
                        case '=':
                            tokens.Add("<->");
                            break;
                        default:
                            StatementInvalid = true;
                            return;
                    }
                }
            }

            Tokens = tokens;
            RPN = getRPN();
            if (!isValidRPN())
            {
                StatementInvalid = true;
                return;
            }


        }

        public List<string> getRPN() //get reverse polish notation version
        {
            List<string> output = new List<string>();
            Stack<string> operators = new Stack<string>();
            foreach (string token in Tokens)
            {
                if (!_precedence.ContainsKey(token) && token != "(" && token != ")")
                {
                    output.Add(token);
                }
                else
                {
                    if (token == "(")
                    {
                        operators.Push(token);
                    }
                    else if (token == ")")
                    {
                        while (operators.Count > 0)
                        {
                            string stackOperator = operators.Pop();
                            if (stackOperator == "(")
                            {
                                break;
                            }
                            output.Add(stackOperator);
                        }
                    }
                    else
                    {
                        while (operators.Count > 0)
                        {
                            string stackOperator = operators.Peek();
                            if (stackOperator == "(")
                            {
                                break;
                            }
                            if (_precedence[stackOperator] > _precedence[token])
                            {
                                output.Add(operators.Pop());
                            }
                            else
                            {
                                break;
                            }
                        }
                        operators.Push(token);
                    }



                }

            }
            foreach (string op in operators)
            {
                output.Add(op);
            }

            foreach (string s in output)
            {
                Debug.WriteLine(s);
            }
            return output;


        }
        private static bool decrease(ref int counter, int n)
        {
            for (int i = 0; i < n; i++)
            {
                counter--;
                if (counter < 0)
                {
                    return false;
                }
            }
            return true;

        }

        private bool isValidRPN()
        {

            int counter = 0;
            foreach (string token in RPN)
            {
                if (Regex.IsMatch(token, @"^[a-zA-Z]+$"))
                {
                    counter++;
                }
                else if (_precedence.ContainsKey(token) && token != "¬")
                {
                    if (!decrease(ref counter, 2))
                    {
                        return false;
                    }
                    counter++;
                }
                else if (token == "¬")
                {
                    if (!decrease(ref counter, 1))
                    {
                        return false;
                    }
                    counter++;
                }
            }
            return (counter == 1);
        }
        public bool PlayMove(bool trueOrFalse)
        {
            Dictionary<string, bool> myDict = new Dictionary<string, bool>();

            foreach (string variable in Variables)
            {
                Console.WriteLine($"{variable}: t or f?");
                switch (Console.ReadLine())
                {
                    case "t":
                        myDict.Add(variable, true);
                        break;
                    case "f":
                        myDict.Add(variable, false);
                        break;
                }
            }
            if (GetValue(myDict) == trueOrFalse)
            {

                return true;
            }
            return false;
        }
        public bool GetValue(Dictionary<string, bool> variables)
        {

            Stack<bool> boolStack = new Stack<bool>();
            foreach (string token in RPN)
            {
                if (!_precedence.ContainsKey(token))
                {
                    boolStack.Push(variables[token]);
                }
                else
                {

                    try
                    {
                        if (token == "¬")
                        {
                            boolStack.Push(!boolStack.Pop());
                        }
                        else if (token == "|")
                        {
                            bool second = boolStack.Pop();
                            bool first = boolStack.Pop();
                            boolStack.Push(first | second);
                        }
                        else if (token == "&")
                        {
                            bool second = boolStack.Pop();
                            bool first = boolStack.Pop();
                            boolStack.Push(first & second);
                        }
                        else if (token == "->")
                        {
                            bool second = boolStack.Pop();
                            bool first = boolStack.Pop();
                            boolStack.Push(!first | second);
                        }
                        else if (token == "<->")
                        {
                            bool second = boolStack.Pop();
                            bool first = boolStack.Pop();
                            boolStack.Push(first == second);

                        }
                    }
                    catch (InvalidOperationException)
                    {
                        StatementInvalid = true;
                        return false;
                    }

                }
            }
            return boolStack.FirstOrDefault();
            //go through stack
            //return result;

        }


    }



    public class HelloWorld
    {
        public static void Main(string[] args)
        {
            TruthTables.GenerateTruthTable(new Statement("p&q"));
        }
    }
}
