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
            kw = new Dictionary<string, Meta>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddKW(string keyWord, double pScore)
        {
            if (!kw.ContainsKey(keyWord)) { kw.Add(keyWord, new Meta()); }
            kw[keyWord].Occur++;
            kw[keyWord].PScore += pScore;
        }

        public void freshen(double threshold = 0.35)
        {
            _pScore = _pScore / Occur;

            string[] kwkeys = new string[kw.Keys.Count];

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
            E = new Dictionary<string, Meta>(StringComparer.InvariantCultureIgnoreCase);
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

    public enum polarity { neg = 1, obj = 0, pos = 1, opn };

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
            Neighbors = new Dictionary<string, kwp>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddNeighbor(string nbor, string linke_kw, polarity mutual_pole)
        {
            if (!Neighbors.ContainsKey(nbor)) Neighbors.Add(nbor, new kwp(mutual_pole));

            Neighbors[nbor].kw += linke_kw + ",";
            Neighbors[nbor].Weight++;
            if (Neighbors[nbor].Pole != mutual_pole) Neighbors[nbor].Pole = polarity.opn;
        }
    }

    public class Community
    {
        private uint _volume;

        public uint Volume
        {
            get { return _volume; }
            //set { _volume = value; }
        }

        private uint _outlinks;

        public uint OutlinksCount
        {
            get { return _outlinks; }//not equal to OutlinkedNodes.Count
            //set { _outlinks = value; }
        }

        private double _pScoreSum;

        public double pScore
        {
            get { return _pScoreSum/Size; }
            //set { _polarity = value; }
        }
        

        public double Conductance
        {
            get
            {
                if (_volume == 0) return 1;
                return (double)(((double)_outlinks) / _volume);
            }
        }
        public int Size
        {
            get { return this.MemberNodes.Count; }
        }
        private double _CommCondThres;

        public double CommCondThres
        {
            get { return _CommCondThres; }
            set { _CommCondThres = value; }
        }

        public Dictionary<string, int> MemberNodes;
        public Dictionary<string, int> OutlinkedNodes;
        public Dictionary<string, int> TrappedKeywords;

        public Community()
        {
            MemberNodes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            OutlinkedNodes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            TrappedKeywords = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            _outlinks = 0;
            _volume = 0;
        }

        public class EvalVal
        {
            public double cond;
            public bool memberable;
            public uint linksToCommunity;
            public uint linksOutCommunity;
        }

        public EvalVal EvalEEVertex(EEVertex eev)
        {
            EvalVal eval = new EvalVal();

            eval.linksOutCommunity = 0;
            eval.linksToCommunity = 0;

            foreach (string nbor in eev.Neighbors.Keys)
            {
                if (MemberNodes.ContainsKey(nbor))
                {
                    eval.linksToCommunity++;
                }
                else { eval.linksOutCommunity++; }
            }

            eval.cond = ((double)this.OutlinksCount + eval.linksOutCommunity - eval.linksToCommunity) / (this.Volume + eev.Neighbors.Count);

            if (eval.cond <= this.Conductance) eval.memberable = true;
            else if (eval.cond - CommCondThres < this.Conductance) eval.memberable = true;
            else eval.memberable = false;

            return eval;
        }

        bool isMember(string entity)
        {
            return MemberNodes.ContainsKey(entity);
        }

        public void AddVertexToCommunity(EEVertex eev, EvalVal eval, double pScore)
        {
            if (!eval.memberable) return;

            if (!MemberNodes.ContainsKey(eev.Entity))
            {
                MemberNodes.Add(eev.Entity, eev.Neighbors.Count);
                _pScoreSum += pScore;
            }
            this._outlinks += (uint)eval.linksOutCommunity;
            this._volume += (uint)eev.Neighbors.Count;
            if (OutlinkedNodes.ContainsKey(eev.Entity)) OutlinkedNodes.Remove(eev.Entity);

            foreach (string nbor in eev.Neighbors.Keys)
            {
                string[] kws = eev.Neighbors[nbor].kw.Split(",".ToCharArray());
                foreach (string kw in kws)
                {
                    if (kw != "" && kw != " ")
                    {
                        if (!TrappedKeywords.ContainsKey(kw)) TrappedKeywords.Add(kw, 0);
                        TrappedKeywords[kw]++;
                    }
                }
                if (!isMember(nbor))
                {
                    if (!OutlinkedNodes.ContainsKey(nbor))
                        OutlinkedNodes.Add(nbor, 1/*eev.Neighbors[nbor].Weight*/);
                }
            }
        }

        public void Consolidate(int kwThreshold=-1)
        {
            if (kwThreshold < 0)
            {
                kwThreshold = (int) Math.Ceiling( Math.Log(Size) );
            }
            string[] kws = new string[TrappedKeywords.Count];

            int i = 0;
            foreach (string kw in TrappedKeywords.Keys)
            {
                kws[i++] = kw;
            }
            foreach (string kw in kws)
            {
                if (TrappedKeywords[kw] < kwThreshold)
                    TrappedKeywords.Remove(kw);
            }
        }
    }
}