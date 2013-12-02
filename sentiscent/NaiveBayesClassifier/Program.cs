using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Net;

namespace ProbabilityFunctions
{
    public partial class Program
    {

        public static Dictionary<string, int> POSIndex = new Dictionary<string, int>();
        public static string[] POSTags = {
                    "CC",	//coordinating conjunction",	//and
                    "CD",	//cardinal number",	//1, third
                    "DT",	//determiner",	//the
                    "EX",	//existential there",	//there is
                    "FW",	//foreign word",	//d'hoevre
                    "IN",	//preposition, subordinating conjunction",	//in, of, like
                    "JJ",	//adjective",	//green
                    "JJR",	//adjective, comparative",	//greener
                    "JJS",	//adjective, superlative",	//greenest
                    "LS",	//list marker",	//1)
                    "MD",	//modal",	//could, will
                    "NN",	//noun, singular or mass",	//table
                    "NNS",	//noun plural",	//tables
                    //np done as NNP
                    "NNP",	//proper noun, singular",	//John
                    "NNPS",	//proper noun, plural",	//Vikings
                    "PDT",	//predeterminer",	//both the boys
                    "POS",	//possessive ending",	//friend's
                    //PP as PRP
                    "PRP",	//personal pronoun",	//I, he, it
                    "PRP$",	//possessive pronoun",	//my, his
                    "RB",	//adverb",	//however, usually, naturally, here, good
                    "RBR",	//adverb, comparative",	//better
                    "RBS",	//adverb, superlative",	//best
                    "RP",	//particle",	//give up
                    //"SENT",	//Sentence-break punctuation",	//. ! ?
                    "SYM",	//Symbol",	/// [ = *
                    "TO",	//infinitive 'to'",	//togo
                    "UH",	//interjection",	//uhhuhhuhh
                    "VB",	//verb be, base form",	//be
                    "VBD",	//verb be, past tense",	//was, were
                    "VBG",	//verb be, gerund/present participle",	//being
                    "VBN",	//verb be, past participle",	//been
                    "VBP",	//verb be, sing. present, non-3d",	//am, are
                    "VBZ",	//verb be, 3rd person sing. present",	//is
                    //"VH",	//verb have, base form",	//have
                    //"VHD",	//verb have, past tense",	//had
                    //"VHG",	//verb have, gerund/present participle",	//having
                    //"VHN",	//verb have, past participle",	//had
                    //"VHP",	//verb have, sing. present, non-3d",	//have
                    //"VHZ",	//verb have, 3rd person sing. present",	//has
                    //"VV",	//verb, base form",	//take
                    //"VVD",	//verb, past tense",	//took
                    //"VVG",	//verb, gerund/present participle",	//taking
                    //"VVN",	//verb, past participle",	//taken
                    //"VVP",	//verb, sing. present, non-3d",	//take
                    //"VVZ",	//verb, 3rd person sing. present",	//takes
                    "WDT",	//wh-determiner",	//which
                    "WP",	//wh-pronoun",	//who, what
                    "WP$",	//possessive wh-pronoun",	//whose
                    "WRB",	//wh-abverb
                    "USR",
                    "URL",
                    "OTHER"
                };

        public static int GetPosIndex(string posTag)
        {
            try
            {
                return POSIndex[posTag];
            }
            catch (Exception e)
            {
                return POSTags.Length;
            }
        }

        public static void initPOSIndex()
        {
            for (int i = 0; i < POSTags.Length; i++)
            {
                POSIndex[POSTags[i]] = (i + 1);
            }
        }

        public static double[] posDist(string line)
        {
            char[] space = { ' ' };
            char[] underScore = { '_' };

            string[] word_tags = line.Split(space, StringSplitOptions.RemoveEmptyEntries);

            double[] P = new double[40];
            Array.Clear(P, 0, 40);

            foreach (string word_tag in word_tags)
            {
                string[] tag = word_tag.Split(underScore, StringSplitOptions.RemoveEmptyEntries);
                if (tag.Length > 1)
                {
                    int pi = GetPosIndex(tag[tag.Length - 1]);
                    P[pi] += 1.0;
                }
            }

            return P;
        }



        static void OpinionGram(string TERM, string tweetFilePath, string reportFilePath, string reportXMLpath, int dataSize)
        {
            NGramClassifier NGC = new NGramClassifier();
            NGC.Load("NGC.csv");

            Classifier POSC = new Classifier();
            POSC.load("sentiobj.csv");

            initPOSIndex();

            StreamWriter fs = new StreamWriter(reportFilePath, false);

            char[] space = { ' ', '\t' };
            char[] underScore = { '_' };

            string[] tData = File.ReadAllLines(tweetFilePath);

            if (tData.Length < dataSize) dataSize = tData.Length;
            double totalScore = 0;

            double posTot = 0, negTot = 0;
            int posCnt = 0, negCnt = 0;

            int i;

            fs.WriteLine("i" + "," + "score" + "," + "marker" + "," + "ngcScore" + "," + "posScore" + "," + "sens" + "," + "tweet");

            for (i = 0; i < dataSize; i++)
            {
                string line = tData[i];

                //string[] lineParts = line.Split(space, 4);

                string taggedTweet = line;

                string[] tweetParts = taggedTweet.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string tweet = "";
                foreach (string twpart in tweetParts)
                {
                    string word = twpart.Split("_".ToCharArray())[0];
                    tweet += word + " ";
                }

                taggedTweet = line;
                
                double[] P = convert(posDist(taggedTweet));

                double ngcScore = NGC.score(tweet);
                double posScore = POSC.BinClassify(P);
                string sens = POSC.Classify(P);

                double score = 0.0;
                double NPosScore = (posScore / 2) + 0.0;
                if (NPosScore > +1) NPosScore = +.85;
                if (NPosScore < -1) NPosScore = -.85;

                score = NPosScore + ngcScore;

                if (score == double.NaN)
                {
                    int q = 0; ;
                }
                string marker = "neutral";
                if (score < -0.25) { marker = "negative"; negCnt++; }
                else if (score > +0.25) { marker = "positive"; posCnt++; }

                totalScore += score;
                
                fs.WriteLine(i + "," + score + "," + marker + "," + ngcScore + "," + posScore + "," + sens + "," + tweet);
            }
           
            fs.Close();

            Console.WriteLine("avg score = " + (totalScore / i));
            Console.WriteLine("neg pct   = " + (((double)negCnt) / i));
            Console.WriteLine("pos pct   = " + (((double)posCnt) / i));
            Console.WriteLine("post count= " + i);

            StreamWriter XMLfs = new StreamWriter(reportXMLpath, false);

            XMLfs.WriteLine("<opinion entity='"+TERM+"'>");
            XMLfs.WriteLine("  <score>"+ (totalScore/i).ToString("F2") +"</score>");
            XMLfs.WriteLine("  <analysis ");
            XMLfs.WriteLine("       post-count='" + i +"'");
            XMLfs.WriteLine("       percent-positive='" + (((double)posCnt) * 100 / i).ToString("F2") + "'");
            XMLfs.WriteLine("       percent-negative='" + (((double)negCnt) * 100 / i).ToString("F2") + "'" + " />");
            XMLfs.WriteLine("</opinion>");

            XMLfs.Close();

        }


        public static void OpinGram(string term)
        {
            OpinionGram(term,
                term + ".vcb-labelled",
                term + ".report.csv",
                term + ".rep.xml",
                2500);
        }

        static void Main(string[] args)
        {
        //    DataTable table = new DataTable();
        //    table.Columns.Add("Sex");
        //    table.Columns.Add("Height", typeof(double));
        //    table.Columns.Add("Weight", typeof(double));
        //    table.Columns.Add("FootSize", typeof(double));

        //    //training data.
        //    table.Rows.Add("male", 6, 180, 12);
        //    table.Rows.Add("male", 5.92, 190, 11);
        //    table.Rows.Add("male", 5.58, 170, 12);
        //    table.Rows.Add("male", 5.92, 165, 10);

        //    table.Rows.Add("female", 5, 100, 6);
        //    table.Rows.Add("female", 5.5, 150, 8);
        //    table.Rows.Add("female", 5.42, 130, 7);
        //    table.Rows.Add("female", 5.75, 150, 9);

        //    table.Rows.Add("transgender", 4, 200, 5);
        //    table.Rows.Add("transgender", 4.10, 150, 8);
        //    table.Rows.Add("transgender", 5.42, 190, 7);
        //    table.Rows.Add("transgender", 5.50, 150, 9);


            //Classifier classifier = new Classifier();
            //classifier.TrainClassifier(table);
            //classifier.Store("gender.csv");
            //classifier.load("gender.csv");
            /*
            Classifier classifier = new Classifier();

            classifier.TrainClassifier(
                    SadHappyObj("sads.vcb-labelled", 
                    "joys.vcb-labelled", 
                    "obj.vcb-labelled",
                    10000)
                );
            classifier.Store("sentiobj.csv");
            */
            //classifier.load("senti.csv");

            //Anlyze("sads.vcb-labelled",
            //        "joys.vcb-labelled",
            //        10000
            //    );

            //Test("test.vcb-labelled", "POS.test.report.csv", 10000);

            //Console.WriteLine(classifier.Classify(new double[] { 6, 130, 10}));

            //string[] ng = NGramClassifier.NGramify("I do not like fish");

            //NGramClassifier.printArrayOfStrings(ng);

            //NGramClassifier ngc = new NGramClassifier();

            //ngc.recordLine("I do not like fish", -1);
            //ngc.recordLine("I hate fish", -1);
            //ngc.recordLine("I love fish", +1);
            //ngc.recordLine("We are human being", 0);

            //NGCTrain("sads.txt", "joys.txt", "obj.txt", 10000);

            //NGCTest("test.txt", "NGC.test.repo.csv", 10000);
            
            //BothTest("test.vcb-labelled", "NGC+POS.test.report.csv", 10000);

            /******* EkwOG *****************/
            EkwOAnalyze EkwOA = new EkwOAnalyze();

            //EkwOA.TEkwP("obj.vcb-labelled", "TEkwP_small", 111);
            EkwOA.TEkwP("sample.vcb-labelled", "TEkwP_sample", -1);

            //EkwOA.genEkwBigraph("TEkwP_small.xml");
            EkwOA.genEkwBigraph("TEkwP_sample.xml");

            //OpinGram("bankrupt");

            //Console.Read();
        }
    }
}