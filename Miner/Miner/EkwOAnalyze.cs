using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Net;


namespace Miner
{
    class EkwOAnalyze
    {
        public static bool isKeyWord(string pos, string word)
        {
            switch (pos)
            {
                case "JJ":	//adjective",	//green
                case "JJR":	//adjective, comparative",	//greener
                case "JJS":	//adjective, superlative",	//greenest
                case "NN":	//noun, singular or mass",	//table
                case "NNS":
                    return true;
                case "RB":	//adverb",	//however, usually, naturally, here, good
                case "RBR":	//adverb, comparative",	//better
                case "RBS":	//adverb, superlative",	//best
                    return true;
                case "VB":	//verb be, base form",	//be
                case "VBD":	//verb be, past tense",	//was, were
                case "VBG":	//verb be, gerund/present participle",	//being
                case "VBN":	//verb be, past participle",	//been
                case "VBP":	//verb be, sing. present, non-3d",	//am, are
                case "VBZ":	//verb be, 3rd person sing. present",	//is
                    {
                        word = word.ToLower();
                        if (word == "be" || word == "am" || word == "are" || word == "is" ||
                            word == "was" || word == "were" || word == "been" || word == "being" ||
                            word == "have" || word == "has" || word == "had" || word == "having" ||
                            word == "took" || word == "take" || word == "taken" || word == "taking" ||
                            word == "a" || word == "an" || word == "the" || word == "this" || word == "these"
                            ) return false;
                        if (word.Contains("n't")) return false;
                        if (word.Contains("'ve")) return false;
                        if (word.Contains("'ll")) return false;
                        else return true;
                    }
                default: return false;
            }
        }
        public static bool isEntity(string pos, string word)
        {
            switch (pos)
            {
                case "NNP":
                case "NNPS":
                case "USR":
                    {
                        if (word == "US") return true;
                        if (word == "RT") return true;
                        word = word.ToLower();
                        if (word == "be" || word == "am" || word == "are" || word == "is" ||
                             word == "was" || word == "were" || word == "been" || word == "being" ||
                             word == "have" || word == "has" || word == "had" || word == "having" ||
                             word == "took" || word == "take" || word == "taken" || word == "taking" ||
                             word == "a" || word == "an" || word == "the" || word == "this" || word == "these" ||
                             word == "it" || word == "i'm" || word == "i'd" || word == "us" || word == "that" ||
                             word == "let" || word == "me" || word == "idk"
                             ) return false;
                        if (word.Contains("n't")) return false;
                        if (word.Contains("'ve")) return false;
                        if (word.Contains("'d")) return false;
                        if (word.Contains("'re")) return false;
                        if (word.Contains("'ll")) return false;
                        return true;
                    }
                default:
                    return false;
            }
        }

        public static string[] filters = { "@", "#", "’s", "'s", ".", ":", ";", "[", "]", "(", ")", "{", "}", ",", "?", "\"", "!" };

        public static string keyWordiFy(string word)
        {
            //word = System.Net.WebUtility.HtmlDecode(word);
            //word = word.ToLower().Trim();
            //foreach (string filter in filters) { word = word.Replace(filter, ""); }
            return word;
        }

        public static string EntitiFy(string word)
        {
            //word = System.Net.WebUtility.HtmlDecode(word);
            //word = word.Trim();
            //foreach (string filter in filters) { word = word.Replace(filter, ""); }
            if (word != null && word.Length > 2)
                word = char.ToUpper(word[0]) + word.Substring(1);
            return word;
        }

        public static string CleanWord(string word)
        {
            word = System.Net.WebUtility.HtmlDecode(word);
            word = word.Trim().Trim(",'-".ToCharArray()).Trim();
            foreach (string filter in filters) { word = word.Replace(filter, ""); }
            return word;
        }


        public static bool ContainFilter(string word)
        {
            foreach (string filter in filters)
            {
                if (word.Contains(filter)) return true;
            }
            return false;
        }


        private Dictionary<string, EkwO> EkwDict;
        private Dictionary<string, kwEo> kwEDict;
        private Dictionary<string, kwEo> kwLexicon;
        private Dictionary<string, EEVertex> EkwEgg;

        public EkwOAnalyze()
        {
            EkwDict = new Dictionary<string, EkwO>(StringComparer.InvariantCultureIgnoreCase);
            kwEDict = new Dictionary<string, kwEo>(StringComparer.InvariantCultureIgnoreCase);
            kwLexicon = new Dictionary<string, kwEo>(StringComparer.InvariantCultureIgnoreCase);
        }

        //TakeUp: Tweet, Entity, kw, polarity
        public void TakeUp(string tweetFilePath, string reportFilePath, int dataSize) { TEkwP(tweetFilePath, reportFilePath, dataSize); }
        //Entity,KeyWords, Opiniongram Scores
        public void TEkwP(string tweetFilePath, string reportFilePath, int dataSize)
        {
            DateTime startTime = DateTime.Now;
            NGramClassifier NGC = new NGramClassifier();
            NGC.Load("NGC.csv");

            Classifier POSC = new Classifier();
            POSC.load("sentiobj.csv");

            Program.initPOSIndex();

            StreamWriter fs = new StreamWriter(reportFilePath + ".csv", false);
            StreamWriter xfs = new StreamWriter(reportFilePath + ".xml", false);

            char[] space = { ' ', '\t' };
            char[] underScore = { '_' };

            string[] tData = File.ReadAllLines(tweetFilePath);

            if (tData.Length < dataSize || dataSize == -1) dataSize = tData.Length;
            double totalScore = 0;

            int posCnt = 0, negCnt = 0;

            int i;

            fs.WriteLine("i" + "," + "Tweet" + "," + "E" + "," + "kw" + "," + "Polarity Score" + "," + "marker" + "," + "ngcScore" + "," + "posScore" + "," + "sens");
            xfs.WriteLine("<TEkwPs>");

            Console.WriteLine("TEkwP");
            Console.WriteLine("take up: lyzing tweets");

            for (i = 0; i < dataSize; i++)
            {
                string line = tData[i];
                string E = "";
                string kw = "";
                if (i % 100 == 0) Console.Write("\r" + i);

                //string[] lineParts = line.Split(space, 4);

                string taggedTweet = line;

                string[] tweetParts = taggedTweet.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string tweet = "";
                int ti = 0;
                int prevEntityIndex = -999;
                string prevEntity = "";
                foreach (string twpart in tweetParts)
                {
                    string word = twpart.Split("_".ToCharArray())[0];
                    string pos = twpart.Split("_".ToCharArray())[1];
                    tweet += word + " ";
                    word = CleanWord(word);
                    if (isKeyWord(pos, word))
                    {
                        kw += keyWordiFy(word) + ";";
                    }
                    if (isEntity(pos, word))
                    {
                        string bs = "";
                        if ((prevEntityIndex + 1 == ti) && !ContainFilter(prevEntity)) bs = "<:< ";
                        E += bs + EntitiFy(word) + ";";
                        prevEntityIndex = ti;
                        prevEntity = word;
                    }
                    ti++;
                }
                E = E.Replace(";<:< ", " ");
                taggedTweet = line;

                double[] P = Program.convert(Program.posDist(taggedTweet));

                double ngcScore = NGC.score(tweet);
                double posScore = POSC.BinClassify(P);
                string sens = POSC.Classify(P);

                double pScore = 0.0;
                double NPosScore = (posScore / 2) + 0.20;
                if (NPosScore > +1) NPosScore = +.85;
                if (NPosScore < -1) NPosScore = -.85;

                pScore = NPosScore + ngcScore;

                if (pScore == double.NaN)
                {
                    int q = 0; ;
                }
                string marker = "neutral";
                if (pScore <= -0.45) { marker = "negative"; negCnt++; }
                else if (pScore >= +0.45) { marker = "positive"; posCnt++; }

                totalScore += pScore;

                fs.WriteLine(i + ",\"" + tweet.Replace(",", "_CM_") + "\"," + E + "," + kw + "," + pScore + "," + marker + "," + ngcScore + "," + posScore + "," + sens);
                xfs.WriteLine("<TEkwP i='" + i + "' pScore='" + pScore + "' marker='" + marker + "'>");
                xfs.WriteLine("  <T>" + WebUtility.HtmlEncode(tweet.Replace("_CM_", ",")) + "</T>");
                xfs.WriteLine("  <E>" + WebUtility.HtmlEncode(E.Replace(";", ",")) + "</E>");
                xfs.WriteLine("  <kw>" + WebUtility.HtmlEncode(kw.Replace(";", ",")) + "</kw>");
                xfs.WriteLine("</TEkwP>");
            }

            xfs.WriteLine("</TEkwPs>");
            fs.Close();
            xfs.Close();

            DateTime endTime = DateTime.Now;
            TimeSpan ts = endTime - startTime;

            Console.WriteLine("\r" + dataSize + "\ndone! in " + ts + "\nStored TEkwP file at " + reportFilePath + ".csv \n\tand at " + reportFilePath + ".xml");

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(reportFilePath + ".xml");
            xDoc.Normalize();
            xDoc.Save(reportFilePath + ".xml");
        }

        public void echo(string TEkwPpath, string reportPath = "a") { genEkwBigraph(TEkwPpath, reportPath); }
        public void genEkwBigraph(string TEkwPpath, string reportPath = "a")
        {
            DateTime startTime = DateTime.Now;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(TEkwPpath);
            xDoc.Normalize();
            XmlNode root = xDoc.DocumentElement;

            StreamWriter fse = new StreamWriter(reportPath + ".E.csv", false);
            StreamWriter fsk = new StreamWriter(reportPath + ".kw.csv", false);

            XmlNodeList TEkwPNodeList = root.SelectNodes("//TEkwP");

            Console.WriteLine("E-kw");
            Console.WriteLine("echo: building E-kw bigraph with E-kw matrix and kw-E matrix + kwLexicon");

            int tekwp_i = 0;
            foreach (XmlNode tekwpNode in TEkwPNodeList)
            {
                if ((tekwp_i++) % 100 == 0) Console.Write("\r" + tekwp_i);

                //get E of this TEkwP
                string ENodeInText = tekwpNode.SelectSingleNode("E").InnerText;
                string kwNodeInText = tekwpNode.SelectSingleNode("kw").InnerText;
                string pScoreVal = tekwpNode.Attributes["pScore"].Value;
                double pScore = 0;
                double.TryParse(pScoreVal, out pScore);
                //get kw for this
                string[] kw = kwNodeInText.Split(",".ToCharArray());
                //process each entity
                string[] E = ENodeInText.Split(",".ToCharArray());

                //E-kw matrix
                foreach (string entity in E)
                {
                    if (entity != null && entity != "")
                    {
                        if (!EkwDict.ContainsKey(entity)) EkwDict.Add(entity, new EkwO(entity));
                        EkwDict[entity].Occur++;
                        EkwDict[entity].PScore += pScore;
                        //register kws
                        foreach (string keyw in kw)
                        {
                            if (keyw != null && keyw != "")
                            {
                                EkwDict[entity].AddKW(keyw, pScore);
                            }
                        }
                    }
                }//built E-kw

                //kw-E matrix
                foreach (string keyword in kw)
                {
                    if (keyword != null && keyword != "")
                    {
                        if (!kwEDict.ContainsKey(keyword)) kwEDict.Add(keyword, new kwEo(keyword));
                        kwEDict[keyword].Occur++;
                        kwEDict[keyword].PScore += pScore;
                        //register kws
                        foreach (string en in E)
                        {
                            if (en != null && en != "")
                            {
                                kwEDict[keyword].AddEntity(en, pScore);
                            }
                        }
                    }
                }//built kw-E
            }//E-kw bigraph with E-kw and kw-E
            Console.WriteLine("\r" + tekwp_i);

            //freshen up E-kw to remove lowfreq kw
            int kwCount_uf = 0, kwCount_f = 0;
            foreach (string entity in EkwDict.Keys)
            {
                kwCount_uf += EkwDict[entity].kw.Count;
                EkwDict[entity].freshen(0.35);
                kwCount_f += EkwDict[entity].kw.Count;
            }
            //keyword lexicon
            foreach (KeyValuePair<string, EkwO> ekwo in EkwDict)
            {
                EkwO this_Ekw = ekwo.Value;
                foreach (string kwl in this_Ekw.kw.Keys)
                {
                    if (kwl != null && kwl != "")
                    {
                        if (!kwLexicon.ContainsKey(kwl)) kwLexicon.Add(kwl, new kwEo(kwl));
                        //kwLexicon[kwl].Occur++;
                        //kwLexicon[kwl].PScore += this_Ekw.kw[kwl].PScore * this_Ekw.kw[kwl].Occur;
                        kwLexicon[kwl].AddEntity(this_Ekw.E, this_Ekw.PScore);
                    }
                }
            }
            foreach (string kwl in kwLexicon.Keys)
            {
                int occur = kwLexicon[kwl].Occur = kwEDict[kwl].Occur;
                kwLexicon[kwl].PScore = kwEDict[kwl].PScore/occur;
                if (occur > 1) fsk.WriteLine(kwl + "," + occur + "," + Math.Log(occur) + "," + kwLexicon[kwl].PScore + "," + kwLexicon[kwl].E.Count);
            }
            Console.WriteLine(kwCount_f + " kw of " + kwCount_uf);
            Console.WriteLine(kwLexicon.Count + " kw in lexicon");
            //review
            int i = 0;
            foreach (KeyValuePair<string, EkwO> e_ekwo in EkwDict)
            {
                int occur = e_ekwo.Value.Occur;
                if (occur > 1)
                {
                    fse.WriteLine(e_ekwo.Key + "," + occur + "," + Math.Log(occur) + "," + e_ekwo.Value.PScore + "," + e_ekwo.Value.kw.Count);
                    i++;
                }
            }
            fse.Close();
            fsk.Close();
            Console.WriteLine(i + " of " + EkwDict.Count + " E occured more than once");

            DateTime endTime = DateTime.Now;
            TimeSpan ts = endTime - startTime;

            Console.WriteLine("echo time to calculate: " + ts);
        }

        public void genEEgraph(string reportPath,int kwLimitThreshold=350, int entityOccurThreshold=2, double polarityThresh=0.45, bool PoleInvariant = false)
        {
            if (EkwEgg == null) { EkwEgg = new Dictionary<string, EEVertex>(); }
            DateTime startTime = DateTime.Now;
            foreach(KeyValuePair<string, EkwO> ekwo in EkwDict)
            {
                string E = ekwo.Key;
                EkwO ekwp = ekwo.Value;
                double pScore = ekwp.PScore;

                if (ekwp.Occur <= entityOccurThreshold) continue;

                EkwEgg.Add(E, new EEVertex(E));

                foreach (string keyword in ekwp.kw.Keys)
                {
                    //got the linker
                    if (kwLexicon.ContainsKey(keyword) && kwLexicon[keyword].E.Count<kwLimitThreshold)
                    {
                        Dictionary<string,Meta> PotentialNeighbors = kwLexicon[keyword].E;
                        foreach (string nborE in PotentialNeighbors.Keys)
                        {
                            double nborPScore = EkwDict[nborE].PScore;
                            polarity pole = polarity.opn;
                            if (nborPScore >= polarityThresh && pScore >= polarityThresh) { pole = polarity.pos; }
                            else if (nborPScore <= -polarityThresh && pScore <= -polarityThresh) { pole = polarity.neg; }
                            else if (nborPScore > 0 && pScore > 0)
                            {
                                if (nborPScore + pScore > 2 * polarityThresh) { pole = polarity.pos; }
                                else pole = polarity.obj;
                            } 
                            else if (nborPScore < 0 && pScore < 0)
                            {
                                if (nborPScore + pScore > 2 * polarityThresh) { pole = polarity.neg; }
                                else pole = polarity.obj;
                            }
                            else if (nborPScore == 0 || pScore == 0) { pole = polarity.obj; }

                            if (pole != polarity.opn || PoleInvariant)
                            {
                                EkwEgg[E].AddNeighbor(nborE, keyword, pole);
                            }//added nbor
                        }//finding nbors by kw
                    }//this kw is in lex
                }//iterating over kw
            }//iterating over EkwDict

            DateTime endTime = DateTime.Now;
            TimeSpan ts = endTime - startTime;
            Console.WriteLine("EkwEG time to generate EE graph: " + ts);

            StreamWriter fs = new StreamWriter(reportPath + ".EkwEGg.csv", false);
            foreach (KeyValuePair<string, EEVertex> evert in EkwEgg)
            {
                fs.Write(evert.Key + ","+evert.Value.Neighbors.Count+",");
                foreach (string nbor in evert.Value.Neighbors.Keys)
                {
                    fs.Write(nbor + ";");
                }
                fs.WriteLine();
            }
        }

        public static void RemoveDupLines(string filePath, string filteredFilepath)
        {
            Dictionary<string, int> input = new Dictionary<string, int>();
            string [] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (!input.ContainsKey(line)) input.Add(line, input.Count);
            }
            StreamWriter fs = new StreamWriter(filteredFilepath);

            foreach (string line in input.Keys)
            {
                fs.WriteLine(line);
            }
        }
    }
}
