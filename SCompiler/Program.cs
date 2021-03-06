using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SCompiler
{
    /// <summary>
    /// Instituição: IESB
    /// Aluno: Lucas Hipólito 
    /// Matrícula: 1722130054
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
    ///     programa   -> <linha>
    ///     
    ///     linha      -> <comando> | <comando><linha>
    ///     
    ///     comando    -> <N> <palavra>
    ///     
    ///     N          -> 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 
    ///                   0<N> | 1<N> | 2<N> | 3<N> | 4<N> | 5<N> | 6<N> | 7<N> | 8<N> | 9<N>
    ///                     
    ///     palavra    -> rem | input <id> | let <corpoLet> | 
    ///                   print <id> | goto <N> | if <corpoIf> | end
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

        /// <summary>
        /// Controla se existe algum erro
        /// </summary>
        private bool hasError = false;

        private List<int> lineIDs = new List<int>();

        static void Main(string[] args)
        {
            Program compiler = new Program();
            StringBuilder pathBuiler = new StringBuilder();
            foreach(var arg in args)
            {
                pathBuiler.Append(arg);
            }
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            var splitedPath = path.Split("bin");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = Path.Combine(splitedPath[0], $"{pathBuiler}");
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
            this.hasError = false;
            if (qLinha())
            {
                return !hasError;
            }

            return false;
        }

        private bool qLinha()
        {
            this.state = "Linha";

            if (qComando())
            {
                nextToken(state);
                if('\r' == getToken())
                {
                    nextToken(state);
                    if('\n' == getToken())
                    {
                        nextToken(state);
                        if (qLinha())
                        {
                            return true;
                        }
                        return true;
                    }
                    error(state, "quebra de linha");
                    return false;
                }

                if('\n' == getToken())
                {
                    nextToken(state);
                    if (qLinha())
                    {
                        return true;
                    }
                    return true;
                }

                if('$' == getToken())
                {
                    return true;
                }
                error(state, "quebra de linha");
                return false;
            }

            return false;
        }

        private bool qComando()
        {
            this.state = "Comando";
            int count = 0;
            while (count < 3)
            {
                if (qComandoA())
                {
                    break;
                }
                nextToken(state);
                count++;
            }
            if(count == 3)
            {
                hasError = true;
                return false;
            }
            return true;
        }

        private bool qComandoA()
        {
            var ln = qN();
            if (ln != string.Empty)
            {
                this.lineNumber = ln;
                var lineID = Convert.ToInt32(this.lineNumber);
                if (lineIDs.Count == 0)
                {
                    lineIDs.Add(lineID);
                }
                else
                {
                    var last = lineIDs[lineIDs.Count - 1];
                    if(lineID <= last)
                    {
                        errorSemantico("Rótulo de linha menor ou igual a algum anterior", "Os rótulos devem ser crescentes e nunca devem se repetir");
                    }
                    else
                    {
                        lineIDs.Add(lineID);
                    }
                }
                int countA = 0;
                while (countA < 3)
                {
                    if (qComandoB())
                    {
                        break;
                    }
                    nextToken(state);
                    countA++;
                }
                if(countA == 3)
                {
                    hasError = true;
                    return false;
                }
                return true;
            }
            errorN();
            return false;
        }

        private bool qComandoB()
        {
            if (' ' == getToken())
            {
                nextToken(state);
                if (qPalavra())
                {
                    return true;
                }
                hasError = true;
                return false;
            }
            errorEspaco();
            return false;
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
                        int count = 0;
                        while('\r' != getToken())
                        {
                            if (count == 100)
                            {
                                errorSemantico("Nenhum token encontrado depois da palavra-chave 'rem'", "'rem' não pode ser a última palavra-chave do programa. Favor utilziar 'end'");
                                return false;
                            }
                            nextToken(state, false);
                            count++;
                        }
                        
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
                        errorEspaco();
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
                nextToken(state);
                if('r' == getToken())
                {
                    nextToken(state);
                    if('i' == getToken())
                    {
                        nextToken(state);
                        if('n' == getToken())
                        {
                            nextToken(state);
                            if('t' == getToken())
                            {
                                nextToken(state);
                                if(' ' == getToken())
                                {
                                    nextToken(state);
                                    if (qId())
                                    {
                                        return true;
                                    }
                                    errorId();
                                    return false;
                                }
                                errorEspaco();
                                return false;
                            }
                            error(state, 't');
                            return false;
                        }
                        error(state, 'n');
                    }
                    error(state, 'i');
                    return false;
                }
                error(state, 'r');
                return false;
            }

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
                                errorN();
                                return false;
                            }
                            errorEspaco();
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

            if('i' == getToken())
            {
                nextToken(state);

                if('n' == getToken())
                {
                    nextToken(state);
                    if('p' == getToken())
                    {
                        nextToken(state);
                        if('u' == getToken())
                        {
                            nextToken(state);
                            if('t' == getToken())
                            {
                                nextToken(state);
                                if(' ' == getToken())
                                {
                                    nextToken(state);
                                    if (qId())
                                    {
                                        return true;
                                    }
                                    errorId();
                                    return false;
                                }
                                errorEspaco();
                                return false;
                            }
                            error(state, 't');
                            return false;
                        }
                        error(state, 'u');
                        return false;
                    }
                    error(state, 'p');
                    return false;
                }

                if('f' == getToken())
                {
                    nextToken(state);
                    if(' ' == getToken())
                    {
                        nextToken(state);
                        if (qCorpoIf())
                        {
                            return true;
                        }
                        return false;
                    }
                    errorEspaco();
                    return false;
                }

                error(state, 'n', 'f');
                return false;
            }

            if ('e' == getToken())
            {
                nextToken(state);
                if ('n' == getToken())
                {
                    nextToken(state);
                    if ('d' == getToken())
                    {
                        index = tokens.Length - 2;
                        return true;
                    }
                    error(state, 'd');
                    return false;
                }
                error(state, 'n');
                return false;
            }

            error(state, "if", "input", "goto", "rem", "let", "print", "end");
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
                                if(' ' == getToken())
                                {
                                    nextToken(state);
                                    if (qOpr())
                                    {
                                        nextToken(state);
                                        if (' ' == getToken())
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
                                            errorIdN();
                                        }
                                        if (qId())
                                        {
                                            return true;
                                        }
                                        if (qN() != string.Empty)
                                        {
                                            return true;
                                        }
                                        errorIdN();

                                        return false;
                                    }
                                }
                                if (qOpr())
                                {
                                    nextToken(state);
                                    if(' ' == getToken())
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
                                        errorIdN();
                                    }
                                    if (qId())
                                    {
                                        return true;
                                    }
                                    if (qN() != string.Empty)
                                    {
                                        return true;
                                    }
                                    errorIdN();

                                    return false;
                                }

                                errorOpr();
                                return false;
                            }

                            if (qN() != string.Empty)
                            {
                                return true;
                            }
                            errorIdN();

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
                                errorIdN();

                                return false;
                            }
                            errorOpr();

                            return false;
                        }

                        if (qN() != string.Empty)
                        {
                            return true;
                        }

                        errorIdNEspaco();

                        return false;
                    }

                    error(state, '=');
                    return false;
                }

                if('=' == getToken())
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
                            errorIdN();

                            return false;
                        }
                    }

                    if(qN() != string.Empty)
                    {
                        return true;
                    }
                }

                error(state, "espaço em branco", "=");
                return false;
            }

            errorId();

            return false;
        }

        private bool qCorpoIf()
        {
            this.state = "CorpoIf";
            if (qId())
            {
                nextToken(state);
                if(' ' == getToken())
                {
                    nextToken(state);
                    if (qComp())
                    {
                        if (' ' == getToken())
                        {
                            nextToken(state);
                            if (qId())
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
                                                        errorN();
                                                        return false;
                                                    }
                                                    errorEspaco();
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
                            if (qN() != string.Empty)
                            {
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
                                                        errorN();
                                                        return false;
                                                    }
                                                    errorEspaco();
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
                                errorEspaco();
                                return false;
                            }
                        }
                        nextToken(state);
                        if(' ' == getToken())
                        {
                            nextToken(state);
                            if (qId())
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
                                                        errorN();
                                                        return false;
                                                    }
                                                    errorEspaco();
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
                            if (qN() != string.Empty)
                            {
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
                                                        errorN();
                                                        return false;
                                                    }
                                                    errorEspaco();
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
                                errorEspaco();
                                return false;
                            }
                        }
                        if (qId())
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
                                                    errorN();
                                                    return false;
                                                }
                                                errorEspaco();
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
                        if (qN() != string.Empty)
                        {
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
                                                    errorN();
                                                    return false;
                                                }
                                                errorEspaco();
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
                            errorEspaco();
                            return false;
                        }
                        errorIdN();

                        return false;
                    }
                }
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
                                                errorN();
                                                return false;
                                            }
                                            errorEspaco();
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
                                                errorN();
                                                return false;
                                            }
                                            errorEspaco();
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
                        errorEspaco();
                        return false;
                    }
                    errorIdN();

                    return false;
                }
                errorComp();
                return false;
            }
            errorId();

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

            return false;
        }

        private bool qOpr()
        {
            this.state = "Opr";
            if(' ' == getToken())
            {
                nextToken(state);
                if ('+' == getToken()
                || '-' == getToken()
                || '*' == getToken()
                || '/' == getToken()
                || '%' == getToken())
                {
                    return true;
                }
                errorOpr();
                return false;
            }

            if('+' == getToken()
                || '-' == getToken()
                || '*' == getToken()
                || '/' == getToken()
                || '%' == getToken())
            {
                return true;
            }

            errorOpr();
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

            errorComp();
            return false;
        }

        private void errorSemantico(string situacaoEncontrada, string situacaoEsperada)
        {
            hasError = true;
            Console.WriteLine($"Erro estado q{state} Linha {lineNumber}");
            Console.WriteLine($"Comportamento: {situacaoEncontrada}. Esperado: {situacaoEsperada}\n");
        }

        /// <summary>
        /// Apresenta a mensagem de erro
        /// </summary>
        /// <param name="estado">Estado onde o erro ocorreu</param>
        /// <param name="validos">Tokens validos</param>
        private void error(string estado, params char[] validos)
        {
            hasError = true;
            Console.WriteLine($"Erro estado q{estado} Linha {lineNumber}");
            Console.Write("Token encontrado: '" + getToken().ToString() + "' ");
            Console.Write("Esperado: ");


            bool first = true;

            foreach (char valido in validos)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.Write(" ou ");
                }

                Console.Write("'" + valido + "'");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Apresenta a mensagem de erro
        /// </summary>
        /// <param name="estado">Estado onde o erro ocorreu</param>
        /// <param name="validos">Tokens validos</param>
        private void error(string estado, params string[] validos)
        {
            hasError = true;
            Console.WriteLine($"Erro estado q{estado} Linha {lineNumber}");
            Console.Write("Token encontrado: '" + getToken().ToString() + "' ");
            Console.Write("Esperado: ");


            bool first = true;

            foreach (string valido in validos)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.Write(" ou ");
                }

                Console.Write("'" + valido + "'");
            }

            Console.WriteLine("\n");
        }

        private void errorId()
        {
            error(state, "id (letras a-z)");
        }

        private void errorIdN()
        {
            error(state, "id (letras a-z)", "número (0-9)");
        }

        private void errorIdNEspaco()
        {
            error(state, "id (letras a-z)", "número (0-9)", "espaço em branco");
        }

        private void errorOpr()
        {
            error(state, '+', '-', '*', '/', '%');
        }

        private void errorComp()
        {
            error(state, '<', '>', '=', '!');
        }

        private void errorEspaco()
        {
            error(state, "espaço em branco");
        }

        private void errorN()
        {
            error(state, "número (0-9)");
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
        private void nextToken(string estado, bool print = true)
        {
            if (print)
            {
                Console.WriteLine($"Estado atual: q{estado} Token: {this.tokens[index]}");
            }

            index +=  1;

            Console.WriteLine("\n");
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
