using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acsy.Common.Data;
using Acsy.Common.Utility;

namespace GEP_Factor_System
{
    public class Two_Variable_Function_Node:Node
    {
        private Node Lchild;
        private Node Rchild;
        public static short Add = 1;
        public static short Subtract = 2;
        public static short Multiply = 3;
        public static short Dividen = 4;
        public static short GT = 5;
        public static short LT = 6;
        public static short EQ = 7;
        public static short OR = 8;
        public static short AND = 9;
        public static short Delay = 10;
        public static short Delta = 11;
        public static short Powd = 12;
        public static short Min = 13;
        public static short Max = 14;
        public static short ArgMax = 15;
        public static short ArgMin = 16;
        public static short Rank = 17;
        public static short Avg = 18;
        public static short STD = 19;
        public static short Sum = 20;
        public static short Product = 21;
        movingAverageLine ma;
        movingStandardDeviationLine std;
        RollingQueue<double> queue;
        public Two_Variable_Function_Node(short name)
        {
            this.Type = "Two_Variable_Function_Node";
            this.Name = name;
            this.Num = 1;

        }
        public override void set_possible_value()
        {
            if(this.Name == Two_Variable_Function_Node.Add)
            {
                this.value_type = this.Lchild.value_type || this.Rchild.value_type;
                this.min_possible_value = this.Lchild.min_possible_value + this.Rchild.min_possible_value;
                this.max_possible_value = this.Lchild.max_possible_value + this.Rchild.max_possible_value;
            }
            else if (this.Name == Two_Variable_Function_Node.Subtract)
            {
                this.value_type = this.Lchild.value_type || this.Rchild.value_type;
                this.min_possible_value = this.Lchild.min_possible_value - this.Rchild.max_possible_value;
                this.max_possible_value = this.Lchild.max_possible_value - this.Rchild.min_possible_value;
            }
            else if (this.Name == Two_Variable_Function_Node.Multiply)
            {
                this.value_type = this.Lchild.value_type || this.Rchild.value_type;
                this.min_possible_value = Math.Min(Math.Min(Math.Min(this.Lchild.min_possible_value * this.Rchild.min_possible_value, this.Lchild.min_possible_value * this.Rchild.max_possible_value), this.Lchild.max_possible_value * this.Rchild.min_possible_value),this.Lchild.max_possible_value*this.Rchild.max_possible_value);
                this.max_possible_value = Math.Max(Math.Max(Math.Max(this.Lchild.min_possible_value * this.Rchild.min_possible_value, this.Lchild.min_possible_value * this.Rchild.max_possible_value), this.Lchild.max_possible_value * this.Rchild.min_possible_value),this.Lchild.max_possible_value * this.Rchild.max_possible_value);
            }
            else if (this.Name == Two_Variable_Function_Node.Dividen)
            {
                this.value_type = this.Lchild.value_type || this.Rchild.value_type;
                if (this.Rchild.min_possible_value == 0) this.min_possible_value = 0;
                else this.min_possible_value = Math.Min(Math.Min(Math.Min(this.Lchild.max_possible_value / this.Rchild.min_possible_value, this.Lchild.min_possible_value / this.Rchild.max_possible_value), this.Rchild.max_possible_value / this.Lchild.min_possible_value), this.Rchild.min_possible_value / this.Lchild.max_possible_value);
                if (this.Rchild.max_possible_value == 0) this.max_possible_value = 0;
                else this.max_possible_value = Math.Max(Math.Max(Math.Max(this.Lchild.max_possible_value / this.Rchild.min_possible_value, this.Lchild.min_possible_value / this.Rchild.max_possible_value), this.Rchild.max_possible_value / this.Lchild.min_possible_value), this.Rchild.min_possible_value / this.Lchild.max_possible_value);
            }
            else if (this.Name == Two_Variable_Function_Node.GT)
            {
                this.value_type = false;
                this.min_possible_value = this.Lchild.min_possible_value >= this.Rchild.max_possible_value ? 1:0 ;
                this.max_possible_value = this.Lchild.max_possible_value <= this.Rchild.min_possible_value ? 0:1;
            }
            else if (this.Name == Two_Variable_Function_Node.LT)
            {
                this.value_type = false;
                this.min_possible_value = this.Lchild.max_possible_value <= this.Rchild.min_possible_value ? 1 : 0;
                this.max_possible_value = this.Lchild.min_possible_value >= this.Rchild.max_possible_value ? 0 : 1;
            }
            else if (this.Name == Two_Variable_Function_Node.EQ)
            {
                this.value_type = false;
                this.min_possible_value = 0;
                this.max_possible_value = 1;
            }
            else if (this.Name == Two_Variable_Function_Node.OR)
            {
                this.value_type = false;
                this.min_possible_value = this.Lchild.min_possible_value+this.Rchild.min_possible_value>0?1:0;
                this.max_possible_value = this.Lchild.max_possible_value + this.Rchild.max_possible_value > 1 ? 1 : 0;
            }
            else if (this.Name == Two_Variable_Function_Node.AND)
            {
                this.value_type = false;
                this.min_possible_value = this.Lchild.min_possible_value * this.Rchild.min_possible_value > 0 ? 1 : 0;
                this.max_possible_value = this.Lchild.max_possible_value * this.Rchild.max_possible_value > 0 ? 1 : 0;
            }
            else if (this.Name >= Two_Variable_Function_Node.Delay&&this.Name <= Two_Variable_Function_Node.Max)
            {
                this.value_type = this.Lchild.value_type;
                this.min_possible_value = this.Lchild.min_possible_value;
                this.max_possible_value = this.Lchild.max_possible_value;
            }
            else if (this.Name >= Two_Variable_Function_Node.ArgMax && this.Name <= Two_Variable_Function_Node.Rank)
            {
                this.value_type = this.Lchild.value_type;
                this.min_possible_value = 1;
                this.max_possible_value = this.Rchild.max_possible_value;
            }
            else if (this.Name >= Two_Variable_Function_Node.Avg && this.Name <= Two_Variable_Function_Node.Product)
            {
                this.value_type = this.Lchild.value_type;
                this.min_possible_value = this.Lchild.min_possible_value;
                this.max_possible_value = this.Lchild.max_possible_value;
            }
            this.Num += this.Lchild.Num;
            this.Num += this.Rchild.Num;
            if (this.father != null) this.father.set_possible_value();
        }
        public bool set_Lchild(Node Lchild)
        {
            if (Lchild != null) { this.Lchild = Lchild;Lchild.father = this;this.Lchild.child_type = 1; }
            else return false;
            return true;
        }
        public bool set_Rchild(Node Rchild)
        {
            if (Rchild != null) { this.Rchild = Rchild;  Rchild.father = this;Rchild.child_type = 2; }
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
        public override bool set_Value(Bar newBar)
        {
            if ((this.Lchild != null) && (this.Rchild != null))
            {
                this.Lchild.set_Value(newBar);
                this.Rchild.set_Value(newBar);
                if (this.Name.Equals(Two_Variable_Function_Node.Add))
                {
                    this.value_type = true;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if(Lchild.value_type&&Rchild.value_type) this.Value = Lchild.Value + Rchild.Value;
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Subtract))
                {
                    this.value_type = true;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (Lchild.value_type && Rchild.value_type) this.Value = Lchild.Value - Rchild.Value;
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Multiply))
                {
                    this.value_type = true;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (Lchild.value_type && Rchild.value_type) this.Value = Lchild.Value * Rchild.Value;
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Dividen))
                {
                    this.value_type = true;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (Lchild.value_type && Rchild.value_type)
                    {
                        if (Rchild.Value == 0) this.Value = Double.NaN;
                        else this.Value = Lchild.Value * Rchild.Value;
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.GT))
                {
                    this.value_type = false;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (Lchild.value_type && Rchild.value_type)
                    {
                        if (Lchild.Value > Rchild.Value) this.Value = 1;
                        else this.Value = 0;
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.LT))
                {
                    this.value_type = false;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (Lchild.value_type && Rchild.value_type)
                    {
                        if (Lchild.Value < Rchild.Value) this.Value = 1;
                        else this.Value = 0;
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.EQ))
                {
                    this.value_type = false;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (Lchild.value_type && Rchild.value_type)
                    {
                        if (Lchild.Value == Rchild.Value) this.Value = 1;
                        else this.Value = 0;
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.OR))
                {
                    this.value_type = false;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (!Lchild.value_type && !Rchild.value_type)
                    {
                        if (Lchild.Value > 0 || Rchild.Value > 0) this.Value = 1;
                        else this.Value = 0;
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.AND))
                {
                    this.value_type = false;
                    if (Double.IsNaN(Lchild.Value) || Double.IsNaN(Rchild.Value)) this.Value = Double.NaN;
                    else if (!Lchild.value_type && !Rchild.value_type)
                    {
                        if (Lchild.Value > 0 && Rchild.Value > 0) this.Value = 1;
                        else this.Value = 0;
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Delay))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else queue.Push(this.Lchild.Value);
                            if (queue.Count() == (int)this.Rchild.Value) this.Value = queue.Front;
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Delta))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else queue.Push(this.Lchild.Value);
                            if (queue.Count() == (int)this.Rchild.Value) this.Value = this.Lchild.Value - queue.Front;
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                    
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Powd))
                {
                    this.Value = Math.Pow(this.Lchild.Value, this.Rchild.Value);
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Max))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else queue.Push(this.Lchild.Value);
                            if (queue.Count() == (int)this.Rchild.Value) this.Value = this.queue.Max();
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                   
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Min))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else queue.Push(this.Lchild.Value);
                            if (queue.Count() == (int)this.Rchild.Value) this.Value = this.queue.Min();
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                   
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Avg))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (ma == null) ma = new movingAverageLine((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else ma.Push(this.Lchild.Value);
                            if (ma.Count == (int)this.Rchild.Value) this.Value = this.ma.Value;
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                  
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.STD))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (std == null) std = new movingStandardDeviationLine((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else std.Push(this.Lchild.Value);
                            if (std.Count == (int)this.Rchild.Value) this.Value = this.std.Value;
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                    
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Sum))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else queue.Push(this.Lchild.Value);
                            if (queue.Count() == (int)this.Rchild.Value) this.Value = this.queue.Cumsum().Last();
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                    
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Product))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (this.Lchild.value_type)
                        {
                            if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                            if (Double.IsNaN(this.Lchild.Value)) ;
                            else queue.Push(this.Lchild.Value);
                            if (queue.Count() == (int)this.Rchild.Value)
                            {
                                double p = 1;
                                foreach (double v in queue) p = p * v;
                                this.Value = p;
                            }
                            else this.Value = Double.NaN;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                  
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.Rank))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (queue == null)
                        { queue = new RollingQueue<double>((int)this.Rchild.Value); }
                        if (Double.IsNaN(this.Lchild.Value)) ;
                        else queue.Push(this.Lchild.Value);
                        if (queue.Count() == (int)this.Rchild.Value)
                        {
                            int count = 0;
                            foreach (double v in queue) if (this.Lchild.Value > v) count++;
                            this.Value = count;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                    
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.ArgMax))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                        if (Double.IsNaN(this.Lchild.Value)) ;
                        else queue.Push(this.Lchild.Value);
                        if (queue.Count() == (int)this.Rchild.Value)
                        {
                            int index = 0; double max = -10000000;
                            for (int i = 0; i < queue.Count; i++)
                            {
                                if (queue[i] > max)
                                {
                                    max = queue[i];
                                    index = i;
                                }
                            }
                            this.Value = index;
                        }
                        else this.Value = Double.NaN;
                    }
                    else this.Value = Double.NaN;
                   
                }
                else if (this.Name.Equals(Two_Variable_Function_Node.ArgMin))
                {
                    this.value_type = true;
                    if (this.Rchild.Type.Equals("Constant_Node"))
                    {
                        if (queue == null) queue = new RollingQueue<double>((int)this.Rchild.Value);
                        if (Double.IsNaN(this.Lchild.Value)) ;
                        else queue.Push(this.Lchild.Value);
                        if (queue.Count() == (int)this.Rchild.Value)
                        {
                            int index = 0; double min = 1000000;
                            for (int i = 0; i < queue.Count; i++)
                            {
                                if (queue[i] < min)
                                {
                                    min = queue[i];
                                    index = i;
                                }
                            }
                            this.Value = index;
                        }
                        else this.Value = Double.NaN;
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
                    return this.Rchild.traversal(ref num);
                }
                else return node1;
            }
            else return this;
        }
        public override bool recombination(Node newNode,int child_type)
        {
            if (this.Name >= 1 && this.Name <= 4)
            {
                if (newNode.value_type)
                {
                    if (child_type == 1)
                    {
                        bool result = this.set_Lchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else
                    {
                        bool result = this.set_Rchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                }
                else return false;
            }
            else if (this.Name >= 5 && this.Name <= 7)
            {
                if (newNode.value_type)
                {
                    if (child_type == 1)
                    {
                        if (!(newNode.min_possible_value > this.Rchild.max_possible_value || newNode.max_possible_value < this.Rchild.min_possible_value))
                        {
                            bool result = this.set_Lchild(newNode);
                            if (result) this.set_possible_value();
                            return result;
                        }
                        else return false;
                    }
                    else
                    {
                        if (!(newNode.min_possible_value > this.Lchild.max_possible_value || newNode.max_possible_value < this.Lchild.min_possible_value))
                        {
                            bool result = this.set_Lchild(newNode);
                            if (result) this.set_possible_value();
                            return result;
                        }
                        else return false;
                    }
                }
                else return false;
            }
            else if (this.Name >= 8 && this.Name <= 9)
            {
                if (!newNode.value_type)
                {
                    if (child_type == 1)
                    {
                        bool result = this.set_Lchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else
                    {
                        bool result = this.set_Rchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                }
                else return false;
            }
            else if (this.Name >= 10)
            {
                if (newNode.value_type)
                {
                    if (child_type == 1)
                    {
                        bool result = this.set_Lchild(newNode);
                        if (result) this.set_possible_value();
                        return result;
                    }
                    else
                    {
                        return false;
                    }
                }
                else return false;
            }
            else return false;
            //if (child_type == 1)
            //{
            //    bool s =  this.set_Lchild(newNode);
            //    return s;
            //}
            //else
            //{
            //    bool s = this.set_Rchild(newNode);
            //    return s;
            //}
        }
        public override void mutation()
        {
            if (this.Name >= 1 && this.Name <= 4)
            {
                Random r = new Random();
                this.Name = (short)r.Next(1, 5);
            }
            else if(this.Name >= 5 && this.Name <=7)
            {
                Random r = new Random();
                this.Name = (short)r.Next(5,8);
            }
            else if (this.Name >= 8 && this.Name <= 9)
            {
                Random r = new Random();
                this.Name = (short)r.Next(8, 10);
            }
            else if (this.Name >= 10 && this.Name <= 20)
            {
                Random r = new Random();
                this.Name = (short)r.Next(10, 21);
                while(this.Name == 12) this.Name = (short)r.Next(10, 21);
            }
            this.set_possible_value();
        }
        public override string ToString()
        {
            switch (this.Name)
            {
                case 1:
                    return "(" + this.Lchild.ToString() + "+" + this.Rchild.ToString()+")";
                case 2:
                    return "(" + this.Lchild.ToString() + "-" + this.Rchild.ToString() + ")";
                case 3:
                    return "(" + this.Lchild.ToString() + "*" + this.Rchild.ToString() + ")";
                case 4:
                    return "(" + this.Lchild.ToString() + "/" + this.Rchild.ToString() + ")";
                case 5:
                    return "(" + this.Lchild.ToString() + ">" + this.Rchild.ToString() + ")";
                case 6:
                    return "(" + this.Lchild.ToString() + "<" + this.Rchild.ToString() + ")";
                case 7:
                    return "(" + this.Lchild.ToString() + "==" + this.Rchild.ToString() + ")";
                case 8:
                    return "(" + this.Lchild.ToString() + "||" + this.Rchild.ToString() + ")";
                case 9:
                    return "(" + this.Lchild.ToString() + "&&" + this.Rchild.ToString() + ")";
                case 10:
                    return "Delay(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 11:
                    return "Delta(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 12:
                    return "Pow(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 13:
                    return "Min(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 14:
                    return "Max(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 15:
                    return "ArgMax(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 16:
                    return "ArgMin(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 17:
                    return "Rank(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 18:
                    return "Avg(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 19:
                    return "STD(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 20:
                    return "Sum(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                case 21:
                    return "Product(" + this.Lchild.ToString() + "," + this.Rchild.ToString() + ")";
                default:
                    throw new Exception();

            }
        }
        public override void update_num()
        {
            this.Num = this.Lchild.Num+this.Rchild.Num + 1;
        }
    }
}
