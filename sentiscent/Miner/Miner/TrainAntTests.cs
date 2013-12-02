using System;
using System.Data;
using System.Collections.Generic;
using System.IO;

namespace Miner
{
    public partial class Program
    {

        static DataTable SadHappy(string sadFilePath, string happyFilePath, int dataSize)
        {
            DataTable table = new DataTable();
            initPOSIndex();

            table.Columns.Add("sentiment");

            foreach (string pt in POSTags)
            {
                table.Columns.Add(pt, typeof(double));
            }

            string[] sads = File.ReadAllLines(sadFilePath);
            string[] joys = File.ReadAllLines(happyFilePath);

            for (int i = 0; i < dataSize; i++)
            {
                string sline = sads[i];

                double[] P = posDist(sline);

                table.Rows.Add("negative", P[1], P[2], P[3], P[4], P[5], P[6], P[7], P[8], P[9],
                                     P[10], P[11], P[12], P[13], P[14], P[15], P[16], P[17], P[18], P[19],
                                     P[20], P[21], P[22], P[23], P[24], P[25], P[26], P[27], P[28], P[29],
                                     P[30], P[31], P[32], P[33], P[34], P[35], P[36], P[37], P[38], 0.0
                                    );

                string jline = joys[i];
                Console.WriteLine(i);

                P = posDist(jline);

                table.Rows.Add("positive", P[1], P[2], P[3], P[4], P[5], P[6], P[7], P[8], P[9],
                                    P[10], P[11], P[12], P[13], P[14], P[15], P[16], P[17], P[18], P[19],
                                    P[20], P[21], P[22], P[23], P[24], P[25], P[26], P[27], P[28], P[29],
                                    P[30], P[31], P[32], P[33], P[34], P[35], P[36], P[37], P[38], 0.0
                                   );


            }

            return table;
        }

        static DataTable SadHappyObj(string sadFilePath, string happyFilePath, string objFilePath, int dataSize)
        {
            DataTable table = new DataTable();
            initPOSIndex();

            table.Columns.Add("sentiment");

            foreach (string pt in POSTags)
            {
                table.Columns.Add(pt, typeof(double));
            }

            string[] sads = File.ReadAllLines(sadFilePath);
            string[] joys = File.ReadAllLines(happyFilePath);
            string[] objs = File.ReadAllLines(objFilePath);

            for (int i = 0; i < dataSize; i++)
            {
                string sline = sads[i];

                double[] P = posDist(sline);

                table.Rows.Add("negative", P[1], P[2], P[3], P[4], P[5], P[6], P[7], P[8], P[9],
                                     P[10], P[11], P[12], P[13], P[14], P[15], P[16], P[17], P[18], P[19],
                                     P[20], P[21], P[22], P[23], P[24], P[25], P[26], P[27], P[28], P[29],
                                     P[30], P[31], P[32], P[33], P[34], P[35], P[36], P[37], P[38], 0.0
                                    );

                string jline = joys[i];
                Console.WriteLine(i);

                P = posDist(jline);

                table.Rows.Add("positive", P[1], P[2], P[3], P[4], P[5], P[6], P[7], P[8], P[9],
                                    P[10], P[11], P[12], P[13], P[14], P[15], P[16], P[17], P[18], P[19],
                                    P[20], P[21], P[22], P[23], P[24], P[25], P[26], P[27], P[28], P[29],
                                    P[30], P[31], P[32], P[33], P[34], P[35], P[36], P[37], P[38], 0.0
                                   );

                string oline = joys[i];
                P = posDist(oline);

                table.Rows.Add("objective", P[1], P[2], P[3], P[4], P[5], P[6], P[7], P[8], P[9],
                                    P[10], P[11], P[12], P[13], P[14], P[15], P[16], P[17], P[18], P[19],
                                    P[20], P[21], P[22], P[23], P[24], P[25], P[26], P[27], P[28], P[29],
                                    P[30], P[31], P[32], P[33], P[34], P[35], P[36], P[37], P[38], 0.0
                                   );


            }

            return table;
        }

        public static double[] convert(double[] P)
        {
            double[] D = new double[38];
            for (int i = 0; i < 38; i++)
            {
                D[i] = P[i + 1];
            }

            return D;
        }

        static void Anlyze(string sadFilePath, string happyFilePath, int dataSize)
        {
            Classifier classifier = new Classifier();
            classifier.load("senti.csv");

            initPOSIndex();

            string[] sads = File.ReadAllLines(sadFilePath);
            string[] joys = File.ReadAllLines(happyFilePath);
            int erC = 0;
            int erS = 0;
            double total = 0;
            int n = 0;
            for (int i = 0; i < dataSize; i++)
            {
                string line = joys[i];

                double[] P = convert(posDist(line));

                double sent = classifier.BinClassify(P);
                //string sens = classifier.Classify(P);
                if (sent < 0)
                {
                    erC++;
                    Console.WriteLine(erC + " at C " + i);
                }
                else if (sent > 0)
                {
                    total += sent; n++;
                }

                //if (sens != "positive")
                //{
                //    erS++;
                //    Console.WriteLine(erS + " at S " + i);
                //}
                //string jline = joys[i];
                //Console.WriteLine(total);
                //P = posDist(jline);

                //Console.WriteLine(sens);
            }
            Console.WriteLine("avg = " + (total / n));
        }

        static void Test(string testFilePath, string reportFilePath, int dataSize)
        {
            Classifier classifier = new Classifier();
            classifier.load("senti.csv");
            Classifier objOpinCl = new Classifier();
            objOpinCl.load("sentiobj.csv");

            StreamWriter fs = new StreamWriter(reportFilePath, false);

            initPOSIndex();

            char[] space = { ' ' };
            char[] underScore = { '_' };

            string[] tData = File.ReadAllLines(testFilePath);
            //string[] joys = File.ReadAllLines(happyFilePath);
            if (tData.Length < dataSize) dataSize = tData.Length;
            double total = 0;
            int n = 0;

            int err = 0, ers = 0, ero = 0;

            double posTot = 0, negTot = 0;
            int posCnt = 0, negCnt = 0;
            int realPos = 0;
            int realNut = 0;
            for (int i = 1; i < dataSize; i++)
            {
                string line = tData[i];

                string[] lineParts = line.Split(space, 4);

                string tweet = lineParts[3];

                string tag = lineParts[0];
                tag = tag.Split(underScore, 2)[0];

                double[] P = convert(posDist(tweet));

                double sent = classifier.BinClassify(P);
                string sens = objOpinCl.Classify(P);
                double objSent = objOpinCl.BinClassify(P);

                Console.WriteLine(tag + " " + sent + " " + sens);

                /*if (tag != "neutral")
                {
                    if (tag != sens) ers++;
                }*/

                if (sens == "objective") sens = "neutral";
                if (tag != sens) ers++;


                if (tag == "positive") realPos++;
                if (tag == "neutral") realNut++;

                if (sent > 0)
                {
                    posTot += sent;
                    posCnt++;
                }
                else
                {
                    negTot += sent;
                    negCnt++;
                }

                string marker = "neutral";
                if (sent < -1.2) marker = "negative";
                else if (sent > 1.01) marker = "positive";

                string ObSeMarker = "neutral";
                if (objSent < -1.2) ObSeMarker = "negative";
                else if (objSent > 1.01) ObSeMarker = "positive";

                fs.WriteLine(i + "," + tag + "," + sent + "," + objSent + "," + sens + "," + tweet);

                if (tag != marker) err++;
                if (tag != ObSeMarker) ero++;
            }
            fs.Close();
            Console.WriteLine("s err: " + ers + " b err:" + err + " obse err:" + ero);

        }

        static void NGCTrain(string sadFilePath, string happyFilePath, string objFilePath, int dataSize)
        {
            NGramClassifier NGC = new NGramClassifier();

            string[] sads = File.ReadAllLines(sadFilePath);
            string[] joys = File.ReadAllLines(happyFilePath);
            string[] objs = File.ReadAllLines(objFilePath);

            for (int i = 0; i < dataSize; i++)
            {
                NGC.recordLine(sads[i], -1);
                NGC.recordLine(joys[i], +1);
                NGC.recordLine(objs[i], 00);
            }

            NGC.Store("NGC.csv");
        }


        static void NGCTest(string testFilePath, string reportFilePath, int dataSize)
        {
            NGramClassifier NGC = new NGramClassifier();
            NGC.Load("NGC.csv");

            StreamWriter fs = new StreamWriter(reportFilePath, false);

            char[] space = { ' ', '\t' };
            char[] underScore = { '_' };

            string[] tData = File.ReadAllLines(testFilePath);

            if (tData.Length < dataSize) dataSize = tData.Length;
            double total = 0;
            int n = 0;

            int err = 0;

            double posTot = 0, negTot = 0;
            int posCnt = 0, negCnt = 0;
            int realPos = 0;
            int realNut = 0;

            for (int i = 1; i < dataSize; i++)
            {
                string line = tData[i];

                string[] lineParts = line.Split(space, 4);

                string tweet = lineParts[3];

                string tag = lineParts[0];
                if (tag != "neutral")
                {
                    int x = 0;
                }

                double score = NGC.score(tweet);

                Console.WriteLine(tag + " " + score);

                if (tag == "positive") realPos++;
                if (tag == "neutral") realNut++;

                if (score > 0)
                {
                    posTot += score;
                    posCnt++;
                }
                else
                {
                    negTot += score;
                    negCnt++;
                }

                string marker = "neutral";
                if (score < -0.1) marker = "negative";
                else if (score > +.01) marker = "positive";


                if (tag != marker) err++;

                fs.WriteLine(i + "," + tag + "," + marker + "," + score + "," + tweet);
            }
            fs.Close();
            Console.WriteLine("err: " + err);
        }



        static void BothTest(string testFilePath, string reportFilePath, int dataSize)
        {
            NGramClassifier NGC = new NGramClassifier();
            NGC.Load("NGC.csv");

            Classifier POSC = new Classifier();
            POSC.load("sentiobj.csv");

            initPOSIndex();

            StreamWriter fs = new StreamWriter(reportFilePath, false);

            char[] space = { ' ', '\t' };
            char[] underScore = { '_' };

            string[] tData = File.ReadAllLines(testFilePath);

            if (tData.Length < dataSize) dataSize = tData.Length;
            double total = 0;
            int n = 0;

            int err = 0;
            int berr = 0;
            int btot = 0;

            double posTot = 0, negTot = 0;
            int posCnt = 0, negCnt = 0;
            int realPos = 0;
            int realNut = 0;


            fs.WriteLine("i" + "," + "tag" + "," + "score" + "," + "marker" + "," + "ngcScore" + "," + "posScore" + "," + "sens" + "," + "score" + "," + "tweet");

            for (int i = 1; i < dataSize; i++)
            {
                string line = tData[i];

                string[] lineParts = line.Split(space, 4);

                string taggedTweet = lineParts[3];

                string[] tweetParts = taggedTweet.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string tweet = "";
                foreach (string twpart in tweetParts)
                {
                    string word = twpart.Split("_".ToCharArray())[0];
                    tweet += word + " ";
                }

                taggedTweet = lineParts[3];

                string tag = lineParts[0];
                tag = tag.Split(underScore, 2)[0];

                if (tag != "neutral")
                {
                    int x = 0;
                }


                double[] P = convert(posDist(taggedTweet));

                double ngcScore = NGC.score(tweet);
                double posScore = POSC.BinClassify(P);
                string sens = POSC.Classify(P);

                if (tag == "positive") realPos++;
                if (tag == "neutral") realNut++;

                double score = 0.0;
                double NPosScore = (posScore / 2) + .23;
                if (NPosScore > +1) NPosScore = +.71;
                if (NPosScore < -1) NPosScore = -.6;

                score = NPosScore + ngcScore;

                if (score > 0)
                {
                    posTot += score;
                    posCnt++;
                }
                else
                {
                    negTot += score;
                    negCnt++;
                }

                string marker = "neutral";
                if (score < -0.5) marker = "negative";
                else if (score > +0.5) marker = "positive";

                if (tag != marker) err++;

                if (tag != "neutral" && marker != "neutral")
                {
                    btot++;
                    if (tag != marker) berr++;
                }

                fs.WriteLine(i + "," + tag + "," + score + "," + marker + "," + ngcScore + "," + posScore + "," + sens + "," + tweet);
            }
            fs.Close();
            Console.WriteLine("err: " + err);
            Console.WriteLine("real biased: " + btot);
            Console.WriteLine("err on biased: " + berr);
            Console.WriteLine("err on biased ratio: " + ((double)berr / btot));
        }
    }
}