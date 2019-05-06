using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acsy.Common.Data;
using Acsy.Common.Utility;

namespace GEP_Factor_System
{
    public class Variable_Node:Node
    {
        public static short Open = 1;
        public static short Close = 2;
        public static short High = 3;
        public static short Low = 4;
        public static short Volume = 5;
        public static short Turnover = 6;
        public static short Price = 7;
        EMA ma = new EMA(60);
        movingStandardDeviationLine std = new movingStandardDeviationLine(60);
        public Variable_Node(short name)
        {
            this.Type = "Variable_Node";
            this.Name = name;
            this.Num = 1;
            this.set_possible_value();
        }
        public override void set_possible_value()
        {
            this.value_type = true;
            if (Name < 3||Name == 7)
            {
                this.max_possible_value = 110;
                this.min_possible_value = 100;
            }
            else if(Name < 4)
            {
                this.max_possible_value = 121;
                this.min_possible_value = 111;
            }
            else if(Name < 5)
            {
                this.max_possible_value = 99;
                this.min_possible_value = 89;
            }
            else if(Name < 6)
            {
                this.max_possible_value = 10000;
                this.min_possible_value = 1000;
            }
            else if(Name < 7)
            {
                this.max_possible_value = 100000;
                this.min_possible_value = 10000;
            }
            if (this.father != null) this.father.set_possible_value();
        }
        public override bool set_Value(Bar newBar)
        {
            //if (this.Name.Equals(Variable_Node.Open))
            //{
            //    if (!Double.IsNaN(newBar.Open))
            //    {
            //        ma.Push(newBar.Open);
            //        std.Push(newBar.Open);
            //    }
            //    if (std.Value != 0 && !Double.IsNaN(std.Value))
            //    {
            //        this.Value = (newBar.Open - ma.Value) / std.Value;
            //    }
            //}
            //else if (this.Name.Equals(Variable_Node.Close))
            //{
            //    if (!Double.IsNaN(newBar.Close))
            //    {
            //        ma.Push(newBar.Close);
            //        std.Push(newBar.Close);
            //    }
            //    if (std.Value != 0 && !Double.IsNaN(std.Value))
            //    {
            //        this.Value = (newBar.Close - ma.Value) / std.Value;
            //    }
            //}
            //else if (this.Name.Equals(Variable_Node.High))
            //{
            //    if (!Double.IsNaN(newBar.High))
            //    {
            //        ma.Push(newBar.High);
            //        std.Push(newBar.High);
            //    }
            //    if (std.Value != 0 && !Double.IsNaN(std.Value))
            //    {
            //        this.Value = (newBar.High - ma.Value) / std.Value;
            //    }
            //}
            //else if (this.Name.Equals(Variable_Node.Low))
            //{
            //    if (!Double.IsNaN(newBar.Low))
            //    {
            //        ma.Push(newBar.Low);
            //        std.Push(newBar.Low);
            //    }
            //    if (std.Value != 0 && !Double.IsNaN(std.Value))
            //    {
            //        this.Value = (newBar.Low - ma.Value) / std.Value;
            //    }
            //}
            //else if (this.Name.Equals(Variable_Node.Volume))
            //{
            //    if (!Double.IsNaN(newBar.Volume))
            //    {
            //        ma.Push(newBar.Volume);
            //        std.Push(newBar.Volume);
            //    }
            //    if (std.Value != 0 && !Double.IsNaN(std.Value))
            //    {
            //        this.Value = (newBar.Volume - ma.Value) / std.Value;
            //    }
            //}
            //else if (this.Name.Equals(Variable_Node.Turnover))
            //{
            //    if (!Double.IsNaN(newBar.Turnover))
            //    {
            //        ma.Push(newBar.Turnover);
            //        std.Push(newBar.Turnover);
            //    }
            //    if (std.Value != 0 && !Double.IsNaN(std.Value))
            //    {
            //        this.Value = (newBar.Turnover - ma.Value) / std.Value;
            //    }
            //}
            //else if (this.Name.Equals(Variable_Node.Price))
            //{
            //    if (!Double.IsNaN(newBar.Price))
            //    {
            //        ma.Push(newBar.Price);
            //        std.Push(newBar.Price);
            //    }
            //    if (std.Value != 0 && !Double.IsNaN(std.Value))
            //    {
            //        this.Value = (newBar.Price - ma.Value) / std.Value;
            //    }
            //}
            this.value_type = true;
            if (this.Name.Equals(Variable_Node.Close)) this.Value = newBar.Close;
            else if (this.Name.Equals(Variable_Node.Open)) this.Value = newBar.Open;
            else if (this.Name.Equals(Variable_Node.High)) this.Value = newBar.High;
            else if (this.Name.Equals(Variable_Node.Low)) this.Value = newBar.Low;
            else if (this.Name.Equals(Variable_Node.Volume)) this.Value = newBar.Volume;
            else if (this.Name.Equals(Variable_Node.Turnover)) this.Value = newBar.Turnover;
            else if (this.Name.Equals(Variable_Node.Price)) this.Value = newBar.Price;
            else return false;
            return true;
        }
        public override Node traversal(ref int num)
        {
            num--;
            return this;
        }
        public override bool recombination(Node newNode,int child_type)
        {
            return false;
        }
        public override void mutation()
        {
            Random r = new Random();
            this.Name = (short)r.Next(1,8);
            this.set_possible_value();
        }
        public override string ToString()
        {
            switch (this.Name)
            {
                case 1:
                    return "Open";
                case 2:
                    return "Close";
                case 3:
                    return "High";
                case 4:
                    return "Low";
                case 5:
                    return "Volume";
                case 6:
                    return "Turnover";
                case 7:
                    return "Price";
                default:
                    throw new Exception();
            }
        }
        public override void update_num()
        {
            this.Num = 1;
        }
    }
}
