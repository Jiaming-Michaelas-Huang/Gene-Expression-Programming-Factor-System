using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acsy.Common.Data;

namespace GEP_Factor_System
{
    public class One_Variable_Function_Node : Node
    {
        private Node Child;
        public static short Abs = 1;
        public static short Log = 2;
        public static short Sign = 3;
        public One_Variable_Function_Node(short name)
        {
            this.Type = "One_Variable_Function_Node";
            this.Name = name;
            this.Num = 1;
        }
        public override void set_possible_value()
        {
            this.value_type = this.Child.value_type;
            if (this.Name == One_Variable_Function_Node.Abs)
            {
                this.min_possible_value = Math.Abs(this.Child.min_possible_value);
                this.max_possible_value = Math.Abs(this.Child.max_possible_value);
            }
            else if (this.Name == One_Variable_Function_Node.Log)
            {
                this.min_possible_value = Math.Log(this.Child.min_possible_value);
                this.max_possible_value = Math.Log(this.Child.max_possible_value);
            }
            else if (this.Name == One_Variable_Function_Node.Sign)
            {
                this.min_possible_value = Math.Sign(this.Child.min_possible_value);
                this.max_possible_value = Math.Sign(this.Child.max_possible_value);
            }
            this.Num += this.Child.Num;
            if (this.father != null) this.father.set_possible_value();
            
        }
        public bool set_Child(Node child)
        {
            if (child != null)
            {
                this.Child = child;
                //this.Num += this.Child.Num;
                child.father = this;
            }
            else return false;
            return true;
        }
        public Node get_Child()
        {
            return this.Child;
        }
        public override bool set_Value(Bar newBar)
        {
            if (this.Child != null)
            {
                this.Child.set_Value(newBar);
                this.value_type = true;
                if (this.Name.Equals(One_Variable_Function_Node.Abs))
                {
                    if (Double.IsNaN(this.Child.Value)||!this.Child.value_type) this.Value = Double.NaN;
                    this.Value = Math.Abs(this.Child.Value);
                }
                else if (this.Name.Equals(One_Variable_Function_Node.Log))
                {
                    if (this.Child.Value <= 0) this.Value = Double.NaN;
                    else if (Double.IsNaN(this.Child.Value) || !this.Child.value_type) this.Value = Double.NaN;
                    else this.Value = Math.Log(this.Child.Value);
                }
                else if (this.Name.Equals(One_Variable_Function_Node.Sign))
                {
                    if (Double.IsNaN(this.Child.Value) || !this.Child.value_type) this.Value = Double.NaN;
                    else this.Value = Math.Sign(this.Child.Value);
                }
                else { return false; }
                return true;
            }
            else { return false; }
        }
        public override Node traversal(ref int num)
        {
            num--;
            if (num > 0)
            {
                return this.Child.traversal(ref num);
            }
            else return this;
        }
        public override bool recombination(Node newNode,int child_type)
        {
            if (this.Name == One_Variable_Function_Node.Abs)
            {
                if (!newNode.value_type || newNode.min_possible_value > 0) return false;
                else
                {
                    bool result = this.set_Child(newNode);
                    if (result) this.set_possible_value();
                    return result;
                }
            }
            else if (this.Name == One_Variable_Function_Node.Log)
            {
                if (!newNode.value_type || newNode.min_possible_value < 0) return false;
                else
                {
                    bool result = this.set_Child(newNode);
                    if (result) this.set_possible_value();
                    return result;
                }
            }
            else if (this.Name == One_Variable_Function_Node.Sign)
            {
                if (!newNode.value_type || newNode.min_possible_value > 0||newNode.max_possible_value<0) return false;
                else
                {
                    bool result = this.set_Child(newNode);
                    if (result) this.set_possible_value();
                    return result;
                }
            }
            else return false;
            //return this.set_Child(newNode);
        }
        public override void mutation()
        {
            //Random r = new Random();
            //this.Name = (short)r.Next(1, 4);
            if (this.Name == One_Variable_Function_Node.Abs)
            {
                this.Name = One_Variable_Function_Node.Sign;
            }
            else if (this.Name == One_Variable_Function_Node.Sign)
            {
                this.Name = One_Variable_Function_Node.Abs;
            }
            this.set_possible_value();
        }
        public override string ToString()
        {
            switch (this.Name)
            {
                case 1:
                    return "Abs(" + this.Child.ToString() + ")";
                case 2:
                    return "Log(" + this.Child.ToString() + ")";
                case 3:
                    return "Sign(" +this.Child.ToString() + ")";
                default:
                    throw new Exception();
            }
        }
        public override void update_num()
        {
            this.Num = this.Child.Num + 1;
        }
    }
}
