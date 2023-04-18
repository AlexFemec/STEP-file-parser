using System;
using System.Collections.Generic;
using System.Linq;

namespace STEP_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            //various schemas avaliable at https://github.com/stepcode/stepcode/tree/develop/data
            string[] schemas = new string[] { @"..\..\data\ap203e2_mim_lf.exp" }; 

            EXPRESS_Schema AP203 = new EXPRESS_Schema(schemas);    //load and parse schema(s)

            StepFile step1 = new StepFile(@"..\..\data\sample02.STEP", AP203);  //parse STEP file

            StepPrinter printer = new StepPrinter(step1);

            List<String> tree = printer.TreeToString();

            for (int y = 0; y < tree.Count; y++)
            {
                Console.WriteLine(tree[y]);
            }

        }
    }
}
