using System;
using System.Text.RegularExpressions;
using System.Text;
//using System.Threading.Tasks;
using System.IO;

namespace Ref_To_glob_vars
{
    public struct TFunction
    {
        public string Identifier;
        public string Declaration;
        public string[] GlobalVariables;
        public string[] ReferencesToGlobalVariables;
        public int LengthOfGlobalVariablesArray; // Количество глобальных переменных, доступных функции
        public int LengthOfReferencesToGlobalVariablesArray; // Количество обращений к глобальной переменной в функции
    };

    class metric_reference_to_global_vars
    {
        static void Main(string[] args)
        {
            string code_string;

            code_string = readCodeFromFile();  // Считываем исходный код с файла

            // Удаляем то, что может помешать правильно проанализировать код
            code_string = DeleteElement(code_string, @"\/\/[^\r\n]*");      // Удаляем однострочные комментарии //
            code_string = DeleteElement(code_string, @"#[^\r\n]*");         // Удаляем однострочные комментарии #
            code_string = DeleteElement(code_string, @"\/\*[\s\S]*?\*\/");  // Удаляем многострочные комментарии /* */
            code_string = DeleteElement(code_string, @"(?<!\[)'[^\]][\s\S]*?'"); // Удаляем литералы ' ', при этом оставляем $GLOBALS
            code_string = DeleteElement(code_string, @"""[\s\S]*?""");       // Удаляем литералы " "
                      
            // Заносим идентификаторы и объявления функций в отдельный массив структур
            const int array_length = 30; 
            TFunction[] Function; 
            string pattern_Identifier = @"(?<=[Ff][Uu][Nn][Cc][Tt][Ii][Oo][Nn]\s)[a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*";
            string pattern_Declaration = @"[Ff][Uu][Nn][Cc][Tt][Ii][Oo][Nn]\s*[a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*\s*\(?[^\(\)]*\)?\s*\{[\s\S]*?\}";
            int functions_count = getFunctionElements(
                                    out Function, code_string,
                                    pattern_Identifier, pattern_Declaration,
                                    array_length
                                  );
            
            // Удаляем объявления функций
            code_string = DeleteElement(code_string, pattern_Declaration);

            // Ищем количество глобальных переменных, доступных для функций
            const int max_variable_count = 100;
            int index_GlobalVariables;
            int index_code_string;

            for (int index_Functions = 0; index_Functions < functions_count; index_Functions++)
            {
                index_code_string = 0;
                index_GlobalVariables = 0;
                Function[index_Functions].GlobalVariables = new string[max_variable_count]; 

                // В этом цикле проходим исходный код и ищем переменные до тех пор, пока не наткнемся на вызов функции
                while (index_code_string < code_string.IndexOf(Function[index_Functions].Identifier))
                {
                    if (code_string[index_code_string] == '$')
                    {
                        string VariableIdentifier = FindVariable(code_string, ref index_code_string);

                        // Проверка на то, встречалась ли у нас эта переменная
                        if (isNeedToAdd(Function[index_Functions].GlobalVariables, VariableIdentifier))
                            Function[index_Functions].GlobalVariables[index_GlobalVariables++] = VariableIdentifier;
                    }
                    else
                        index_code_string++;
                }

                // После идентификатора может в скобочках объявляться переменная при вызове функции, поэтому мы учтем это.
                while (code_string[index_code_string] != ';')
                { 
                    if (code_string[index_code_string] == '$')
                    {
                        string VariableIdentifier = FindVariable(code_string, ref index_code_string);

                        if (isNeedToAdd(Function[index_Functions].GlobalVariables, VariableIdentifier))
                            Function[index_Functions].GlobalVariables[index_GlobalVariables++] = VariableIdentifier;
                    }
                    else
                        index_code_string++;
                }
                    
                Function[index_Functions].LengthOfGlobalVariablesArray = index_GlobalVariables;
            }
            
            FeelField_ReferencesToGlobalVars(ref Function, functions_count); // Находим все обращения к глобальным переменным
            
            int possible_references = 0;
            int real_references = 0;
            for (int index_Functions = 0; index_Functions < functions_count; index_Functions++)
            {
                Console.WriteLine("Идентификатор функции: {0}", Function[index_Functions].Identifier);

                Console.WriteLine("Возможные обращения:");
                for (int i = 0; i < Function[index_Functions].LengthOfGlobalVariablesArray; i++)
                    Console.WriteLine(Function[index_Functions].GlobalVariables[i]);

                Console.WriteLine("Действительные обращения:");
                for (int i = 0; i < Function[index_Functions].LengthOfReferencesToGlobalVariablesArray; i++)
                    Console.WriteLine(Function[index_Functions].ReferencesToGlobalVariables[i]);
                possible_references += Function[index_Functions].LengthOfGlobalVariablesArray;
                real_references += Function[index_Functions].LengthOfReferencesToGlobalVariablesArray;
                Console.WriteLine("-------------------------------------------------");
            }

            

            Console.WriteLine("Количество возможных обращений к глобальным переменным = {0}", possible_references);
            Console.WriteLine("Количество действительных обращений к глобальным переменным = {0}", real_references);
            Console.WriteLine("Вероятность допущения ошибки = {0}", (float) real_references / possible_references);
            
            Console.Read();
            
        }

        public static string readCodeFromFile()
        {
            string returned_string;
            string path_to_file = @"C:\Documents and Settings\Admin\Рабочий стол\Metrix\C#_METRIC\Ref_To_glob_vars\Input_File_Code\operations_with_2_numbers.php";

            StreamReader stream_reader = new StreamReader(path_to_file);
            returned_string = stream_reader.ReadToEnd();
            return returned_string;

        }

        public static string DeleteElement(string source_string, string pattern)
        {
            string returned_string;
            
            Regex regular_expression = new Regex(pattern);
            returned_string = regular_expression.Replace(source_string, String.Empty);

            return returned_string;            
        }
        
        // Возвращает количество функций
        public static int getFunctionElements(
            out TFunction[] Function, string source_string,
            string pattern_Identifier, string pattern_Declaration, 
            int array_length)
        {
            Function = new TFunction[array_length];

            int index = 0;
            Regex regular_expression = new Regex(pattern_Identifier);
            Match match = regular_expression.Match(source_string);
            while (match.Success)
            {
                Function[index++].Identifier = match.Groups[0].Value;
                match = match.NextMatch();
            }
            
            index = 0;
            regular_expression = new Regex(pattern_Declaration);
            match = regular_expression.Match(source_string);
            while (match.Success)
            {
                Function[index++].Declaration = match.Groups[0].Value;
                match = match.NextMatch();
            }

            return index;
        }

        public static bool isNotEndOfVariable(char Chr)
        {
            bool Result = false;

            //if ((Chr != ' ') && (Chr != '\t') && (Chr != '\r') && (Chr != '\n') && (Chr != ',') && (Chr != ')') && (Chr != '[') && (Chr != ';'))
            if (( (Chr >= 'A' && Chr <= 'Z') || (Chr >= 'a' && Chr <= 'z') || (Chr >= '0' && Chr <= '9') || (Chr == '_') ))    
                Result = true;

            return Result;
        }

        public static bool isSpaceSymbol(char Chr)
        {
            bool Result = false;

            if (Chr == ' ' || Chr == '\t' || Chr == '\n' || Chr == '\r')
                Result = true;

            return Result;
        }

        public static bool isNeedToAdd(string[] GlobalVariables, String Identifier)
        {
            int i = 0;
            bool Result = true;

            while (GlobalVariables[i] != null && Result)
            {
                if (GlobalVariables[i] == Identifier)
                    Result = false;
                i++;
            }

            return Result;
        }

        public static void FeelField_ReferencesToGlobalVars(ref TFunction[] Function, int functions_count)
        {
            int max_references_count = 100;
            int index_References;

            for (int index_Functions = 0; index_Functions < functions_count; index_Functions++)
            {
                Function[index_Functions].ReferencesToGlobalVariables = new string[max_references_count];
                index_References = 0;
                int index_Declaration = 0;

                while (Function[index_Functions].Declaration[index_Declaration] != '{')
                    index_Declaration++;

                int Length = Function[index_Functions].Declaration.Length;

                // Находим вхождение global в объявлении функции (IndexOf нежадный) 
                int index_global = Function[index_Functions].Declaration.IndexOf(
                                        "global", 
                                        index_Declaration,
                                        Length - index_Declaration - 1,
                                        StringComparison.OrdinalIgnoreCase
                                   );

                while (index_global >= 0)
                {
                    if (Function[index_Functions].Declaration[index_global - 1] == '$') // Если это массив GLOBALS
                    {
                        while (Function[index_Functions].Declaration[index_global] != '\'')
                            index_global++;

                        string VariableIdentifier = FindVariable(Function[index_Functions].Declaration, ref index_global);

                        // Проверка на то, встречалась ли у нас эта переменная
                        if (isNeedToAdd(Function[index_Functions].ReferencesToGlobalVariables, VariableIdentifier))
                            Function[index_Functions].ReferencesToGlobalVariables[index_References++] = VariableIdentifier; 
                    }
                    else // если обращение через ключевое слово global
                    {
                        index_global += 6; // Проходим global от g до первого пробельного символа

                        // После следующего цикла index_global будет указывать на первую переменнную после ключевого слова global (на $)
                        while (isSpaceSymbol(Function[index_Functions].Declaration[index_global]))
                            index_global++;

                        while (Function[index_Functions].Declaration[index_global] != ';')
                        {
                            if (Function[index_Functions].Declaration[index_global] == '$')
                            {
                                string VariableIdentifier = FindVariable(Function[index_Functions].Declaration, ref index_global);

                                // Проверка на то, встречалась ли у нас эта переменная
                                if (isNeedToAdd(Function[index_Functions].ReferencesToGlobalVariables, VariableIdentifier))
                                    Function[index_Functions].ReferencesToGlobalVariables[index_References++] = VariableIdentifier; 
                            }
                            else
                                index_global++;
                        }
                    }

                    index_global = Function[index_Functions].Declaration.IndexOf(
                                        "global",
                                        index_global,
                                        Length - index_global - 1,
                                        StringComparison.OrdinalIgnoreCase
                                   );
                }

                Function[index_Functions].LengthOfReferencesToGlobalVariablesArray = index_References;
            }
        }

        public static String FindVariable(string code_string, ref int index_code_string)
        {
            StringBuilder sb = new System.Text.StringBuilder();
            index_code_string++;

            while (isNotEndOfVariable(code_string[index_code_string]))
            {
                sb.Append(code_string[index_code_string]);
                index_code_string++;
            }

            return sb.ToString();
        }
    }
}

