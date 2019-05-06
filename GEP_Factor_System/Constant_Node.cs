using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acsy.Common.Data;

namespace GEP_Factor_System
{
    public class Constant_Node : Node
    {
        public Constant_Node(short name,double value)
        {
            this.Type = "Constant_Node";
            this.Name = name;
            this.Num = 1;
            this.Value = value;
            this.set_possible_value();
        }
        public override void set_possible_value()
        {
            this.value_type = true;
            this.max_possible_value = this.Value;
            this.min_possible_value = this.Value;
            if (this.father != null) this.father.set_possible_value();
        }
        public override bool set_Value(Bar newBar)
        {
            this.value_type = true;
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
            this.Value = (short)r.Next(Math.Min(Math.Abs((int)this.Value - 5)+1, Math.Abs((int)this.Value + 5)+1), Math.Max(Math.Abs((int)this.Value - 5), Math.Abs((int)this.Value + 5)));
            this.set_possible_value();
            //this.Value = (short)r.Next(Math.Abs((int)this.Value/5),Math.Abs((int)this.Value*5));
        }
        public override string ToString()
        {
            return this.Value.ToString();
        }
        public override void update_num()
        {
            this.Num = 1;
        }
    }
}
