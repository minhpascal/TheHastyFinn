﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot.Series;

using YSQ.core.Historical;

namespace TheHastyFinn
{
    public class XFactor
    {
        public XFactor(string ticker)
        {
            Ticker = ticker;

            //_graphmodel = new XFactorGraphModel();
            //_graphmodel.StockTicker = ticker;

            StockQuotes sq = new StockQuotes(Ticker);
            Quotes = sq.HistPrices();

            Periods = new List<int>() { 25, 50, 75, 100 };

            PeriodVelocityData = new Dictionary<int, List<decimal>>();
            PeriodGravityData = new Dictionary<int, List<decimal>>();

            GenXFactor();
        }

        public string Ticker { get; private set; }
        public Dictionary<int,List<decimal>> PeriodVelocityData { get; set; }
        public Dictionary<int, List<decimal>> PeriodGravityData { get; set; }

        public List<HistoricalPrice> Quotes { get; set; }
        public List<int> Periods { get; set; }
        
        /*
        private XFactorGraphModel _graphmodel;
        public XFactorGraphModel XFGraphModel
        {
            get { return _graphmodel; }
            private set { _graphmodel = value; }
        }
        */

        // calculate XFactor across many moving windows (25, 50, 100, 125, 150, etc).
        // maybe even have a spread? what do the trends look like for diff intervals and ranges?

        private void GenXFactor()
        {
            XFactorAlgoData data = new XFactorAlgoData();
            
            foreach (int period in Periods)
            {
                List<decimal> velocity = new List<decimal>();
                List<decimal> gravity = new List<decimal>();

                data.ResetValues();

                for(int i = 1; i < Quotes.Count(); i++)
                {
                    int range = 1;
                    
                    if (period >= Quotes.Count() - i) // tail end of segments
                    {
                        range = Quotes.Count() - i;
                    }
                    else if (i > period) // middle of segment
                    {
                        range = period;
                    }
                    else // beginning of segment quotes
                    {
                        range = i;
                    }

                    // TODO 
                    // TODO --- two ways to calculate the window . start full or end full
                    // TODO --- current way is to end full
                    // TODO 
                    List<HistoricalPrice> segment = Quotes.GetRange(i, range);

                    // this can expire !! must account for that? - old comment

                    // find the highest high for period
                    if (data.pHighHi < (i - period))
                    {
                        data.HighestHi = 0;
                    }
                    SearchData fpmax = FindPeriodMax(segment);
                    if (fpmax.Price > data.HighestHi)
                    {
                        data.HighestHi = fpmax.Price;
                        data.pHighHi = fpmax.Index + (i - period);
                    }

                    // find lowest low for period
                    if (data.pLowLo < (i - period))
                    {
                        data.LowestLo = decimal.MaxValue;
                    }
                    SearchData fpmin = FindPeriodMin(segment);
                    if(fpmin.Price < data.LowestLo)
                    {
                        data.LowestLo = fpmin.Price;
                        data.pLowLo = fpmin.Index + (i - period);
                    }

                    // calc num of periods since high/low
                    data.pSinceHighHi = i - data.pHighHi;
                    data.pSinceLowLo = i - data.pSinceLowLo;

                    // calc velocity
                    decimal v = ((period - data.pSinceHighHi) / period) * 100;
                    velocity.Add(v);

                    // calc gravity
                    decimal g = ((period - data.pSinceLowLo) / period) * 100;
                    gravity.Add(g);
                }

                PeriodVelocityData.Add(period, velocity);
                PeriodGravityData.Add(period, gravity);
            }
        }

        private SearchData FindPeriodMax(List<HistoricalPrice> segment)
        {
            SearchData data = new SearchData();
            decimal max = 0;

            for(int i = 0; i < segment.Count(); i++)
            {
                if (segment[i].Price > max)
                {
                    data.Price = segment[i].Price;
                    data.Index = i;
                }
            }
            return data;
        }

        private SearchData FindPeriodMin(List<HistoricalPrice> segment)
        {
            SearchData data = new SearchData();
            decimal min = decimal.MaxValue;

            for (int i = 0; i < segment.Count(); i++)
            {
                if(segment[i].Price > min)
                {
                    data.Price = segment[i].Price;
                    data.Index = i;
                }
            }
            return data;
        }

    }

    class SearchData
    {
        public decimal Price { get; set; }
        public int Index { get; set; }

        public SearchData() { }
    }
}
