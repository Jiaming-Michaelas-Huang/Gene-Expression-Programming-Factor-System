using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acsy.Common.Data;
using Acsy.Common.Utility;
using MathNet.Numerics.Statistics;

namespace GEP_Factor_System
{
    public class Three_Variable_Function_Node:Node
    {
        private Node Lchild;
        private Node Rchild;
        private Node Mchild;
        public static short IF = 1;
        public static short CORR = 2;
        public static short WeightedMean = 3;
        RollingQueue<double> queue1,queue2;
        public Three_Variable_Function_Node(short name)
        {
            this.Type = "Three_Variable_Function_Node";
            this.Name = name;
            this.Num = 1;

        }
        public override void set_possible_value()
        {
            if (this.Name == Three_Variable_Function_Node.IF)
            {
                this.value_type = this.Mchild.value_type && this.Rchild.value_type;
                this.min_possible_value = Math.Min(this.Mchild.min_possible_value,this.Rchild.min_possible_value);
                this.max_possible_value = Math.Max(this.Mchild.max_possible_value, this.Rchild.max_possible_value);
            }
            if (this.Name == Three_Variable_Function_Node.CORR)
            {
                this.value_type = true;
                this.min_possible_value = 0;
                this.max_possible_value = 1;
            }
            if (this.Name == Three_Variable_Function_Node.WeightedMean)
            {
                this.value_type = this.Lchild.value_type && this.Rchild.value_type;
                this.min_possible_value = this.Lchild.min_possible_value;
                this.max_possible_value = this.Lchild.max_possible_value;
            }
            this.Num += this.Lchild.Num;
            this.Num += this.Mchild.Num;
            this.Num += this.Rchild.Num;
            if (this.father != null) this.father.set_possible_value();
        }
        public bool set_Lchild(Node Lchild)
        {
            if (Lchild != null) { this.Lchild = Lchild; Lchild.father = this;Lchild.child_type = 1; }
            else return false;
            return true;
        }
        public bool set_Rchild(Node Rchild)
        {
            if (Rchild != null) { this.Rchild = Rchild; Rchild.father = this;Rchild.child_type = 3; }
            else return false;
            return true;
        }
        public Node get_Lchild()
        {
            return this.Lchild;
        }
        public Node get_Rchild()
        {
            return this.Rchild;
        }
        public bool set_Mchild(Node Mchild)
        {
            if (Mchild != null) { this.Mchild = Mchild; Mchild.father = this;Mchild.child_type = 2; }
            else return false;
            return true;
        }
        public Node get_Mchild()
        {
            return this.Mchild;
        }
        public override bool set_Value(Bar newBar)
        {
            this.Lchild.set_Value(newBar);
            this.Mchild.set_Value(newBar);
            this.Rchild.set_Value(newBar);
            this.value_type = true;
            if (this.Lchild != null && this.Mchild != null && this.Rchild != null)
            {
                if (this.Name.Equals(Three_Variable_Function_Node.IF))
                {
                    if (!this.Lchild.value_type && this.Mchild.value_type && this.Rchild.value_type)
                    {
                        if (Double.IsNaN(this.Lchild.Value)) this.Value = Double.NaN;
                        else if (this.Lchild.Value > 0)
                        {
                            if (Double.IsNaN(this.Mchild.Value)) this.Value = Double.NaN;
                            else this.Value = this.Mchild.Value;
                        }
                        else
                        {
                            if (Double.IsNaN(this.Rchild.Value)) this.Value = Double.NaN;
                            else this.Value = Rchild.Value;
                        }
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Three_Variable_Function_Node.CORR))
                {
                    if (this.Lchild.value_type && this.Mchild.value_type)
                    {
                        if (this.Rchild.Type.Equals("Constant_Node"))
                        {
                            if (queue1 == null) { queue1 = new RollingQueue<double>((int)this.Rchild.Value); queue2 = new RollingQueue<double>((int)this.Rchild.Value); }
                            if (Double.IsNaN(this.Lchild.Value) || Double.IsNaN(this.Mchild.Value)) ;
                            else { queue1.Push(this.Lchild.Value); queue2.Push(this.Mchild.Value); }
                            var icc = Correlation.Pearson(queue1, queue2);
                            if (Double.IsNaN(icc))
                            {
                                this.Value = Double.NaN;
                            }
                            else if (queue1.Count != (int)this.Rchild.Value) this.Value = Double.NaN;
                            else this.Value = icc;
                        }
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Three_Variable_Function_Node.WeightedMean))
                {
                    if (this.Lchild.value_type && this.Mchild.value_type)
                    {
                        if (this.Rchild.Type.Equals("Constant_Node"))
                        {
                            if (queue1 == null) { queue1 = new RollingQueue<double>((int)this.Rchild.Value); queue2 = new RollingQueue<double>((int)this.Rchild.Value); }
                            if (Double.IsNaN(this.Lchild.Value) || Double.IsNaN(this.Mchild.Value)) ;
                            else { queue1.Push(this.Lchild.Value); queue2.Push(this.Mchild.Value); }
                            if (queue1.Count != (int)this.Rchild.Value) this.Value = Double.NaN;
                            else if (queue2.Cumsum().Last() == 0) this.Value = Double.NaN;
                            else this.Value = queue1.Cumsum().Last() / queue2.Cumsum().Last();
                        }
                    }
                    else this.Value = Double.NaN;
                }
            }
            else return false;
            return true;
        }
        public override Node traversal(ref int num)
        {
            num--;
            if (num > 0)
            {
                Node node1 = this.Lchild.traversal(ref num);
                if (num > 0)
                {
                    Node node2 = this.Mchild.traversal(ref num);
                    if (num > 0)
                    {
                        return this.Rchild.traversal(ref num);
                    }
                    else return node2;
                }
                else return node1;
            }
            else return this;
        }
        public override bool recombination(Node newNode,int child_type)
        {
            if (this.Name == Three_Variable_Function_Node.IF)
            {
                if (child_type == 1)
                {
                    if (!this.value_type)
                    {
                        bool result = this.set_Lchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else return false;
                }
                else if (child_type == 2)
                {
                    if (this.value_type)
                    {
                        bool result = this.set_Mchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else return false;
                }
                else if (child_type == 3)
                {
                    if (this.value_type)
                    {
                        bool result = this.set_Rchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else return false;
                }
                else return false;
            }
            else if (this.Name >= Three_Variable_Function_Node.CORR)
            {
                if (child_type == 1)
                {
                    if (this.value_type)
                    {
                        bool result = this.set_Lchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else return false;
                }
                else if (child_type == 2)
                {
                    if (this.value_type)
                    {
                        bool result = this.set_Mchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else return false;
                }
                else if (child_type == 3)
                {
                    return false;
                }
                else return false;
            }
            else return false;

                //if (child_type == 1) return this.set_Lchild(newNode);
                //else if (child_type == 2) return this.set_Mchild(newNode);
                //else return this.set_Rchild(newNode);
            }
        public override void mutation()
        {
            if(this.Name==Three_Variable_Function_Node.CORR) this.Name = Three_Variable_Function_Node.WeightedMean;
            if (this.Name == Three_Variable_Function_Node.WeightedMean) this.Name = Three_Variable_Function_Node.CORR;
            this.set_possible_value();
        }
        public override string ToString()
        {
            switch (this.Name)
            {
                case 1:
                    return "(" + this.Lchild.ToString() + "?" + this.Mchild.ToString() + ":" + this.Rchild.ToString() + ")";
                case 2:
                    return "Corr(" + this.Lchild.ToString() + "," + this.Mchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 3:
                    return "WeightedMean(" + this.Lchild.ToString() + "," + this.Mchild.ToString() + "," + this.Rchild.ToString() + ")";
                default:
                    throw new Exception();
            }
        }
        public override void update_num()
        {
            this.Num = this.Lchild.Num+this.Mchild.Num+this.Rchild.Num + 1;
        }
    }
}
