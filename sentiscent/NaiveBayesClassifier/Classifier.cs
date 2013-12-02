using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace ProbabilityFunctions
{
    public class Classifier
    {
        private DataSet dataSet = new DataSet();

        public DataSet DataSet
        {
            get { return dataSet; }
            set { dataSet = value; }
        }

        public void TrainClassifier(DataTable table)
        {
            dataSet.Tables.Add(table);

            //table
            DataTable GaussianDistribution = dataSet.Tables.Add("Gaussian");
            GaussianDistribution.Columns.Add(table.Columns[0].ColumnName);

            //columns
            for (int i = 1; i < table.Columns.Count; i++)
            {
                GaussianDistribution.Columns.Add(table.Columns[i].ColumnName + "Mean");
                GaussianDistribution.Columns.Add(table.Columns[i].ColumnName + "Variance");
            }

            //calc data
            var results = (from myRow in table.AsEnumerable()
                           group myRow by myRow.Field<string>(table.Columns[0].ColumnName) into g
                           select new { Name = g.Key, Count = g.Count() }).ToList();

            for (int j = 0; j < results.Count; j++)
            {
                DataRow row = GaussianDistribution.Rows.Add();
                row[0] = results[j].Name;

                int a = 1;
                for (int i = 1; i < table.Columns.Count; i++)
                {
                    row[a] = Helper.Mean(SelectRows(table, i, string.Format("{0} = '{1}'", table.Columns[0].ColumnName, results[j].Name)));
                    row[++a] = Helper.Variance(SelectRows(table, i, string.Format("{0} = '{1}'", table.Columns[0].ColumnName, results[j].Name)));
                    a++;
                }
            }
        }

        public void Store(string filepath)
        {
            int rowCount = dataSet.Tables["Gaussian"].Rows.Count;
            int colCount = dataSet.Tables["Gaussian"].Columns.Count;

            StreamWriter fs = new StreamWriter(filepath, false);

            string strBuf = "";

            strBuf = "";

            foreach (DataColumn col in dataSet.Tables["Gaussian"].Columns)
            {
                strBuf += col.ColumnName + ",";
            }

            fs.WriteLine(strBuf);

            foreach (DataRow row in dataSet.Tables["Gaussian"].Rows)
            {
                strBuf = "";
                foreach (object obj in row.ItemArray)
                {
                    strBuf += obj.ToString() + "," ;
                }
                fs.WriteLine(strBuf);
            }
            fs.Close();

        }

        public void load(string filepath, bool noNonZero = true)
        {
            DataTable GaussianDistribution = dataSet.Tables.Add("Gaussian");
            //GaussianDistribution.Columns.Add(table.Columns[0].ColumnName);
            String[] lines = File.ReadAllLines(filepath);

            char[] comma = {','};
            string[] colNames = lines[0].Split(comma, StringSplitOptions.RemoveEmptyEntries);

            foreach (string colName in colNames)
            {
                GaussianDistribution.Columns.Add(colName);
            }
            //Console.WriteLine(GaussianDistribution.Columns.Count);

            for (int i = 1; i < lines.Length; i++)
            {
                DataRow row = GaussianDistribution.Rows.Add();
                string[] rowData = lines[i].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                int a=0;
                row[a] = rowData[a];
                for (a = 1; a < rowData.Length;a++)
                {
                    double d = Double.Parse( rowData[a] );
                    if(noNonZero && d==0.0) d=0.000000001;
                    row[a] = d;

                }

            }

        }

        public string Classify(double[] obj)
        {
            Dictionary<string, double> score = new Dictionary<string, double>();

            var results = (from myRow in dataSet.Tables[0].AsEnumerable()
                           group myRow by myRow.Field<string>(dataSet.Tables["Gaussian"].Columns[0].ColumnName) into g
                           select new { Name = g.Key, Count = g.Count() }).ToList();

            for (int i = 0; i < results.Count; i++)
            {
                List<double> subScoreList = new List<double>();
                int a = 1, b = 1;
                for (int k = 1; k < dataSet.Tables["Gaussian"].Columns.Count; k = k + 2)
                {
                    double mean = Convert.ToDouble(dataSet.Tables["Gaussian"].Rows[i][a]);
                    double variance = Convert.ToDouble(dataSet.Tables["Gaussian"].Rows[i][++a]);
                    double result = Helper.NormalDist(obj[b - 1], mean, Helper.SquareRoot(variance));
                    subScoreList.Add(result);
                    a++; b++;
                }

                double finalScore = 0;
                for (int z = 0; z < subScoreList.Count; z++)
                {
                    if (finalScore == 0)
                    {
                        finalScore = subScoreList[z];
                        continue;
                    }

                    finalScore = finalScore * subScoreList[z];
                }

                score.Add(results[i].Name, finalScore * 0.5);
            }

            double maxOne = score.Max(c => c.Value);

            double pos, neg;
            pos = score["positive"];
            neg = score["negative"];

            double positivity = (pos - neg) / (pos + neg);

            //Console.WriteLine(positivity);

            var name = (from c in score
                        where c.Value == maxOne
                        select c.Key).First();

            return name;
        }

        public double BinClassify(double[] obj)
        {
            Dictionary<string, double> score = new Dictionary<string, double>();

            var results = (from myRow in dataSet.Tables[0].AsEnumerable()
                           group myRow by myRow.Field<string>(dataSet.Tables["Gaussian"].Columns[0].ColumnName) into g
                           select new { Name = g.Key, Count = g.Count() }).ToList();

            double pbias = 0, nbias=0, total = 0;

            for (int k = 1, i=0; k < dataSet.Tables["Gaussian"].Columns.Count; k = k + 2, i++)
            {
                double n, p;
                n = Convert.ToDouble(dataSet.Tables["Gaussian"].Rows[0][k]);
                p = Convert.ToDouble(dataSet.Tables["Gaussian"].Rows[1][k]);
                double positivity = (p - n) / (p + n);
                if (Math.Abs(positivity) > 0.15)
                {
                    if (positivity > 0)
                    {
                        pbias += obj[i] * positivity;
                    }
                    else
                    {
                        nbias += obj[i] * positivity;
                    }
                    total += obj[i];
                }
            }

            if (pbias == nbias) return 0;

            double bias = (pbias - nbias) / (pbias + nbias);

            double normalBias = bias / total;

            if (normalBias == double.NaN) normalBias = 0;

            //Console.WriteLine(normalBias);

            return normalBias;
        }

        #region Helper Function

        public IEnumerable<double> SelectRows(DataTable table, int column, string filter)
        {
            List<double> _doubleList = new List<double>();
            DataRow[] rows = table.Select(filter);
            for (int i = 0; i < rows.Length; i++)
            {
                _doubleList.Add((double)rows[i][column]);
            }

            return _doubleList;
        }

        public void Clear()
        {
            dataSet = new DataSet();
        }

        #endregion
    }
}
