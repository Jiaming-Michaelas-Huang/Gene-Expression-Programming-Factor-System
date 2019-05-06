using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Acsy.Common.Utility;
using Acsy.Model.Analysis;

namespace GEP_Factor_System
{
    class Main_Program
    {
        static void Main(string[] args)
        {
            List<double> hhhhh = new List<double>();
            for (int original_generation = 10; original_generation < 50; original_generation++)
            {
                FileStream fs = new FileStream(@"C:\Users\073\Desktop\gene\" + "002016" + @"\gene_first_generations_adaptability" + original_generation + ".txt", FileMode.OpenOrCreate);
                StreamReader sr = new StreamReader(fs);
                String line;
                int iii = 0;
                double hall = 0;
                List<double> hh = new List<double>();
                while (iii < 50)
                {
                    line = sr.ReadLine();
                    if (line == null) break;
                    String tree = line.Split('%')[0];
                    String corr = line.Split('%')[1];
                    hh.Add(corr.ToDouble());
                    iii++;
                }
                hhhhh.Add(hh.Average());
            }
            hhhhh.DrawLine();
            //FileStream fs1 = new FileStream(@"C:\Users\073\Desktop\codelist1.txt", FileMode.Open);
            //StreamReader sr1 = new StreamReader(fs1);
            //String line1;
            //List<string> codes1 = new List<string>();
            //while ((line1 = sr1.ReadLine()) != null)
            //{
            //    line1 = line1.PadLeft(6, '0');
            //    codes1.Add(line1);
            //}
            //List<DataItem> pre_re = new List<DataItem>();
            //codes1.ForEach(code1 =>
            //{
            //    var datatable = new DataTable().FromCsv(@"C:\Users\073\Desktop\wppa4\" + code1 + ".csv").Select().DropNA();
            //    double lstpre = 0;
            //    foreach (DataRow dr in datatable)
            //    {
            //        double pstpre=0;
            //        if (Double.TryParse(dr["Prediction"].ToString(), out pstpre)) ;
            //        else continue;
            //        double pstre=0;
            //        if (Double.TryParse(dr["Return"].ToString(), out pstre)) ;
            //        else continue;
            //        if (lstpre != pstpre)
            //        {
            //            pre_re.Add(new DataItem(pstpre,pstre));
            //        }
            //        lstpre = pstpre;
            //    }
            //    Console.WriteLine(code1);
            //});
            //FactorAnalysis.Analysis(pre_re);
            //Factor_Tree factor_Tree = new Factor_Tree("((Volume<Min(ArgMax(High,25),20))?High:Close)");
            ////Factor_Tree factor_Tree = new Factor_Tree("Avg(((Close/Delay(Close,15))-1),75)");
            ////Factor_Tree factor_Tree = new Factor_Tree("((Rank(Volume,150)*(1-Rank(((Close+High)-Low)),75))*(1-Rank(((Close-Delay(Close,15))/Delay(Close,15)),75)))");
            //Factor factor = new Factor(factor_Tree);
            //FileStream fs = new FileStream(@"C:\Users\073\Desktop\codelista.txt", FileMode.Open);
            //StreamReader sr = new StreamReader(fs);
            //String line;
            //int iii = 0;
            //List<string> codes = new List<string>();
            //while ((line = sr.ReadLine()) != null)
            //{
            //    line = line.PadLeft(6, '0');
            //    codes.Add(line);
            //}
            //int generation = 300;

            while (true)
            {
                Console.WriteLine("输入股票代码：");
                string code = Console.ReadLine();
                //Factor_Tree ft = new Factor_Tree("avg std delta std Volume Sign || Turnover Low Low Low Turnover Low High High Low Open High Low Volume Open Open 15 10 25 10 5 15 10", 7,15,7);
                string[] code1 = { code };
                Adaptability_System ads = new Adaptability_System(0, 0, "20150101", "20170101", code1);
                //ads.Adaptability_Test1(new Factor(ft),"20150101","20170101",40,code);
                Factor_Forest ff = new Factor_Forest();
                Console.WriteLine("输入代数：");
                int generation = Convert.ToInt32(Console.ReadLine());
                //int generation = 35;
                var filepath = @"C:\Users\073\Desktop\gene\"+code+@"\";
                new FileInfo(filepath).Directory.Create();
                ////    //Factor_Tree ft = new Factor_Tree("? + argmin max * high close open volume turnover close high low close high close 20 10 24 34 12 43",5,11,5);
                ////    //Console.WriteLine(ft.ToString());


                ////    ////////////ads.Adaptability_Test1(factor, "20160101", "20170101", 20, "600600");
                if (generation == 1)
                {
                    ff.Generate_Trees(100);

                    ff.Aaptability_Test(ads, code1);
                    ff.output2(generation, code);
                    ff.output1(generation, code);
                }
                Console.WriteLine("请输入开始时间：");
                int start = Console.ReadLine().ToInt();
                Console.WriteLine("请输入结束时间：");
                int end = Console.ReadLine().ToInt();

                //////generation++;
                ////codes.ForEach(code =>
                ////{
                ////    Adaptability_System ads = new Adaptability_System(0, 0);
                ////    Factor_Forest ff = new Factor_Forest(generation, 3, 7, 3);
                ////    var filepath = @"C:\Users\073\Desktop\wppa\" + code + @"\";
                ////    new FileInfo(filepath).Directory.Create();
                ////    ff.Aaptability_Test(ads, code);
                ////    ff.output1(generation, code);
                ////    ff.output2(generation, code);
                ////});
                var res = code1.Select(code2 =>
                {
                    //var filepath = @"C:\Users\073\Desktop\wppa\" + code + @"\";
                    //new FileInfo(filepath).Directory.Create();
                    //ff.Aaptability_Test(ads, code);
                    //ff.output1(generation, code);
                    //ff.output2(generation, code);
                    Factor_Forest ff1 = new Factor_Forest(generation, 7, 15, 7, code2);
                    //ff1.Aaptability_Test(ads, code1);
                    Real_Test_System real_Test_System = new Real_Test_System(ff1, 40);
                    var rtn = real_Test_System.Real_Test2(code2, start, end, 40);
                    //rtn.ToCsvAutoMap(@"C:\Users\073\Desktop\wppa5\" + code + ".csv");
                    //var rtn1 = real_Test_System.Real_Test2(code, "20170101", "20180101", 40);
                    //iii++;
                    //Console.WriteLine("第" + iii + "只票：" + code);
                    return rtn;
                }).ToArray();
                int t = 0;
                Dictionary<DateTime, double> real_rtn = new Dictionary<DateTime, double>();
                foreach (Dictionary<DateTime, double> retu in res)
                {
                    retu.ForEach(tup =>
                    {
                        if (real_rtn.Keys.Contains(tup.Key))
                        {
                            real_rtn[tup.Key] += tup.Value;
                        }
                        else
                        {
                            real_rtn[tup.Key] = tup.Value;
                        }
                    });
                    t++;
                    Console.WriteLine(t);
                }
                real_rtn.Values.Cumsum().DrawLine();
                //Console.WriteLine();
                //Factor_Forest ff = new Factor_Forest(generation);
                //ff.Aaptability_Test(ads);
                //generation++;
                //ff.output2(generation);
                //List<double[]> gen = new List<double[]>();
                //for (int i = 8; i <= 33; i = i + 1)
                //{
                //Factor_Forest ff = new Factor_Forest(generation, 3, 7, 3);
                ////ff.Aaptability_Test(ads);
                //Real_Test_System real_Test_System = new Real_Test_System(ff);
                //real_Test_System.Real_Test("000537", "20170101", "20170901", 20);
                //}
                //double[] axisx = new double[gen[0].Count()];
                //for (int ii = 0; ii < gen[0].Count(); ii++) axisx[ii] = ii + 1;
                //axisx.DrawLine(gen[0].FillNA(0).Cumsum().ToArray(), gen[1].FillNA(0).Cumsum().ToArray(),gen[2].FillNA(0).Cumsum().ToArray()
                //    ,gen[3].FillNA(0).Cumsum().ToArray(), gen[4].FillNA(0).Cumsum().ToArray(), gen[5].FillNA(0).Cumsum().ToArray()
                //     , gen[6].FillNA(0).Cumsum().ToArray(), gen[7].FillNA(0).Cumsum().ToArray(), gen[8].FillNA(0).Cumsum().ToArray()
                //      , gen[9].FillNA(0).Cumsum().ToArray(), gen[10].FillNA(0).Cumsum().ToArray(), gen[11].FillNA(0).Cumsum().ToArray()
                //       , gen[12].FillNA(0).Cumsum().ToArray(), gen[13].FillNA(0).Cumsum().ToArray(), gen[14].FillNA(0).Cumsum().ToArray()
                //        , gen[15].FillNA(0).Cumsum().ToArray(), gen[16].FillNA(0).Cumsum().ToArray(), gen[17].FillNA(0).Cumsum().ToArray()
                //         , gen[18].FillNA(0).Cumsum().ToArray(), gen[19].FillNA(0).Cumsum().ToArray(), gen[20].FillNA(0).Cumsum().ToArray());
                //    while (generation < 200)
                //    {

                //        Factor_Forest ff1 = new Factor_Forest(generation, 7, 15, 7, code);
                //        //ff1.Aaptability_Test(ads,code1);
                //        int count = ff1.Forest.Count;

                //        ff1.Generate_Trees(100);
                //        int count1 = ff1.Forest.Count;
                //        int seed = 0;
                //        Random r1 = new Random();
                //        Random r2 = new Random();
                //        for (int i = 0; i < count; i++)
                //        {
                //            ff1.gene_mutation(i);
                //            ff1.gene_insert_sequence(i);
                //            ff1.gene_root_insert_sequence(i);
                //            ff1.gene_one_point_crossover(i, r1.Next(0, count1));
                //            ff1.gene_two_point_crossover(i, r1.Next(0, count1));
                //            //int tree2 = r2.Next(count, count1);
                //            //ff1.recommbination(i, tree2);
                //        }
                //        //        //while (ff1.Forest.Count <= 500)
                //        //        //{
                //        //        //    int rr1 = r1.Next(0, 2);
                //        //        //    if (rr1 > 0)
                //        //        //    {
                //        //        //        int tree = r2.Next(0, count);
                //        //        //        ff1.mutation(tree);
                //        //        //    }
                //        //        //    else
                //        //        //    {
                //        //        //        int tree1 = r2.Next(0, count);
                //        //        //        int tree2 = r2.Next(count, count1);
                //        //        //        ff1.recommbination(tree1, tree2);
                //        //        //    }
                //        //        //    seed++;
                //        //        //}
                //        ff1.Aaptability_Test(ads, code1);
                //        generation++;
                //        ff1.output2(generation, code);
                //        ff1.output1(generation, code);
                //    }
                }
                //Factor_Forest ff1 = new Factor_Forest();
                //Adaptability_System ads = new Adaptability_System(0, 0);
                //Factor_Tree ft1 = new Factor_Tree("Corr(Pow(Avg(((ArgMin(Pow(High,3),10)*Close)/Min(Volume,25)),5),1),Delta(Min(High,25),15),20) ");
                //Factor alpha1 = new Factor(ft1);
                //Console.WriteLine(ft1.ToString());
                //double adp1 = ads.Adaptability_Test1(alpha1, "20150101", "20171225", 40, "990905");
                //while (ff.Adaptability.Count < 100)
                //{
                //    ff1.Generate_Trees(1000);
                //    ff.output1();
                //    ff1.Aaptability_Test(ads);
                //    Console.WriteLine(ff1.Adaptability.Count);
                //    //}
                //    generation++;
                //    ff1.output2(generation);
                //    ff1.Forest = ff1.Adaptability.Keys.ToList();
                //}
                //Factor_Tree ft1 = new Factor_Tree(" ((-1 * ((low - close) * pow(open,5))) / ((low - high) * pow(close,5)))");
                //Factor alpha1 = new Factor(ft1);
                //Console.WriteLine(ft1.ToString());
                //Factor_Tree ft2 = new Factor_Tree(" (((sum(high, 20) / 20) < high) ? (-1 * delta(high, 2)) : 0)");
                //Factor alpha2 = new Factor(ft2);
                //Console.WriteLine(ft2.ToString());
                //var new_fts = ft1.recombination(ft2);
                //double adp1 = ads.Adaptability_Test1(alpha1, "20150101", "20171225", 40, "990905");
                //double adp2 = ads.Adaptability_Test1(alpha2, "20150101", "20171225", 40, "990905");
                ////Factor_Tree ft3 = ft1.mutation();
                ////Factor alpha3 = new Factor(ft3);
                ////Console.WriteLine(ft3.ToString());
                //////var new_fts= ft1.recombination(ft2);
                ////double adp3 = ads.Adaptability_Test1(alpha3, "20150101", "20171225", 40, "990905");
                ////Factor_Tree ft4 = ft2.mutation();
                ////Factor alpha4 = new Factor(ft4);
                ////Console.WriteLine(ft4.ToString());
                ////var new_fts = ft1.recombination(ft2);
                ////double adp4 = ads.Adaptability_Test1(alpha4, "20150101", "20171225", 40, "990905");
                //foreach (Factor_Tree ft in new_fts)
                //{
                //    Factor alpha = new Factor(ft);
                //    Console.WriteLine(ft.ToString());
                //    adp1 = ads.Adaptability_Test1(alpha, "20150101", "20171225", 40, "990905");
                //}
                //Factor_Tree ft2 = new Factor_Tree(" (((sum(high, 20) / 20) < high) ? (-1 * delta(high, 2)) : 0)");
                //Factor alpha2 = new Factor(ft2);
                ////ft1.mutation();
                //Console.WriteLine(ft2.ToString());
                //bool adp2 = ads.Adaptability_Test1(alpha2, "20150101", "20171225", 40, "990905");
                //ft1.recombination(ft2);
                //Factor alpha3 = new Factor(ft1);
                ////ft1.mutation();
                //Console.WriteLine(ft1.ToString());
                //bool adp3 = ads.Adaptability_Test1(alpha3, "20150101", "20171225", 40, "990905");
                //Factor alpha4 = new Factor(ft2);
                ////ft1.mutation();
                //Console.WriteLine(ft2.ToString());
                //bool adp4 = ads.Adaptability_Test1(alpha4, "20150101", "20171225", 40, "990905");
                //Constant_Node para1 = new Constant_Node(-1, -1);
                //Constant_Node para2 = new Constant_Node(5, 5);
                //Constant_Node para3 = new Constant_Node(5, 5);
                //Variable_Node var1 = new Variable_Node(Variable_Node.Low);
                //Variable_Node var2 = new Variable_Node(Variable_Node.Close);
                //Variable_Node var3 = new Variable_Node(Variable_Node.Open);
                //Variable_Node var4 = new Variable_Node(Variable_Node.Low);
                //Variable_Node var5 = new Variable_Node(Variable_Node.High);
                //Variable_Node var6 = new Variable_Node(Variable_Node.Close);
                //Two_Variable_Function_Node tfunc1 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Multiply);
                //Two_Variable_Function_Node tfunc2 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Subtract);
                //Two_Variable_Function_Node tfunc3 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Powd);
                //Two_Variable_Function_Node tfunc4 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Multiply);
                //Two_Variable_Function_Node tfunc5 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Subtract);
                //Two_Variable_Function_Node tfunc6 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Powd);
                //Two_Variable_Function_Node tfunc7 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Multiply);
                //Two_Variable_Function_Node tfunc8 = new Two_Variable_Function_Node(Two_Variable_Function_Node.Dividen);
                //tfunc3.set_Lchild(var3); tfunc3.set_Rchild(para3);
                //tfunc2.set_Lchild(var1); tfunc2.set_Rchild(var2);
                //tfunc5.set_Lchild(var4); tfunc5.set_Rchild(var5);
                //tfunc6.set_Lchild(var6); tfunc6.set_Rchild(para2);
                //tfunc7.set_Lchild(tfunc5); tfunc7.set_Rchild(tfunc6);
                //tfunc1.set_Lchild(tfunc2); tfunc1.set_Rchild(tfunc3);
                //tfunc4.set_Lchild(tfunc1);tfunc4.set_Rchild(para1);
                //tfunc8.set_Lchild(tfunc4); tfunc8.set_Rchild(tfunc7);
                //Factor_Tree ft = new Factor_Tree(tfunc8);
                //ft.Parse(" ((-1 * ((low - close) * pow(open,5))) / ((low - high) * pow(close,5)))");
                //Factor alpha = new Factor(ft);
                //ft.mutation();
                //Console.WriteLine(ft.ToString());
                //Adaptability_System ads = new Adaptability_System(0, 0);
                //bool adp = ads.Adaptability_Test1(alpha, "20150101", "20171225", 40, "990905");
                //int x = 0;
            }
        }
}
