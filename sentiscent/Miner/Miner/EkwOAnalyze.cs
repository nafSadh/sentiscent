using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Net;
using System.Linq;
using System.Drawing;

namespace Miner
{
    class EkwOAnalyze
    {
        public static bool AllNounsEntity = false;
        public static bool UsersEntity = true;

        public static bool isKeyWord(string pos, string word)
        {
            if (AllNounsEntity)
            {
                if (pos == "NN" || pos == "NNS") return false;
            }
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
            if (AllNounsEntity)
            {
                //if (pos == "NN" || pos == "NNS") return true;
            }

            if (!UsersEntity && pos == "USR") return false;

            switch (pos)
            {
                case "NNP":
                case "NNPS":
                case "USR":
                    {
                        if (word == "US") return true;
                        if (word == "RT") return false;
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

        public static string[] filters = { "@", "#", "’s", "'s", ".", ":", ";", "[", "]", "(", ")", "{", "}", ",", "?", "\"", "!", "+", "+", "=", "<", ">", "*",":(",":)",":","(",")" };

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
        private List<Community> Communities;
        Dictionary<string, int> bfsedE;
        Dictionary<string, int> sigEntities;
        Dictionary<string, int> kwFinal;


        public EkwOAnalyze()
        {
            EkwDict = new Dictionary<string, EkwO>(StringComparer.InvariantCultureIgnoreCase);
            kwEDict = new Dictionary<string, kwEo>(StringComparer.InvariantCultureIgnoreCase);
            kwLexicon = new Dictionary<string, kwEo>(StringComparer.InvariantCultureIgnoreCase);
            Communities = new List<Community>();
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

                string taggedTweet = System.Text.RegularExpressions.Regex.Replace(line, @"[^\u0000-\u007F]", string.Empty); ;

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
                pScore /= 2;
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


            Console.WriteLine("echo time to calculate: " + (TimeSpan)(DateTime.Now-startTime));
            Console.WriteLine("storing in files");
            fsk.WriteLine("keyword"+ "," + "occur" + "," + "ln(occur)" + "," + "PScore" + "," + "E count" + "described entities");
            foreach (string kwl in kwLexicon.Keys)
            {
                int occur = kwLexicon[kwl].Occur = kwEDict[kwl].Occur;
                kwLexicon[kwl].PScore = kwEDict[kwl].PScore/occur;
                if (occur > 1)
                {
                    /*string all_E = "";
                    foreach (string an_E in kwLexicon[kwl].E.Keys) all_E += an_E.Replace(",", "") + ";";*/
                    fsk.WriteLine(kwl + "," + occur + "," + Math.Log(occur) + "," + kwLexicon[kwl].PScore + "," + kwLexicon[kwl].E.Count /*+ "," + all_E*/);
                }
            }
            Console.WriteLine("good freq kw: "+kwCount_f + " all kw(raw count) " + kwCount_uf);
            Console.WriteLine(kwLexicon.Count + " kw in lexicon, unique kw");
            //review
            int i = 0;
            fse.WriteLine("Entity" + "," + "occur" + "," + "ln(occur)" + "," + "PScore" + "," + "kw Count" + "," + "keywords");
            foreach (KeyValuePair<string, EkwO> e_ekwo in EkwDict)
            {
                int occur = e_ekwo.Value.Occur;
                if (occur > 2)
                {
                    /*string all_kw = "";
                    foreach (string a_kw in e_ekwo.Value.kw.Keys) all_kw += a_kw.Replace(",","") + ";";*/
                    fse.WriteLine(e_ekwo.Key + "," + occur + "," + Math.Log(occur) + "," + e_ekwo.Value.PScore + "," + e_ekwo.Value.kw.Count/*+","+all_kw*/);
                    i++;
                }
            }
            fse.Close();
            fsk.Close();
            Console.WriteLine(i + " of " + EkwDict.Count + " E occured more than twice");
            Console.WriteLine("echo time to store and calculate: " + (TimeSpan)(DateTime.Now - startTime));
        }

        public void genEEgraph(string reportPath,int kwLimitThreshold=350, int entityOccurThreshold=2, double polarityThresh=0.35, bool PoleInvariant = false, bool NoSelfEdge=true)
        {
            if (EkwEgg == null) { EkwEgg = new Dictionary<string, EEVertex>(StringComparer.InvariantCultureIgnoreCase); }
            Console.WriteLine("Generating EE graph");

            int legitKWCnt = 0;
            foreach (KeyValuePair<string, kwEo> cnt in kwLexicon) { if (cnt.Value.E.Count < kwLimitThreshold)legitKWCnt++; }
            Console.WriteLine("from " + legitKWCnt + " legit kw");
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
                            if (nborE == E && NoSelfEdge) continue;
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

            //cleanup
            foreach (string node in EkwEgg.Keys)
            {
                string[] nborvs = new string[EkwEgg[node].Neighbors.Keys.Count];
                int i = 0;
                foreach (string nbk in EkwEgg[node].Neighbors.Keys)
                {
                    nborvs[i++] = nbk;
                }
                foreach (string nbork in nborvs)
                {
                    if (!EkwEgg.ContainsKey(nbork))
                    {
                        EkwEgg[node].Neighbors.Remove(nbork);
                    }
                }
            }
            Console.WriteLine("and " + EkwEgg.Count + " entities");
            Console.WriteLine("EkwEG time to generate EE graph: " + (TimeSpan)(DateTime.Now - startTime));

            StreamWriter fs = new StreamWriter(reportPath + "-"+ kwLimitThreshold+"," + entityOccurThreshold +".EkwEGg.csv", false);
            foreach (KeyValuePair<string, EEVertex> evert in EkwEgg)
            {
                fs.Write(evert.Key + ","+evert.Value.Neighbors.Count+",");
                foreach (string nbor in evert.Value.Neighbors.Keys)
                {
                    fs.Write(nbor + ";");
                }
                fs.WriteLine();
            }
            fs.Close();
            Console.WriteLine("EkwEG time to store & generate EE graph: " + (TimeSpan)(DateTime.Now - startTime));
        }

        public void Communitize(string filepath,double comThres=0.01, int MinNeighborForSeed = 2, string forcedSeed="")
        {
            DateTime startTime = DateTime.Now;
            Dictionary<string, int> AllEntity = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            foreach (string entity in EkwEgg.Keys)
            {
                if (EkwEgg[entity].Neighbors.Count >= MinNeighborForSeed)
                    if (!AllEntity.ContainsKey(entity))
                        AllEntity.Add(entity, EkwEgg[entity].Neighbors.Count);
            }

            bool keepRunning = true;

            string seed = forcedSeed;

            Console.WriteLine("building communities");
            Console.WriteLine("SignificantEntity.Count: " + AllEntity.Count);
            while (keepRunning)
            {
                if (AllEntity.Count < 1) { keepRunning = false; continue; }

                Community aCommunity = new Community();
                aCommunity.CommCondThres = comThres;
                //seed it
                if (seed == "" || !AllEntity.ContainsKey(seed))
                    seed = AllEntity.Keys.First();
                while (EkwEgg[seed].Neighbors.Count < MinNeighborForSeed)
                {
                    AllEntity.Remove(seed);
                    seed = AllEntity.Keys.First();
                    Console.Write("\r" + AllEntity.Count);
                }

                Console.WriteLine("\t EntityRemain : " + AllEntity.Count);
                bool keepBuildingThisComnty = true;
                
                string entity = seed;
                while (keepBuildingThisComnty)
                {
                    EEVertex eev = EkwEgg[entity];
                    Community.EvalVal eval = aCommunity.EvalEEVertex(eev);
                    if (eval.memberable)
                    {
                        aCommunity.AddVertexToCommunity(eev, eval, EkwDict[entity].PScore);
                        AllEntity.Remove(entity);
                        Console.Write("\r" + AllEntity.Count);
                    }
                    else if (aCommunity.OutlinkedNodes.Count < 1)
                    {
                        keepBuildingThisComnty = false;
                        continue;
                    }
                    else
                    {
                        //this is not going to be a potential member
                        aCommunity.OutlinkedNodes.Remove(entity);
                    }
                    if (aCommunity.OutlinkedNodes.Count > 0)
                    {
                        entity = aCommunity.OutlinkedNodes.Keys.First();
                    }
                    else
                    {
                        keepBuildingThisComnty = false;
                        continue;
                    }
                }
                if (aCommunity.Size> 0)
                {
                    aCommunity.Consolidate();
                    Communities.Add(aCommunity);
                }
                Console.Write("\tCommunity size:"+ aCommunity.MemberNodes.Count);
                seed = entity;
            }

            Console.WriteLine("time to find communitieis: " + (TimeSpan)(DateTime.Now - startTime));
            int largesComntySz = 0;

            StreamWriter fs = new StreamWriter(filepath);
            fs.WriteLine("<Communities>");
            int i = 1;
            foreach (Community comnty in Communities)
            {
                //int kwThreshhold = comnty;
                fs.WriteLine(" <Community id='"+i+++"' size='"+ comnty.Size +"' conductance='"+comnty.Conductance+"' pScore='"+ comnty.pScore +"'>");
                fs.WriteLine("  <trapped-keywords count='" + comnty.TrappedKeywords.Count + "'>");
                fs.Write("       ");
                foreach (string kw in comnty.TrappedKeywords.Keys)
                {
                    fs.Write(System.Net.WebUtility.HtmlEncode(kw)+":"+comnty.TrappedKeywords[kw]+",");
                }
                fs.WriteLine("\n    </trapped-keywords>");
                foreach (string member in comnty.MemberNodes.Keys)
                {
                    fs.WriteLine("  <e>" + System.Net.WebUtility.HtmlEncode(member)+ "</e>");
                }
                fs.WriteLine(" </Community>");

                largesComntySz = (largesComntySz > comnty.Size) ? largesComntySz : comnty.Size;
            }

            fs.WriteLine("</Communities>");
            fs.Close();
            Console.WriteLine("\n\nCommunities.Count: " + Communities.Count);
            Console.WriteLine("Largest Communities Size: " + largesComntySz);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filepath);
            xDoc.Normalize();
            xDoc.Save(filepath);
        }

        public void BFS(string path)
        {
            Dictionary<string, int> AllEntity = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            bfsedE = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            Queue<string> Q = new Queue<string>();
            int i = 0;
            foreach (string entity in EkwEgg.Keys)
            {
                if (!AllEntity.ContainsKey(entity))
                    AllEntity.Add(entity, EkwEgg[entity].Neighbors.Count);
            }

            bool run = true;
            string enty = AllEntity.Keys.First();
            AllEntity.Remove(enty);
            while (run)
            {
                if (Q.Count < 1)
                {
                    if (AllEntity.Count < 1) { run = false; continue; }
                    enty = AllEntity.Keys.First();
                    AllEntity.Remove(enty);
                    if (!bfsedE.ContainsKey(enty))
                        bfsedE.Add(enty,i++);
                }
                else
                {
                    enty = Q.Dequeue();
                }

                foreach (string ne in EkwEgg[enty].Neighbors.Keys)
                {
                    if (!bfsedE.ContainsKey(ne))
                    {
                        AllEntity.Remove(ne);
                        Q.Enqueue(ne);
                        bfsedE.Add(ne, i++);
                    }
                }
            }
            StreamWriter fs = new StreamWriter(path);
            foreach (string E in bfsedE.Keys)
            {
                fs.WriteLine(E+",");
            }
            fs.Close();
        }

        public int FinalKW(string filepath)
        {
            kwFinal = new Dictionary<string,int>(StringComparer.CurrentCultureIgnoreCase);

            foreach (Community comnty in Communities)
            {
                foreach (string kw in comnty.TrappedKeywords.Keys)
                {
                    if (!kwFinal.ContainsKey(kw)) kwFinal.Add(kw, 0);
                    kwFinal[kw] += comnty.TrappedKeywords[kw];
                }
            }

            StreamWriter fs = new StreamWriter(filepath + ".kw_final.csv");
            fs.WriteLine("kw,Ocurnc");
            foreach (string kw in kwFinal.Keys)
            {
                fs.WriteLine(kw, kwFinal[kw]);
            }
            fs.Close();
            return kwFinal.Count;
        }

        public void BFS_Image(string path, int minNborCnt = 2)
        {

            sigEntities= new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            int ix=0;
            foreach (string entity in bfsedE.Keys)
            {
                if (EkwEgg[entity].Neighbors.Count > minNborCnt)
                {
                    sigEntities.Add(entity, ix++);
                }
            }

            int dim = sigEntities.Count;
            Bitmap bmp = new System.Drawing.Bitmap(dim, dim);
            Console.WriteLine("drawing " + dim + "x" + dim + " adj mat image");
            //init white
            //for(int i=0;i<dim;i++)
            //    for(int j=0;j<dim;j++)
            //        bmp.SetPixel(i,j, Color.White);

            ix = 0;
            foreach (string entity in sigEntities.Keys)
            {
                int x = sigEntities[entity];
                Console.Write("\r" + ix++);
                foreach (string nbor in EkwEgg[entity].Neighbors.Keys)
                {
                    if (sigEntities.ContainsKey(nbor))
                    {
                        int y = sigEntities[nbor];
                        bmp.SetPixel(x, y, Color.DarkBlue);
                    }
                }
            }

            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            Console.WriteLine("\nSaved at: " + path);
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

            fs.Close();
        }
    }
}
