using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SCompiler
{
    /// <summary>
    /// Compilador de SIMPLE seguindo os seguintes padrões:
    /// Gramática SIMPLE {N, T, P, S}
    /// N = Símbolos não-terminais = 
    /// {
    ///     palavra, linha, comando,
    ///     N, palavra, corpoLet, corpoIf,
    ///     id, opr, comp
    /// }
    /// 
    /// T = Símbolos terminais = 
    /// {
    ///     rem, input, let, print, goto, if, end, , 
    ///     +, -, /, *, %, 
    ///     =, ==, !=, <, >, <=, >=,
    ///     0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
    ///     a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z
    ///  }
    ///  
    ///  P = Produções = {
    ///     programa   -> <linha>end
    ///     
    ///     linha      -> <comando> | <comando><linha>
    ///     
    ///     comando    -> <N> <palavra>
    ///     
    ///     N          -> 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 
    ///                   0<N> | 1<N> | 2<N> | 3<N> | 4<N> | 5<N> | 6<N> | 7<N> | 8<N> | 9<N>
    ///                     
    ///     palavra    -> rem | input <id> | let <corpoLet> | 
    ///                   print <id> | goto <N> | if <corpoIf>
    ///                     
    ///     corpoLet   -> <id> = <id><opr><id> | <id> = <id><opr><N> | <id> = <N>
    ///     
    ///     corpoIf    -> <id><comp><id> goto <N> | <id><comp><N> goto <N>
    ///     
    ///     id         -> a | b | c | d | e | f | g | h | i | j | k | l | m | n |
    ///                   o | p | q | r | s | t | u | v | w | x | y | z
    ///                   
    ///     opr        -> + | - | * | / | %
    ///     
    ///     comp       -> == | != | < | > | <= | >=
    /// }
    /// 
    /// S = Primeiro símbolo não-terminal esperado = programa
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Token atual
        /// </summary>
        private int index;

        /// <summary>
        /// Lista dos tokens
        /// </summary>
        private string tokens;

        /// <summary>
        /// Controla o número da linha sendo processada
        /// </summary>
        private string lineNumber;

        /// <summary>
        /// Variável que controla o estado da análise
        /// </summary>
        private string state;

        static void Main(string[] args)
        {
            Program compiler = new Program();
            StringBuilder pathBuiler = new StringBuilder();
            foreach(var arg in args)
            {
                pathBuiler.Append(arg);
            }
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            var splitedPath = path.Split("SIMPLE Compiler");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = Path.Combine(splitedPath[0], $"SIMPLE Compiler\\SCompiler\\SCompiler\\{pathBuiler}");
            }
            else
            {
                path = Path.Combine(splitedPath[0], $"SIMPLE Compiler/SCompiler/SCompiler/{pathBuiler}");
            }
            var file = File.ReadAllText(path.ToString());
            if (compiler.parse(file))
            {
                Console.WriteLine("Análise concluída com sucesso!");
            }
            else
            {
                Console.WriteLine("Análise concluída com erro!");
            }
        }

        private bool qPrograma()
        {
            this.state = "Programa";
            return true;
        }

        private bool qPalavra()
        {
            this.state = "Palavra";
            if ('r' == getToken())
            {
                nextToken(state);
                if('e' == getToken())
                {
                    nextToken(state);
                    if('m' == getToken())
                    {
                        return true;
                    }
                    error(state, 'm');
                    return false;
                }
                error(state, 'e');
                return false;
            }

            if('l' == getToken())
            {
                nextToken(state);
                if('e' == getToken())
                {
                    nextToken(state);
                    if('t' == getToken())
                    {
                        nextToken(state);
                        if(' ' == getToken())
                        {
                            nextToken(state);
                            if (qCorpoLet())
                            {
                                return true;
                            }
                            return false;
                        }
                        error(state, "espaço em branco");
                        return false;
                    }
                    error(state, 't');
                    return false;
                }
                error(state, 'e');
                return false;
            }

            if('p' == getToken())
            {

            }

            if('g' == getToken())
            {

            }

            if('i' == getToken())
            {

            }

            error(state, "if", "input", "goto", "rem", "let", "print");
            return false;
        }

        private bool qCorpoLet()
        {
            this.state = "CorpoLet";
            if (qId())
            {
                nextToken(state);
                if(' ' == getToken())
                {
                    nextToken(state);
                    if('=' == getToken())
                    {
                        nextToken(state);
                        if(' ' == getToken())
                        {
                            nextToken(state);
                            if (qId())
                            {
                                nextToken(state);
                                if (qOpr())
                                {
                                    nextToken(state);
                                    if (qId())
                                    {
                                        return true;
                                    }
                                    if (qN() != string.Empty)
                                    {
                                        return true;
                                    }
                                    error(state, 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                                        'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                                        'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

                                    return false;

                                }

                                error(state, '+', '-', '*', '/', '%');
                                return false;
                            }

                            if (qN() != string.Empty)
                            {
                                return true;
                            }
                            error(state, 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                                'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                                'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

                            return false;
                        }

                        if (qId())
                        {
                            nextToken(state);
                            if (qOpr())
                            {
                                nextToken(state);
                                if (qId())
                                {
                                    return true;
                                }
                                if (qN() != string.Empty)
                                {
                                    return true;
                                }
                                error(state, 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                                    'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                                    'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

                                return false;
                            }
                            error(state, '+', '-', '*', '/', '%');

                            return false;
                        }

                        if (qN() != string.Empty)
                        {
                            return true;
                        }

                        error(state, ' ', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                                'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                                'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

                        return false;
                    }

                    error(state, '=');
                    return false;
                }

                if ('=' == getToken())
                {
                    nextToken(state);
                    if (' ' == getToken())
                    {
                        nextToken(state);
                        if (qId())
                        {
                            nextToken(state);
                            if (qOpr())
                            {
                                nextToken(state);
                                if (qId())
                                {
                                    return true;
                                }
                                if (qN() != string.Empty)
                                {
                                    return true;
                                }
                            }
                        }

                        if (qN() != string.Empty)
                        {
                            return true;
                        }
                    }

                    if (qId())
                    {
                        nextToken(state);
                        if (qOpr())
                        {
                            nextToken(state);
                            if (qId())
                            {
                                return true;
                            }
                            if (qN() != string.Empty)
                            {
                                return true;
                            }
                            error(state, 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                                'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                                'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

                            return false;
                        }
                    }

                    if(qN() != string.Empty)
                    {
                        return true;
                    }
                }

                error(state, ' ', '=');
                return false;
            }

            error(state, 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z');

            return false;
        }

        private bool qCorpoIf()
        {
            this.state = "CorpoIf";
            if (qId())
            {
                nextToken(state);
                if (qComp())
                {
                    if (qId())
                    {
                        nextToken(state);
                        if(' ' == getToken())
                        {
                            nextToken(state);
                            if('g' == getToken())
                            {
                                nextToken(state);
                                if('o' == getToken())
                                {
                                    nextToken(state);
                                    if('t' == getToken())
                                    {
                                        nextToken(state);
                                        if('o' == getToken())
                                        {
                                            nextToken(state);
                                            if(' ' == getToken())
                                            {
                                                nextToken(state);
                                                if(qN() != string.Empty)
                                                {
                                                    return true;
                                                }
                                            }
                                            error(state, ' ');
                                            return false;
                                        }
                                        error(state, 'o');
                                        return false;
                                    }
                                    error(state, 't');
                                    return false;
                                }
                                error(state, 'o');
                                return false;
                            }
                            error(state, 'g');
                            return false;
                        }
                        error(state, ' ');
                        return false;
                    }
                    if(qN() != string.Empty)
                    {
                        nextToken(state);
                        if (' ' == getToken())
                        {
                            nextToken(state);
                            if ('g' == getToken())
                            {
                                nextToken(state);
                                if ('o' == getToken())
                                {
                                    nextToken(state);
                                    if ('t' == getToken())
                                    {
                                        nextToken(state);
                                        if ('o' == getToken())
                                        {
                                            nextToken(state);
                                            if (' ' == getToken())
                                            {
                                                nextToken(state);
                                                if (qN() != string.Empty)
                                                {
                                                    return true;
                                                }
                                            }
                                            error(state, ' ');
                                            return false;
                                        }
                                        error(state, 'o');
                                        return false;
                                    }
                                    error(state, 't');
                                    return false;
                                }
                                error(state, 'o');
                                return false;
                            }
                            error(state, 'g');
                            return false;
                        }
                        error(state, ' ');
                        return false;
                    }
                    error(state, 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                        'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                        'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

                    return false;
                }
                error(state, '=', '!', '<', '>');
                return false;
            }
            error(state, 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z');

            return false;
        }

        private string qN()
        {
            this.state = "N";
            if('0' == getToken()
                || '1' == getToken()
                || '2' == getToken()
                || '3' == getToken()
                || '4' == getToken()
                || '5' == getToken()
                || '6' == getToken()
                || '7' == getToken()
                || '8' == getToken()
                || '9' == getToken()
                )
            {
                string current = getToken().ToString();
                nextToken(state);
                return current + qN();
            }
            return string.Empty;
        }

        private bool qId()
        {
            this.state = "Id";
            if('a' == getToken()
                || 'b'== getToken()
                || 'c' == getToken()
                || 'd' == getToken()
                || 'e' == getToken()
                || 'f' == getToken()
                || 'g' == getToken()
                || 'h' == getToken()
                || 'i' == getToken()
                || 'j' == getToken()
                || 'k' == getToken()
                || 'l' == getToken()
                || 'm' == getToken()
                || 'n' == getToken()
                || 'o' == getToken()
                || 'p' == getToken()
                || 'q' == getToken()
                || 'r' == getToken()
                || 's' == getToken()
                || 't' == getToken()
                || 'u' == getToken()
                || 'v' == getToken()
                || 'w' == getToken()
                || 'x' == getToken()
                || 'y' == getToken()
                || 'z' == getToken()
                )
            {
                return true;
            }

            error(state,
                'a', 'b','c','d', 'e','f', 'g','h','i','j','k','l','m','n','o','p',
                'q','r','s', 't', 'u', 'v', 'w', 'x', 'y', 'z');
            return false;
        }

        private bool qOpr()
        {
            this.state = "Opr";
            if('+' == getToken()
                || '-' == getToken()
                || '*' == getToken()
                || '/' == getToken()
                || '%' == getToken())
            {
                return true;
            }

            error(state, '+', '-', '*', '/', '%');
            return false;
        }

        private bool qComp()
        {
            this.state = "Comp";
            if ('=' == getToken())
            {
                nextToken(state);
                if('=' == getToken())
                {
                    return true;
                }
                error(state, '=');
                return false;
            }

            if('!' == getToken())
            {
                nextToken(state);
                if('=' == getToken())
                {
                    return true;
                }
                error(state, '=');
                return false;
            }

            if('<' == getToken())
            {
                nextToken(state);
                if(' ' == getToken() || '=' == getToken())
                {
                    return true;
                }
                error(state, ' ', '=');
                return false;
            }

            if ('>' == getToken())
            {
                nextToken(state);
                if (' ' == getToken() || '=' == getToken())
                {
                    return true;
                }
                error(state, ' ', '=');
                return false;
            }

            error(state, '<', '>', '=', '!');
            return false;
        }

        /// <summary>
        /// Apresenta a mensagem de erro
        /// </summary>
        /// <param name="estado">Estado onde o erro ocorreu</param>
        /// <param name="validos">Tokens validos</param>
        private void error(string estado, params char[] validos)
        {
            Console.WriteLine("q" + estado + ". Linha: " + lineNumber);

            bool first = true;

            foreach (char valido in validos)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.WriteLine(" ou ");
                }

                Console.WriteLine("'" + valido + "'");
            }

            Console.WriteLine(" - '");

            for (int i = index; i < tokens.Length; i++)
            {
                Console.WriteLine(tokens[i]);
            }

            Console.WriteLine("'\n");
        }

        /// <summary>
        /// Apresenta a mensagem de erro
        /// </summary>
        /// <param name="estado">Estado onde o erro ocorreu</param>
        /// <param name="validos">Tokens validos</param>
        private void error(string estado, params string[] validos)
        {
            Console.WriteLine("q" + estado + ". Linha: " + lineNumber);

            bool first = true;

            foreach (string valido in validos)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.WriteLine(" ou ");
                }

                Console.WriteLine("'" + valido + "'");
            }

            Console.WriteLine(" - '");

            for (int i = index; i < tokens.Length; i++)
            {
                Console.WriteLine(tokens[i]);
            }

            Console.WriteLine("'\n");
        }

        /// <summary>
        /// Retorna o token em análise
        /// </summary>
        /// <returns>
        /// token em analise
        /// </returns>
        private char getToken()
        {
            if (index < tokens.Length)
            {
                return tokens[index];
            }

            return '$';
        }

        /// <summary>
        /// Pula para o proximo token
        /// </summary>
        /// <param name="estado">Estado atual</param>
        private void nextToken(string estado)
        {
            Console.WriteLine("q" + estado + " '" + tokens[index] + "' - '");

            index +=  1;

            for (int i = index; i < tokens.Length; i++)
            {
                Console.WriteLine(tokens[i]);
            }

            Console.WriteLine("'\n");
        }

        public bool parse(string source)
        {
            this.tokens = source + '$';

            this.index = 0;

            this.lineNumber = "00";

            return qPrograma() && '$' == getToken();
        }
    }
}
