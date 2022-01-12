using System;


namespace STEP_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] schemas = new string[] { @"..\..\data\AP203E2_November_2008.exp" };

            EXPRESS_Schema AP203 = new EXPRESS_Schema(schemas);    //load and parse schema

            StepFile step1 = new StepFile(@"..\..\data\sample01.STEP", AP203);  //parse STEP file
        }
    }
}
