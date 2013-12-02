using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Net;

namespace Miner
{
    public class Meta
    {
        private int _occur;

        public int Occur
        {
            get { return _occur; }
            set { _occur = value; }
        }

        private double _pScore;

        public double PScore
        {
            get { return _pScore; }
            set { _pScore = value; }
        }
        
        
    }

    public class EkwO
    {
        private string _E;

        /// <summary>
        /// Entity
        /// </summary>
        public string E
        {
            get { return _E; }
            set { _E = value; }
        }
        private int _e_Occcur;

        public int Occur
        {
            get { return _e_Occcur; }
            set { _e_Occcur = value; }
        }

        private double _pScore;

        public double PScore
        {
            get { return _pScore; }
            set { _pScore = value; }
        }
        
        
        //associated keyword
        public Dictionary<string, Meta> kw;

        public EkwO(string entity)
        {
            E = entity;
            PScore = 0.0;
            Occur = 0;
            kw = new Dictionary<string, Meta>();
        }

        public void AddKW(string keyWord, double pScore)
        {
            if (!kw.ContainsKey(keyWord)) { kw.Add(keyWord, new Meta()); }
            kw[keyWord].Occur++;
            kw[keyWord].PScore += pScore;
        }

        public void freshen(double threshold=0.35)
        {
            _pScore = _pScore / Occur;

            string [] kwkeys = new string[kw.Keys.Count];
            
            int i = 0;
            foreach (string keyw in kw.Keys)
            {
                kwkeys[i++] = keyw;
            }
            
            foreach (string keyw in kwkeys)
            {
                kw[keyw].PScore /= kw[keyw].Occur;
                double freq = ((double)(kw[keyw].Occur)) / Occur;
                if (freq < threshold)
                {
                    kw.Remove(keyw);
                }
            }

        }
    }


    public class kwEo
    {
        private string _kw;

        /// <summary>
        /// Entity
        /// </summary>
        public string kw
        {
            get { return _kw; }
            set { _kw = value; }
        }
        private int _kw_Occcur;

        public int Occur
        {
            get { return _kw_Occcur; }
            set { _kw_Occcur = value; }
        }

        private double _pScore;

        public double PScore
        {
            get { return _pScore; }
            set { _pScore = value; }
        }


        //associated Entitities 
        public Dictionary<string, Meta> E;

        public kwEo(string keyword)
        {
            kw = keyword;
            PScore = 0.0;
            Occur = 0;
            E = new Dictionary<string, Meta>();
        }

        public void AddEntity(string entity, double pScore)
        {
            if (!E.ContainsKey(entity)) { E.Add(entity, new Meta()); }
            E[entity].Occur++;
            E[entity].PScore += pScore;
        }

        public void freshen(double threshold = 0.35)
        {
            _pScore = _pScore / Occur;

            foreach (string entity in E.Keys)
            {
                E[entity].PScore /= E[entity].Occur;
            }
        }
    }

    public enum polarity{neg=1, obj=0, pos=1, opn};

    public class kwp 
    {
        private string _kw;

        public string kw
        {
            get { return _kw; }
            set { _kw = value; }
        }

        private polarity _pole;

        public polarity Pole
        {
            get { return _pole; }
            set { _pole = value; }
        }

        private int _weight;

        public int Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        public kwp(polarity pole)
        {
            Weight = 0; kw = ""; Pole = pole;
        }
        
    }

    public class EEVertex
    {
        private string _entity;

        public string Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }

        public Dictionary<string, kwp> Neighbors;

        public EEVertex(string thisEntity)
        {
            Entity = thisEntity;
            Neighbors = new Dictionary<string, kwp>();
        }

        public void AddNeighbor(string nbor, string linke_kw, polarity mutual_pole)
        {
            if (!Neighbors.ContainsKey(nbor)) Neighbors.Add(nbor, new kwp(mutual_pole));

            Neighbors[nbor].kw += linke_kw + ",";
            Neighbors[nbor].Weight++;
            if (Neighbors[nbor].Pole != mutual_pole) Neighbors[nbor].Pole = polarity.opn;
        }
    }
}