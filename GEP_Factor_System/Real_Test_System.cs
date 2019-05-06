using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Acsy.Common.Data;
using Acsy.Common.Info;
using Acsy.Common.Utility;
using Acsy.Model.Analysis;
using Acsy.Model.Forward;
using Acsy.Model.Models;
using Acsy.Model.Signals;
using MathNet.Numerics.Statistics;


namespace GEP_Factor_System
{
    class Real_Test_System
    {
        public Factor_Forest factor_Forest;
        Dictionary<DateTime, double> beta;
        public Real_Test_System(Factor_Forest factor_Forest,int forward)
        {
            IEnumerable<Bar> GetBarData(string[] datelist1)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily("990905", date);
                }).ToArray();
            }
            this.factor_Forest = factor_Forest;
            var datelist_beta = DatePosInfo.GetDateListTradingRange("20160101", "20180101").ToArray();
            var scBeta = new SignalCollection<Bar>(q => q.Symbol.StartsWith("990905"), null);
            scBeta.AddForward("Return", new ForwardBarReturn(forward));
            beta = new Dictionary<DateTime, double>();
            var data_beta1 = GetBarData(datelist_beta);
            var data_beta = scBeta.GenerateData(data_beta1).ToArray();
            data_beta.ForEach(dr => {
                beta.Add((DateTime)dr["DateTime"], (double)dr["Return"]);
            });
        }
        public void Out_Of_Sample_Test(String code)
        {

        }
        public Dictionary<DateTime,double> Real_Test3(string code, String start_date, String end_date, int forward)
        {
            IEnumerable<Bar> GetBarData(string[] datelist1)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily(code, date);
                }).ToArray();
            }
            var datelist = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
            var datelist_train = DatePosInfo.GetDateListTradingRange("20150101", "20170101").ToArray();
            var ic_all = factor_Forest.Adaptability.Keys.AsParallel().Select(i =>
            {
                var ics = datelist_train.RollingNonOverlap(30).Select(d =>
                {
                    var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
                    sc.AddSignal(i.ToString(), new Factor(new Factor_Tree(i.gene, i.head_length, i.tail_length, i.constant_length)), q => q.Symbol.StartsWith(code));
                    sc.AddForward("Return", new ForwardBarReturn(forward));
                    var data_test = sc.GenerateData(GetBarData(d)).ToArray();
                    if (data_test.Count() == 0) return 0;
                    var count2 = data_test.Count();
                    var count1 = data_test.Select(dr => { if ((double)dr[i.ToString()] == 0) return 0; else return 1; }).Cumsum().Last();
                    double co;
                    if (count1 < count2 * 0.6) co = 0.0;
                    else
                    {
                        //data_test = data_test.ToArray();//FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                        co = Correlation.Pearson(data_test.Select(x => (double)x[i.ToString()]).FillNA(0).ToArray(), data_test.Select(x => (double)x["Return"]).FillNA(0).ToArray());
                    }
                    if (Double.IsNaN(co)) return 0;
                    return co;
                }).ToArray();
                var ic_acc = ics.Cumsum().ToArray();
                int j = 0;
                var an = ic_acc.Select(icc =>
                {
                    j++;
                    return new DataItem(j, icc);
                }).ToArray();
                //List<int> axisx = new List<int>();
                //for (int i = 1; i <= axisx.Count();i++) axisx.Add(i);DataItem
                var rere = FactorAnalysis.Analysis(an, bUseIntercept: true, bShowChart: false);
                var ic = rere.slrAll.Slope;
                return new Tuple<Factor_Tree, double>(i, ic);
            }).ToArray();
            var ic_all1 = ic_all.OrderByDescending(p=>Math.Abs(p.Item2)).ToDictionary(p=>p.Item1,p=>p.Item2);
            var sc1 = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            for(int i = 0; i < 50; i++)
            {
                Factor factor = new Factor(ic_all1.Keys.ElementAt(i));
                if (sc1.SignalNames.Contains(factor.ft.ToString())) continue;
                if (Math.Abs(ic_all1.Values.ElementAt(i))>0.04)
                sc1.AddSignal(factor.ft.ToString(), new Factor(new Factor_Tree(factor.ft.gene, factor.ft.head_length, factor.ft.tail_length, factor.ft.constant_length)), q => q.Symbol.StartsWith(code));
            }
            sc1.AddForward("Return", new ForwardBarReturn(forward));
            sc1.AddForward("Return1", new ForwardBarReturn(1));
            int jj = 0;
            int pos = 0;
            int count = 0;
            List<int> counts = new List<int>();
            LinearRegressionModel linearRegressionModel = new LinearRegressionModel() { UseIntercept = true };
            var lstPos = datelist.RollingNonOverlap(30).SelectMany(datelist_test =>
            {
                jj++;
                
                if (jj < 2)
                {
                    var data_train = sc1.GenerateData(GetBarData(datelist_train)).ToArray();//FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()],0.05).ToArray();
                    linearRegressionModel.SetVariableName("Return", sc1.SignalNames.ToArray());
                    var data_train1 = data_train.DropNA();//.Select(dr => { for (int i = 0; i < dr.ItemArray.Count(); i++) if (Double.IsNaN((double)dr.ItemArray.ElementAt(i))) dr.ItemArray.ElementAt(i) = 0; });//.FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()], 0.05);
                    if (data_train1.Count() == 0) return null;
                    linearRegressionModel.TrainModel(data_train1);
                    sc1.Reset();
                }
                Console.WriteLine(linearRegressionModel);
                var data_test = sc1.GenerateData(GetBarData(datelist_test)).ToArray();
                var lstPrd = linearRegressionModel.GetPrediction(data_test);
                var lstPosCurr = lstPrd.Select(prd =>
                {
                    //if (pos > 0)
                    //{
                    //    if (prd > 0.0016)
                    //    {
                    //        if (pos < 10) { count++; pos++; counts.Add(1); return pos; }
                    //        else { counts.Add(0); return pos; }
                    //    }
                    //    else if (prd > -0.0002) { counts.Add(0); return pos; }
                    //    else if (prd > -0.0016) { pos = 0; counts.Add(0); return pos; }
                    //    else { pos = -1; count++; counts.Add(0); return pos; }
                    //}
                    //else if (pos < 0)
                    //{
                    //    if (prd < -0.0016)
                    //    {
                    //        if (pos > -10) { count++; pos--; counts.Add(1); return pos; }
                    //        else { counts.Add(0); return pos; }
                    //    }
                    //    else if (prd < 0.0002) { counts.Add(0); return pos; }
                    //    else if (prd < 0.0016) { pos = 0; counts.Add(0); return pos; }
                    //    else { pos = 1; count++; counts.Add(1); return pos; }
                    //}
                    //else
                    //{
                    //    if (prd > 0.0016) { count++; pos = 1; counts.Add(1); return pos; }
                    //    else if (prd < -0.0016) { count++; pos = -1; counts.Add(1); return pos; }
                    //    else { pos = 0; counts.Add(0); return pos; }
                    //}
                    if (Double.IsNaN(prd)) { counts.Add(0); return 0; }
                    if (prd > 0.003) { count++; counts.Add(1); return 1; }
                    else if (prd < -0.003) { count++; counts.Add(1); return -1; }
                    else { counts.Add(0); return 0; }
                });
                return lstPosCurr;
            }).ToArray();
            var sc2 = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            sc2.AddForward("Return", new ForwardBarReturn(forward));
            sc2.AddForward("Return1", new ForwardBarReturn(1));
            var dataLongOnly = sc2.GenerateData(GetBarData(datelist)).ToArray();
            var time = dataLongOnly.Select(row => (DateTime)row["DateTime"]).ToArray();
            var lstRtnLongOnly = dataLongOnly.Select(row => (double)row["Return"]).FillNA(0).ToArray();
            var lstRtn = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
            {
                return lstRtnLongOnly[i] * lstPos[i];// - counts[i] * 0.0014;
            }).ToArray();
            Dictionary<DateTime, double> LastRtn = new Dictionary<DateTime, double>();
            for (int i = 0; i < lstRtn.Count(); i++) LastRtn.Add(time[i], lstRtn[i]);
            double[] axisx1 = new double[lstRtn.Count()];
            for (int ii = 0; ii < lstRtn.Count(); ii++) axisx1[ii] = ii + 1;
            axisx1.DrawLine(lstRtn.FillNA(0).Cumsum().ToArray(), lstRtnLongOnly.Cumsum().ToArray());
            Console.WriteLine(lstRtn.Cumsum().Last() / count);
            Console.WriteLine(count);
            return LastRtn;
        }
        public Dictionary<DateTime,double> Real_Test2(string code, int start_date, int end_date, int forward)
        {
            IEnumerable<Bar> GetBarData(string[] datelist1)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily(code, date);
                }).ToArray();
            }
            var datelist = DatePosInfo.GetDateListTradingRange(start_date.ToString()+"0101", end_date.ToString()+"0101").ToArray();
            var datelist_train = DatePosInfo.GetDateListTradingRange((start_date-1).ToString() + "0101", (end_date - 1).ToString() + "0101").ToArray();
            int count = 0;
            List<int> counts = new List<int>();
            int jj = 0;
            int cccc = 0;
            List<double> cos = new List<double>();
            List<double> pre = new List<double>();
            List<Factor_Tree> chosen = new List<Factor_Tree>();
            Dictionary<Factor_Tree, double> chosen2 = new Dictionary<Factor_Tree, double>();
            Dictionary<string, double> ad_train = new Dictionary<string, double>();
            Dictionary<DateTime, double> pres = new Dictionary<DateTime, double>();
            double train_std=0;
            var lstPos = datelist.RollingNonOverlap(250).SelectMany(datelist_test =>
            {
                //Dictionary<string, double> dictionary = new Dictionary<string, double>();
                var sc1 = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
                double corr2 = 0;
                List<double> list = new List<double>();
                int t = 0;
                string[] datelist_sig=null;
                factor_Forest.Adaptability.Keys.ForEach(ft =>
                {
                    t++;
                    var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
                    Factor factor = new Factor(ft);
                    sc.AddSignal(factor.ft.ToString(), new Factor(new Factor_Tree(factor.ft.gene, factor.ft.head_length, factor.ft.tail_length, factor.ft.constant_length)), q => q.Symbol.StartsWith(code));
                    sc.AddForward("Return", new ForwardBarReturn(forward));
                    datelist_sig = DatePosInfo.GetDateListTradingRange(start_date.ToString()+"0101",end_date.ToString()+"0101").ToArray();
                    var datelist_sig_train = DatePosInfo.GetDateListTradingRange((start_date - 1).ToString() + "0101", (end_date - 1).ToString() + "0101").ToArray();
                    if (jj <= 0)
                    {
                        List<double> train_rtns = new List<double>();
                        var tics = datelist_sig_train.RollingNonOverlap(30).Select(date =>
                        {
                            var data_sig = sc.GenerateData(GetBarData(date)).DropNA().ToArray();
                            foreach (double rtn in data_sig.Select(drr => (double)drr["Return"])) if(!Double.IsNaN(rtn))train_rtns.Add(rtn);
                            //data_sig.ForEach(dr => { dr["Return"] = (double)dr["Return"] - beta[(DateTime)dr["DateTime"]]; });
                            var c = data_sig.Count();
                            if (c < 5000) return 0;
                            var c1 = data_sig.Select(dr => { if ((double)dr[factor.ft.ToString()] == 0) return 0; else return 1; }).Cumsum().Last();
                            double co;
                            if (c1 < c * 0.6) co = 0.0;
                            else
                            {
                                //data_test = data_test.ToArray();//FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                                co = Correlation.Pearson(data_sig.Select(x => (double)x[factor.ft.ToString()]).FillNA(0).ToArray(), data_sig.Select(x => (double)x["Return"]).FillNA(0).ToArray());
                            }
                            if (Double.IsNaN(co)) return 0;
                            return co;
                        }).ToArray();
                        train_std = train_rtns.StandardDeviation();
                        var tics1 = tics.ToList();
                        for (int i = 0; i < tics1.Count(); i++) { if (tics1[i] == 0) tics1.RemoveAt(i); }
                        tics = tics1.ToArray();
                        var tic_acc = tics.Cumsum().ToArray();
                        //tic_acc.DrawLine();
                        int tj = 0;
                        var tan = tic_acc.Select(icc =>
                        {
                            tj++;
                            return new DataItem(tj, icc);
                        }).ToArray();
                        Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares tlrm = new Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares() { UseIntercept = true };
                        var trere = tlrm.Learn(tan.Select(aa => aa.indicator).ToArray(), tan.Select(aa => aa.predicator).ToArray());
                        var tic1 = tics.Average();
                        sc.Reset();
                        if (Math.Abs(tic1) > 0.01) { cccc++; }
                        ad_train.Add(ft.ToString(), tic1);
                        //Console.WriteLine(ft.ToString()+tic1);
                    }
                    var ics = datelist_sig.RollingNonOverlap(30).Select(date =>
                    {
                        var data_sig = sc.GenerateData(GetBarData(date)).DropNA().ToArray();
                        //data_sig.ForEach(dr => { dr["Return"] = (double)dr["Return"] - beta[(DateTime)dr["DateTime"]]; });
                        var c = data_sig.Count();
                        if (c < 1000) return 0;
                        var c1 = data_sig.Select(dr => { if ((double)dr[factor.ft.ToString()] == 0) return 0; else return 1; }).Cumsum().Last();
                        double co;
                        if (c1 < c * 0.6) co = 0.0;
                        else
                        {
                            //data_test = data_test.ToArray();//FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                            co = Correlation.Pearson(data_sig.Select(x => (double)x[factor.ft.ToString()]).FillNA(0).ToArray(), data_sig.Select(x => (double)x["Return"]).FillNA(0).ToArray());
                        }
                        if (Double.IsNaN(co)) return 0;
                        return co;
                    }).ToArray();
                    var ics1 = ics.ToList();
                    for (int i = 0; i < ics1.Count(); i++) { if(ics1[i]==0)ics1.RemoveAt(i); }
                    ics = ics1.ToArray();
                    var ic_acc = ics.Cumsum().ToArray();
                    int j = 0;
                    var an = ic_acc.Select(icc =>
                    {
                        j++;
                        return new DataItem(j, icc);
                    }).ToArray();
                    Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares lrm = new Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares() { UseIntercept = true };
                    var rere = lrm.Learn(an.Select(aa => aa.indicator).ToArray(), an.Select(aa => aa.predicator).ToArray());
                    double ic1 = 0;
                    if (rere == null) ic1 = 0;
                    else ic1 = ics.Average();
                    var ic2 = ad_train[ft.ToString()];
                    var corr = ics.Average() / ics.Std();
                    
                    if (Math.Abs(ic2) / Math.Abs(ic1) > 0.8 && Math.Abs(ic1)/Math.Abs(ic2)>0.8&&Math.Sign(ic2)==Math.Sign(ic1) && Math.Abs(ic2) > 0.015 && Math.Abs(ic1) > 0.015)
                    {
                        list.Add(Math.Abs(ic1)); sc1.AddSignal(factor.ft.ToString(), new Factor(new Factor_Tree(factor.ft.gene, factor.ft.head_length, factor.ft.tail_length, factor.ft.constant_length)), q => q.Symbol.StartsWith(code));
                        chosen.Add(factor.ft);
                        chosen2.Add(factor.ft, ic1);
                        Console.WriteLine(factor.ft+" ic1:"+ic1+" ic2:"+ic2);
                        //ic_acc.DrawLine();
                        //int x = 0;
                    }
                });
                if (list.Count() != 0) cos.Add(list.Cumsum().Last());
                jj++;
                sc1.AddForward("Return", new ForwardBarReturn(forward));
                sc1.AddForward("Return1", new ForwardBarReturn(5));
                //FileStream fs = new FileStream(@"C:\Users\073\Desktop\hjmfs2.txt", FileMode.OpenOrCreate);
                //for (int tt = 0; tt < chosen.Count(); tt++)
                //{
                //    byte[] array = System.Text.Encoding.UTF8.GetBytes(chosen[tt].gene + "%" + chosen2[chosen[tt]] + "\r\n");
                //    fs.Write(array, 0, array.Length);
                //}
                //fs.Close();
                Console.WriteLine(cccc);
                Console.WriteLine(jj+","+sc1.SignalNames.Count());
                
                var rtnstd = sc1.GenerateData(GetBarData(datelist_sig)).ToArray().DropNA().Select(drr=>(double)drr["Return"]).ToArray().Std();
                Console.WriteLine("rtnstd:" + (0.0016 * train_std / rtnstd).ToString());
                sc1.Reset();
                LinearRegressionModel linearRegressionModel = new LinearRegressionModel { UseIntercept = true };
                var data_train = sc1.GenerateData(GetBarData(datelist_train)).ToArray();
                //data_train.ForEach(dr => { dr["Return"] = (double)dr["Return"] - beta[(DateTime)dr["DateTime"]]; });
                bool enough = false;
                if (data_train.Count() > 30000&&sc1.SignalNames.Count()>0) enough = true;
                linearRegressionModel.SetVariableName("Return", sc1.SignalNames.ToArray());
                var data_train1 = data_train.DropNA();//.Select(dr => { for (int i = 0; i < dr.ItemArray.Count(); i++) if (Double.IsNaN((double)dr.ItemArray.ElementAt(i))) dr.ItemArray.ElementAt(i) = 0; });//.FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()], 0.05);
                linearRegressionModel.TrainModel(data_train1);
                Console.WriteLine(linearRegressionModel);
                sc1.Reset();
                int pos = 0;
                var data_test = sc1.GenerateData(GetBarData(datelist_test)).ToArray();
                //data_test.ForEach(dr => { dr["Return"] = (double)dr["Return"] - beta[(DateTime)dr["DateTime"]]; });
                var lstPrd = linearRegressionModel.GetPrediction(data_test).ToArray();
                var ret = data_test.Select(dddd => (double)dddd["Return1"]).ToArray();
                var lstPosCurr = Enumerable.Range(0,lstPrd.Count()).Select(index_prd =>
                {
                    var prd = lstPrd[index_prd]*rtnstd/train_std;
                    //pres.Add((DateTime)data_test[index_prd]["DateTime"], prd);
                    pre.Add(prd);
                    //var thr = 0.0016 * train_std / rtnstd;
                    if (!enough) { counts.Add(0); return 0; }
                    if (Double.IsNaN(prd)) { pos = 0; counts.Add(0); return 0; }
                    //if(index_prd<=15) { pos = 0; counts.Add(0); return pos; }
                    //if (pos > 0)
                    //{
                    //    if(ret[index_prd-1]<-0.0014) { pos = 0; counts.Add(0); return pos; }
                    //    if (prd > 0.0018)
                    //    {
                    //        if (pos < 10) { count++; pos++; counts.Add(1); return pos; }
                    //        else { counts.Add(0); return pos; }
                    //    }
                    //    else if (prd > -0.0018) { counts.Add(0); return pos; }
                    //    else { pos = -1; count++; counts.Add(0); return pos; }
                    //}
                    //else if (pos < 0)
                    //{
                    //    if (ret[index_prd-1] > 0.0014) { pos = 0; counts.Add(0); return pos; }
                    //    if (prd < -0.0018)
                    //    {
                    //        if (pos > -10) { count++; pos--; counts.Add(1); return pos; }
                    //        else { counts.Add(0); return pos; }
                    //    }
                    //    else if (prd < 0.0018) { counts.Add(0); return pos; }
                    //    else { pos = 1; count++; counts.Add(1); return pos; }
                    //}
                    //else
                    //{
                    //    if (prd > 0.0018) { count++; pos = 1; counts.Add(1); return pos; }
                    //    else if (prd < -0.0018) { count++; pos = -1; counts.Add(1); return pos; }
                    //    else { pos = 0; counts.Add(0); return pos; }
                    //}

                    if (prd > 0.0018) { count++; counts.Add(1); return 1; }
                    else if (prd < -0.0018) { count++; counts.Add(1); return -1; }
                    else { counts.Add(0); return 0; }
                });
                return lstPosCurr;
            }).ToArray();
            var sc2 = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            sc2.AddForward("Return", new ForwardBarReturn(forward));
            sc2.AddForward("Return1", new ForwardBarReturn(1));
            var dataLongOnly = sc2.GenerateData(GetBarData(datelist)).ToArray();
            //dataLongOnly.ForEach(dr => { dr["Return"] = (double)dr["Return"] - beta[(DateTime)dr["DateTime"]]; });
            //dataLongOnly.ToDataTable().ToCsv(@"C:\Users\073\Desktop\000541.csv");
            //var d2 = dataLongOnly.ToDataTable().InsertColumn("Prediction", pre);
            //d2.ToCsv(@"C:\Users\073\Desktop\wppa4\" + code + ".csv");
            var time = dataLongOnly.Select(row => (DateTime)row["DateTime"]).ToArray();
            var lstRtnLongOnly = dataLongOnly.Select(row => (double)row["Return"]).FillNA(0).ToArray();
            var lstRtnLongOnly3 = dataLongOnly.Select(row => (double)row["Return"]).FillNA(0).ToArray();
            var lstRtnLongOnly4 = dataLongOnly.Select(row => (double)row["Return1"]).FillNA(0).ToArray();
            var f = FactorAnalysis.Analysis(Enumerable.Range(0, lstRtnLongOnly3.Count()).Select(i => new DataItem(pre[i], lstRtnLongOnly3[i])),bShowChart:true,dExtremeValue:0);
            //Console.WriteLine("Y and YHat:"+f.slrAll.Slope);
            var bstRtn = lstRtnLongOnly.Select(r =>
            {
                if (r > 0.004) return 1;
                else if (r < -0.004) return -1;
                else return 0;
            }).ToArray();
            var mistake = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
            {
                if (lstPos[i] == 1) if (bstRtn[i] != 1) return 1;
                if (lstPos[i] == -1) if (bstRtn[i] != -1) return 1;
                return 0;
            }).Cumsum().Last();
            var misopen = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
            {
                if (bstRtn[i] == 1) if (lstPos[i] != 1) return 1;
                if (bstRtn[i] == -1) if (lstPos[i] != -1) return 1;
                return 0;
            }).Cumsum().Last();
            var bcall =bstRtn.Select(r => Math.Abs(r)).Cumsum().Last();
            if (cos.Count != 0)
            {
                Console.WriteLine(cos.Cumsum().Last() / (double)cos.Count());
                Console.WriteLine((double)(bcall - misopen) / (double)bcall);
                Console.WriteLine((double)(bcall - misopen) / (double)(mistake));
            }
            var lstRtn = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
            {
                //if (lstPos[i] > 0)
                //{
                //    double lstrtnall = 0;
                //    for(int j = 0; j < 40; j++)
                //    {
                //        lstrtnall += lstRtnLongOnly4[i + j];
                //        if (lstrtnall < -0.008)
                //        {
                //            break;
                //        }
                //    }
                //    return lstPos[i] * lstrtnall;
                //}
                //else if (lstPos[i] < 0)
                //{
                //    double lstrtnall = 0;
                //    for (int j = 0; j < 40; j++)
                //    {
                //        lstrtnall += lstRtnLongOnly4[i + j];
                //        if (lstrtnall > 0.008)
                //        {
                //            break;
                //        }
                //    }
                //    return lstPos[i] * lstrtnall;
                //}
                //if (lstPos[i] > 0 && lstRtnLongOnly4[i] < -0.0014) return lstRtnLongOnly4[i] * lstPos[i];
                //else if (lstPos[i] < 0 && lstRtnLongOnly4[i] > 0.0014) return lstRtnLongOnly4[i] * lstPos[i];
                /*else*/ return lstRtnLongOnly[i] * lstPos[i];//-counts[i]*0.0014;
            }).ToArray();
            Dictionary<DateTime, double> LastRtn = new Dictionary<DateTime, double>();
            for (int i = 0; i < lstRtn.Count(); i++) LastRtn.Add(time[i], lstRtn[i]);
            double[] axisx1 = new double[lstRtn.Count()];
            for (int ii = 0; ii < lstRtn.Count(); ii++) axisx1[ii] = ii + 1;
            axisx1.DrawLine(lstRtn.FillNA(0).Cumsum().ToArray(), lstRtnLongOnly.Cumsum().ToArray());
            Console.WriteLine(lstRtn.Cumsum().Last() / count);
            Console.WriteLine(count);
            return LastRtn;
        }
        public void Real_Test1(string code, String start_date, String end_date, int forward)
        {
            var datelist = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
            Dictionary<string, LinearRegressionModel> dictionary1 = new Dictionary<string, LinearRegressionModel>();
            
            IEnumerable<Bar> GetBarData(string[] datelist1)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily(code, date);
                }).ToArray();
            }
            
            foreach (Factor_Tree factor_Tree in factor_Forest.Forest)
            {
                var sc1 = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
                var linearRegressionModel = new LinearRegressionModel() { UseIntercept = true };
                Factor factor = new Factor(factor_Tree);
                sc1.AddSignal(factor.ft.ToString(), new Factor(new Factor_Tree(factor.ft.gene, factor.ft.head_length, factor.ft.tail_length, factor.ft.constant_length)), q => q.Symbol.StartsWith(code));
                sc1.AddForward("Return", new ForwardBarReturn(forward));
                var datelist_train = DatePosInfo.GetDateListTradingRange("20160101", "20170101").ToArray();
                var data_train = sc1.GenerateData(GetBarData(datelist_train)).FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                linearRegressionModel.SetVariableName("Return", sc1.SignalNames.ToArray());
                var data_train1 = data_train.DropNA();
                linearRegressionModel.TrainModel(data_train1);
                Console.WriteLine(linearRegressionModel);
                dictionary1.Add(factor.ft.ToString(), linearRegressionModel);
            }
            int count = 0;
            var lstPos = datelist.RollingNonOverlap(30).SelectMany(datelist_test =>
            {
                //Dictionary<string, double> dictionary = new Dictionary<string, double>();
                var lstPrd = factor_Forest.Forest.Select(ft =>
                {
                    var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
                    Factor factor = new Factor(ft);
                    sc.AddSignal(factor.ft.ToString(), new Factor(new Factor_Tree(factor.ft.gene, factor.ft.head_length, factor.ft.tail_length, factor.ft.constant_length)), q => q.Symbol.StartsWith(code));
                    sc.AddForward("Return", new ForwardBarReturn(forward));
                    var datelist_sig = DatePosInfo.GetDateListTradingBefore(datelist_test[0], 60).ToArray();
                    var data_sig = sc.GenerateData(GetBarData(datelist_sig)).FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                    var corr1 = Correlation.Pearson(data_sig.Select(x => (double)x[factor.ft.ToString()]).FillNA(0), data_sig.Select(x => (double)x["Return"]).FillNA(0));
                    var datelist_sig2 = DatePosInfo.GetDateListTradingBefore(datelist_test[0], 250).ToArray();
                    sc.Reset();
                    var data_sig2 = sc.GenerateData(GetBarData(datelist_sig2)).FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                    var corr2 = Correlation.Pearson(data_sig2.Select(x => (double)x[factor.ft.ToString()]).FillNA(0), data_sig2.Select(x => (double)x["Return"]).FillNA(0));
                    var corr = (corr1 / corr2)>0.7?corr1/corr2:0;
                    Console.WriteLine(factor.ft.ToString() + " : " + corr);
                    sc.Reset();
                    var data_test = sc.GenerateData(GetBarData(datelist_test)).ToArray();//FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                    var prd = dictionary1[factor.ft.ToString()].GetPrediction(data_test).FillNA(0).ToArray();
                    return prd.Select(p => p * corr).ToArray();
                }).ToArray();
                lstPrd = lstPrd.Transpose();
                var lstPosCurr = lstPrd.Select(prd =>
                {
                    if (prd.Cumsum().Last() > 0.01) { count++; return 1; }
                    else if (prd.Cumsum().Last() < -0.01) { count++; return -1; }
                    else return 0;
                });
                return lstPosCurr;
            }).ToArray();
            var sc2 = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            sc2.AddForward("Return", new ForwardBarReturn(forward));
            var dataLongOnly = sc2.GenerateData(GetBarData(datelist)).ToArray();
            var lstRtnLongOnly = dataLongOnly.Select(row => (double)row["Return"]).FillNA(0).ToArray();
            var lstRtn = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
            {
                return lstRtnLongOnly[i] * lstPos[i];
            }).ToArray();
            double[] axisx1 = new double[lstRtn.Count()];
            for (int ii = 0; ii < lstRtn.Count(); ii++) axisx1[ii] = ii + 1;
            axisx1.DrawLine(lstRtn.FillNA(0).Cumsum().ToArray(), lstRtnLongOnly.Cumsum().ToArray());
            Console.WriteLine(lstRtn.Cumsum().Last() / count);
            Console.WriteLine(count);

        }
        public Dictionary<DateTime,double> Real_Test(string code, String start_date, String end_date, int forward)
        {
            var datelist = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
            var datelist2 = DatePosInfo.GetDateListTradingRange("20150101", "20160101").ToArray();
            var datelist3 = DatePosInfo.GetDateListTradingRange("20150101", "20160101").ToArray();
            var datelist4 = DatePosInfo.GetDateListTradingRange("20140101", "20150101").ToArray();
            List<double> pre = new List<double>();
            var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            var sc1 = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            int factor_Num = 0;
            int sn = 0;
            foreach (Factor_Tree factor_Tree in factor_Forest.Forest)
            {
                factor_Num++;
                if (factor_Num >40 && factor_Num < 42)
                {
                    sn = factor_Num;
                    Factor factor = new Factor(factor_Tree);
                    if (sc.SignalNames.Contains(factor.ft.ToString())) continue;
                    sc.AddSignal(factor.ft.ToString(), new Factor(new Factor_Tree(factor.ft.gene, factor.ft.head_length, factor.ft.tail_length, factor.ft.constant_length)), q => q.Symbol.StartsWith(code));
                    sc1.AddSignal(factor.ft.ToString(), new Factor(new Factor_Tree(factor.ft.gene, factor.ft.head_length, factor.ft.tail_length, factor.ft.constant_length)), q => q.Symbol.StartsWith(code));
                }
            }
            sc.AddForward("Return", new ForwardBarReturn(forward));
            sc.AddForward("Return1", new ForwardBarReturn(1));
            sc1.AddForward("Return", new ForwardBarReturn(forward));
            IEnumerable<Bar> GetBarData(string[] datelist1)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily(code, date);
                }).ToArray();
            }
            var linearRegressionModel = new LinearRegressionModel() { UseIntercept = true};
            int count = 0;
            var dataLongOnly = sc.GenerateData(GetBarData(datelist)).ToArray();//FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()], 0.05).ToArray();
            //var dataLongOnly11 = sc.GenerateData(GetBarData(datelist)).ToDataTable().FillNAInPlace();
            //dataLongOnly.ToDataTable().FillNAInPlace().ToCsv(@"C:\Users\073\Desktop\hjm.csv");
            List<double> corrs = new List<double>();
            List<double> corrs1 = new List<double>();
            List<double> corrs2 = new List<double>();
            List<double> corrs3 = new List<double>();
            sc.Reset();
            int c = 0;
            int jj = 0;
            int pos = 0;
            List<int> counts = new List<int>();
            
            var lstPos = datelist.RollingNonOverlap(30).SelectMany(datelist_test =>
            {
                jj++;
                sc.Reset();
                String[] datelist_train;
                //if ((c + 1) * 30 <= datelist2.Count())
                //{
                //    datelist_train = DatePosInfo.GetDateListTradingRange(datelist2[c * 30], datelist2[(c + 1) * 30]).ToArray();
                //}
                //else datelist_train = DatePosInfo.GetDateListTradingRange(datelist2[c * 30], datelist2.Last()).ToArray();
                if (jj < 2)
                {
                    //datelist_train = DatePosInfo.GetDateListTradingBefore(datelist_test[0], 30).ToArray();
                    var data_train = sc.GenerateData(GetBarData(datelist2)).ToArray();//FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()],0.05).ToArray();
                    //data_train.ToDataTable().ToCsv(@"C:\Users\073\Desktop\wahhh500.csv");
                    linearRegressionModel.SetVariableName("Return", sc.SignalNames.ToArray());
                    var data_train1 = data_train.DropNA();//.Select(dr => { for (int i = 0; i < dr.ItemArray.Count(); i++) if (Double.IsNaN((double)dr.ItemArray.ElementAt(i))) dr.ItemArray.ElementAt(i) = 0; });//.FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()], 0.05);
                    if (data_train1.Count() == 0) return null ;
                    FactorAnalysis.Analysis(data_train1.ToDataItem(sc.SignalNames.ToArray()[0], "Return"),dExtremeValue:0,bUseIntercept:true);
                    linearRegressionModel.TrainModel(data_train1);

                }
                Console.WriteLine(linearRegressionModel);
                //var dddd = GetBarData(datelist_test);
                var data_test = sc1.GenerateData(GetBarData(datelist_test)).ToArray();
                var corr = Correlation.Pearson(data_test.Select(x=>(double)x[factor_Forest.Forest[sn-1].ToString()]).FillNA(0), data_test.Select(x => (double)x["Return"]).FillNA(0));
                if (double.IsNaN(corr)) ;else corrs.Add(corr);
                //var corr1 = Correlation.Pearson(data_test.Select(x => (double)x[factor_Forest.Forest[0].ToString()]).FillNA(0), data_test.Select(x => (double)x["Return"]).FillNA(0));
                //corrs1.Add(corr1);
                c++;
                var lstPrd = linearRegressionModel.GetPrediction(data_test);
                var lstPosCurr = lstPrd.Select(prd =>
                {
                    pre.Add(prd);
                    //if (pos > 0)
                    //{
                    //    if (prd > 0.0014)
                    //    {
                    //        if (pos < 10) { count++; pos++; counts.Add(1); return pos; }
                    //        else { counts.Add(0); return pos; }
                    //    }
                    //    else if (prd > -0.0002) { counts.Add(0); return pos; }
                    //    else if (prd > -0.0014) { pos = 0; counts.Add(0); return pos; }
                    //    else { pos = -1; count++; counts.Add(0); return pos; }
                    //}
                    //else if (pos < 0)
                    //{
                    //    if (prd < -0.0014)
                    //    {
                    //        if (pos > -10) { count++; pos--; counts.Add(1); return pos; }
                    //        else { counts.Add(0); return pos; }
                    //    }
                    //    else if (prd < 0.0002) { counts.Add(0); return pos; }
                    //    else if (prd < 0.0014) { pos = 0; counts.Add(0); return pos; }
                    //    else { pos = 1; count++; counts.Add(1); return pos; }
                    //}
                    //else
                    //{
                    //    if (prd > 0.0014) { count++; pos = 1; counts.Add(1); return pos; }
                    //    else if (prd < -0.0014) { count++; pos = -1; counts.Add(1); return pos; }
                    //    else { pos = 0; counts.Add(0); return pos; }
                    //}
                    if (Double.IsNaN(prd)) { counts.Add(0); return 0; }
                    if (prd > 0.0014) { count++; counts.Add(1); return 1; }
                    else if (prd < -0.0014) { count++; counts.Add(1); return -1; }
                    else { counts.Add(0); return 0; }
                });
                return lstPosCurr;
            }).ToArray();

            var lstRtnLongOnly = dataLongOnly.Select(row => (double)row["Return"]).FillNA(0).ToArray();
            var time = dataLongOnly.Select(row => (DateTime)row["DateTime"]).ToArray();
            //var lstdataLongOnly = dataLongOnly.Select(row => (double)row["Alpha3"]).FillNA(0).ToArray();
            //var corr1 = Correlation.Pearson(lstdataLongOnly,lstRtnLongOnly);
            //Console.WriteLine(corr);
            double[] axisx1 = new double[corrs.Count];
            for (int ii = 0; ii < corrs.Count; ii++) axisx1[ii] = ii + 1;
            //axisx1.DrawLine(corrs.FillNA(0).Cumsum().ToArray(), corrs1.Cumsum().ToArray());
            corrs.Cumsum().ToArray().DrawLine();
            var a = dataLongOnly.ToDataItem(factor_Forest.Forest[sn-1].ToString(), "Return").ToArray();//.FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()], 0.05).ToDataItem(factor_Forest.Forest[0].ToString(), "Return").ToArray();
            //var dada = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i => new DataItem(pre[i], lstRtnLongOnly[i]));
            //FactorAnalysis.Analysis(Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i => new DataItem(pre[i], lstRtnLongOnly[i])), dExtremeValue:0.001, bShowChart:true);
            //for (int i = 0; i < a.Count; i++)
            //{
            //    if (a[i].predicator < 0)
            //        a.RemoveAt(i);
            //}
            //FactorAnalysis.Analysis(a,bUseIntercept:true);
            int count1 = 0;
            var bestRtn = lstRtnLongOnly.Select(rtn =>
            {
                if (rtn > 0.0014) { count1++; return rtn; }
                else if (rtn < -0.0014) { count1++; return -1 * rtn; }
                else return 0;
            }).ToArray();
            var lstRtn = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
             {
                 return lstRtnLongOnly[i] * lstPos[i];// -counts[i]*0.0014;
             }).ToArray();
            Dictionary<DateTime, double> LastRtn = new Dictionary<DateTime, double>();
            for (int i = 0; i < lstRtn.Count(); i++) LastRtn.Add(time[i], lstRtn[i]);
            var AcclstRtn = lstRtn.Cumsum().ToArray();
            var PurelstRtn = Enumerable.Range(0, AcclstRtn.Count()).Select(i =>
            {
                return AcclstRtn[i] - 0.0014 * counts[i];
            }).ToArray();
            double[] axisx = new double[lstRtn.Count()];
            for (int ii = 0; ii < lstRtn.Count(); ii++) axisx[ii] = ii + 1;
            //axisx.DrawLine(lstRtn.FillNA(0).Cumsum().ToArray());//, bestRtn.Cumsum().ToArray());
            ////axisx.DrawLine(PurelstRtn);
            ////Console.WriteLine(PurelstRtn.Last()/count);
            //Console.WriteLine(count);
            Console.WriteLine(lstRtn.Cumsum().Last() / count);
            //Console.WriteLine(lstPos.Select(i => Math.Abs(i)).Cumsum().Last() / count);
            //Console.WriteLine(bestRtn.Cumsum().Last() / count1);
            //Console.WriteLine(count1);
            return  LastRtn;
        }
        private static IEnumerable<Bar> DataCleanDaily(string code, string date)
        {
            //var list = CacheManager.GetObjectFromCache($"{code}bar60second", () =>

            //   DataCenter.GetQuoteList<Snapshot>(code, date).ToBarSecond(60).Where(DataCenter.filterValidTimeRecord).ToArray()
            //, false);
            int g = 60;
            var list = DataCenter.GetQuoteList<Bar>(code, date).ToArray();
            if (list.Length == 0) yield break;
            DateTime zerotime = list[0].DateTime;
            if (list[0].TimeIndexMinute > 0)
            {
                Bar newbar = new Bar()
                {
                    Close = list[0].Open,
                    Open = list[0].Open,
                    High = list[0].Open,
                    Low = list[0].Open,
                    Volume = 0,
                    DateTime = new DateTime(zerotime.Year, zerotime.Month, zerotime.Day, 9, 30, 0),
                    Symbol = list[0].Symbol
                };
                list = new Bar[] { newbar }.Concat(list).ToBarMinute(1).ToArray();
            }
            else
            {
                list = list.ToBarMinute(1).ToArray();
            }
            if (list.Length == 0 || list.Max(b => b.High).Equal(list.Min(b => b.Low))) yield break;
            Bar lstbar = null;
            foreach (Bar bar in list.Where(DataCenter.filterValidTimeRecord))
            {
                if (bar.DateTime.Hour < 12)
                {

                    if (lstbar != null)
                    {
                        int gap = (bar.DateTime.Hour - lstbar.DateTime.Hour) * 3600 + (bar.DateTime.Minute - lstbar.DateTime.Minute) * 60 + bar.DateTime.Second - lstbar.DateTime.Second;
                        for (int c = g; c < gap; c += g)
                        {
                            Bar newbar = new Bar();
                            newbar.DateTime = lstbar.DateTime.AddSeconds(g);
                            newbar.Open = newbar.Close = newbar.High = newbar.Low = lstbar.Close;
                            newbar.Symbol = lstbar.Symbol;
                            newbar.Volume = newbar.Turnover = 0;
                            yield return newbar;
                        }

                    }
                    else
                    {
                        int gap1 = (bar.DateTime.Hour - 9) * 3600 + (bar.DateTime.Minute - 30) * 60 + bar.DateTime.Second;
                        for (double c = gap1; c > 0; c -= g)
                        {
                            Bar newbar = new Bar();
                            newbar.DateTime = bar.DateTime.AddSeconds(-1 * c);
                            newbar.Open = newbar.Close = newbar.High = newbar.Low = bar.Close;
                            newbar.Symbol = bar.Symbol;
                            newbar.Volume = newbar.Turnover = 0;
                            yield return newbar;
                        }
                    }
                    lstbar = bar;
                    yield return bar;
                }
                else
                {
                    if (lstbar == null)
                        yield break;
                    int gap1 = (11 - lstbar.DateTime.Hour) * 3600 + (30 - lstbar.DateTime.Minute) * 60 + (0 - lstbar.DateTime.Second);
                    for (double c = g; c < gap1; c += g)
                    {
                        Bar newbar = new Bar();
                        newbar.DateTime = lstbar.DateTime.AddSeconds(c);
                        newbar.Open = newbar.Close = newbar.High = newbar.Low = lstbar.Close;
                        newbar.Symbol = lstbar.Symbol;
                        newbar.Volume = newbar.Turnover = 0;
                        yield return newbar;
                    }
                    gap1 = (bar.DateTime.Hour - 13) * 3600 + bar.DateTime.Minute * 60 + bar.DateTime.Second;
                    if (lstbar.DateTime.Hour < 12)
                    {
                        for (int c = gap1; c > 0; c -= g)
                        {
                            Bar newbar = new Bar();
                            newbar.DateTime = bar.DateTime.AddSeconds(-1 * c);
                            newbar.Open = newbar.Close = newbar.High = newbar.Low = lstbar.Close;
                            newbar.Symbol = lstbar.Symbol;
                            newbar.Volume = newbar.Turnover = 0;
                            yield return newbar;
                        }
                    }
                    else
                    {
                        int gap = (bar.DateTime.Hour - lstbar.DateTime.Hour) * 3600 + (bar.DateTime.Minute - lstbar.DateTime.Minute) * 60 + bar.DateTime.Second - lstbar.DateTime.Second;
                        for (int c = g; c < gap; c += g)
                        {
                            Bar newbar = new Bar();
                            newbar.DateTime = lstbar.DateTime.AddSeconds(c);
                            newbar.Open = newbar.Close = newbar.High = newbar.Low = lstbar.Close;
                            newbar.Symbol = lstbar.Symbol;
                            newbar.Volume = newbar.Turnover = 0;
                            yield return newbar;
                        }
                    }
                    lstbar = bar;
                    yield return bar;
                }


            }
            int gap2 = (15 - lstbar.DateTime.Hour) * 3600 + (0 - lstbar.DateTime.Minute) * 60 + (0 - lstbar.DateTime.Second);
            for (int c = g; c < gap2; c += g)
            {
                Bar newbar = new Bar();
                newbar.DateTime = lstbar.DateTime.AddSeconds(c);
                newbar.Open = newbar.Close = newbar.High = newbar.Low = lstbar.Close;
                newbar.Symbol = lstbar.Symbol;
                newbar.Volume = newbar.Turnover = 0;
                yield return newbar;
            }
        }

    }
}
