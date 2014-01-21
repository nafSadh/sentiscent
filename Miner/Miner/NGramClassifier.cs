using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Miner
{
    class NGramClassifier
    {
        class nGramDist
        {
            public double negCnt;
            public double posCnt;
            public double nutCnt;

            public nGramDist(double negs=1, double poss = 1, double nuts = 1)
            {
                negCnt = negs;
                posCnt = poss;
                nutCnt = nuts;
            }

            public void addCnt(int pole)
            {
                if(pole<0) negCnt++;
                else if(pole>0) posCnt++;
                else if(pole==0) nutCnt++;
            }

            public double total
            {
                get { return negCnt + posCnt + nutCnt; }
            }

            public static double threshold = 0.25;
            public static double biasThres = 0.10;

            public double polarityWt
            {
                get { return (negCnt+posCnt)/(negCnt+posCnt+2*nutCnt); }
            }

            public double nutralityWt
            {
                get { return 1 - polarityWt; }
            }

            public double positiviy
            {
                get { return (posCnt - negCnt) / (posCnt + negCnt); }
            }

            public double objective
            {
                get 
                {
                    return nutralityWt;
                    if (nutralityWt < threshold)
                    {
                        return 0;
                    }
                    else
                    {
                        return nutralityWt;
                    }
                }
            }

            public double bias
            {
                get
                {
                    return positiviy;
                    if (polarityWt < threshold)
                    {
                        return 0;
                    }
                    else 
                    {
                        if (Math.Abs(positiviy) < biasThres) return 0;
                        else return positiviy;
                    }
                }
            }

            public bool isSalient
            {
                get
                {
                    if (Math.Abs(positiviy) > biasThres) return true;
                    if (nutralityWt > 0.75) return true;
                    return false;
                }
            }

        }//end nGramDist

        Dictionary<string, nGramDist> corpus;
        Int64 count;

        public NGramClassifier()
        {
            corpus = new Dictionary<string, nGramDist>();
            count = 0;
        }

        public static string[] stopWords = { "i", "you", "and", "the", "me", "he", "she", "they", ",", "rt" };
        public static string[] NGramify(string line, int n=2)
        {
            List<string> ngrams = new List<string>();

            //clean
            char[] trimchars = {','};
            line = line.ToLower().Trim(trimchars).Trim();
            line = EkwOAnalyze.CleanWord(line);

            //notStopWord
            foreach (string stopword in stopWords)
            {
                line = line.Replace(stopword, "");
            }

            //neg
            line = line.Replace(" not ", "+not not+");
            line = line.Replace(" no ", "+no no+");

            char[] space = { ' ' };
            string[] splits = line.Split(space, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < splits.Length; i++)
            {
                string gram = splits[i - 1] + " " + splits[i];
                gram = gram.Replace("+not not+", " not ");
                gram = gram.Replace("+no no+", " no ");
                gram = gram.Replace("+", " ");
                ngrams.Add(gram);
            }

            //for (int i = 0; i < splits.Length; i++)
            //{
            //    string gram = splits[i];
            //    gram = gram.Replace("+", " ");
            //    ngrams.Add(gram);
            //}

            return ngrams.ToArray();
        }

        public static void printArrayOfStrings(string[] arrayOfStrings)
        {
            foreach (string str in arrayOfStrings)
            {
                Console.WriteLine(str);
            }
        }

        public void recordLine(string line, int pole)
        {
            line = line.Trim("'.".ToCharArray());

            line = line.ToLower();

            string[] ngrams = NGramify(line);

            foreach (string ng in ngrams)
            {
                if (!corpus.ContainsKey(ng))
                {
                    corpus.Add(ng, new nGramDist());
                }
                count++;

                corpus[ng].addCnt(pole);
            }
        }

        public void Store(string filepath)
        {
            StreamWriter fs = new StreamWriter(filepath, false);
            string csvStr = "";

            Int64 avg = count/corpus.Count+2;

            int csvC = 0;

            foreach (var entry in corpus)
            {
                nGramDist ngd = entry.Value;

                if (ngd.isSalient && ngd.total>avg)
                {
                    csvStr = "'"+entry.Key + "',";
                    csvStr += ngd.negCnt + ",";
                    csvStr += ngd.posCnt + ",";
                    csvStr += ngd.nutCnt + ",";
                    csvStr += ngd.bias + ",";
                    csvStr += ngd.objective+",";
                    csvStr += ngd.total;
                    csvC++;
                    fs.WriteLine(csvStr);
                }
            }
            Console.WriteLine(csvC);
            fs.Close();
        }

        public void Load(string filepath)
        {
            string[] entries = File.ReadAllLines(filepath);

            char[] comma = { ',' };
            int i = 0;

            Console.WriteLine("Loading stat...");
            foreach (string entry in entries)
            {
                i++;
                if(i%100==0)Console.Write("\r" + i);

                try
                {
                    string[] val = entry.Split(comma, StringSplitOptions.RemoveEmptyEntries);

                    string key = val[0];
                    key = key.Trim("'".ToCharArray());

                    double negC = double.Parse(val[1]);
                    double posC = double.Parse(val[2]);
                    double nutC = double.Parse(val[3]);

                    corpus.Add(key, new nGramDist(negC, posC, nutC));
                }
                catch (Exception) { ; }
            }
            Console.WriteLine("Loaded.");
        }

        public double score(string line)
        {
            double score = 0.0;

            line = line.ToLower();
            line = line.Replace(",", "");
            line = line.Replace(".", "");
            line = line.Replace("!", "");
            line = line.Replace("?", "");
            line = line.Replace("\"", "");
            line = line.Replace("'", "");
            line = line.Replace("#", "");

            string[] ngrams = NGramify(line);

            double negS = 0.0000001, posS = 0.0000001, objS = 0.0000001;

            foreach (string ng in ngrams)
            {
                if (corpus.ContainsKey(ng))
                {
                    nGramDist ngd = corpus[ng];

                    //if (ngd.isSalient)
                    {
                        if (ngd.objective > 0.5)
                        {
                            objS += ngd.objective;
                        }
                        else
                        {
                            double bias = ngd.bias;

                            if (bias < 0) negS -= bias;
                            else posS += bias;
                        }
                    }
                }
            }
            
            if (objS > negS && objS > posS)
            {
                return 0;
            }

            if (negS > posS)
            {
                return -negS / (negS + posS + objS);
            }
            else if (negS < posS)
            {
                return posS / (negS + posS + objS);
            }

            score = (posS - negS) / (negS + posS + objS);
            return score;

        }

    }
}
