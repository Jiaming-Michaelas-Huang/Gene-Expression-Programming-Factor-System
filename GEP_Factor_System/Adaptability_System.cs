using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acsy.Common.Data;
using Acsy.Common.Info;
using Acsy.Common.Utility;
using Acsy.Model.Forward;
using Acsy.Model.Signals;
using Acsy.Model.Analysis;
using MathNet.Numerics.Statistics;
using System.Data;
using Acsy.Model.Models;
using Accord.Math;

namespace GEP_Factor_System
{
    class Adaptability_System
    {
        double thr_sharp, thr_ic;
        public Dictionary<Factor_Tree,List<DataRow>> original_generation;
        public Dictionary<Factor_Tree, double> Adaptability;
        public Dictionary<string, IEnumerable<Bar>> data_all;
        public Adaptability_System(double thr_sharp,int thr_ic)
        {
            this.thr_ic = thr_ic;
            this.thr_sharp = thr_sharp;
            Adaptability = new Dictionary<Factor_Tree, double>();
            original_generation = new Dictionary<Factor_Tree, List<DataRow>>();
        }
        public Adaptability_System(double thr_sharp, int thr_ic, string start, string end, string[] codes)
        {
            this.thr_ic = thr_ic;
            this.thr_sharp = thr_sharp;
            Adaptability = new Dictionary<Factor_Tree, double>();
            original_generation = new Dictionary<Factor_Tree, List<DataRow>>();
            data_all = new Dictionary<string, IEnumerable<Bar>>();
            IEnumerable<Bar> GetBarData(string[] datelist1, String code)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily(code, date);
                }).ToArray();
            }
            var datelist_all = DatePosInfo.GetDateListTradingRange(start, end).ToArray();
            codes.ForEach(code =>
            {
                var da = GetBarData(datelist_all, code);
                data_all.Add(code, da);
            });
            //datelist_all.ForEach(date =>
            //{
            //    Dictionary<string, IEnumerable<Bar>> temp = new Dictionary<string, IEnumerable<Bar>>();
            //    codes.ForEach(code =>
            //    {
            //        string[] dates = { date };
            //        var ddd = GetBarData(dates, code);
            //        temp.Add(code, ddd);
            //    });
            //    data_all.Add(date, temp);
            //});
        }
        public void Set_Original_Generation(Factor factor, String start_date, String end_date, int forward, String code)
        {
            var datelist = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
            var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            sc.AddSignal("Alpha" + factor.ft.Number, factor, q => q.Symbol.StartsWith(code));
            sc.AddForward("Return", new ForwardBarReturn(forward));
            IEnumerable<Bar> GetBarData(string[] datelist1)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily(code, date);
                }).ToArray();
            }
            var data = sc.GenerateData(GetBarData(datelist)).ToArray();
            var ic = Correlation.Pearson(data.Select(x => (double)x[factor.ft.Number]).FillNA(0).ToArray(), data.Select(x => (double)x["Return"]).FillNA(0).ToArray());
            original_generation.Add(factor.ft, data.ToList());
            Adaptability.Add(factor.ft, ic);
        }
        public Dictionary<Factor_Tree, double> Adaptability_Test1(Factor factor, String start_date, String end_date, int forward, String[] codes)
        {
            Dictionary<string, DataRow[]> code_factor_data = new Dictionary<string, DataRow[]>();
            //var datelist = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
            //var data = sc.GenerateData(GetBarData(datelist)).ToArray();//.FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
            ////data.ToDataTable().ToCsv(@"C:\Users\073\Desktop\wahhh60.csv");
            ////FactorAnalysis.Analysis(data.ToDataItem(factor.ft.ToString(), "Return").ToArray(),bUseIntercept:true);
            //LinearRegressionModel linearRegressionModel = new LinearRegressionModel { UseIntercept = true };
            //linearRegressionModel.SetVariableName("Return", sc.SignalNames.ToArray());
            //var data1 = data.DropNA();//.Select(dr => { for (int i = 0; i < dr.ItemArray.Count(); i++) if (Double.IsNaN((double)dr.ItemArray.ElementAt(i))) dr.ItemArray.ElementAt(i) = 0; });//.FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()], 0.05);
            //var fact = data1.Select(row => (double)row[factor.ft.ToString()]).FillNA(0).ToArray();
            //if (data1.Count() == 0) return Adaptability;
            //if (fact.Max() == fact.Min()) return Adaptability;
            //linearRegressionModel.TrainModel(data1);
            //Console.WriteLine(linearRegressionModel);
            //var lstPrd = linearRegressionModel.GetPrediction(data1).FillNA(0);
            //int count = 0;
            //var lstPos = lstPrd.Select(prd =>
            //{
            //    if (prd > 0.0006) { count++; return 1; }
            //    else if (prd < -0.0006) { count++; return -1; }
            //    else return 0;
            //}).ToArray();
            //var lstRtnLongOnly = data1.Select(row => (double)row["Return"]).FillNA(0).ToArray();
            //var lstRtn = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
            //{
            //    return lstRtnLongOnly[i] * lstPos[i];
            //}).ToArray();
            //var ic = lstRtn.Cumsum().Last();
            //lstRtn.Cumsum().DrawLine();
            //Console.WriteLine("累计收益:" + ic);
            //Console.WriteLine("平均单笔收益:" + ic / count);
            List<DataRow> data1 = new List<DataRow>();
            List<DataRow> data_train = new List<DataRow>();
            Dictionary<string, LinearRegressionModel> models = new Dictionary<string, LinearRegressionModel>();
            var date = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
            codes.ForEach(code =>
            {
                var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
                sc.AddSignal(factor.ft.ToString(), factor, q => q.Symbol.StartsWith(code));
                sc.AddForward("Return", new ForwardBarReturn(forward));
                var ddd = sc.GenerateData(data_all[code]);

                //var sssss = ddd.Count();
                //ddd.ToDataTable().ToCsv(@"C:\Users\073\Desktop\hhaff.csv");
                ddd = ddd.DropNA();
                //sssss = ddd.Count();
                //FactorAnalysis.Analysis(ddd.ToDataItem(factor.ft.ToString(), "Return"), bUseIntercept: true);
                LinearRegressionModel lr = new LinearRegressionModel { UseIntercept = true };
                lr.SetVariableName("Return", factor.ft.ToString());
                //int cccc1 = ddd.Count();
                ddd = ddd.FilterOutlier(d => Math.Abs((double)d[factor.ft.ToString()]), 0.001);
                int cccc = ddd.Count();
                int ii = 0;
                //var ddd1 = ddd.ToDataItem(factor.ft.ToString(),"Return").FilterOutlier(0.001);
                if (cccc > 50000)
                {
                    lr.TrainModel(ddd);
                    ddd.ForEach(dr =>
                    {
                        if (ii <= 0) { dr.Table.Columns.Add("Code"); dr.Table.Columns.Add("Prediction"); }
                        ii++;
                        dr["Code"] = code;
                        dr["Prediction"] = lr.GetPrediction(dr);
                        data1.Add(dr);
                    });
                    models.Add(code, lr);
                }
                else
                {
                    ddd.ForEach(dr =>
                    {
                        if (ii <= 0) { dr.Table.Columns.Add("Code"); dr.Table.Columns.Add("Prediction"); }
                        ii++;
                        dr["Code"] = code;
                        dr["Prediction"] = Double.NaN;
                        data1.Add(dr);
                    });
                }
                code_factor_data.Add(code, ddd.ToArray());
            });
            var ic_all = codes.SelectMany(code =>
            {
                var icss = code_factor_data[code].RollingNonOverlap(7200).Select(dt =>
                {
                    var a = dt.Select(x => Convert.ToDouble(x["Prediction"])).FillNA(0).ToArray();
                    var b = dt.Select(x => (double)x["Return"]).FillNA(0).ToArray();
                    var co = Correlation.Pearson(a, b);
                    return co;
                }).ToArray();
                return icss;
            }).ToArray();

            //var ic_all = data_all.RollingNonOverlap(10).Select(data_test_p =>
            //{
            //    var data_test = data_test_p.SelectMany(d =>
            //    {
            //        var data_temp1 = codes.SelectMany(code =>
            //        {
            //            var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            //            sc.AddSignal(factor.ft.ToString(), factor, q => q.Symbol.StartsWith(code));
            //            sc.AddForward("Return", new ForwardBarReturn(forward));
            //            int i = 0;
            //            var data_temp = sc.GenerateData(d.Value[code]).ToArray().Select(dr =>
            //            {
            //                if (i <= 0) { dr.Table.Columns.Add("Code"); dr.Table.Columns.Add("Prediction"); }
            //                i++;
            //                if (models.Keys.Contains(code))
            //                {
            //                    dr["Prediction"] = models[code].GetPrediction(dr);
            //                }
            //                else dr["Prediction"] = Double.NaN;
            //                dr["Code"] = code;
            //                data1.Add(dr);
            //                return dr;
            //            }).ToArray();
            //            sc.Reset();
            //            return data_temp;
            //        }).ToArray();
            //        return data_temp1;
            //    }).ToArray();

            //    //var count = data_train.Count();
            //    //data_train = data_train.DropNA().ToList();
            //    //double count1 = 0;
            //    //if (count == 0) count1 = 0;
            //    //else if (data_train.DropNA().Count() == 0)count1 = 0 ;
            //    //else count1 = data_train.DropNA().Select(dr => { if ((double)dr[factor.ft.ToString()] == 0) return 0; else return 1; }).Cumsum().Last();
            //    double co;
            //    data_test.DropNA();
            //    //if (count1 < 1500) co = 0.0;
            //    //else
            //    //{
            //    //    LinearRegressionModel lr = new LinearRegressionModel { UseIntercept = true };
            //    //    lr.SetVariableName("Return", factor.ft.ToString());
            //    //    lr.TrainModel(data_train);
            //    //    var prd = lr.GetPrediction(data_test).ToArray();
            //    //    int i = 0;
            //    //    data_test = data_test.Select(dr =>
            //    //    {
            //    //        dr["Prediction"] = prd[i];
            //    //        i++;
            //    //        return dr;
            //    //    }).DropNA().ToArray();
            //    //    //FactorAnalysis.Analysis(data_test.DropNA().ToDataItem("Prediction", "Return"));
            //    //    //data_test = data_test.ToArray();//FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
            //    //    //var c = data_test.Select(dr => (double)dr["Prediction"]).ToArray();
            //    var a = data_test.Select(x => Convert.ToDouble(x["Prediction"])).FillNA(0).ToArray();
            //    var b = data_test.Select(x => (double)x["Return"]).FillNA(0).ToArray();
            //    if (a.Count() > 1500) co = Correlation.Pearson(a, b);
            //    else co = 0.0;
            //    //}
            //    //data_train.Clear();
            //    //foreach (DataRow dr in data_test) data_train.Add(dr);
            //    int tttt = 0;
            //    if (Double.IsNaN(co)) return 0;
            //    return co;
            //}).ToArray();
            List<double> ics1 = new List<double>();
            //var ics1 = ic_all.ToList();
            for (int i = 0; i < ic_all.Count(); i++)
            {
                if (ic_all[i] != 0)
                {
                    ics1.Add(ic_all[i]);
                }
            }
            ic_all = ics1.ToArray();
            var ic_acc = ic_all.Cumsum().ToArray();
            int j = 0;
            var an = ic_acc.Select(icc =>
            {
                j++;
                return new DataItem(j, icc);
            }).ToArray();
            //List<int> axisx = new List<int>();
            //for (int i = 1; i <= axisx.Count();i++) axisx.Add(i);DataItem
            var ic = 0.0; var sharp_rate = 0.0;
            if (ic_all.Count() > 5)
            {
                //Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares lrm = new Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares() { UseIntercept = true };
                //var rere = lrm.Learn(an.Select(aa => aa.indicator).ToArray(), an.Select(aa => aa.predicator).ToArray());
                ic = ic_all.Average();
                sharp_rate = ic_all.Std();
            }
            //ic_acc.DrawLine();
            Console.WriteLine(factor.ft + ":" + ic);
            Console.WriteLine(factor.ft + ":" + sharp_rate);
            //data1.ToDataTable().ToCsv(@"C:\Users\073\Desktop\hhaff1.csv");
            var data = data1.DropNA().ToArray();
            //data1.ToDataTable().ToCsv(@"C:\Users\073\Desktop\"+factor.ft.ToString().Count()+".csv");
            //FactorAnalysis.Analysis(data.ToDataItem("Prediction", "Return"));
            //ic_acc.DrawLine();
            //sc.Reset();
            //var data = sc.GenerateData(GetBarData(datelist)).ToArray();
            //var data = data_all.SelectMany(dd =>
            //{
            //    var data_temp2 = codes.SelectMany(code =>
            //    {
            //        var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            //        sc.AddSignal(factor.ft.ToString(), factor, q => q.Symbol.StartsWith(code));
            //        sc.AddForward("Return", new ForwardBarReturn(forward));
            //        int i = 0;
            //        var data_temp = sc.GenerateData(dd.Value[code]).ToArray().Select(dr =>
            //        {
            //            if (i <= 0) dr.Table.Columns.Add("Code");
            //            i++;
            //            dr["Code"] = code;
            //            return dr;
            //        }).ToArray();
            //        sc.Reset();
            //        return data_temp;
            //    }).ToArray();
            //    return data_temp2;
            //}).ToArray();
            int replace = 0;
            bool conflict = false;
            int removed = 0;
            List<Factor_Tree> temp_list = new List<Factor_Tree>();
            //if (Math.Abs(ic) > 0.01 &&  Math.Abs(ic) > 0 && Math.Abs(sharp_rate) < 0.03)
            //{
            //    if (!Adaptability.Keys.Contains(factor.ft))
            //    {
            //        Adaptability.Add(factor.ft, ic);
            //        //original_generation.Add(factor.ft, data.ToList());
            //        Console.WriteLine("增加：" + factor.ft);
            //    }
            //}
            if (Adaptability.Count == 50)
            {
                if (Math.Abs(ic) > 0.01 && Math.Abs(ic) > Math.Abs(Adaptability.Values.Last()) && Math.Abs(ic) > 0 && Math.Abs(sharp_rate) < 0.03)
                {
                    Enumerable.Range(0, Adaptability.Keys.Count).AsParallel().ForAll(a =>
                    {
                        var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => ((DateTime)key1["DateTime"], (string)key1["Code"]), key2 => ((DateTime)key2["DateTime"], (string)key2["Code"]), (re1, re2) => Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re1["Code"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
                        double corre = Correlation.Pearson(data2.Select(d => d.Item2), data2.Select(d => d.Item4));
                        if (Double.IsNaN(corre))
                        {
                            int yy = 0;
                        }
                        if (Math.Abs(corre) > 0.4 && Math.Abs(corre) <= 1.1)
                        {
                            conflict = true;
                            if (Math.Abs(ic) > Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1000)
                            {
                                replace++;
                                //Console.WriteLine("替换掉：" + a.Key);
                                temp_list.Add(Adaptability.Keys.ElementAt(a));
                                //removed++;
                                //if (removed > 1) { return new KeyValuePair<Factor_Tree, double>(a.Key, 0); }
                                //else { Console.WriteLine("替换成：" + factor.ft); original_generation.Remove(a.Key);
                                //    original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList()); return new KeyValuePair<Factor_Tree, double>(factor.ft, ic); }
                            }
                            else replace = -1000;
                        }
                    });
                    if (replace > 0)
                    {
                        foreach (Factor_Tree factort in temp_list)
                        {
                            Adaptability.Remove(factort);
                            original_generation.Remove(factort);
                            Console.WriteLine("替换掉：" + factort);
                        }
                        Console.WriteLine("替换成：" + factor.ft);
                        Adaptability.Add(factor.ft, ic);
                        original_generation.Add(factor.ft, data.ToList());
                    }
                    //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
                    else if (!conflict)
                    {
                        if (Adaptability.Count != 50)
                        {
                            Adaptability.Add(factor.ft, ic);
                            original_generation.Add(factor.ft, data.ToList());
                            Console.WriteLine("增加：" + factor.ft);
                        }
                        else
                        {
                            Factor_Tree erased = Adaptability.Keys.Last();
                            Adaptability.Remove(erased);
                            original_generation.Remove(erased);
                            Console.WriteLine("删除末位的：" + erased);
                            Adaptability.Add(factor.ft, ic);
                            original_generation.Add(factor.ft, data.ToList());
                            Console.WriteLine("增加：" + factor.ft);
                        }
                    }
                }
            }
            else
            {
                if (Math.Abs(ic) > 0 && Math.Abs(sharp_rate) < 0.03 && Math.Abs(ic) > 0.01)
                {
                    Enumerable.Range(0, Adaptability.Keys.Count).AsParallel().ForAll(a =>
                    {
                        var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => key1["DateTime"], key2 => key2["DateTime"], (re1, re2) => Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re2["DateTime"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
                        double corre = Correlation.Pearson(data2.Select(d => d.Item2), data2.Select(d => d.Item4));
                        if (Double.IsNaN(corre))
                        {
                            int yy = 0;
                        }
                        if (Double.IsNaN(corre))
                        {
                            int x = 0;
                        }
                        if (Math.Abs(corre) > 0.4 && Math.Abs(corre) <= 1.1)
                        {
                            conflict = true;
                            if (Math.Abs(ic) > Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1000)
                            {
                                replace++;
                                //Console.WriteLine("替换掉：" + a.Key);
                                temp_list.Add(Adaptability.Keys.ElementAt(a));
                            }
                            else replace = -1000;
                        }
                    });
                    if (replace > 0)
                    {
                        foreach (Factor_Tree factort in temp_list)
                        {
                            Adaptability.Remove(factort);
                            original_generation.Remove(factort);
                            Console.WriteLine("替换掉：" + factort);
                        }
                        Console.WriteLine("替换成：" + factor.ft);
                        Adaptability.Add(factor.ft, ic);
                        original_generation.Add(factor.ft, data.ToList());
                    }
                    //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
                    else if (!conflict)
                    {
                        if (Adaptability.Count < 50)
                        {
                            Adaptability.Add(factor.ft, ic);
                            original_generation.Add(factor.ft, data.ToList());
                            Console.WriteLine("增加：" + factor.ft);
                        }
                        else
                        {
                            throw new Exception("逻辑有误");
                        }
                    }
                }
            }
            Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
            return Adaptability;
        }
        public Dictionary<Factor_Tree, double> Adaptability_Test1(Factor factor, String start_date, String end_date, int forward, String code)
        {
            var datelist = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
            var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
            sc.AddSignal(factor.ft.ToString(), factor, q => q.Symbol.StartsWith(code));
            sc.AddForward("Return", new ForwardBarReturn(forward));
            IEnumerable<Bar> GetBarData(string[] datelist1)
            {
                return datelist1.SelectMany(date =>
                {
                    return DataCleanDaily(code, date);
                }).ToArray();
            }
            //var data = sc.GenerateData(GetBarData(datelist)).ToArray();//.FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
            ////data.ToDataTable().ToCsv(@"C:\Users\073\Desktop\wahhh60.csv");
            ////FactorAnalysis.Analysis(data.ToDataItem(factor.ft.ToString(), "Return").ToArray(),bUseIntercept:true);
            //LinearRegressionModel linearRegressionModel = new LinearRegressionModel { UseIntercept = true };
            //linearRegressionModel.SetVariableName("Return", sc.SignalNames.ToArray());
            //var data1 = data.DropNA();//.Select(dr => { for (int i = 0; i < dr.ItemArray.Count(); i++) if (Double.IsNaN((double)dr.ItemArray.ElementAt(i))) dr.ItemArray.ElementAt(i) = 0; });//.FilterOutlier(x => (double)x[factor_Forest.Forest[0].ToString()], 0.05);
            //var fact = data1.Select(row => (double)row[factor.ft.ToString()]).FillNA(0).ToArray();
            //if (data1.Count() == 0) return Adaptability;
            //if (fact.Max() == fact.Min()) return Adaptability;
            //linearRegressionModel.TrainModel(data1);
            //Console.WriteLine(linearRegressionModel);
            //var lstPrd = linearRegressionModel.GetPrediction(data1).FillNA(0);
            //int count = 0;
            //var lstPos = lstPrd.Select(prd =>
            //{
            //    if (prd > 0.0006) { count++; return 1; }
            //    else if (prd < -0.0006) { count++; return -1; }
            //    else return 0;
            //}).ToArray();
            //var lstRtnLongOnly = data1.Select(row => (double)row["Return"]).FillNA(0).ToArray();
            //var lstRtn = Enumerable.Range(0, lstRtnLongOnly.Count()).Select(i =>
            //{
            //    return lstRtnLongOnly[i] * lstPos[i];
            //}).ToArray();
            //var ic = lstRtn.Cumsum().Last();
            //lstRtn.Cumsum().DrawLine();
            //Console.WriteLine("累计收益:" + ic);
            //Console.WriteLine("平均单笔收益:" + ic / count);
            var ic_all = datelist.RollingNonOverlap(30).Select(datelist_test =>
             {
                 var data_test = sc.GenerateData(GetBarData(datelist_test)).ToArray();
                 var count = data_test.Count();
                 if (count == 0) return 0;
                 var count1 = data_test.Select(dr => { if ((double)dr[factor.ft.ToString()] == 0) return 0; else return 1; }).Cumsum().Last();
                 double co;
                 if (count1 < count * 0.6) co = 0.0;
                 else
                 {
                     //data_test = data_test.ToArray();//FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
                     co = Correlation.Pearson(data_test.Select(x => (double)x[factor.ft.ToString()]).FillNA(0).ToArray(), data_test.Select(x => (double)x["Return"]).FillNA(0).ToArray());
                 }
                 if (Double.IsNaN(co)) return 0;
                 return co;
             }).ToArray();
            var ics1 = ic_all.ToList();
            for (int i = 0; i < ics1.Count(); i++) { if (ics1[i] == 0) ics1.RemoveAt(i); }
            ic_all = ics1.ToArray();
            var ic_acc = ic_all.Cumsum().ToArray();
            int j = 0;
            var an = ic_acc.Select(icc =>
            {
                j++;
                return new DataItem(j, icc);
            }).ToArray();
            //List<int> axisx = new List<int>();
            //for (int i = 1; i <= axisx.Count();i++) axisx.Add(i);DataItem
            Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares lrm = new Accord.Statistics.Models.Regression.Linear.OrdinaryLeastSquares() { UseIntercept = true };
            var rere = lrm.Learn(an.Select(aa => aa.indicator).ToArray(), an.Select(aa => aa.predicator).ToArray());
            var ic = rere.Slope;
            var sharp_rate = ic_all.Average() / ic_all.Std();
            Console.WriteLine(factor.ft+":"+ic);
            Console.WriteLine(factor.ft+":"+sharp_rate);
            //ic_acc.DrawLine();
            sc.Reset();
            var data = sc.GenerateData(GetBarData(datelist)).ToArray();
            //FactorAnalysis.Analysis(data.ToDataItem(factor.ft.ToString(),"Return"));
            int replace = 0;
            bool conflict = false;
            int removed = 0;
            List<Factor_Tree> temp_list = new List<Factor_Tree>();
            if (Adaptability.Count == 50)
            {
                if (Math.Abs(ic) >Math.Abs(Adaptability.Values.Last()) && Math.Abs(ic) >0&&Math.Abs(sharp_rate)>0.5)
                {
                    Enumerable.Range(0, Adaptability.Keys.Count).AsParallel().ForAll(a =>
                    {
                        var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => key1["DateTime"], key2 => key2["DateTime"], (re1, re2) => Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re2["DateTime"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
                        double corre = Correlation.Pearson(data2.Select(d => d.Item2), data2.Select(d => d.Item4));
                        if (Double.IsNaN(corre))
                        {
                            int x = 0;
                        }
                        if (Math.Abs(corre) > 0.4 && Math.Abs(corre) <= 1.1)
                        {
                            conflict = true;
                            if (Math.Abs(ic) >Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1000)
                            {
                                replace++;
                                //Console.WriteLine("替换掉：" + a.Key);
                                temp_list.Add(Adaptability.Keys.ElementAt(a));
                                //removed++;
                                //if (removed > 1) { return new KeyValuePair<Factor_Tree, double>(a.Key, 0); }
                                //else { Console.WriteLine("替换成：" + factor.ft); original_generation.Remove(a.Key);
                                //    original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList()); return new KeyValuePair<Factor_Tree, double>(factor.ft, ic); }
                            }
                            else replace = -1000;
                        }
                    });
                    if (replace > 0)
                    {
                        foreach (Factor_Tree factort in temp_list)
                        {
                            Adaptability.Remove(factort);
                            original_generation.Remove(factort);
                            Console.WriteLine("替换掉：" + factort);
                        }
                        Console.WriteLine("替换成：" + factor.ft);
                        Adaptability.Add(factor.ft, ic);
                        original_generation.Add(factor.ft, data.ToList());
                    }
                    //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
                    else if (!conflict)
                    {
                        if (Adaptability.Count != 50)
                        {
                            Adaptability.Add(factor.ft, ic);
                            original_generation.Add(factor.ft, data.ToList());
                            Console.WriteLine("增加：" + factor.ft);
                        }
                        else
                        {
                            Factor_Tree erased = Adaptability.Keys.Last();
                            Adaptability.Remove(erased);
                            original_generation.Remove(erased);
                            Console.WriteLine("删除末位的：" + erased);
                            Adaptability.Add(factor.ft, ic);
                            original_generation.Add(factor.ft, data.ToList());
                            Console.WriteLine("增加：" + factor.ft);
                        }
                    }
                }
            }
            else
            {
                if (Math.Abs(ic) >0 && Math.Abs(sharp_rate) > 0.5)
                {
                    Enumerable.Range(0, Adaptability.Keys.Count).AsParallel().ForAll(a =>
                    {
                        var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => key1["DateTime"], key2 => key2["DateTime"], (re1, re2) => Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re2["DateTime"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
                        double corre = Correlation.Pearson(data2.Select(d => d.Item2), data2.Select(d => d.Item4));
                        if (Double.IsNaN(corre))
                        {
                            int x = 0;
                        }
                        if (Math.Abs(corre) > 0.4 && Math.Abs(corre) <= 1.1)
                        {
                            conflict = true;
                            if (Math.Abs(ic) > Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1000)
                            {
                                replace++;
                                //Console.WriteLine("替换掉：" + a.Key);
                                temp_list.Add(Adaptability.Keys.ElementAt(a));
                            }
                            else replace = -1000;
                        }
                    });
                    if (replace > 0)
                    {
                        foreach (Factor_Tree factort in temp_list)
                        {
                            Adaptability.Remove(factort);
                            original_generation.Remove(factort);
                            Console.WriteLine("替换掉：" + factort);
                        }
                        Console.WriteLine("替换成：" + factor.ft);
                        Adaptability.Add(factor.ft, ic);
                        original_generation.Add(factor.ft, data.ToList());
                    }
                    //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
                    else if (!conflict)
                    {
                        if (Adaptability.Count < 50)
                        {
                            Adaptability.Add(factor.ft, ic);
                            original_generation.Add(factor.ft, data.ToList());
                            Console.WriteLine("增加：" + factor.ft);
                        }
                        else
                        {
                            throw new Exception("逻辑有误");
                        }
                    }
                }
            }
            Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
            return Adaptability;
        }
        //public Dictionary<Factor_Tree,double> Adaptability_Test1(Factor factor,String start_date, String end_date,int forward,String code)
        //{
        //    bool bb = false;
        //    foreach(Factor_Tree f in Adaptability.Keys)
        //    {
        //        String s1 = factor.ft.ToString();
        //        String s2 = f.ToString();
        //        if (s1.Equals(s2))
        //        {
        //            bb = true;
        //            break;
        //        }
        //    }
        //    if (bb) return Adaptability;
        //    var datelist = DatePosInfo.GetDateListTradingRange(start_date, end_date).ToArray();
        //    var sc = new SignalCollection<Bar>(q => q.Symbol.StartsWith(code), null);
        //    sc.AddSignal(factor.ft.ToString(), factor, q => q.Symbol.StartsWith(code));
        //    sc.AddForward("Return", new ForwardBarReturn(forward));
        //    IEnumerable<Bar> GetBarData(string[] datelist1)
        //    {
        //        return datelist1.SelectMany(date =>
        //        {
        //            return DataCleanDaily(code, date);
        //        }).ToArray();
        //    }

        //    //data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToArray().ToCsv(@"C:\Users\073\Desktop\try1\3.csv", "header", data.Select(x => (double)x["Return"]).FillNA(0).ToArray());
        //    //data1.ToCsv(@"C:\Users\073\Desktop\56663.csv");
        //    //var ic_all = datelist.Rolling(30,10).Select(datelist_test =>
        //    //{
        //    //    var data_test = sc.GenerateData(GetBarData(datelist_test)).ToArray();
        //    //    var count = data_test.Count();
        //    //    var count1 = data_test.Select(dr => { if ((double)dr[factor.ft.ToString()] == 0) return 0; else return 1; }).Cumsum().Last();
        //    //    double co;
        //    //    if (count1 < count * 0.6) co = 0.0;
        //    //    else
        //    //    {
        //    //        data_test = data_test.FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
        //    //        co = Correlation.Pearson(data_test.Select(x => (double)x[factor.ft.ToString()]).FillNA(0).ToArray(), data_test.Select(x => (double)x["Return"]).FillNA(0).ToArray());
        //    //    }
        //    //        return co;
        //    //}).ToArray();
        //    ////ic_all.Cumsum().DrawLine();
        //    ////Console.ReadKey();
        //    //var ic = ic_all.Average()/ic_all.Std();
        //    //var sharp_rate = ic / ic_all.Std();
        //    sc.Reset();
        //    var data = sc.GenerateData(GetBarData(datelist)).FilterOutlier(x => (double)x[factor.ft.ToString()], 0.05).ToArray();
        //    if (data.Count() == 0) return Adaptability;
            
        //    //var ic = ar.Rho;
        //    var ic = Correlation.Pearson(data.Select(x => (double)x[factor.ft.ToString()]).FillNA(0).ToArray(), data.Select(x => (double)x["Return"]).FillNA(0).ToArray());
        //    if (ic == 0||Double.IsNaN(ic)) return Adaptability;
        //    var ar = FactorAnalysis.Analysis(data.ToDataItem(factor.ft.ToString(), "Return"), bUseIntercept: true, bShowChart: false);
        //    var slope = ar.StdX * ar.slrAll.Slope;
        //    Console.WriteLine("IC:" + ic);
        //    if (!(Math.Abs(ic) > 0))
        //    {
        //        int x = 0;
        //    }
        //    int replace = 0;
        //    bool conflict = false;
        //    int removed = 0;
        //    List<Factor_Tree> temp_list = new List<Factor_Tree>();
        //    if (Math.Abs(ic) > 0.007 && Math.Abs(ic) <= 1.1 && Math.Abs(slope) > 0.00009)
        //    {
        //        if (Adaptability.Count == 50)
        //        {
        //            if (Math.Abs(ic) > Math.Abs(Adaptability.Values.Last()) && Math.Abs(ic) <= 1.1)
        //            {
        //                Enumerable.Range(0, Adaptability.Keys.Count).AsParallel().ForAll(a =>
        //                 {
        //                     var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => key1["DateTime"], key2 => key2["DateTime"], (re1, re2) => Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re2["DateTime"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
        //                     double corre = Correlation.Pearson(data2.Select(d => d.Item2), data2.Select(d => d.Item4));
        //                     if (Math.Abs(corre) > 0.6 && Math.Abs(corre) <= 1.1)
        //                     {
        //                         conflict = true;
        //                         if (Math.Abs(ic) > Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1.1)
        //                         {
        //                             replace++;
        //                            //Console.WriteLine("替换掉：" + a.Key);
        //                            temp_list.Add(Adaptability.Keys.ElementAt(a));
        //                            //removed++;
        //                            //if (removed > 1) { return new KeyValuePair<Factor_Tree, double>(a.Key, 0); }
        //                            //else { Console.WriteLine("替换成：" + factor.ft); original_generation.Remove(a.Key);
        //                            //    original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList()); return new KeyValuePair<Factor_Tree, double>(factor.ft, ic); }
        //                        }
        //                         else replace = -1000;
        //                     }
        //                 });
        //                if (replace > 0)
        //                {
        //                    foreach (Factor_Tree factort in temp_list)
        //                    {
        //                        Adaptability.Remove(factort);
        //                        original_generation.Remove(factort);
        //                        Console.WriteLine("替换掉：" + factort);
        //                    }
        //                    Console.WriteLine("替换成：" + factor.ft);
        //                    Adaptability.Add(factor.ft, ic);
        //                    original_generation.Add(factor.ft, data.ToList());
        //                }
        //                //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
        //                else if (!conflict)
        //                {
        //                    if (Adaptability.Count != 50)
        //                    {
        //                        Adaptability.Add(factor.ft, ic);
        //                        original_generation.Add(factor.ft, data.ToList());
        //                        Console.WriteLine("增加：" + factor.ft);
        //                    }
        //                    else
        //                    {
        //                        Factor_Tree erased = Adaptability.Keys.Last();
        //                        Adaptability.Remove(erased);
        //                        original_generation.Remove(erased);
        //                        Console.WriteLine("删除末位的：" + erased);
        //                        Adaptability.Add(factor.ft, ic);
        //                        original_generation.Add(factor.ft, data.ToList());
        //                        Console.WriteLine("增加：" + factor.ft);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (Math.Abs(ic) <= 1.1)
        //            {
        //                Enumerable.Range(0, Adaptability.Keys.Count).AsParallel().ForAll(a =>
        //                {
        //                    var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => key1["DateTime"], key2 => key2["DateTime"], (re1, re2) => Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re2["DateTime"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
        //                    double corre = Correlation.Pearson(data2.Select(d => d.Item2), data2.Select(d => d.Item4));
        //                    if (Math.Abs(corre) > 0.6 && Math.Abs(corre) <= 1.1)
        //                    {
        //                        conflict = true;
        //                        if (Math.Abs(ic) > Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1.1)
        //                        {
        //                            replace++;
        //                            //Console.WriteLine("替换掉：" + a.Key);
        //                            temp_list.Add(Adaptability.Keys.ElementAt(a));
        //                        }
        //                        else replace = -1000;
        //                    }
        //                });
        //                if (replace > 0)
        //                {
        //                    foreach (Factor_Tree factort in temp_list)
        //                    {
        //                        Adaptability.Remove(factort);
        //                        original_generation.Remove(factort);
        //                        Console.WriteLine("替换掉：" + factort);
        //                    }
        //                    Console.WriteLine("替换成：" + factor.ft);
        //                    Adaptability.Add(factor.ft, ic);
        //                    original_generation.Add(factor.ft, data.ToList());
        //                }
        //                //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
        //                else if (!conflict)
        //                {
        //                    if (Adaptability.Count < 50)
        //                    {
        //                        Adaptability.Add(factor.ft, ic);
        //                        original_generation.Add(factor.ft, data.ToList());
        //                        Console.WriteLine("增加：" + factor.ft);
        //                    }
        //                    else
        //                    {
        //                        throw new Exception("逻辑有误");
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    //if (Adaptability.Count == 50)
        //    //{
        //    //    if (Math.Abs(ic) > Math.Abs(Adaptability.Values.Last()) && Math.Abs(ic) <= 1.1)
        //    //    {
        //    //        Enumerable.Range(0,Adaptability.Keys.Count).AsParallel().ForAll(a =>
        //    //        {
        //    //            var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => key1["DateTime"], key2 => key2["DateTime"],(re1,re2)=>Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re2["DateTime"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
        //    //            double corre = Correlation.Pearson(data2.Select(d=>d.Item2),data2.Select(d=>d.Item4));
        //    //            if (Math.Abs(corre) > 0.6 && Math.Abs(corre) <= 1.1)
        //    //            {
        //    //                conflict = true;
        //    //                if (Math.Abs(ic) > Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1.1)
        //    //                {
        //    //                    replace ++;
        //    //                    //Console.WriteLine("替换掉：" + a.Key);
        //    //                    temp_list.Add(Adaptability.Keys.ElementAt(a));
        //    //                    //removed++;
        //    //                    //if (removed > 1) { return new KeyValuePair<Factor_Tree, double>(a.Key, 0); }
        //    //                    //else { Console.WriteLine("替换成：" + factor.ft); original_generation.Remove(a.Key);
        //    //                    //    original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList()); return new KeyValuePair<Factor_Tree, double>(factor.ft, ic); }
        //    //                }
        //    //                else replace = -1000;
        //    //            }
        //    //        });
        //    //        if (replace>0)
        //    //        {
        //    //            foreach (Factor_Tree factort in temp_list)
        //    //            {
        //    //                Adaptability.Remove(factort);
        //    //                original_generation.Remove(factort);
        //    //                Console.WriteLine("替换掉：" + factort);
        //    //            }
        //    //            Console.WriteLine("替换成：" + factor.ft);
        //    //            Adaptability.Add(factor.ft, ic);
        //    //            original_generation.Add(factor.ft, data.ToList());
        //    //        }
        //    //        //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
        //    //        else if (!conflict)
        //    //        {
        //    //            if (Adaptability.Count != 50)
        //    //            {
        //    //                Adaptability.Add(factor.ft, ic);
        //    //                original_generation.Add(factor.ft, data.ToList());
        //    //                Console.WriteLine("增加：" + factor.ft);
        //    //            }
        //    //            else
        //    //            {
        //    //                Factor_Tree erased = Adaptability.Keys.Last();
        //    //                Adaptability.Remove(erased);
        //    //                original_generation.Remove(erased);
        //    //                Console.WriteLine("删除末位的：" + erased);
        //    //                Adaptability.Add(factor.ft, ic);
        //    //                original_generation.Add(factor.ft, data.ToList());
        //    //                Console.WriteLine("增加：" + factor.ft);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    if (Math.Abs(ic) <= 1.1)
        //    //    {
        //    //        Enumerable.Range(0, Adaptability.Keys.Count).AsParallel().ForAll(a =>
        //    //        {
        //    //            var data2 = data.Join(original_generation[Adaptability.Keys.ElementAt(a)], key1 => key1["DateTime"], key2 => key2["DateTime"], (re1, re2) => Tuple.Create(re1["DateTime"], (double)re1[factor.ft.ToString()], re2["DateTime"], (double)re2[Adaptability.Keys.ElementAt(a).ToString()]));
        //    //            double corre = Correlation.Pearson(data2.Select(d => d.Item2), data2.Select(d => d.Item4));
        //    //            if (Math.Abs(corre) > 0.6 && Math.Abs(corre) <= 1.1)
        //    //            {
        //    //                conflict = true;
        //    //                if (Math.Abs(ic) > Math.Abs(Adaptability[Adaptability.Keys.ElementAt(a)]) && Math.Abs(ic) <= 1.1)
        //    //                {
        //    //                    replace ++;
        //    //                //Console.WriteLine("替换掉：" + a.Key);
        //    //                temp_list.Add(Adaptability.Keys.ElementAt(a));
        //    //                }
        //    //                else replace = -1000;
        //    //            }
        //    //        });
        //    //        if (replace>0)
        //    //        {
        //    //            foreach (Factor_Tree factort in temp_list)
        //    //            {
        //    //                Adaptability.Remove(factort);
        //    //                original_generation.Remove(factort);
        //    //                Console.WriteLine("替换掉：" + factort);
        //    //            }
        //    //            Console.WriteLine("替换成：" + factor.ft);
        //    //            Adaptability.Add(factor.ft, ic);
        //    //            original_generation.Add(factor.ft, data.ToList());
        //    //        }
        //    //        //Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
        //    //        else if (!conflict)
        //    //        {
        //    //            if (Adaptability.Count < 50)
        //    //            {
        //    //                Adaptability.Add(factor.ft, ic);
        //    //                original_generation.Add(factor.ft, data.ToList());
        //    //                Console.WriteLine("增加：" + factor.ft);
        //    //            }
        //    //            else
        //    //            {
        //    //                throw new Exception("逻辑有误");
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    Adaptability = Adaptability.OrderByDescending(p => Math.Abs(p.Value)).ToDictionary(p => p.Key, p => p.Value);
        //    //original_generation = original_generation.AsParallel().Select(d =>
        //    //{
        //    //    double corre = Correlation.Pearson(data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToArray(),d.Value);
        //    //    var ic_o = Correlation.Pearson(data.Select(x => (double)x["Return"]).FillNA(0).ToArray(), d.Value);
        //    //    if (Math.Abs(corre) > 0.6&& Math.Abs(corre) <=1)
        //    //    {
        //    //        conflict = true;
        //    //        if (Math.Abs(ic) > Math.Abs(ic_o)&&Math.Abs(ic) <= 1)
        //    //        {
        //    //            Adaptability.Remove(d.Key);
        //    //            Console.WriteLine("删除：" + d.Key);
        //    //            replace = true;
        //    //            removed++;
        //    //            if (removed>1) return d ;
        //    //            else return new KeyValuePair<Factor_Tree,List<double>>(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList()) ;
        //    //        }
        //    //    }
        //    //    return d;
        //    //}).ToDictionary(d => d.Key,d =>d.Value );
        //    //if (conflict)
        //    //{
        //    //    if (replace)
        //    //    {
        //    //        //original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList());
        //    //        Adaptability.Add(factor.ft, ic);
        //    //        Console.WriteLine("替换:" + factor.ft);
        //    //    }
        //    //}
        //    //else if(Math.Abs(ic)> 0&&Math.Abs(ic) <= 1)
        //    //{
        //    //    original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList());
        //    //    Adaptability.Add(factor.ft, ic);
        //    //    Console.WriteLine("增加:" + factor.ft);
        //    //}
        //    return Adaptability;
        //    //for (int i = 0; i < original_generation.Count; i++)
        //    //{
        //    //    double corre = Correlation.Pearson(data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToArray(), original_generation.Values.ElementAt(i));
        //    //    var ic_o = Correlation.Pearson(data.Select(x => (double)x["Return"]).FillNA(0).ToArray(),original_generation.Values.ElementAt(i));
        //    //    if (Math.Abs(corre) > 0.6)
        //    //    {
        //    //        if (Math.Abs(ic) > Math.Abs(ic_o))
        //    //        {
        //    //            original_generation.Remove(original_generation.Keys.ElementAt(i));
        //    //            original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList());
        //    //            Adaptability .Remove(Adaptability .Keys.ElementAt(i));
        //    //            Adaptability .Add(factor.ft, ic);
        //    //        }
        //    //         return Adaptability;

        //    //    }
        //    //}
        //    //var ic = data.RollingNonOverlap(250).Select(data_test =>
        //    //{
        //    //    return Correlation.Pearson(data_test.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToArray(), data_test.Select(x => (double)x["Return"]).FillNA(0).ToArray());
        //    //}).ToArray();
        //    //ic.Cumsum().ToArray().DrawLine();
        //    //var avg = ic.Average();
        //    //var sharp_rate = ic.Average() / ic.Std();
        //    //Console.WriteLine("ic:" + ic );
        //    ////Console.WriteLine("sharp rate:" + sharp_rate);
        //    //if(!(ic>-1&&ic < 1))
        //    //{
        //    //    int x = 0;
        //    //}
        //    //if(Math.Abs(ic) > 0)
        //    //{
        //    //    original_generation.Add(factor.ft, data.Select(x => (double)x["Alpha" + factor.ft.Number]).FillNA(0).ToList());
        //    //    Adaptability.Add(factor.ft, ic);
        //    //}
        //    //return Adaptability;
        //}
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
