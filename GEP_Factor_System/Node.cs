using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acsy.Common.Data;

namespace GEP_Factor_System
{
    public abstract class Node
    {
        public bool value_type { get; set; }
        public double max_possible_value { get; set; }
        public double min_possible_value { get; set; }
        public Node father { get; set; }
        public int child_type { get; set; }
        public String Type { get; set; }
        public short Name { get; set; }
        public int Num { get; set; }
        public double Value { get; set; }
        public abstract bool set_Value(Bar newBar);
        public abstract Node traversal(ref int num);
        public abstract bool recombination(Node newNode,int child_type);
        public abstract void mutation();
        public abstract void update_num();
        public abstract void set_possible_value();
        //public abstract String level_traversal();
    }
}
